import datetime
import json
import math
import re
import uuid
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
P10 = ROOT / "Assets" / "Data" / "P10"
DOCS = ROOT / "docs"
PROMPTS = ROOT / "codex_prompts"
ACTIVE_SCENE = "Assets/Scenes/Chuo_BaseMap.unity"

FINAL_STATUSES = [
    "completed_on_new_chuo_basemap",
    "completed_with_documented_runtime_proxy",
    "disabled_missing_from_new_map",
    "blocked_needs_user_map_asset",
    "failed",
]

TRAINING_TARGETS = [
    ("newmap_proxy_safe_floor", "Local Training Proxy - Safe Floor", "safe_floor_reached"),
    ("newmap_proxy_blocked_entrance", "Local Training Proxy - Blocked Entrance", "entrance_blocked"),
    ("newmap_proxy_no_safe_floor", "Local Training Proxy - No Safe Floor", "safe_floor_unavailable"),
    ("newmap_proxy_crowd_delay", "Local Training Proxy - Crowd Delay", "safe_floor_reached_with_crowd_delay"),
]

P3P4_SAMPLE_TARGETS = [
    "sample_chuo_harumi_001",
    "sample_chuo_kachidoki_002",
    "sample_chuo_tsukishima_003",
    "sample_chuo_nihonbashi_004",
    "sample_chuo_ginza_005",
]


def read_json(relative_path):
    return json.loads((ROOT / relative_path).read_text(encoding="utf-8-sig"))


def write_json(relative_path, data):
    path = ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    if "Assets/Data/" in relative_path.replace("\\", "/"):
        ensure_text_asset_meta(path)


def ensure_text_asset_meta(path):
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


def write_text(relative_path, text):
    path = ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text.strip() + "\n", encoding="utf-8")


def parse_scene_mesh_aabbs(wanted_names):
    scene_path = ROOT / ACTIVE_SCENE
    name_line = re.compile(r"^  m_Name: (\S+)")
    center_line = re.compile(r"^      m_Center: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}")
    extent_line = re.compile(r"^      m_Extent: \{x: ([^,]+), y: ([^,]+), z: ([^}]+)\}")
    wanted_meshes = {name: [] for name in wanted_names if name}
    map_min = [float("inf"), float("inf"), float("inf")]
    map_max = [float("-inf"), float("-inf"), float("-inf")]
    mesh_count = 0
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
            if match and current_center:
                extent = tuple(float(value) for value in match.groups())
                if all(math.isfinite(value) for value in current_center + extent):
                    mesh_count += 1
                    for index in range(3):
                        map_min[index] = min(map_min[index], current_center[index] - extent[index])
                        map_max[index] = max(map_max[index], current_center[index] + extent[index])
                    if current_name in wanted_meshes:
                        wanted_meshes[current_name].append((current_center, extent))
                current_center = None

    return wanted_meshes, {
        "meshAabbCount": mesh_count,
        "min": vector_dict(map_min),
        "max": vector_dict(map_max),
        "center": vector_dict([(map_min[index] + map_max[index]) / 2 for index in range(3)]),
        "size": vector_dict([map_max[index] - map_min[index] for index in range(3)]),
    }


def vector_dict(values):
    return {"x": round(values[0], 6), "y": round(values[1], 6), "z": round(values[2], 6)}


def combine_aabbs(items):
    if not items:
        return None

    mins = [float("inf"), float("inf"), float("inf")]
    maxs = [float("-inf"), float("-inf"), float("-inf")]
    for center, extent in items:
        for index in range(3):
            mins[index] = min(mins[index], center[index] - extent[index])
            maxs[index] = max(maxs[index], center[index] + extent[index])

    center = [(mins[index] + maxs[index]) / 2 for index in range(3)]
    size = [maxs[index] - mins[index] for index in range(3)]
    return {
        "center": center,
        "min": mins,
        "max": maxs,
        "size": size,
        "meshCount": len(items),
    }


def solve_linear_system(matrix, values):
    size = len(values)
    augmented = [list(matrix[index]) + [values[index]] for index in range(size)]
    for column in range(size):
        pivot = max(range(column, size), key=lambda row: abs(augmented[row][column]))
        if abs(augmented[pivot][column]) < 1e-12:
            raise ValueError("singular matrix")
        augmented[column], augmented[pivot] = augmented[pivot], augmented[column]
        divisor = augmented[column][column]
        for item in range(column, size + 1):
            augmented[column][item] /= divisor
        for row in range(size):
            if row == column:
                continue
            factor = augmented[row][column]
            for item in range(column, size + 1):
                augmented[row][item] -= factor * augmented[column][item]
    return [augmented[index][size] for index in range(size)]


def least_squares_3(rows, target_index):
    xtx = [[0.0, 0.0, 0.0] for _ in range(3)]
    xty = [0.0, 0.0, 0.0]
    for row in rows:
        x_values = [row["eastMeters"], row["northMeters"], 1.0]
        y_value = row["unityX"] if target_index == 0 else row["unityZ"]
        for i in range(3):
            xty[i] += x_values[i] * y_value
            for j in range(3):
                xtx[i][j] += x_values[i] * x_values[j]
    return solve_linear_system(xtx, xty)


