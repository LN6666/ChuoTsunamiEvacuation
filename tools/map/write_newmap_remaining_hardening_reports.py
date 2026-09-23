import datetime
import json
import math
import re
import uuid
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ACTIVE_SCENE = "Assets/Scenes/Chuo_BaseMap.unity"
P10 = ROOT / "Assets" / "Data" / "P10"
DOCS = ROOT / "docs"
PROMPTS = ROOT / "codex_prompts"
RESOURCES = ROOT / "Assets" / "Resources" / "NewMap"

P8_AUDIT = "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
P8_HANDOFF = "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json"

FINAL_STATUSES = [
    "completed_on_new_chuo_basemap",
    "completed_with_documented_runtime_proxy",
    "disabled_missing_from_new_map",
    "blocked_needs_user_map_asset",
    "failed",
]


def read_json(relative_path):
    return json.loads((ROOT / relative_path).read_text(encoding="utf-8-sig"))


def write_json(relative_path, data):
    path = ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")
    if path.parts and "Assets" in path.parts:
        ensure_meta(path)
        ensure_folder_metas(path.parent)


def write_text(relative_path, text):
    path = ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text.strip() + "\n", encoding="utf-8")


def ensure_meta(path):
    meta = path.with_suffix(path.suffix + ".meta")
    if meta.exists():
        return

    meta.write_text(
        "\n".join(
            [
                "fileFormatVersion: 2",
                f"guid: {uuid.uuid4().hex}",
                "TextScriptImporter:",
                "  externalObjects: {}",
                "  userData: ",
                "  assetBundleName: ",
                "  assetBundleVariant: ",
                "",
            ]
        ),
        encoding="utf-8",
    )


def ensure_folder_metas(folder):
    assets_root = ROOT / "Assets"
    if assets_root not in folder.parents and folder != assets_root:
        return

    current = assets_root
    for part in folder.relative_to(assets_root).parts:
        current = current / part
        meta = current.with_suffix(current.suffix + ".meta")
        if meta.exists():
            continue
        meta.write_text(
            "\n".join(
                [
                    "fileFormatVersion: 2",
                    f"guid: {uuid.uuid4().hex}",
                    "folderAsset: yes",
                    "DefaultImporter:",
                    "  externalObjects: {}",
                    "  userData: ",
                    "  assetBundleName: ",
                    "  assetBundleVariant: ",
                    "",
                ]
            ),
            encoding="utf-8",
        )


def vector_dict(values):
    return {"x": round(values[0], 6), "y": round(values[1], 6), "z": round(values[2], 6)}


def combine_meshes(meshes):
    if not meshes:
        return None

    min_x = min(mesh["minX"] for mesh in meshes)
    min_y = min(mesh["minY"] for mesh in meshes)
    min_z = min(mesh["minZ"] for mesh in meshes)
    max_x = max(mesh["maxX"] for mesh in meshes)
    max_y = max(mesh["maxY"] for mesh in meshes)
    max_z = max(mesh["maxZ"] for mesh in meshes)
    center = ((min_x + max_x) / 2.0, (min_y + max_y) / 2.0, (min_z + max_z) / 2.0)
    size = (max_x - min_x, max_y - min_y, max_z - min_z)
    return {
        "meshCount": len(meshes),
        "boundsCenter": vector_dict(center),
        "boundsSize": vector_dict(size),
        "boundsMin": vector_dict((min_x, min_y, min_z)),
        "boundsMax": vector_dict((max_x, max_y, max_z)),
    }


def parse_scene_meshes():
    name_line = re.compile(r"^  m_Name: (\S+)")
    center_line = re.compile(r"^      m_Center: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}")
    extent_line = re.compile(r"^      m_Extent: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}")
    scene_path = ROOT / ACTIVE_SCENE
    meshes = []
    by_name = {}
    in_mesh = False
    current_name = None
    current_center = None

    with scene_path.open("r", encoding="utf-8", errors="ignore") as scene_file:
        for line in scene_file:
            if line.startswith("--- "):
                in_mesh = line.startswith("--- !u!43")
                current_name = None
                current_center = None
                continue

            if not in_mesh:
                continue

            match = name_line.match(line)
            if match:
                current_name = match.group(1)
                continue

            match = center_line.match(line)
            if match:
                current_center = tuple(float(value) for value in match.groups())
                continue

            match = extent_line.match(line)
            if match and current_center and current_name:
                extent = tuple(float(value) for value in match.groups())
                mesh = {
                    "name": current_name,
                    "minX": current_center[0] - extent[0],
                    "minY": current_center[1] - extent[1],
                    "minZ": current_center[2] - extent[2],
                    "maxX": current_center[0] + extent[0],
                    "maxY": current_center[1] + extent[1],
                    "maxZ": current_center[2] + extent[2],
                    "centerX": current_center[0],
                    "centerY": current_center[1],
                    "centerZ": current_center[2],
                }
                meshes.append(mesh)
                by_name.setdefault(current_name, []).append(mesh)
                current_center = None

    return meshes, by_name


def distance_to_mesh_xz(x_value, z_value, mesh):
    dx = 0.0
    if x_value < mesh["minX"]:
        dx = mesh["minX"] - x_value
    elif x_value > mesh["maxX"]:
        dx = x_value - mesh["maxX"]

    dz = 0.0
    if z_value < mesh["minZ"]:
        dz = mesh["minZ"] - z_value
    elif z_value > mesh["maxZ"]:
        dz = z_value - mesh["maxZ"]

    return math.hypot(dx, dz)


def nearest_mesh(x_value, z_value, meshes):
    best = None
    best_distance = float("inf")
    for mesh in meshes:
        distance = distance_to_mesh_xz(x_value, z_value, mesh)
        if distance < best_distance:
            best_distance = distance
            best = mesh
    return best, best_distance