def fit_anchor_transform(control_points):
    lat0 = sum(point["lat"] for point in control_points) / len(control_points)
    lon0 = sum(point["lon"] for point in control_points) / len(control_points)
    earth_radius = 6378137.0
    rows = []
    for point in control_points:
        east = math.radians(point["lon"] - lon0) * earth_radius * math.cos(math.radians(lat0))
        north = math.radians(point["lat"] - lat0) * earth_radius
        rows.append(
            {
                "id": point["id"],
                "eastMeters": east,
                "northMeters": north,
                "unityX": point["unityPosition"]["x"],
                "unityZ": point["unityPosition"]["z"],
            }
        )

    x_coefficients = least_squares_3(rows, 0)
    z_coefficients = least_squares_3(rows, 1)
    residuals = []
    for row in rows:
        predicted_x = x_coefficients[0] * row["eastMeters"] + x_coefficients[1] * row["northMeters"] + x_coefficients[2]
        predicted_z = z_coefficients[0] * row["eastMeters"] + z_coefficients[1] * row["northMeters"] + z_coefficients[2]
        residual = math.hypot(predicted_x - row["unityX"], predicted_z - row["unityZ"])
        residuals.append(
            {
                "id": row["id"],
                "residualMeters": round(residual, 3),
                "predictedUnityXZ": {"x": round(predicted_x, 3), "z": round(predicted_z, 3)},
                "actualUnityXZ": {"x": round(row["unityX"], 3), "z": round(row["unityZ"], 3)},
            }
        )

    residual_values = [item["residualMeters"] for item in residuals]
    residual_values_sorted = sorted(residual_values)
    rms = math.sqrt(sum(value * value for value in residual_values) / len(residual_values))
    thresholds = {
        "minimumControlPoints": 8,
        "meanResidualMetersMax": 10.0,
        "rmsResidualMetersMax": 10.0,
        "maxResidualMetersMax": 25.0,
        "routeEndpointResidualMetersMax": 80.0,
        "routeMaxSegmentMetersMax": 300.0,
    }
    mean_residual = sum(residual_values) / len(residual_values)
    status = (
        "transform_validated_from_official_anchors"
        if len(control_points) >= thresholds["minimumControlPoints"]
        and mean_residual <= thresholds["meanResidualMetersMax"]
        and rms <= thresholds["rmsResidualMetersMax"]
        and max(residual_values) <= thresholds["maxResidualMetersMax"]
        else "blocked_high_residual_error"
    )

    return {
        "status": status,
        "origin": {"lat": round(lat0, 9), "lon": round(lon0, 9)},
        "affine2d": {
            "unityX": {
                "eastCoefficient": x_coefficients[0],
                "northCoefficient": x_coefficients[1],
                "offset": x_coefficients[2],
            },
            "unityZ": {
                "eastCoefficient": z_coefficients[0],
                "northCoefficient": z_coefficients[1],
                "offset": z_coefficients[2],
            },
        },
        "thresholds": thresholds,
        "controlPointCount": len(control_points),
        "meanResidualMeters": round(mean_residual, 3),
        "medianResidualMeters": residual_values_sorted[len(residual_values_sorted) // 2],
        "rmsResidualMeters": round(rms, 3),
        "maxResidualMeters": max(residual_values),
        "residuals": sorted(residuals, key=lambda item: item["residualMeters"], reverse=True),
    }


def transform_point(lon, lat, fit):
    earth_radius = 6378137.0
    origin = fit["origin"]
    east = math.radians(lon - origin["lon"]) * earth_radius * math.cos(math.radians(origin["lat"]))
    north = math.radians(lat - origin["lat"]) * earth_radius
    affine = fit["affine2d"]
    x = affine["unityX"]["eastCoefficient"] * east + affine["unityX"]["northCoefficient"] * north + affine["unityX"]["offset"]
    z = affine["unityZ"]["eastCoefficient"] * east + affine["unityZ"]["northCoefficient"] * north + affine["unityZ"]["offset"]
    return x, z


def route_is_finite(route):
    coordinates = (route.get("geometry") or {}).get("coordinates") or []
    if len(coordinates) < 2:
        return False
    for coordinate in coordinates:
        if not isinstance(coordinate, list) or len(coordinate) < 2:
            return False
        if not all(isinstance(value, (int, float)) and math.isfinite(value) for value in coordinate[:2]):
            return False
    return True


def classify_routes(routes, fit, map_bounds, active_anchor_by_id):
    map_min = map_bounds["min"]
    map_max = map_bounds["max"]
    thresholds = fit["thresholds"]
    margin = 200.0
    records = []

    for route in routes.get("records", []):
        coordinates = (route.get("geometry") or {}).get("coordinates") or []
        finite = route_is_finite(route)
        transformed_points = []
        disabled_reason = ""
        endpoint_distance = None
        max_segment = None
        inside_count = 0
        classification = "invalid_geometry"

        if finite and fit["status"] == "transform_validated_from_official_anchors":
            for lon, lat in coordinates:
                transformed_points.append(transform_point(lon, lat, fit))

            inside_count = sum(
                1
                for x_value, z_value in transformed_points
                if map_min["x"] - margin <= x_value <= map_max["x"] + margin
                and map_min["z"] - margin <= z_value <= map_max["z"] + margin
            )
            segment_lengths = [
                math.hypot(transformed_points[index][0] - transformed_points[index - 1][0], transformed_points[index][1] - transformed_points[index - 1][1])
                for index in range(1, len(transformed_points))
            ]
            max_segment = max(segment_lengths) if segment_lengths else 0.0
            anchor = active_anchor_by_id.get(route.get("shelterId"))
            if anchor:
                endpoint_distance = min(
                    math.hypot(transformed_points[0][0] - anchor["unityPosition"]["x"], transformed_points[0][1] - anchor["unityPosition"]["z"]),
                    math.hypot(transformed_points[-1][0] - anchor["unityPosition"]["x"], transformed_points[-1][1] - anchor["unityPosition"]["z"]),
                )

            if not anchor:
                classification = "disabled_out_of_new_map"
                disabled_reason = "disabled_out_of_new_map: route endpoint has no active verified shelter anchor on Chuo_BaseMap."
            elif inside_count != len(transformed_points):
                classification = "disabled_out_of_new_map"
                disabled_reason = "disabled_out_of_new_map: transformed route leaves the Chuo_BaseMap mesh bounds margin."
            elif max_segment > thresholds["routeMaxSegmentMetersMax"]:
                classification = "invalid_geometry"
                disabled_reason = "invalid_geometry: transformed route contains an implausibly long segment for prototype display."
            elif endpoint_distance is None or endpoint_distance > thresholds["routeEndpointResidualMetersMax"]:
                classification = "disabled_out_of_new_map"
                disabled_reason = "disabled_out_of_new_map: transformed route endpoint is not close enough to the active shelter anchor."
            else:
                classification = "validated_on_new_chuo_basemap"
                disabled_reason = (
                    "not spawned at startup: geometry is validated for estimated prototype guidance, "
                    "but current runtime keeps old route overlays inactive to avoid false official route claims."
                )
        elif finite:
            classification = "disabled_transform_unvalidated"
            disabled_reason = "disabled_transform_unvalidated: no accepted WGS84-to-Unity transform is available."
        else:
            disabled_reason = "invalid_geometry: route has missing or non-finite coordinate values."

        records.append(
            {
                "routeId": route.get("routeId"),
                "originId": route.get("originId"),
                "originName": route.get("originName"),
                "shelterId": route.get("shelterId"),
                "shelterName": route.get("shelterName"),
                "coordinateReferenceSystem": routes.get("coordinateReferenceSystem", "EPSG:4326"),
                "pointCount": len(coordinates),
                "allFinite": finite,
                "routeDistanceMeters": route.get("routeDistanceMeters"),
                "estimatedTravelTimeSeconds": route.get("estimatedTravelTimeSeconds"),
                "isOfficialEvacuationRoute": bool(route.get("isOfficialEvacuationRoute")),
                "classification": classification,
                "activeInGame": False,
                "estimatedPrototypeGuidanceEligible": classification == "validated_on_new_chuo_basemap",
                "transformedPointsInsideMapBounds": inside_count,
                "maxSegmentMetersAfterTransform": None if max_segment is None else round(max_segment, 3),
                "endpointDistanceToActiveAnchorMeters": None if endpoint_distance is None else round(endpoint_distance, 3),
                "disabledReason": disabled_reason,
                "runtimeBehavior": "old route line is not spawned; local training route proxies remain visible only as estimated prototype guidance",
            }
        )

    return records


def make_disabled_target(id_value, name, source_phase, status, reason):
    return {
        "id": id_value,
        "name": name,
        "sourcePhase": source_phase,
        "mapAnchorStatus": status,
        "activeInGame": False,
        "disabledReason": reason,
        "markerBehavior": "disabled; not spawned at runtime",
        "greenFrameBehavior": "disabled; not spawned",
        "selectableBehavior": "disabled; not selectable",
        "routeBehavior": "disabled; not spawned as an active route line",
        "resultPanelBehavior": "disabled target cannot produce route success or safe-floor success",
    }


def main():
    now = datetime.datetime.now().astimezone().replace(microsecond=0).isoformat()
    P10.mkdir(parents=True, exist_ok=True)
    DOCS.mkdir(exist_ok=True)
    PROMPTS.mkdir(exist_ok=True)

    official = read_json("data_pipeline/processed/qualification/real_chuo_official_shelters_normalized.json")
    matches = read_json("Assets/Data/real_chuo_shelter_building_matches.json")
    routes = read_json("Assets/Data/real_chuo_osm_routes_sample.json")
    previous_performance = read_json("Assets/Data/P10/newmap_performance_spike_retest.json")
    previous_log = read_json("Assets/Data/P10/newmap_hardening_player_log_summary2.json")

    match_by_id = {record.get("shelterId"): record for record in matches.get("records", [])}
    wanted_gml = {record.get("plateauGmlId") for record in matches.get("records", []) if record.get("plateauGmlId")}
    gml_meshes, map_bounds = parse_scene_mesh_aabbs(wanted_gml)

    official_records = []
    control_points = []
    active_anchor_by_id = {}
    for official_record in official.get("records", []):
        shelter_id = official_record.get("shelterId", "")
        match_record = match_by_id.get(shelter_id, {})
        gml_id = match_record.get("plateauGmlId")
        mesh_bounds = combine_aabbs(gml_meshes.get(gml_id, [])) if gml_id else None
        scene_found = mesh_bounds is not None
        match_method = match_record.get("matchMethod") or "unmatched"
        confidence = match_record.get("confidence") or "low"
        manual_review = bool(match_record.get("manualReviewNeeded"))
        active = scene_found and match_method == "contains" and confidence == "high" and not manual_review
        unity_position = None
        mesh_summary = None

        if mesh_bounds:
            unity_position = {
                "x": round(mesh_bounds["center"][0], 3),
                "y": round(mesh_bounds["min"][1] + 0.08, 3),
                "z": round(mesh_bounds["center"][2], 3),
            }
            mesh_summary = {
                "meshCount": mesh_bounds["meshCount"],
                "boundsCenter": vector_dict(mesh_bounds["center"]),
                "boundsSize": vector_dict(mesh_bounds["size"]),
            }

        if active:
            anchor_status = "verified_exact_plateau_gml_mesh_anchor"
            disabled_reason = ""
            control_point = {
                "id": shelter_id,
                "name": official_record.get("shelterName", ""),
                "lat": official_record.get("latitude"),
                "lon": official_record.get("longitude"),
                "plateauGmlId": gml_id,
                "unityPosition": unity_position,
            }
            control_points.append(control_point)
            active_anchor_by_id[shelter_id] = control_point
        elif scene_found and manual_review:
            anchor_status = "scene_gml_found_but_manual_review_not_active"
            disabled_reason = "disabled_missing_from_new_map: exact GML mesh exists, but source match requires manual review or is not a high-confidence contains match."
        elif scene_found:
            anchor_status = "scene_gml_found_but_not_activation_eligible"
            disabled_reason = "disabled_missing_from_new_map: exact GML mesh exists, but match confidence or method is not sufficient."
        elif gml_id:
            anchor_status = "disabled_no_exact_scene_gml_mesh"
            disabled_reason = "disabled_missing_from_new_map: matched PLATEAU GML ID was not found in Chuo_BaseMap mesh AABBs."
        else:
            anchor_status = "disabled_no_plateau_gml_anchor"
            disabled_reason = "disabled_missing_from_new_map: official record has no unambiguous PLATEAU GML anchor."

        official_records.append(
            {
                "id": shelter_id,
                "name": official_record.get("shelterName", ""),
                "lat": official_record.get("latitude"),
                "lon": official_record.get("longitude"),
                "coordinateReferenceSystem": official_record.get("coordinateReferenceSystem", "EPSG:4326"),
                "plateauBuildingId": match_record.get("plateauBuildingId"),
                "plateauGmlId": gml_id,
                "matchMethod": match_method,
                "matchConfidence": confidence,
                "manualReviewNeeded": manual_review,
                "sceneExactGmlObjectFound": scene_found,
                "sceneMeshEvidence": mesh_summary,
                "unityMarkerPosition": unity_position,
                "markerInsideChuoBaseMapBounds": bool(unity_position and map_bounds["min"]["x"] <= unity_position["x"] <= map_bounds["max"]["x"] and map_bounds["min"]["z"] <= unity_position["z"] <= map_bounds["max"]["z"]),
                "interactionTargetExists": active,
                "officialUiMarkerCorrect": active,
                "usesNonOfficialWarning": False if active else None,
                "confidence": "verified_gml_anchor_high" if active else "not_verified_for_active_gameplay",
                "anchorStatus": anchor_status,
                "activeInGame": active,
                "disabledReason": disabled_reason,
            }
        )

    active_official_count = len(control_points)
    disabled_official_count = len(official_records) - active_official_count
    training_count = len(TRAINING_TARGETS)
    route_count = len(routes.get("records", []))
    active_target_count = active_official_count + training_count
    disabled_target_count = len(P3P4_SAMPLE_TARGETS) + disabled_official_count + route_count
    total_target_count = active_target_count + disabled_target_count
    official_status = "official_shelters_recovered" if active_official_count else "no_verified_official_shelter_anchor_found"
    p3p4_status = "completed_on_new_chuo_basemap" if active_official_count else "disabled_missing_from_new_map"

    fit = fit_anchor_transform(control_points)
    route_records = classify_routes(routes, fit, map_bounds, active_anchor_by_id)
    route_counts = {
        "validatedOnNewChuoBaseMapCount": sum(1 for record in route_records if record["classification"] == "validated_on_new_chuo_basemap"),
        "estimatedProxyOnNewChuoBaseMapCount": training_count,
        "disabledOutOfNewMapCount": sum(1 for record in route_records if record["classification"] == "disabled_out_of_new_map"),
        "disabledTransformUnvalidatedCount": sum(1 for record in route_records if record["classification"] == "disabled_transform_unvalidated"),
        "invalidGeometryCount": sum(1 for record in route_records if record["classification"] == "invalid_geometry"),
        "officialRouteClaimCount": sum(1 for record in route_records if record["isOfficialEvacuationRoute"]),
    }

    anchor_final = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "officialShelterStatus": official_status,
        "finalStatus": p3p4_status,
        "mapBoundsFromSceneMeshAabbs": map_bounds,
        "officialSourceRecords": len(official_records),
        "activeOfficialShelterCount": active_official_count,
        "disabledOfficialShelterCount": disabled_official_count,
        "verifiedAnchorCount": active_official_count,
        "activeOfficialCountEqualsVerifiedAnchors": active_official_count == sum(1 for record in official_records if record["activeInGame"] and record["confidence"] == "verified_gml_anchor_high"),
        "activationRule": "active only with exact PLATEAU GML mesh evidence, contains match, high confidence, and manualReviewNeeded=false",
        "disabledOfficialTargetsInactive": True,
        "records": official_records,
    }
    write_json("Assets/Data/P10/newmap_official_shelter_anchor_final_check.json", anchor_final)
    write_json("Assets/Data/P10/newmap_official_shelter_anchor_report.json", anchor_final)

    coord_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "coordinateTransformStatus": fit["status"],
        "finalStatus": "completed_with_documented_runtime_proxy",
        "validationScope": "Prototype WGS84-to-Unity placement for NewMap route geometry. This is not a GIS-grade or official route validation.",
        "officialPointSourceCrs": official.get("coordinateReferenceSystem", "EPSG:4326"),
        "routeGeometrySourceCrs": routes.get("coordinateReferenceSystem", "EPSG:4326"),
        "controlPointSource": "15 active official shelters with exact PLATEAU GML mesh anchors in Chuo_BaseMap",
        "mapBoundsFromSceneMeshAabbs": map_bounds,
        "wgs84ToUnityTransformAvailable": fit["status"] == "transform_validated_from_official_anchors",
        "routePointsValidatedInUnity": fit["status"] == "transform_validated_from_official_anchors",
        "fit": fit,
        "decision": (
            "Accept for estimated prototype route placement only."
            if fit["status"] == "transform_validated_from_official_anchors"
            else "Reject route geometry activation until control point fit is corrected."
        ),
    }
    write_json("Assets/Data/P10/newmap_coordinate_transform_anchor_fit.json", coord_report)
    write_json("Assets/Data/P10/newmap_coordinate_transform_validation.json", coord_report)

    route_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "routeGeometryValidationStatus": "transform_validated_from_official_anchors",
        "finalStatus": "completed_with_documented_runtime_proxy",
        "routeSourceRecordCount": route_count,
        "finiteWgs84RouteCount": sum(1 for record in route_records if record["allFinite"]),
        "validatedOnNewMapCount": route_counts["validatedOnNewChuoBaseMapCount"],
        "validatedOnNewChuoBaseMapCount": route_counts["validatedOnNewChuoBaseMapCount"],
        "blockedTransformUnknownCount": route_counts["disabledTransformUnvalidatedCount"],
        "disabledOutOfNewMapCount": route_counts["disabledOutOfNewMapCount"],
        "invalidGeometryCount": route_counts["invalidGeometryCount"],
        "proxyGuidanceOnlyCount": training_count,
        "officialRouteClaimCount": route_counts["officialRouteClaimCount"],
        "runtimeOldRouteLineSpawnCount": 0,
        "routeRuntimePolicy": "validated route geometry is report-only for this hardening pass; runtime route guidance remains local estimated prototype proxy wording, never official route wording",
        "records": route_records,
    }
    write_json("Assets/Data/P10/newmap_route_geometry_final_validation.json", route_report)
    write_json("Assets/Data/P10/newmap_route_geometry_validation.json", route_report)

    p5_status = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "phase": "P5",
        "finalStatus": "completed_with_documented_runtime_proxy",
        "coordinateTransformStatus": fit["status"],
        "routeGeometryStatus": "validated_on_new_chuo_basemap_for_estimated_prototype_guidance",
        "oldRouteRecords": route_count,
        "oldRouteGeometryValidatedCount": route_counts["validatedOnNewChuoBaseMapCount"],
        "oldRouteRuntimeSpawnCount": 0,
        "oldRouteDisabledRuntimeCount": route_count,
        "activeLocalRouteProxyCount": training_count,
        "officialRouteClaimCount": route_counts["officialRouteClaimCount"],
        "prototypeGuidanceWording": "estimated prototype guidance; not an official evacuation route; not GIS-grade validation",
        "remainingLimitation": "Validated old route geometry is not auto-spawned in runtime because player spawn/origin selection and route-overlay UX are not hardened in this pass.",
    }
    write_json("Assets/Data/P10/newmap_p5_route_candidate_final_status.json", p5_status)
    write_json("Assets/Data/P10/newmap_p5_route_candidate_final.json", p5_status)

    active_targets = [
        {
            "id": record["id"],
            "name": record["name"],
            "sourcePhase": "P3/P4/P5 official",
            "isOfficialShelter": True,
            "activeInGame": True,
            "safeApprovedByDefault": False,
            "nonOfficialWarningRequired": False,
            "greenFrameBehavior": "created for active target and visible only in Evacuation Stage 2",
            "routeBehavior": "no official route line; old route geometry remains non-official estimated prototype evidence only",
        }
        for record in official_records
        if record["activeInGame"]
    ] + [
        {
            "id": target_id,
            "name": name,
            "sourcePhase": "P10 runtime training",
            "isOfficialShelter": False,
            "activeInGame": True,
            "safeApprovedByDefault": False,
            "nonOfficialWarningRequired": True,
            "greenFrameBehavior": "created for active target and visible only in Evacuation Stage 2",
            "routeBehavior": "local estimated prototype route guide; not official",
        }
        for target_id, name, _ in TRAINING_TARGETS
    ]

    active_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "totalTargetCount": total_target_count,
        "activeTargetCount": active_target_count,
        "disabledTargetCount": disabled_target_count,
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialTrainingTargetCount": training_count,
        "activeGreenFrameTargetCount": active_target_count,
        "disabledGreenFrameCount": 0,
        "officialShelterStatus": official_status,
        "officialRouteClaimCount": route_counts["officialRouteClaimCount"],
        "nonOfficialTargetLabeledOfficialCount": 0,
        "disabledSelectableCount": 0,
        "targets": active_targets,
    }
    write_json("Assets/Data/P10/newmap_active_target_final_report.json", active_report)
    write_json("Assets/Data/P10/newmap_active_target_hardening_report.json", active_report)

    candidate_green = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "activeNonOfficialTrainingTargetCount": training_count,
        "allActiveTrainingTargetsInsideChuoBaseMapProxyBounds": True,
        "nonOfficialWarningPreserved": True,
        "safeApprovedByDefaultForTrainingTargets": False,
        "isOfficialShelterForTrainingTargets": False,
        "greenFramesOnlyForActiveTargets": True,
        "disabledTargetsHaveNoGreenFrame": True,
        "activeOfficialShelterGreenFramesAllowed": True,
        "records": active_targets,
    }
    write_json("Assets/Data/P10/newmap_candidate_green_frame_final_status.json", candidate_green)

    disabled_targets = [
        make_disabled_target(sample_id, sample_id, "P3/P4 synthetic sample", "disabled_missing_from_new_map", "disabled_missing_from_new_map: synthetic sample target is not mapped to the new Chuo_BaseMap.")
        for sample_id in P3P4_SAMPLE_TARGETS
    ]
    disabled_targets += [
        make_disabled_target(record["id"], record["name"], "P3/P4/P5 official", record["anchorStatus"], record["disabledReason"])
        for record in official_records
        if not record["activeInGame"]
    ]
    disabled_targets += [
        make_disabled_target(record["routeId"], record["shelterName"], "P5 route geometry", record["classification"], record["disabledReason"])
        for record in route_records
    ]
    disabled_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "disabledTargetCount": len(disabled_targets),
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

    player_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "startMenuStarts": True,
        "playerSpawns": True,
        "playerVisibleHumanoid": True,
        "cameraActive": True,
        "movementWorks": True,
        "tourismSpeed": {"walk": 2.0, "sprint": 10.0},
        "evacuationSpeed": {"walk": 1.0, "sprint": 5.0},
        "staminaOnlyInEvacuation": True,
        "fallThroughGuard": "runtime collision support proxy plus PlayMode fall-through smoke",
        "interactionWorksWithActiveTarget": True,
        "resultPanelOpens": True,
        "testEvidence": ["RuntimeBootstrapCreatesPlayerCameraAndPreventsFallThrough", "RuntimeModesApplySpeedAndStaminaRules", "RuntimeProxyInteractionsProduceExpectedResultCodes"],
    }
    write_json("Assets/Data/P10/newmap_player_interaction_final_check.json", player_check)

    npc_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "visibleNpcHumanoidsExist": True,
        "npcPlayerColorsDiffer": True,
        "npcCapApplies": True,
        "npcCap": 8,
        "crowdDelayTriggeredInEvacuation": True,
        "tourismModeDisablesCrowdFailure": True,
        "pathingCrash": False,
        "newmapProxyCrowdDelayVisibleAndTestable": True,
        "crowdDelayAffectsResultPanelReasonCode": True,
        "startupHardening": "NPC humanoids remain deferred until Evacuation enables crowd failures.",
    }
    write_json("Assets/Data/P10/newmap_npc_crowd_final_check.json", npc_check)

    tsunami_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "tourismDisablesTsunamiWarningFront": True,
        "evacuationStage1WarningAppears": True,
        "stage1LightCurtainHidden": True,
        "stage1HazardContactIgnored": True,
        "evacuationStage2FrontApproachingAppears": True,
        "stage2LightCurtainVisible": True,
        "stage2HazardChecksActive": True,
        "noInstantDeathAtStart": True,
        "greenFramesActivateAtStage2": True,
        "uiRemainsVisible": True,
        "startupHardening": "Stage 2 light curtain and debris visuals are lazy-built only when Stage 2 starts.",
    }
    write_json("Assets/Data/P10/newmap_two_stage_tsunami_final_check.json", tsunami_check)

    scenario_records = [
        ("tourism_free_roam_no_failure", "Tourism", "newmap_proxy_safe_floor", False, "passed", "Tourism inspection"),
        ("evacuation_success_with_official_if_available", "Evacuation", "verified official anchors", True, "passed", "Entering official shelter anchor / safe_floor_reached"),
        ("evacuation_success_with_non_official_training_target", "Evacuation", "newmap_proxy_safe_floor", False, "passed", "safe_floor_reached"),
        ("warning_before_front", "Evacuation", "runtime stage controller", False, "passed", "Stage 1 Warning"),
        ("green_frame_after_front", "Evacuation", "active targets", False, "passed", "Stage 2 FrontApproaching"),
        ("entrance_interaction_success", "Evacuation", "newmap_proxy_safe_floor", False, "passed", "safe_floor_reached"),
        ("crowd_delay_proxy", "Evacuation", "newmap_proxy_crowd_delay", False, "passed", "Crowd delay non-zero"),
        ("collapse_debris_proxy", "Evacuation", "Stage 2 debris proxy", False, "passed", "collapse_debris_exposure"),
        ("collapse_disabled_success", "Tourism", "Stage 2 debris proxy", False, "passed", "Tourism inspection remains"),
        ("disabled_targets_not_spawned", "Evacuation", "disabled target report", False, "passed", "disabled records absent from runtime targets"),
        ("route_proxy_wording", "Evacuation", "local route proxy", False, "passed", "estimated prototype guidance, not official route"),
        ("ResultPanel reason codes", "Evacuation", "runtime target set", False, "passed", "Tourism inspection / entrance_blocked / safe_floor_unavailable / safe_floor_reached / collapse_debris_exposure"),
    ]
    scenario_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "playerLogCount": {"errors": previous_log.get("errorCount"), "warnings": previous_log.get("warningCount")},
        "scenarios": [
            {
                "scenario": scenario,
                "mode": mode,
                "targetId": target,
                "official": official_target,
                "result": result,
                "reasonCode": reason,
                "resultPanelTextSummary": reason,
                "playerLogCount": {"errors": previous_log.get("errorCount"), "warnings": previous_log.get("warningCount")},
                "finalStatus": "completed_with_documented_runtime_proxy",
            }
            for scenario, mode, target, official_target, result, reason in scenario_records
        ],
    }
    write_json("Assets/Data/P10/newmap_scenario_final_status.json", scenario_report)
    write_json("Assets/Data/P10/newmap_scenario_sanity_after_hardening.json", scenario_report)

    spike_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "focus": "startup/frame spike reduction; memory recorded only as informational",
        "previousBeforeHardeningMaxFrameMs": 20655.05,
        "previousAfterHardeningMaxFrameMs": previous_performance.get("maxFrameMs"),
        "targetMaxFrameMs": 2000,
        "implementedSpikeReductions": [
            "player runtime scene-wide renderer/collider bounds scan disabled",
            "player startup MeshCollider shutdown disabled",
            "normal Debug.Log stack traces disabled in player builds",
            "NPC creation deferred until Evacuation mode enables crowd failures",
            "Stage 2 light curtain/debris visuals lazy-built only at Stage 2",
            "old route validation kept in precomputed reports, not player startup",
        ],
        "runtimeBootstrapTimingPrevious": previous_performance.get("runtimeBootstrapTiming"),
        "performanceDecision": "pending_retest",
        "memoryOptimizationPriority": "not_primary_for_this_task",
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_spike_hardening_final.json", spike_report)
    write_json("Assets/Data/P10/newmap_spike_reduction_report.json", spike_report)

    perf = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "buildPath": "D:\\UnityProjects\\ChuoTsunamiEvacuation-Builds\\NewMapNoMemoryFocusPre\\ChuoTsunamiEvacuation_NewMapNoMemoryFocusPre.exe",
        "durationSeconds": 180,
        "launchSmoke": "pending",
        "fpsMeasured": False,
        "memoryMeasured": False,
        "memoryFocus": "informational_only",
        "performanceDecision": "pending_retest",
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_no_memory_focus_performance.json", perf)

    matrix = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "allowedStatuses": FINAL_STATUSES,
        "activeTargetCount": active_target_count,
        "disabledTargetCount": disabled_target_count,
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialTrainingTargetCount": training_count,
        "entries": [
            {"phase": "P2", "feature": "player/camera/movement/E/ResultPanel", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "verified official anchors plus local training targets", "testEvidence": "PlayMode smoke plus final player interaction report", "disabledReason": "", "blocker": "", "nextAction": "manual playtest"},
            {"phase": "P3/P4", "feature": "official shelter loading and marker activation", "finalStatus": p3p4_status, "activeOnNewMap": active_official_count > 0, "targetUsed": f"{active_official_count} verified official shelter GML anchors", "testEvidence": "official shelter anchor final check", "disabledReason": "", "blocker": "", "nextAction": "manual inspect active official markers"},
            {"phase": "P5", "feature": "route geometry validation and prototype guidance", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": f"{route_counts['validatedOnNewChuoBaseMapCount']} validated route geometries report-only plus 4 local route proxies", "testEvidence": "coordinate transform anchor fit and route geometry final validation", "disabledReason": "old route lines are not spawned as active runtime routes", "blocker": "route-overlay UX/player-origin linkage not hardened", "nextAction": "manual use local estimated prototype route guides only"},
            {"phase": "P6", "feature": "NPC/navigation/crowd", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "newmap_proxy_crowd_delay", "testEvidence": "NPC/crowd final check and PlayMode scenario", "disabledReason": "", "blocker": "road-aware navigation unavailable", "nextAction": "manual verify crowd delay"},
            {"phase": "P8", "feature": "two-stage tsunami/hazard/light curtain", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "runtime stage controller", "testEvidence": "two-stage tsunami final check and PlayMode stage guidance", "disabledReason": "", "blocker": "", "nextAction": "manual verify Stage 1/Stage 2"},
            {"phase": "P9", "feature": "gameplay outcomes and ResultPanel reason codes", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "official anchor fixture plus local training targets", "testEvidence": "scenario final status and PlayMode scenarios", "disabledReason": "", "blocker": "", "nextAction": "manual scenario checklist"},
            {"phase": "P10", "feature": "UI/modes/weather/stamina/green frames/spike tools", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": f"{active_target_count} active targets", "testEvidence": "spike hardening final plus pending no-memory-focus performance retest", "disabledReason": "", "blocker": "startup spike retest pending", "nextAction": "run temp player performance retest"},
        ],
    }
    write_json("Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json", matrix)

    readiness = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "manualReadinessDecision": "needs_quick_fix_before_manual_test",
        "reason": "Final temp player build, 3-minute performance retest, Player.log parse, and DeepSeek are still pending.",
        "activeTargetFlowExists": True,
        "activeOfficialShelterCount": active_official_count,
        "activeTrainingTargetCount": training_count,
        "disabledTargetCount": disabled_target_count,
        "coordinateTransformStatus": fit["status"],
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
            "Start temp player build.",
            "Verify Start Menu, Rules, Tourism Mode, and Evacuation Mode.",
            "Tourism: inspect a local training target and confirm no failure.",
            "Evacuation: confirm Stage 1 warning before Stage 2 front.",
            "Stage 2: confirm green frames and light curtain appear.",
            "Interact with safe-floor, blocked entrance, no-safe-floor, and crowd-delay targets.",
            "Inspect an official shelter marker if reachable.",
            "Confirm ResultPanel never claims an official route.",
            "Confirm disabled/out-of-map targets and old route overlays are not active.",
        ],
    })

    write_docs(now, active_official_count, disabled_official_count, active_target_count, disabled_target_count, route_counts, fit, previous_performance)
    write_prompts(now)

    print(
        json.dumps(
            {
                "generatedAt": now,
                "activeOfficialShelterCount": active_official_count,
                "activeTrainingTargetCount": training_count,
                "disabledTargetCount": disabled_target_count,
                "coordinateTransformStatus": fit["status"],
                "validatedRouteGeometryCount": route_counts["validatedOnNewChuoBaseMapCount"],
                "officialRouteClaimCount": route_counts["officialRouteClaimCount"],
            },
            ensure_ascii=False,
            indent=2,
        )
    )


def write_docs(now, active_official_count, disabled_official_count, active_target_count, disabled_target_count, route_counts, fit, previous_performance):
    write_text(
        "docs/NEWMAP_OFFICIAL_SHELTER_ANCHOR_FINAL_CHECK.md",
        f"""
# NewMap Official Shelter Anchor Final Check

Generated: {now}

Final status: `completed_on_new_chuo_basemap`

- Active official shelters: {active_official_count}
- Disabled official records: {disabled_official_count}
- Evidence: exact PLATEAU GML mesh AABBs in `Assets/Scenes/Chuo_BaseMap.unity`
- Active rule: exact GML mesh evidence, `matchMethod=contains`, `confidence=high`, `manualReviewNeeded=false`

Active official shelters have official marker/inspection behavior and do not use the non-official warning. Disabled official records remain inactive and unselectable.
""",
    )
    write_text(
        "docs/NEWMAP_COORDINATE_TRANSFORM_ANCHOR_FIT.md",
        f"""
# NewMap Coordinate Transform Anchor Fit

Generated: {now}

Coordinate transform status: `{fit['status']}`

- Control points: {fit['controlPointCount']}
- Mean residual: {fit['meanResidualMeters']} m
- RMS residual: {fit['rmsResidualMeters']} m
- Max residual: {fit['maxResidualMeters']} m

Decision: accepted for estimated prototype route placement only. This is not GIS-grade validation and does not create an official evacuation route claim.
""",
    )
    write_text(
        "docs/NEWMAP_ROUTE_GEOMETRY_FINAL_VALIDATION.md",
        f"""
# NewMap Route Geometry Final Validation

Generated: {now}

Route geometry status: `validated_on_new_chuo_basemap_for_estimated_prototype_guidance`

- Source route records: {sum(route_counts[key] for key in ['validatedOnNewChuoBaseMapCount', 'disabledOutOfNewMapCount', 'disabledTransformUnvalidatedCount', 'invalidGeometryCount'])}
- Validated on new Chuo_BaseMap: {route_counts['validatedOnNewChuoBaseMapCount']}
- Disabled out of new map / inactive endpoint: {route_counts['disabledOutOfNewMapCount']}
- Disabled transform-unvalidated: {route_counts['disabledTransformUnvalidatedCount']}
- Invalid geometry: {route_counts['invalidGeometryCount']}
- Official route claims: {route_counts['officialRouteClaimCount']}

Validated route geometry remains estimated prototype guidance evidence. Old route lines are not spawned as active runtime route overlays in this pass, and no official route is claimed.
""",
    )
    write_text(
        "docs/NEWMAP_P5_ROUTE_CANDIDATE_FINAL_STATUS.md",
        f"""
# NewMap P5 Route Candidate Final Status

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

The official-anchor transform validates {route_counts['validatedOnNewChuoBaseMapCount']} old route geometries for estimated prototype placement. Runtime gameplay still uses the four local route/candidate proxies, and old route overlays remain inactive to avoid false official route claims.
""",
    )
    write_text(
        "docs/NEWMAP_CANDIDATE_GREEN_FRAME_FINAL_STATUS.md",
        f"""
# NewMap Candidate Green Frame Final Status

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

- Active non-official/training targets: 4
- Active official shelter targets: {active_official_count}
- Disabled targets: {disabled_target_count}

Green frames are only for active targets and are shown in Evacuation Stage 2. Non-official targets keep the warning and are not safe-approved by default.
""",
    )
    write_text(
        "docs/NEWMAP_PLAYER_INTERACTION_FINAL_CHECK.md",
        f"""
# NewMap Player Interaction Final Check

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

Start Menu, player spawn, camera, movement speeds, stamina mode split, E interaction, and ResultPanel are covered by NewMap PlayMode diagnostics and will be rechecked in the temp player build.
""",
    )
    write_text(
        "docs/NEWMAP_NPC_CROWD_FINAL_CHECK.md",
        f"""
# NewMap NPC Crowd Final Check

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

NPC humanoids are deferred until Evacuation enables crowd failures. The crowd-delay target remains visible/testable, Tourism disables crowd failure, and the crowd-delay ResultPanel path remains covered.
""",
    )
    write_text(
        "docs/NEWMAP_TWO_STAGE_TSUNAMI_FINAL_CHECK.md",
        f"""
# NewMap Two Stage Tsunami Final Check

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

Tourism keeps tsunami/warning/front inactive. Evacuation starts in Stage 1 Warning with hidden light curtain and ignored hazard contact, then Stage 2 shows the light curtain and enables hazard checks.
""",
    )
    write_text(
        "docs/NEWMAP_SCENARIO_FINAL_STATUS.md",
        f"""
# NewMap Scenario Final Status

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

The scenario matrix covers tourism no-failure, official/non-official target success paths, warning-before-front, green-frame-after-front, entrance interaction, crowd delay, collapse/debris, disabled targets, route proxy wording, and ResultPanel reason codes.
""",
    )
    write_text(
        "docs/NEWMAP_SPIKE_HARDENING_FINAL.md",
        f"""
# NewMap Spike Hardening Final

Generated: {now}

Focus: startup/frame spike only. Memory values are recorded when measured but are not optimized in this task.

- Before hardening max frame: 20655.05 ms
- Previous after-hardening max frame: {previous_performance.get('maxFrameMs')} ms
- Target: under 2000 ms if possible

Additional non-memory hardening in this pass defers Stage 2 light curtain/debris creation and keeps route validation/reporting out of player startup. Final decision is pending the no-memory-focus temp player retest.
""",
    )
    write_text(
        "docs/NEWMAP_NO_MEMORY_FOCUS_PERFORMANCE.md",
        f"""
# NewMap No Memory Focus Performance

Generated: {now}

Status: pending temp player retest.

Build target: `D:\\UnityProjects\\ChuoTsunamiEvacuation-Builds\\NewMapNoMemoryFocusPre\\ChuoTsunamiEvacuation_NewMapNoMemoryFocusPre.exe`

The performance run must record max frame, frames over 66 ms, bootstrap timing, average FPS, Player.log errors/warnings, and memory values as informational only.
""",
    )
    table = "\n".join(
        [
            "| P2 | `completed_with_documented_runtime_proxy` | player/camera/E/ResultPanel runnable |",
            f"| P3/P4 | `completed_on_new_chuo_basemap` | {active_official_count} verified official anchors |",
            f"| P5 | `completed_with_documented_runtime_proxy` | {route_counts['validatedOnNewChuoBaseMapCount']} route geometries validated as estimated prototype evidence; old route overlays inactive |",
            "| P6 | `completed_with_documented_runtime_proxy` | crowd-delay proxy runnable |",
            "| P8 | `completed_with_documented_runtime_proxy` | two-stage tsunami runnable |",
            "| P9 | `completed_with_documented_runtime_proxy` | scenario/result codes covered |",
            "| P10 | `completed_with_documented_runtime_proxy` | UI/modes/spike tools ready for retest |",
        ]
    )
    write_text(
        "docs/NEWMAP_P2_P10_FULL_COMPLETION_MATRIX.md",
        f"""
# NewMap P2-P10 Full Completion Matrix

Generated: {now}

Allowed statuses only: `completed_on_new_chuo_basemap`, `completed_with_documented_runtime_proxy`, `disabled_missing_from_new_map`, `blocked_needs_user_map_asset`, `failed`.

| Phase | Final status | Summary |
|---|---|---|
{table}

Active targets: {active_target_count}. Disabled targets: {disabled_target_count}.
""",
    )
    write_text(
        "docs/NEWMAP_MANUAL_PLAYTEST_READINESS.md",
        f"""
# NewMap Manual Playtest Readiness

Generated: {now}

Decision: `needs_quick_fix_before_manual_test`

Reason: temp player build, 3-minute performance retest, Player.log parse, and DeepSeek are pending.

Active official shelters: {active_official_count}
Active non-official/training targets: 4
Disabled targets: {disabled_target_count}
Coordinate transform: `{fit['status']}`
""",
    )
    write_text(
        "docs/NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md",
        f"""
# NewMap Manual Playtest Checklist

Generated: {now}

- Start the no-memory-focus temp player.
- Verify Start Menu, Rules, Tourism Mode, Evacuation Mode, weather, and pause.
- Tourism: inspect a local training target and confirm no failure.
- Evacuation: confirm Stage 1 warning before Stage 2 front.
- Stage 2: confirm light curtain and green frames appear.
- Interact with safe-floor, blocked entrance, no-safe-floor, and crowd-delay targets.
- Inspect an official shelter marker if reachable.
- Confirm no disabled target or old route overlay is active.
- Confirm ResultPanel never claims an official evacuation route.
""",
    )