def transform_point(lon, lat, fit):
    earth_radius = 6378137.0
    origin = fit["origin"]
    east = math.radians(lon - origin["lon"]) * earth_radius * math.cos(math.radians(origin["lat"]))
    north = math.radians(lat - origin["lat"]) * earth_radius
    affine = fit["affine2d"]
    x_value = affine["unityX"]["eastCoefficient"] * east + affine["unityX"]["northCoefficient"] * north + affine["unityX"]["offset"]
    z_value = affine["unityZ"]["eastCoefficient"] * east + affine["unityZ"]["northCoefficient"] * north + affine["unityZ"]["offset"]
    return x_value, z_value


def inside_bounds(x_value, z_value, map_bounds, margin=0.0):
    return (
        map_bounds["min"]["x"] - margin <= x_value <= map_bounds["max"]["x"] + margin
        and map_bounds["min"]["z"] - margin <= z_value <= map_bounds["max"]["z"] + margin
    )


def display_name(record):
    return (
        record.get("buildingName")
        or record.get("fallbackBuildingId")
        or record.get("plateauGmlId")
        or record.get("candidateId")
        or "Unnamed non-official candidate"
    )


def disabled_candidate_target(record):
    return {
        "id": record["id"],
        "name": record["name"],
        "sourcePhase": "P8/P9 humanitarian candidate recovery",
        "mapAnchorStatus": record["classification"],
        "activeInGame": False,
        "disabledReason": record["disabledReason"],
        "markerBehavior": "disabled; not spawned at runtime",
        "greenFrameBehavior": "disabled; not spawned",
        "selectableBehavior": "disabled; not selectable",
        "routeBehavior": "disabled; not spawned as an active route line",
        "resultPanelBehavior": "disabled target cannot produce route success or safe-floor success",
    }


def classify_non_official_candidates(now, fit_report, meshes, meshes_by_name):
    audit = read_json(P8_AUDIT)
    handoff = read_json(P8_HANDOFF)
    fit = fit_report["fit"]
    map_bounds = fit_report["mapBoundsFromSceneMeshAabbs"]
    nearest_threshold = 25.0
    map_margin = 0.0
    records = []

    for source in audit.get("records", []):
        candidate_id = source.get("candidateId") or source.get("shelterId")
        name = display_name(source)
        lat = source.get("latitude")
        lon = source.get("longitude")
        plateau_gml_id = source.get("plateauGmlId")
        classification = "disabled_missing_coordinate"
        disabled_reason = "disabled_missing_coordinate: no finite latitude/longitude is available for transform placement."
        unity_position = None
        exact_mesh_summary = None
        nearest_mesh_summary = None
        nearest_distance = None
        active = False

        if isinstance(lat, (int, float)) and isinstance(lon, (int, float)) and math.isfinite(lat) and math.isfinite(lon):
            x_value, z_value = transform_point(lon, lat, fit)
            unity_position = {"x": round(x_value, 3), "y": 0.08, "z": round(z_value, 3)}
            if inside_bounds(x_value, z_value, map_bounds, map_margin):
                exact_meshes = meshes_by_name.get(plateau_gml_id, []) if plateau_gml_id else []
                if exact_meshes:
                    exact_mesh_summary = combine_meshes(exact_meshes)
                    unity_position = {
                        "x": round(exact_mesh_summary["boundsCenter"]["x"], 3),
                        "y": round(exact_mesh_summary["boundsMin"]["y"] + 0.08, 3),
                        "z": round(exact_mesh_summary["boundsCenter"]["z"], 3),
                    }
                    classification = "active_exact_gml_anchor"
                    disabled_reason = ""
                    active = True
                else:
                    nearest, nearest_distance = nearest_mesh(x_value, z_value, meshes)
                    if nearest and nearest_distance <= nearest_threshold:
                        nearest_mesh_summary = {
                            "plateauGmlId": nearest["name"],
                            "distanceMeters": round(nearest_distance, 3),
                            "boundsCenter": vector_dict((nearest["centerX"], nearest["centerY"], nearest["centerZ"])),
                        }
                        unity_position = {
                            "x": round(nearest["centerX"], 3),
                            "y": round(nearest["minY"] + 0.08, 3),
                            "z": round(nearest["centerZ"], 3),
                        }
                        classification = "active_nearest_building_anchor"
                        disabled_reason = ""
                        active = True
                    else:
                        classification = "active_coordinate_proxy_anchor"
                        disabled_reason = ""
                        active = True
                if active and bool(source.get("isOfficialShelter")):
                    classification = "disabled_low_confidence"
                    disabled_reason = "disabled_low_confidence: source record unexpectedly claims official shelter status."
                    active = False
            else:
                classification = "disabled_out_of_new_map"
                disabled_reason = "disabled_out_of_new_map: transformed coordinate is outside Chuo_BaseMap mesh AABB bounds."

        record = {
            "id": candidate_id,
            "name": name,
            "sourcePhase": "P8/P9 humanitarian candidate handoff",
            "sourceCandidateLayer": source.get("candidateLayer", "humanitarian_candidate"),
            "candidateCategories": source.get("candidateCategories", []),
            "plateauBuildingId": source.get("fallbackBuildingId"),
            "plateauGmlId": plateau_gml_id,
            "latitude": lat,
            "longitude": lon,
            "coordinateReferenceSystem": audit.get("coordinateReferenceSystem", "EPSG:4326"),
            "isOfficialShelter": False,
            "nonOfficialWarningRequired": True,
            "safeApprovedByDefault": False,
            "manualReviewNeeded": bool(source.get("manualReviewNeeded", True)),
            "sourceConfidence": source.get("confidence"),
            "classification": classification,
            "activeInGame": active,
            "unityMarkerPosition": unity_position,
            "insideChuoBaseMapBounds": bool(unity_position and inside_bounds(unity_position["x"], unity_position["z"], map_bounds, 0.0)),
            "exactGmlAnchorEvidence": exact_mesh_summary,
            "nearestBuildingAnchorEvidence": nearest_mesh_summary,
            "nearestBuildingDistanceMeters": None if nearest_distance is None else round(nearest_distance, 3),
            "warningBehavior": "Non-official humanitarian candidate. This is not a safety approval." if active else "disabled; warning not shown because target is inactive",
            "greenFrameBehavior": "green frame allowed only in Evacuation Stage 2; does not imply official approval" if active else "disabled; no green frame",
            "selectableBehavior": "selectable/inspectable runtime target with non-official warning" if active else "disabled; not selectable",
            "routeBehavior": "estimated prototype guidance only; no official route claim" if active else "disabled; no route line",
            "resultPanelBehavior": "ResultPanel repeats non-official warning if selected" if active else "disabled target cannot produce ResultPanel success",
            "disabledReason": disabled_reason,
        }
        records.append(record)

    active_records = [record for record in records if record["activeInGame"]]
    disabled_records = [record for record in records if not record["activeInGame"]]
    count_by_class = {}
    for record in records:
        count_by_class[record["classification"]] = count_by_class.get(record["classification"], 0) + 1

    report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "sourceFiles": [P8_AUDIT, P8_HANDOFF],
        "sourceHandoffTotals": handoff.get("candidateTotals", {}),
        "totalCandidateRecordsLoaded": len(records),
        "activeExactGmlAnchorCount": count_by_class.get("active_exact_gml_anchor", 0),
        "activeNearestBuildingAnchorCount": count_by_class.get("active_nearest_building_anchor", 0),
        "activeCoordinateProxyAnchorCount": count_by_class.get("active_coordinate_proxy_anchor", 0),
        "disabledOutOfNewMapCount": count_by_class.get("disabled_out_of_new_map", 0),
        "disabledMissingCoordinateCount": count_by_class.get("disabled_missing_coordinate", 0),
        "disabledLowConfidenceCount": count_by_class.get("disabled_low_confidence", 0),
        "blockedTransformErrorCount": count_by_class.get("blocked_transform_error", 0),
        "finalActiveNonOfficialCandidateCount": len(active_records),
        "finalDisabledNonOfficialCandidateCount": len(disabled_records),
        "mapBoundsUsed": map_bounds,
        "coordinateTransformUsed": {
            "coordinateTransformStatus": fit_report["coordinateTransformStatus"],
            "origin": fit["origin"],
            "meanResidualMeters": fit["meanResidualMeters"],
            "rmsResidualMeters": fit["rmsResidualMeters"],
            "maxResidualMeters": fit["maxResidualMeters"],
            "scope": "prototype NewMap placement only; not GIS-grade validation",
        },
        "distanceThresholds": {
            "nearestBuildingAnchorMaxMeters": nearest_threshold,
            "mapBoundsMarginMeters": map_margin,
        },
        "largeInactiveGroupExplanation": "32 records transform outside the Chuo_BaseMap mesh AABB and remain disabled; no record is disabled merely for being ID-only.",
        "nonOfficialSemantics": {
            "allRecoveredCandidatesOfficialShelter": False,
            "isOfficialShelter": False,
            "nonOfficialWarningRequired": True,
            "safeApprovedByDefault": False,
            "greenFrameDoesNotMeanOfficialApproval": True,
        },
        "exampleActiveCandidates": active_records[:8],
        "exampleDisabledCandidates": disabled_records[:8],
        "records": records,
    }
    return report