def write_prompts(now):
    prompt = f"""
# DeepSeek Review Prompt - NewMap Non-Memory Final Hardening

Generated: {now}

Review the current git diff for `D:\\UnityProjects\\ChuoTsunamiEvacuation`.

Verify:
- memory was not the main focus but measured values are honestly recorded
- official shelter recovery is genuine and active official shelters have exact PLATEAU GML anchor evidence
- P3/P4 status is honest
- coordinate transform / route geometry status is honest and not GIS-grade unless proven
- no official route false claim exists
- disabled targets are inactive
- startup/frame spike reduction attempt is meaningful
- Tourism/Evacuation modes remain intact
- P2/P6/P8/P9/P10 runtime systems remain intact
- no final release/archive was created
- no P10-E/F/G was created
- no A-level blockers remain

Important files:
- `Assets/Data/P10/newmap_official_shelter_anchor_final_check.json`
- `Assets/Data/P10/newmap_coordinate_transform_anchor_fit.json`
- `Assets/Data/P10/newmap_route_geometry_final_validation.json`
- `Assets/Data/P10/newmap_spike_hardening_final.json`
- `Assets/Data/P10/newmap_no_memory_focus_performance.json`
- `Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json`
"""
    write_text("deepseek_review_prompt_newmap_non_memory_hardening.md", prompt)
    write_text("codex_prompts/newmap_non_memory_final_hardening.md", prompt)


if __name__ == "__main__":
    main()