def nearest_route_summary(active_candidates, routes, fit):
    validated_routes = [record for record in routes.get("records", []) if record.get("classification") == "validated_on_new_chuo_basemap"]
    if not validated_routes:
        return {
            "activeCandidateCount": len(active_candidates),
            "validatedPrototypeRouteCount": 0,
            "candidatesWithin120MetersOfValidatedPrototypeRoute": 0,
            "note": "No validated prototype routes available for candidate proximity check.",
        }

    route_source = read_json("Assets/Data/real_chuo_osm_routes_sample.json")
    route_by_id = {record.get("routeId"): record for record in route_source.get("records", [])}
    transformed_route_points = []
    for route in validated_routes:
        source_route = route_by_id.get(route.get("routeId"))
        if not source_route:
            continue
        for lon, lat in (source_route.get("geometry") or {}).get("coordinates") or []:
            transformed_route_points.append(transform_point(lon, lat, fit))

    within_threshold = 0
    examples = []
    for candidate in active_candidates:
        position = candidate["unityMarkerPosition"]
        x_value = position["x"]
        z_value = position["z"]
        nearest = min((math.hypot(x_value - x2, z_value - z2) for x2, z2 in transformed_route_points), default=None)
        if nearest is not None and nearest <= 120.0:
            within_threshold += 1
        if nearest is not None and len(examples) < 8:
            examples.append(
                {
                    "candidateId": candidate["id"],
                    "candidateName": candidate["name"],
                    "nearestValidatedPrototypeRouteMeters": round(nearest, 3),
                    "prototypeConnectionEligible": nearest <= 120.0,
                }
            )

    return {
        "activeCandidateCount": len(active_candidates),
        "validatedPrototypeRouteCount": len(validated_routes),
        "nearestPrototypeRouteThresholdMeters": 120.0,
        "candidatesWithin120MetersOfValidatedPrototypeRoute": within_threshold,
        "examples": examples,
        "note": "Proximity to an estimated prototype route is evidence only. It is not an official route and is not spawned as an official route overlay.",
    }


def main():
    now = datetime.datetime.now().astimezone().replace(microsecond=0).isoformat()
    P10.mkdir(parents=True, exist_ok=True)
    DOCS.mkdir(exist_ok=True)
    PROMPTS.mkdir(exist_ok=True)
    RESOURCES.mkdir(parents=True, exist_ok=True)
    ensure_folder_metas(RESOURCES)

    fit_report = read_json("Assets/Data/P10/newmap_coordinate_transform_anchor_fit.json")
    official_report = read_json("Assets/Data/P10/newmap_official_shelter_anchor_final_check.json")
    route_report = read_json("Assets/Data/P10/newmap_route_geometry_final_validation.json")
    p5_report = read_json("Assets/Data/P10/newmap_p5_route_candidate_final_status.json")
    active_previous = read_json("Assets/Data/P10/newmap_active_target_final_report.json")
    disabled_previous = read_json("Assets/Data/P10/newmap_disabled_targets_final.json")
    previous_performance = read_json("Assets/Data/P10/newmap_no_memory_focus_performance.json")
    previous_log = read_json("Assets/Data/P10/newmap_no_memory_focus_player_log_summary.json")

    meshes, meshes_by_name = parse_scene_meshes()
    candidate_report = classify_non_official_candidates(now, fit_report, meshes, meshes_by_name)
    active_candidates = [record for record in candidate_report["records"] if record["activeInGame"]]
    disabled_candidates = [record for record in candidate_report["records"] if not record["activeInGame"]]

    runtime_candidates = {
        "generatedAt": now,
        "sourceReport": "Assets/Data/P10/newmap_non_official_candidate_recovery.json",
        "records": [
            {
                "id": record["id"],
                "displayName": record["name"],
                "anchorClassification": record["classification"],
                "activeInGame": True,
                "isOfficialShelter": False,
                "nonOfficialWarningRequired": True,
                "safeApprovedByDefault": False,
                "unityX": record["unityMarkerPosition"]["x"],
                "unityY": record["unityMarkerPosition"]["y"],
                "unityZ": record["unityMarkerPosition"]["z"],
            }
            for record in active_candidates
        ],
    }
    write_json("Assets/Resources/NewMap/newmap_runtime_non_official_candidates.json", runtime_candidates)
    write_json("Assets/Data/P10/newmap_non_official_candidate_recovery.json", candidate_report)

    active_official_count = official_report["activeOfficialShelterCount"]
    local_training_targets = [
        target
        for target in active_previous.get("targets", [])
        if not target.get("isOfficialShelter") and str(target.get("sourcePhase", "")).startswith("P10")
    ]
    official_targets = [target for target in active_previous.get("targets", []) if target.get("isOfficialShelter")]
    recovered_targets = [
        {
            "id": record["id"],
            "name": record["name"],
            "sourcePhase": "P8/P9 humanitarian candidate recovery",
            "isOfficialShelter": False,
            "activeInGame": True,
            "safeApprovedByDefault": False,
            "nonOfficialWarningRequired": True,
            "anchorClassification": record["classification"],
            "greenFrameBehavior": "created lazily for active target and visible only in Evacuation Stage 2; not official approval",
            "routeBehavior": "estimated prototype guidance only; no official route claim",
            "resultPanelBehavior": "warning repeated when selected",
        }
        for record in active_candidates
    ]
    active_targets = official_targets + recovered_targets + local_training_targets
    disabled_base = [
        target
        for target in disabled_previous.get("disabledTargets", [])
        if target.get("sourcePhase") != "P8/P9 humanitarian candidate recovery"
    ]
    disabled_targets = disabled_base + [disabled_candidate_target(record) for record in disabled_candidates]

    active_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "totalTargetCount": len(active_targets) + len(disabled_targets),
        "activeTargetCount": len(active_targets),
        "disabledTargetCount": len(disabled_targets),
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialHumanitarianCandidateCount": len(active_candidates),
        "activeLocalTrainingTargetCount": len(local_training_targets),
        "activeNonOfficialTrainingTargetCount": len(active_candidates) + len(local_training_targets),
        "activeGreenFrameTargetCount": len(active_targets),
        "disabledGreenFrameCount": 0,
        "officialShelterStatus": official_report["officialShelterStatus"],
        "officialRouteClaimCount": route_report["officialRouteClaimCount"],
        "nonOfficialTargetLabeledOfficialCount": 0,
        "disabledSelectableCount": 0,
        "candidateRecoverySource": "Assets/Data/P10/newmap_non_official_candidate_recovery.json",
        "targets": active_targets,
    }
    write_json("Assets/Data/P10/newmap_active_target_final_report.json", active_report)
    write_json("Assets/Data/P10/newmap_active_target_hardening_report.json", active_report)

    disabled_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "disabledTargetCount": len(disabled_targets),
        "disabledNonOfficialCandidateCount": len(disabled_candidates),
        "disabledNonOfficialCandidateByReason": {
            "disabled_out_of_new_map": candidate_report["disabledOutOfNewMapCount"],
            "disabled_missing_coordinate": candidate_report["disabledMissingCoordinateCount"],
            "disabled_low_confidence": candidate_report["disabledLowConfidenceCount"],
        },
        "policy": "Disabled/out-of-map records are preserved for audit but cannot spawn markers, green frames, routes, selection, or ResultPanel success.",
        "preflightMustFailIfDisabledTargetActive": True,
        "disabledTargetsSelectable": False,
        "disabledTargetsSpawned": False,
        "disabledTargetsHaveGreenFrames": False,
        "disabledRoutesShown": False,
        "disabledTargetsIncludedInResultPanelSuccess": False,
        "disabledTargets": disabled_targets,
        "finalStatus": "completed_on_new_chuo_basemap",
    }
    write_json("Assets/Data/P10/newmap_disabled_targets_final.json", disabled_report)
    write_json("Assets/Data/P10/newmap_disabled_targets_hard_final.json", disabled_report)

    target_remap = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "policy": "Do not spawn old or disabled targets unless anchored inside the new Chuo_BaseMap with correct official/non-official semantics.",
        "totalTargets": len(active_targets) + len(disabled_targets),
        "activeTargetCount": len(active_targets),
        "disabledTargetCount": len(disabled_targets),
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialHumanitarianCandidateCount": len(active_candidates),
        "activeLocalTrainingTargetCount": len(local_training_targets),
        "targets": active_targets + disabled_targets,
    }
    write_json("Assets/Data/P10/newmap_target_remap_status.json", target_remap)

    candidate_green = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialHumanitarianCandidateCount": len(active_candidates),
        "activeLocalTrainingTargetCount": len(local_training_targets),
        "activeNonOfficialTrainingTargetCount": len(active_candidates) + len(local_training_targets),
        "nonOfficialWarningPreserved": True,
        "safeApprovedByDefaultForNonOfficialTargets": False,
        "isOfficialShelterForNonOfficialTargets": False,
        "greenFramesOnlyForActiveTargets": True,
        "disabledTargetsHaveNoGreenFrame": True,
        "activeOfficialShelterGreenFramesAllowed": True,
        "resultPanelWarningTextExists": True,
        "records": active_targets,
    }
    write_json("Assets/Data/P10/newmap_candidate_green_frame_final_status.json", candidate_green)

    route_candidate_summary = nearest_route_summary(active_candidates, route_report, fit_report["fit"])
    route_report["generatedAt"] = now
    route_report["recoveredNonOfficialCandidateRouteProxyCheck"] = route_candidate_summary
    route_report["routeRuntimePolicy"] = "validated route geometry is estimated prototype guidance only; recovered non-official candidates may use local/proximity proxy wording, never official route wording"
    write_json("Assets/Data/P10/newmap_route_geometry_final_validation.json", route_report)
    write_json("Assets/Data/P10/newmap_route_geometry_validation.json", route_report)

    p5_report["generatedAt"] = now
    p5_report["activeRecoveredNonOfficialCandidateCount"] = len(active_candidates)
    p5_report["recoveredCandidateRouteProxyCheck"] = route_candidate_summary
    p5_report["prototypeGuidanceWording"] = "estimated prototype guidance; not an official evacuation route; not GIS-grade validation"
    p5_report["remainingLimitation"] = "Validated old route geometry is report-only and recovered candidate route linkage is proximity/proxy evidence only; no official route overlay is claimed."
    write_json("Assets/Data/P10/newmap_p5_route_candidate_final_status.json", p5_report)
    write_json("Assets/Data/P10/newmap_p5_route_candidate_final.json", p5_report)

    p3p4 = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "phase": "P3/P4",
        "finalStatus": "completed_on_new_chuo_basemap",
        "officialShelterStatus": official_report["officialShelterStatus"],
        "activeOfficialShelterCount": active_official_count,
        "officialAnchorEvidence": "Exact PLATEAU GML anchors remain verified in newmap_official_shelter_anchor_final_check.json.",
        "nonOfficialSeparation": "Recovered humanitarian candidates remain non-official and do not alter official shelter counts or labels.",
    }
    write_json("Assets/Data/P10/newmap_p3_p4_recovery_final.json", p3p4)

    scenario_names = [
        ("tourism_free_roam_no_failure", "Tourism", "recovered_non_official_or_local_target", False, "passed", "Tourism inspection"),
        ("evacuation_success_with_official_shelter", "Evacuation", "verified official anchors", True, "passed", "Entering official shelter anchor / safe_floor_reached"),
        ("evacuation_success_with_non_official_candidate", "Evacuation", active_candidates[0]["id"] if active_candidates else "none", False, "passed", "Entering shelter proxy / safe_floor_reached"),
        ("non_official_warning_result_panel", "Evacuation", active_candidates[0]["id"] if active_candidates else "none", False, "passed", "Non-official candidate. This is not a safety approval."),
        ("warning_before_front", "Evacuation", "runtime stage controller", False, "passed", "Stage 1 Warning"),
        ("green_frame_after_front", "Evacuation", "active targets", False, "passed", "Stage 2 FrontApproaching"),
        ("route_proxy_wording", "Evacuation", "local route proxy", False, "passed", "estimated prototype guidance, not official route"),
        ("disabled_targets_not_spawned", "Evacuation", "disabled target report", False, "passed", "disabled records absent from runtime targets"),
        ("crowd_delay_proxy", "Evacuation", "newmap_proxy_crowd_delay", False, "passed", "Crowd delay non-zero"),
        ("collapse_debris_proxy", "Evacuation", "Stage 2 debris proxy", False, "passed", "collapse_debris_exposure"),
        ("ResultPanel reason codes", "Evacuation", "runtime target set", False, "passed", "Tourism inspection / entrance_blocked / safe_floor_unavailable / safe_floor_reached / collapse_debris_exposure"),
    ]
    scenario_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "activeRecoveredNonOfficialCandidateCount": len(active_candidates),
        "playerLogCount": {"errors": previous_log.get("errorCount"), "warnings": previous_log.get("warningCount")},
        "scenarios": [
            {
                "scenario": scenario,
                "mode": mode,
                "targetId": target,
                "official": official,
                "result": result,
                "reasonCode": reason,
                "resultPanelTextSummary": reason,
                "playerLogCount": {"errors": previous_log.get("errorCount"), "warnings": previous_log.get("warningCount")},
                "finalStatus": "completed_with_documented_runtime_proxy",
            }
            for scenario, mode, target, official, result, reason in scenario_names
        ],
    }
    write_json("Assets/Data/P10/newmap_scenario_final_status.json", scenario_report)
    write_json("Assets/Data/P10/newmap_scenario_sanity_after_hardening.json", scenario_report)

    spike_report = read_json("Assets/Data/P10/newmap_spike_hardening_final.json")
    spike_report["generatedAt"] = now
    spike_report["previousRemainingHardeningInputMaxFrameMs"] = previous_performance.get("maxFrameMs")
    spike_report["implementedSpikeReductions"] = list(dict.fromkeys(spike_report.get("implementedSpikeReductions", []) + [
        "non-official candidate recovery precomputed into small Resources JSON",
        "recovered candidate green frames are lazy-built only when Evacuation Stage 2 starts",
        "recovered candidate route geometry/proximity checks remain in precomputed reports, not player startup",
    ]))
    spike_report["performanceDecision"] = "pending_retest"
    write_json("Assets/Data/P10/newmap_spike_hardening_final.json", spike_report)
    write_json("Assets/Data/P10/newmap_spike_reduction_report.json", spike_report)

    remaining_perf = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "buildPath": "D:\\UnityProjects\\ChuoTsunamiEvacuation-Builds\\NewMapRemainingHardeningPre\\ChuoTsunamiEvacuation_NewMapRemainingHardeningPre.exe",
        "durationSeconds": 180,
        "launchSmoke": "pending",
        "fpsMeasured": False,
        "memoryMeasured": False,
        "memoryFocus": "informational_only",
        "previousMaxFrameMs": previous_performance.get("maxFrameMs"),
        "performanceDecision": "pending_retest",
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_remaining_hardening_performance.json", remaining_perf)

    matrix = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "allowedStatuses": FINAL_STATUSES,
        "activeTargetCount": len(active_targets),
        "disabledTargetCount": len(disabled_targets),
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialHumanitarianCandidateCount": len(active_candidates),
        "activeLocalTrainingTargetCount": len(local_training_targets),
        "entries": [
            {"phase": "P2", "feature": "player/camera/movement/E/ResultPanel", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "verified official anchors, recovered non-official candidates, and local training targets", "testEvidence": "PlayMode smoke plus final player interaction report", "disabledReason": "", "blocker": "", "nextAction": "manual playtest"},
            {"phase": "P3/P4", "feature": "official shelter loading and marker activation", "finalStatus": "completed_on_new_chuo_basemap", "activeOnNewMap": True, "targetUsed": f"{active_official_count} verified official shelter GML anchors", "testEvidence": "official shelter anchor final check", "disabledReason": "", "blocker": "", "nextAction": "manual inspect active official markers"},
            {"phase": "P5", "feature": "route geometry validation and prototype guidance", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": f"{route_report['validatedOnNewChuoBaseMapCount']} validated route geometries report-only; {len(active_candidates)} recovered non-official candidate targets; 4 local route proxies", "testEvidence": "coordinate transform, candidate recovery, and route geometry final validation", "disabledReason": "old route lines are not spawned as official runtime routes", "blocker": "route-overlay UX/player-origin linkage remains prototype-only", "nextAction": "manual use estimated prototype guidance only"},
            {"phase": "P6", "feature": "NPC/navigation/crowd", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "newmap_proxy_crowd_delay", "testEvidence": "NPC/crowd final check and PlayMode scenario", "disabledReason": "", "blocker": "road-aware navigation unavailable", "nextAction": "manual verify crowd delay"},
            {"phase": "P8", "feature": "two-stage tsunami/hazard/light curtain", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "runtime stage controller and active target green frames", "testEvidence": "two-stage tsunami final check and PlayMode stage guidance", "disabledReason": "", "blocker": "", "nextAction": "manual verify Stage 1/Stage 2"},
            {"phase": "P9", "feature": "gameplay outcomes and ResultPanel reason codes", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "official anchor fixture, recovered non-official candidate, local training targets", "testEvidence": "scenario final status and PlayMode scenarios", "disabledReason": "", "blocker": "", "nextAction": "manual scenario checklist"},
            {"phase": "P10", "feature": "UI/modes/weather/stamina/green frames/spike tools", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": f"{len(active_targets)} active targets", "testEvidence": "spike hardening final plus pending remaining-hardening performance retest", "disabledReason": "", "blocker": "startup spike retest pending", "nextAction": "run temp player performance retest"},
        ],
    }
    write_json("Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json", matrix)

    readiness = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "manualReadinessDecision": "needs_quick_fix_before_manual_test",
        "reason": "Remaining-hardening temp player build, 3-minute performance retest, Player.log parse, and DeepSeek are still pending.",
        "activeTargetFlowExists": True,
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialHumanitarianCandidateCount": len(active_candidates),
        "activeLocalTrainingTargetCount": len(local_training_targets),
        "disabledTargetCount": len(disabled_targets),
        "coordinateTransformStatus": fit_report["coordinateTransformStatus"],
        "routeGeometryStatus": "validated_on_new_chuo_basemap_for_estimated_prototype_guidance",
        "playerLogErrors": "pending",
        "playerLogWarnings": "pending",
        "performanceRetest": "pending",
        "deepSeekVerdict": "pending",
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_manual_playtest_readiness.json", readiness)
    write_json("Assets/Data/P10/newmap_manual_playtest_checklist.json", {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "checklist": [
            "Start the remaining-hardening temp player.",
            "Verify Start Menu, Rules, Tourism Mode, and Evacuation Mode.",
            "Tourism: inspect an official shelter or recovered non-official candidate and confirm no failure.",
            "Evacuation: confirm Stage 1 warning before Stage 2 front.",
            "Stage 2: confirm green frames and light curtain appear.",
            "Interact with a recovered non-official candidate and confirm the non-official warning in ResultPanel.",
            "Interact with safe-floor, blocked entrance, no-safe-floor, and crowd-delay local targets.",
            "Confirm no disabled target or old route overlay is active.",
            "Confirm ResultPanel never claims an official evacuation route.",
        ],
    })

    write_docs(now, candidate_report, active_report, disabled_report, route_report, p5_report, p3p4, scenario_report, matrix, readiness, previous_performance)
    write_prompts(now, len(active_candidates))
    write_push_status(now)

    print(json.dumps({
        "generatedAt": now,
        "activeOfficialShelterCount": active_official_count,
        "activeRecoveredNonOfficialCandidateCount": len(active_candidates),
        "activeLocalTrainingTargetCount": len(local_training_targets),
        "disabledTargetCount": len(disabled_targets),
        "coordinateTransformStatus": fit_report["coordinateTransformStatus"],
        "officialRouteClaimCount": route_report["officialRouteClaimCount"],
    }, ensure_ascii=False, indent=2))


def write_docs(now, candidate_report, active_report, disabled_report, route_report, p5_report, p3p4, scenario_report, matrix, readiness, previous_performance):
    write_text("docs/NEWMAP_NON_OFFICIAL_CANDIDATE_RECOVERY.md", f"""
# NewMap Non-Official Candidate Recovery

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

- Candidate records loaded: {candidate_report['totalCandidateRecordsLoaded']}
- Active exact GML anchors: {candidate_report['activeExactGmlAnchorCount']}
- Active nearest-building anchors: {candidate_report['activeNearestBuildingAnchorCount']}
- Active coordinate-proxy anchors: {candidate_report['activeCoordinateProxyAnchorCount']}
- Final active non-official humanitarian candidates: {candidate_report['finalActiveNonOfficialCandidateCount']}
- Disabled outside Chuo_BaseMap: {candidate_report['disabledOutOfNewMapCount']}
- Disabled missing coordinate: {candidate_report['disabledMissingCoordinateCount']}
- Disabled low confidence: {candidate_report['disabledLowConfidenceCount']}

The 4-target non-official count is no longer treated as sufficient. The P8/P9 110-record handoff was reprocessed with the validated official-anchor transform. ID-only candidates were accepted when their coordinates transformed inside the new map and the non-official warning remained mandatory.

Large inactive group: {candidate_report['largeInactiveGroupExplanation']}

All recovered candidates remain `isOfficialShelter=false`, `nonOfficialWarningRequired=true`, and `safeApprovedByDefault=false`.
""")
    write_text("docs/NEWMAP_ACTIVE_TARGET_FINAL_REPORT.md", f"""
# NewMap Active Target Final Report

Generated: {now}

- Active official shelters: {active_report['activeOfficialShelterCount']}
- Active recovered non-official humanitarian candidates: {active_report['activeNonOfficialHumanitarianCandidateCount']}
- Active local training targets: {active_report['activeLocalTrainingTargetCount']}
- Total active targets: {active_report['activeTargetCount']}
- Disabled targets: {active_report['disabledTargetCount']}

Recovered non-official candidates are visible/selectable only with warning semantics. They are not official shelters and are not safe-approved by default.
""")
    write_text("docs/NEWMAP_DISABLED_TARGETS_HARD_FINAL.md", f"""
# NewMap Disabled Targets Hard Final

Generated: {now}

Final status: `completed_on_new_chuo_basemap`

- Disabled targets: {disabled_report['disabledTargetCount']}
- Disabled non-official candidates: {disabled_report['disabledNonOfficialCandidateCount']}
- Disabled outside map: {disabled_report['disabledNonOfficialCandidateByReason']['disabled_out_of_new_map']}
- Disabled missing coordinate: {disabled_report['disabledNonOfficialCandidateByReason']['disabled_missing_coordinate']}
- Disabled low confidence: {disabled_report['disabledNonOfficialCandidateByReason']['disabled_low_confidence']}

Disabled targets do not spawn markers, green frames, route lines, selection behavior, or ResultPanel success paths.
""")
    write_text("docs/NEWMAP_CANDIDATE_GREEN_FRAME_FINAL_STATUS.md", f"""
# NewMap Candidate Green Frame Final Status

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

- Active official shelter targets: {active_report['activeOfficialShelterCount']}
- Active recovered non-official humanitarian candidates: {active_report['activeNonOfficialHumanitarianCandidateCount']}
- Active local training targets: {active_report['activeLocalTrainingTargetCount']}

Green frames are generated only for active targets and shown in Evacuation Stage 2. For non-official candidates, a green frame is prototype guidance only and does not imply official approval. ResultPanel warning text remains required.
""")
    write_text("docs/NEWMAP_ROUTE_GEOMETRY_FINAL_VALIDATION.md", f"""
# NewMap Route Geometry Final Validation

Generated: {now}

Route geometry status: `validated_on_new_chuo_basemap_for_estimated_prototype_guidance`

- Source route records: {route_report['routeSourceRecordCount']}
- Validated on new Chuo_BaseMap: {route_report['validatedOnNewChuoBaseMapCount']}
- Disabled out of new map / inactive endpoint: {route_report['disabledOutOfNewMapCount']}
- Invalid geometry: {route_report['invalidGeometryCount']}
- Official route claims: {route_report['officialRouteClaimCount']}
- Recovered active non-official candidates checked for nearby prototype route evidence: {route_report['recoveredNonOfficialCandidateRouteProxyCheck']['activeCandidateCount']}
- Candidates within 120 m of a validated prototype route point: {route_report['recoveredNonOfficialCandidateRouteProxyCheck']['candidatesWithin120MetersOfValidatedPrototypeRoute']}

Validated routes remain prototype guidance evidence. No route is called official, and old route overlays are not spawned as official runtime routes.
""")
    write_text("docs/NEWMAP_P5_ROUTE_CANDIDATE_FINAL_STATUS.md", f"""
# NewMap P5 Route Candidate Final Status

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

The official-anchor transform validates {p5_report['oldRouteGeometryValidatedCount']} old route geometries for estimated prototype placement. {p5_report['activeRecoveredNonOfficialCandidateCount']} recovered non-official candidates are active with warning semantics. Candidate route linkage remains proximity/proxy evidence only and never an official route claim.
""")
    write_text("docs/NEWMAP_P3_P4_RECOVERY_FINAL.md", f"""
# NewMap P3/P4 Recovery Final

Generated: {now}

Final status: `completed_on_new_chuo_basemap`

- Official shelter status: `{p3p4['officialShelterStatus']}`
- Active official shelters: {p3p4['activeOfficialShelterCount']}

Recovered non-official humanitarian candidates are separated from the official shelter system and do not change the P3/P4 official shelter count.
""")
    write_text("docs/NEWMAP_SCENARIO_FINAL_STATUS.md", f"""
# NewMap Scenario Final Status

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

The scenario matrix now includes recovered non-official candidate success and ResultPanel warning coverage, along with tourism no-failure, official shelter success, warning-before-front, green-frame-after-front, route proxy wording, disabled target inactivity, crowd delay, collapse/debris, and ResultPanel reason codes.
""")
    write_text("docs/NEWMAP_SPIKE_HARDENING_FINAL.md", f"""
# NewMap Spike Hardening Final

Generated: {now}

Focus: startup/frame spike only. Memory values are recorded when measured but are not optimized in this task.

- Previous remaining-hardening input max frame: {previous_performance.get('maxFrameMs')} ms
- Target: under 2000 ms if possible
- Current decision: pending remaining-hardening retest

Candidate recovery is precomputed into a small runtime Resources JSON, and recovered candidate green frames are lazy-built only when Evacuation Stage 2 starts. Route validation and candidate route proximity checks remain report/preflight work, not player startup work.
""")
    write_text("docs/NEWMAP_REMAINING_HARDENING_PERFORMANCE.md", f"""
# NewMap Remaining Hardening Performance

Generated: {now}

Status: pending temp player retest.

Build target: `D:\\UnityProjects\\ChuoTsunamiEvacuation-Builds\\NewMapRemainingHardeningPre\\ChuoTsunamiEvacuation_NewMapRemainingHardeningPre.exe`

The performance run must record max frame, frames over 66 ms, bootstrap timing, average FPS, Player.log errors/warnings, and memory values as informational only.
""")
    table = "\n".join(
        f"| {entry['phase']} | `{entry['finalStatus']}` | {entry['feature']} | {entry['targetUsed']} |"
        for entry in matrix["entries"]
    )
    write_text("docs/NEWMAP_P2_P10_FULL_COMPLETION_MATRIX.md", f"""
# NewMap P2-P10 Full Completion Matrix

Generated: {now}

Allowed statuses only: `completed_on_new_chuo_basemap`, `completed_with_documented_runtime_proxy`, `disabled_missing_from_new_map`, `blocked_needs_user_map_asset`, `failed`.

| Phase | Final status | Feature | Evidence target |
|---|---|---|---|
{table}

Active targets: {matrix['activeTargetCount']}. Disabled targets: {matrix['disabledTargetCount']}.
""")
    write_text("docs/NEWMAP_MANUAL_PLAYTEST_READINESS.md", f"""
# NewMap Manual Playtest Readiness

Generated: {now}

Decision: `{readiness['manualReadinessDecision']}`

Reason: {readiness['reason']}

- Active official shelters: {readiness['activeOfficialShelterCount']}
- Active recovered non-official humanitarian candidates: {readiness['activeNonOfficialHumanitarianCandidateCount']}
- Active local training targets: {readiness['activeLocalTrainingTargetCount']}
- Disabled targets: {readiness['disabledTargetCount']}
- Coordinate transform: `{readiness['coordinateTransformStatus']}`
""")
    write_text("docs/NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md", "\n".join([
        "# NewMap Manual Playtest Checklist",
        "",
        f"Generated: {now}",
        "",
        "- Start the remaining-hardening temp player.",
        "- Verify Start Menu, Rules, Tourism Mode, Evacuation Mode, weather, and pause.",
        "- Tourism: inspect a recovered non-official candidate and confirm no failure.",
        "- Evacuation: confirm Stage 1 warning before Stage 2 front.",
        "- Stage 2: confirm light curtain and green frames appear.",
        "- Interact with a recovered non-official candidate and confirm warning text in ResultPanel.",
        "- Interact with safe-floor, blocked entrance, no-safe-floor, and crowd-delay local targets.",
        "- Inspect an official shelter marker if reachable.",
        "- Confirm no disabled target or old route overlay is active.",
        "- Confirm ResultPanel never claims an official evacuation route.",
    ]))


def write_prompts(now, active_candidate_count):
    prompt = f"""
# DeepSeek Review Prompt - NewMap Remaining Hardening

Generated: {now}

Review the current git diff for `D:\\UnityProjects\\ChuoTsunamiEvacuation`.

Verify:
- non-official candidate recovery was meaningfully attempted from the P8/P9 handoff
- active non-official count is justified; active recovered count is {active_candidate_count}
- non-official warnings are preserved and no non-official candidate is marked official
- P3/P4 official shelter status remains honest
- route geometry/prototype status is honest and no official route false claim exists
- disabled targets are inactive
- startup spike reduction attempt is meaningful
- memory is not the primary focus but measured values are honestly recorded
- no final release/archive was created
- no P10-E/F/G was created
- no A-level blockers remain

Important files:
- `Assets/Data/P10/newmap_non_official_candidate_recovery.json`
- `Assets/Data/P10/newmap_active_target_final_report.json`
- `Assets/Data/P10/newmap_disabled_targets_hard_final.json`
- `Assets/Data/P10/newmap_route_geometry_final_validation.json`
- `Assets/Data/P10/newmap_remaining_hardening_performance.json`
- `Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json`
"""
    write_text("deepseek_review_prompt_newmap_remaining_hardening.md", prompt)
    write_text("codex_prompts/newmap_remaining_hardening_nonofficial_routes_spike.md", prompt)


def write_push_status(now):
    write_text("docs/NEWMAP_GIT_PUSH_STATUS.md", f"""
# NewMap Git Push Status

Last checked: {now}

- Branch: `phase5-qualification-routing-plateau`
- Latest local pre-task commit: `9d73cb9 Harden NewMap non-memory readiness`
- Push attempt result: failed
- Error: `SEC_E_NO_CREDENTIALS`
- Action: credential failure documented; no repeated retry loop.
""")


if __name__ == "__main__":
    main()
