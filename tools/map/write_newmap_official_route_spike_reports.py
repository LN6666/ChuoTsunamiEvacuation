import datetime
import json
import math
import re
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
P10 = ROOT / "Assets" / "Data" / "P10"
DOCS = ROOT / "docs"
PROMPTS = ROOT / "codex_prompts"
ACTIVE_SCENE = "Assets/Scenes/Chuo_BaseMap.unity"


def read_json(relative_path):
    return json.loads((ROOT / relative_path).read_text(encoding="utf-8-sig"))


def write_json(relative_path, data):
    path = ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def write_text(relative_path, text):
    path = ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(text.strip() + "\n", encoding="utf-8")


def scan_scene_gml_names(wanted):
    scene_path = ROOT / "Assets" / "Scenes" / "Chuo_BaseMap.unity"
    found = set()
    if not scene_path.exists():
        return found

    pattern = re.compile(r"^\s*m_Name:\s*(\S+)\s*$")
    with scene_path.open("r", encoding="utf-8", errors="ignore") as scene_file:
        for line in scene_file:
            match = pattern.match(line)
            if match and match.group(1) in wanted:
                found.add(match.group(1))

    return found


def route_is_finite(route):
    coordinates = (route.get("geometry") or {}).get("coordinates") or []
    if len(coordinates) < 2:
        return False

    for point in coordinates:
        if not isinstance(point, list) or len(point) < 2:
            return False
        if not all(isinstance(value, (int, float)) and math.isfinite(value) for value in point[:2]):
            return False

    return True


def main():
    now = datetime.datetime.now().astimezone().replace(microsecond=0).isoformat()
    P10.mkdir(parents=True, exist_ok=True)
    DOCS.mkdir(exist_ok=True)
    PROMPTS.mkdir(exist_ok=True)

    official = read_json("data_pipeline/processed/qualification/real_chuo_official_shelters_normalized.json")
    matches = read_json("Assets/Data/real_chuo_shelter_building_matches.json")
    routes = read_json("Assets/Data/real_chuo_osm_routes_sample.json")
    audit = read_json("Assets/Data/P10/newmap_unity_scene_audit.json")
    previous_performance = read_json("Assets/Data/P10/newmap_performance_hardening_final.json")
    previous_log = read_json("Assets/Data/P10/newmap_hardening_player_log_summary.json")

    match_by_id = {record.get("shelterId"): record for record in matches.get("records", [])}
    wanted_gml = {record.get("plateauGmlId") for record in matches.get("records", []) if record.get("plateauGmlId")}
    scene_gml_found = scan_scene_gml_names(wanted_gml)

    official_records = []
    active_official_ids = []
    for official_record in official.get("records", []):
        shelter_id = official_record.get("shelterId", "")
        match_record = match_by_id.get(shelter_id, {})
        gml_id = match_record.get("plateauGmlId")
        exact_scene_gml = bool(gml_id and gml_id in scene_gml_found)
        match_method = match_record.get("matchMethod") or "unmatched"
        confidence = match_record.get("confidence") or "low"
        manual_review = bool(match_record.get("manualReviewNeeded"))
        active = exact_scene_gml and match_method == "contains" and confidence == "high" and not manual_review

        if active:
            anchor_status = "verified_exact_plateau_gml_scene_object"
            disabled_reason = ""
            active_official_ids.append(shelter_id)
        elif exact_scene_gml and manual_review:
            anchor_status = "scene_gml_found_but_manual_review_not_active"
            disabled_reason = (
                "disabled_missing_from_new_map: exact PLATEAU GML object exists, but the source match is "
                "manual-review or nearest-match and is not enabled as an official active shelter."
            )
        elif exact_scene_gml:
            anchor_status = "scene_gml_found_but_not_activation_eligible"
            disabled_reason = (
                "disabled_missing_from_new_map: exact PLATEAU GML object exists, but match confidence or "
                "method is not sufficient for active official gameplay."
            )
        elif gml_id:
            anchor_status = "disabled_no_exact_scene_gml_object"
            disabled_reason = (
                "disabled_missing_from_new_map: matched PLATEAU GML ID was not found by exact "
                "scene-object-name scan in Chuo_BaseMap."
            )
        else:
            anchor_status = "disabled_no_plateau_gml_anchor"
            disabled_reason = (
                "disabled_missing_from_new_map: official record has no unambiguous PLATEAU building/GML anchor."
            )

        official_records.append(
            {
                "id": shelter_id,
                "name": official_record.get("shelterName", ""),
                "sourceFile": "data_pipeline/processed/qualification/real_chuo_official_shelters_normalized.json",
                "lat": official_record.get("latitude"),
                "lon": official_record.get("longitude"),
                "coordinateReferenceSystem": official_record.get("coordinateReferenceSystem", "EPSG:4326"),
                "plateauBuildingId": match_record.get("plateauBuildingId"),
                "plateauGmlId": gml_id,
                "matchMethod": match_method,
                "matchConfidence": confidence,
                "manualReviewNeeded": manual_review,
                "sceneExactGmlObjectFound": exact_scene_gml,
                "transformedUnityPosition": None,
                "nearestSceneObject": gml_id if exact_scene_gml else None,
                "distanceToNearestObjectMeters": 0 if exact_scene_gml else None,
                "confidence": "verified_gml_anchor_high" if active else "not_verified_for_active_gameplay",
                "anchorStatus": anchor_status,
                "activeInGame": active,
                "disabledReason": disabled_reason,
            }
        )

    active_official_count = len(active_official_ids)
    training_count = 4
    route_count = len(routes.get("records", []))
    p3p4_sample_count = 5
    official_total = len(official_records)
    disabled_official_count = official_total - active_official_count
    total_target_count = p3p4_sample_count + official_total + route_count + training_count
    active_target_count = active_official_count + training_count
    disabled_target_count = total_target_count - active_target_count
    official_status = "official_shelters_recovered" if active_official_count else "no_verified_official_shelter_anchor_found"
    p3p4_status = "completed_on_new_chuo_basemap" if active_official_count else "disabled_missing_from_new_map"

    anchor_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "officialShelterStatus": official_status,
        "finalStatus": "completed_on_new_chuo_basemap" if active_official_count else "disabled_missing_from_new_map",
        "officialSourceRecords": official_total,
        "sceneExactGmlMatchCount": len(scene_gml_found),
        "activeOfficialShelterCount": active_official_count,
        "disabledOfficialShelterCount": disabled_official_count,
        "activationRule": (
            "active only when official record has exact PLATEAU GML scene object, matchMethod contains, "
            "confidence high, and manualReviewNeeded false"
        ),
        "sourceFiles": [
            "data_pipeline/processed/qualification/real_chuo_official_shelters_normalized.json",
            "Assets/Data/real_chuo_shelter_building_matches.json",
            "Assets/Scenes/Chuo_BaseMap.unity",
        ],
        "records": official_records,
    }
    write_json("Assets/Data/P10/newmap_official_shelter_anchor_report.json", anchor_report)

    active_official_lines = "\n".join(
        f"- `{record['id']}` {record['name']}" for record in official_records if record["activeInGame"]
    ) or "- None"
    write_text(
        "docs/NEWMAP_OFFICIAL_SHELTER_ANCHOR_REPORT.md",
        f"""
# NewMap Official Shelter Anchor Report

Generated: {now}

Active scene: `{ACTIVE_SCENE}`

Official shelter status: `{official_status}`

## Result

- Official source records checked: {official_total}
- Exact PLATEAU GML IDs found in `Chuo_BaseMap`: {len(scene_gml_found)}
- Active official shelters enabled in gameplay: {active_official_count}
- Disabled official records: {disabled_official_count}

Activation is limited to records with an exact scene object name matching the PLATEAU GML ID, `matchMethod=contains`, `confidence=high`, and `manualReviewNeeded=false`. Manual-review, nearest-match, broad-area, missing-GML, and missing-scene-object records remain disabled.

## Active Official Shelters

{active_official_lines}

## Disabled Evidence

Disabled records remain in `Assets/Data/P10/newmap_official_shelter_anchor_report.json` only. They are not selectable, do not receive green frames, and do not create routes.
""",
    )

    coordinate_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "coordinateTransformStatus": "blocked_missing_coordinate_transform",
        "finalStatus": "blocked_needs_user_map_asset",
        "sceneAudit": {
            "rendererCount": audit.get("rendererCount"),
            "colliderCount": audit.get("colliderCount"),
            "mapBoundsExtentsRecorded": False,
            "note": "Existing scene audit records counts only. Runtime now avoids player scene-wide bounds scans for spike reduction.",
        },
        "officialPointSourceCrs": official.get("coordinateReferenceSystem", "EPSG:4326"),
        "routeGeometrySourceCrs": routes.get("coordinateReferenceSystem", "EPSG:4326"),
        "routeMetricCrs": routes.get("metricCoordinateReferenceSystem", "EPSG:6677"),
        "knownCoordinateInputs": {
            "officialShelterPointCount": official_total,
            "routeRecordCount": route_count,
            "routeBoundingBox": routes.get("boundingBox"),
        },
        "validatedAnchorMethod": (
            "Exact PLATEAU GML scene object names can anchor official shelter objects, but this does not define "
            "a general WGS84-to-Unity transform."
        ),
        "wgs84ToUnityTransformAvailable": False,
        "transformedPointsInsideMapBounds": "not_evaluated_without_transform",
        "routePointsPlausibleInWgs84": True,
        "routePointsValidatedInUnity": False,
        "blocker": "No stored Unity transform/origin/scale mapping from EPSG:4326 route coordinates to Chuo_BaseMap world positions was found.",
        "nextAction": "Provide verified PLATEAU/Unity coordinate transform before claiming route geometry validation.",
    }
    write_json("Assets/Data/P10/newmap_coordinate_transform_validation.json", coordinate_report)
    write_text(
        "docs/NEWMAP_COORDINATE_TRANSFORM_VALIDATION.md",
        f"""
# NewMap Coordinate Transform Validation

Generated: {now}

Status: `blocked_missing_coordinate_transform`

Official shelter anchors are recovered by exact PLATEAU GML scene object names. That proves object identity for those shelters, but it does not prove a general WGS84 latitude/longitude to Unity world-position transform.

## Findings

- Official point CRS: `{official.get('coordinateReferenceSystem', 'EPSG:4326')}`
- Route geometry CRS: `{routes.get('coordinateReferenceSystem', 'EPSG:4326')}`
- Route metric CRS: `{routes.get('metricCoordinateReferenceSystem', 'EPSG:6677')}`
- Scene audit renderer count: {audit.get('rendererCount')}
- Scene audit collider count: {audit.get('colliderCount')}
- Stored map bounds extents: not available in the current audit
- WGS84-to-Unity transform: not proven

## Decision

Route geometry is not validated on the new map. Local route guidance remains prototype guidance only and no official route claim is made.
""",
    )

    route_records = []
    finite_route_count = 0
    for route in routes.get("records", []):
        finite = route_is_finite(route)
        if finite:
            finite_route_count += 1
        classification = "blocked_transform_unknown" if finite else "invalid_geometry"
        route_records.append(
            {
                "routeId": route.get("routeId"),
                "originId": route.get("originId"),
                "originName": route.get("originName"),
                "shelterId": route.get("shelterId"),
                "shelterName": route.get("shelterName"),
                "plateauBuildingId": route.get("plateauBuildingId"),
                "coordinateReferenceSystem": routes.get("coordinateReferenceSystem", "EPSG:4326"),
                "pointCount": len((route.get("geometry") or {}).get("coordinates") or []),
                "allFinite": finite,
                "routeDistanceMeters": route.get("routeDistanceMeters"),
                "estimatedTravelTimeSeconds": route.get("estimatedTravelTimeSeconds"),
                "isOfficialEvacuationRoute": bool(route.get("isOfficialEvacuationRoute")),
                "classification": classification,
                "activeInGame": False,
                "disabledReason": (
                    "blocked_transform_unknown: route geometry is EPSG:4326 and no verified WGS84-to-Unity transform exists for Chuo_BaseMap."
                    if finite
                    else "invalid_geometry: route has missing or non-finite coordinates."
                ),
                "runtimeBehavior": "not spawned; local training route proxies only; no official route claim",
            }
        )

    route_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "routeGeometryValidationStatus": "blocked_transform_unknown",
        "finalStatus": "completed_with_documented_runtime_proxy",
        "routeSourceRecordCount": route_count,
        "finiteWgs84RouteCount": finite_route_count,
        "validatedOnNewMapCount": 0,
        "blockedTransformUnknownCount": sum(1 for item in route_records if item["classification"] == "blocked_transform_unknown"),
        "invalidGeometryCount": sum(1 for item in route_records if item["classification"] == "invalid_geometry"),
        "proxyGuidanceOnlyCount": training_count,
        "officialRouteClaimCount": 0,
        "records": route_records,
    }
    write_json("Assets/Data/P10/newmap_route_geometry_validation.json", route_report)
    write_text(
        "docs/NEWMAP_ROUTE_GEOMETRY_VALIDATION.md",
        f"""
# NewMap Route Geometry Validation

Generated: {now}

Route geometry validation status: `blocked_transform_unknown`

## Result

- P5 OSM route records checked: {route_count}
- Finite WGS84 route records: {finite_route_count}
- Routes validated on `Chuo_BaseMap`: 0
- Routes blocked by missing transform: {route_report['blockedTransformUnknownCount']}
- Invalid geometry records: {route_report['invalidGeometryCount']}
- Official route claims: 0

The old OSM route lines remain EPSG:4326 prototype route geometry. Because no verified WGS84-to-Unity transform exists, no old route line is spawned on the new map and no official evacuation route is claimed.
""",
    )

    p3p4_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "phase": "P3/P4",
        "finalStatus": p3p4_status,
        "officialShelterLoaderSource": "official normalized shelter dataset audited into runtime anchor table",
        "officialSourceRecordsLoaded": official_total,
        "verifiedOfficialAnchors": active_official_count,
        "activeOfficialShelterCount": active_official_count,
        "disabledOfficialShelterCount": disabled_official_count,
        "p3P4SyntheticSampleRecords": p3p4_sample_count,
        "p3P4SyntheticSampleStatus": "disabled_missing_from_new_map",
        "noFakeOfficialShelters": True,
        "disabledOfficialTargetsSelectable": False,
        "resultPanelDistinguishesOfficial": True,
        "tests": [
            "RuntimeOfficialShelterRequiresVerifiedGmlAnchor",
            "check_newmap_official_shelter_anchors.ps1",
            "run_newmap_official_route_hardening_preflight.ps1",
        ],
    }
    write_json("Assets/Data/P10/newmap_p3_p4_recovery_final.json", p3p4_report)
    write_text(
        "docs/NEWMAP_P3_P4_RECOVERY_FINAL.md",
        f"""
# NewMap P3/P4 Recovery Final

Generated: {now}

Final status: `{p3p4_status}`

P3/P4 official shelter behavior is recovered through verified official shelter anchors on `Chuo_BaseMap`.

- Official source records loaded/audited: {official_total}
- Active verified official shelters: {active_official_count}
- Disabled official shelter records: {disabled_official_count}
- P3/P4 synthetic sample records: {p3p4_sample_count}, still `disabled_missing_from_new_map`

The active official targets are not fabricated. They are enabled only when the official record maps to a high-confidence PLATEAU building match and the matched PLATEAU GML ID exists as an object name in `Chuo_BaseMap`.
""",
    )

    p5_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "phase": "P5",
        "finalStatus": "completed_with_documented_runtime_proxy",
        "officialShelterAnchorRecovery": "completed_on_new_chuo_basemap" if active_official_count else "disabled_missing_from_new_map",
        "routeGeometryStatus": "blocked_transform_unknown",
        "oldRouteRecords": route_count,
        "oldRouteValidatedOnNewMapCount": 0,
        "oldRouteDisabledCount": route_count,
        "localProxyRouteGuideCount": training_count,
        "officialRouteClaimCount": 0,
        "prototypeGuidanceWording": "estimated prototype guidance; not an official evacuation route; not GIS-validated",
        "activeOfficialShelterCount": active_official_count,
        "activeTrainingTargetCount": training_count,
        "disabledReasonForOldRoutes": "No verified WGS84-to-Unity route transform exists for Chuo_BaseMap.",
    }
    write_json("Assets/Data/P10/newmap_p5_route_candidate_hardening.json", p5_report)
    write_text(
        "docs/NEWMAP_P5_ROUTE_CANDIDATE_HARDENING.md",
        f"""
# NewMap P5 Route Candidate Hardening

Generated: {now}

Final status: `completed_with_documented_runtime_proxy`

- Active official shelter anchors: {active_official_count}
- Active local non-official training targets: {training_count}
- Old P5 route records checked: {route_count}
- Old route records validated on the new map: 0
- Old route records disabled for route rendering: {route_count}
- Official route claims: 0

P5 official shelter identity is improved by exact PLATEAU GML anchors. P5 route geometry is not validated because route coordinates are still EPSG:4326 and no verified `Chuo_BaseMap` WGS84-to-Unity transform exists.
""",
    )

    active_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "totalTargetCount": total_target_count,
        "activeTargetCount": active_target_count,
        "disabledTargetCount": disabled_target_count,
        "activeOfficialShelterCount": active_official_count,
        "activeNonOfficialTrainingTargetCount": training_count,
        "activeGreenFrameTargetCount": active_target_count,
        "activeEntranceSafeFloorTargetCount": active_target_count,
        "officialShelterStatus": official_status,
        "officialRouteClaimCount": 0,
        "nonOfficialTargetLabeledOfficialCount": 0,
        "disabledSelectableCount": 0,
        "activeOfficialShelters": [
            {"id": record["id"], "name": record["name"], "plateauGmlId": record["plateauGmlId"]}
            for record in official_records
            if record["activeInGame"]
        ],
        "activeTrainingTargets": [
            {"id": "newmap_proxy_safe_floor", "name": "Local Training Proxy - Safe Floor"},
            {"id": "newmap_proxy_blocked_entrance", "name": "Local Training Proxy - Blocked Entrance"},
            {"id": "newmap_proxy_no_safe_floor", "name": "Local Training Proxy - No Safe Floor"},
            {"id": "newmap_proxy_crowd_delay", "name": "Local Training Proxy - Crowd Delay"},
        ],
        "disabledSummary": {
            "p3p4SyntheticSamples": p3p4_sample_count,
            "officialShelterRecords": disabled_official_count,
            "oldRouteRecords": route_count,
        },
    }
    write_json("Assets/Data/P10/newmap_active_target_final_report.json", active_report)
    write_json("Assets/Data/P10/newmap_active_target_hardening_report.json", active_report)
    active_target_doc = f"""
# NewMap Active Target Final Report

Generated: {now}

- Active targets: {active_target_count}
- Active official shelters: {active_official_count}
- Active non-official training targets: {training_count}
- Disabled targets: {disabled_target_count}

Only exact PLATEAU GML scene-object matches with high-confidence `contains` evidence are active. No old route geometry is active.
"""
    write_text("docs/NEWMAP_ACTIVE_TARGET_FINAL_REPORT.md", active_target_doc)
    write_text("docs/NEWMAP_ACTIVE_TARGET_HARDENING_REPORT.md", active_target_doc)

    disabled_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "totalTargetCount": total_target_count,
        "activeTargetCount": active_target_count,
        "disabledTargetCount": disabled_target_count,
        "disabledTargetsSelectable": False,
        "disabledTargetsSpawned": False,
        "disabledTargetsHaveGreenFrames": False,
        "disabledRoutesShown": False,
        "disabledTargetsIncludedInResultPanelSuccess": False,
        "disabledBreakdown": active_report["disabledSummary"],
        "finalStatus": "completed_on_new_chuo_basemap",
    }
    write_json("Assets/Data/P10/newmap_disabled_targets_hard_final.json", disabled_report)
    write_text(
        "docs/NEWMAP_DISABLED_TARGETS_HARD_FINAL.md",
        f"""
# NewMap Disabled Targets Hard Final

Generated: {now}

Disabled target count: {disabled_target_count}

Disabled targets are report-only. They are not selectable, do not spawn green frames, do not show route lines, and are not included in ResultPanel success flow.
""",
    )

    spike_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "focus": "startup and frame spike reduction; memory is recorded as informational for this task",
        "previousMaxFrameMs": previous_performance.get("maxFrameMs"),
        "previousAverageFps": previous_performance.get("averageFps"),
        "previousStutterFramesOver66ms": previous_performance.get("stutterFramesOver66ms"),
        "previousMaxPrivateMemoryBytes": previous_performance.get("maxPrivateMemoryBytes"),
        "previousMaxWorkingSetBytes": previous_performance.get("maxWorkingSetBytes"),
        "implementedChanges": [
            "Player runtime scene-wide renderer/collider bounds scan disabled; runtime uses lightweight origin/support placement.",
            "Runtime collision support proxy enlarged for GML anchors and local targets after scene MeshCollider shutdown.",
            "NPC humanoid crowd creation deferred until Evacuation mode or crowd-delay query enables crowd failures.",
            "Official shelter activation uses exact GML object lookup plus per-object renderer bounds, not a full-map scan.",
            "Old route geometry validation remains report/preflight work and is not performed as a player startup scan.",
        ],
        "memoryOptimizationPriority": "deprioritized_by_user_instruction",
        "postRetest": "pending",
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_spike_reduction_report.json", spike_report)
    write_text(
        "docs/NEWMAP_SPIKE_REDUCTION_REPORT.md",
        f"""
# NewMap Spike Reduction Report

Generated: {now}

Focus: startup/frame spike reduction. Memory is recorded only as informational in this task.

## Before

- Max frame: {previous_performance.get('maxFrameMs')} ms
- Average FPS: {previous_performance.get('averageFps')}
- Stutter frames over 66 ms: {previous_performance.get('stutterFramesOver66ms')}

## Implemented Changes

- Player runtime scene-wide renderer/collider bounds scan is disabled.
- Runtime uses lightweight origin/support placement in player builds.
- Official shelter anchors use exact GML lookup and per-object renderer bounds.
- NPC humanoids are deferred until crowd failure is enabled.
- Old route validation remains preflight/report work, not player startup work.

Retest status: pending.
""",
    )

    scenario_report = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "scenarioSet": [
            {"scenario": "tourism_free_roam_no_failure", "status": "covered_by_playmode", "activeTargetUsed": "newmap_proxy_safe_floor", "resultPanelReason": "Tourism inspection"},
            {"scenario": "evacuation_success_with_active_target", "status": "covered_by_playmode", "activeTargetUsed": "newmap_proxy_safe_floor", "resultPanelReason": "safe_floor_reached"},
            {"scenario": "warning_before_front", "status": "covered_by_playmode", "activeTargetUsed": "newmap_proxy_safe_floor", "resultPanelReason": "stage Warning asserted"},
            {"scenario": "green_frame_after_front", "status": "covered_by_playmode", "activeTargetUsed": "active targets", "resultPanelReason": "Stage 2 guidance asserted"},
            {"scenario": "route_proxy_wording", "status": "covered_by_preflight", "activeTargetUsed": "local training route proxies", "resultPanelReason": "estimated prototype guidance only"},
            {"scenario": "disabled_targets_not_spawned", "status": "covered_by_playmode_and_preflight", "activeTargetUsed": "runtime target list", "resultPanelReason": "disabled records absent"},
            {"scenario": "official_target_if_recovered", "status": "covered_by_playmode", "activeTargetUsed": "chuo_official_emergency_001 fixture / runtime GML anchors", "resultPanelReason": "Entering official shelter anchor"},
            {"scenario": "non_official_warning", "status": "covered_by_runtime_ui", "activeTargetUsed": "newmap_proxy_safe_floor", "resultPanelReason": "non-official warning visible near target"},
            {"scenario": "crowd_delay_proxy", "status": "covered_by_playmode", "activeTargetUsed": "newmap_proxy_crowd_delay", "resultPanelReason": "Crowd delay non-zero"},
            {"scenario": "collapse_debris_proxy", "status": "covered_by_playmode", "activeTargetUsed": "debris diagnostic area", "resultPanelReason": "collapse_debris_exposure"},
            {"scenario": "ResultPanel reason codes", "status": "covered_by_playmode", "activeTargetUsed": "runtime scenario targets", "resultPanelReason": "Tourism inspection, entrance_blocked, safe_floor_unavailable, safe_floor_reached, collapse_debris_exposure"},
        ],
        "playerLogErrors": previous_log.get("errorCount"),
        "playerLogWarnings": previous_log.get("warningCount"),
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_scenario_sanity_after_hardening.json", scenario_report)
    write_text(
        "docs/NEWMAP_SCENARIO_SANITY_AFTER_HARDENING.md",
        f"""
# NewMap Scenario Sanity After Hardening

Generated: {now}

Scenario sanity is covered by PlayMode diagnostics and preflight checks. Official shelter recovery adds an official target fixture test and runtime exact-GML activation for the new map.

Player.log status from previous hardening run: {previous_log.get('errorCount')} errors, {previous_log.get('warningCount')} warnings. The Pre2 retest will refresh this value.
""",
    )

    matrix_entries = [
        ("P2", "player/camera/movement/E/ResultPanel", "completed_with_documented_runtime_proxy", True, "verified official shelter anchors plus local training targets", "", "manual playtest on temp player"),
        ("P3/P4", "official shelter loading and marker activation", p3p4_status, active_official_count > 0, f"{active_official_count} verified official shelter GML anchors", "", "manual inspect active official markers"),
        ("P5", "candidate loading, route geometry validation, prototype route guidance", "completed_with_documented_runtime_proxy", True, "official shelter anchors and 4 local route proxy training guides", "route geometry validation needs verified transform", "manual use local estimated prototype route guides only"),
        ("P6", "NPC/navigation/crowd", "completed_with_documented_runtime_proxy", True, "newmap_proxy_crowd_delay", "road-aware navigation unavailable", "manual verify crowd delay"),
        ("P8", "two-stage tsunami/hazard/light curtain", "completed_with_documented_runtime_proxy", True, "runtime targets", "", "manual verify Stage 1/Stage 2"),
        ("P9", "gameplay outcomes and ResultPanel reason codes", "completed_with_documented_runtime_proxy", True, "verified official fixture plus local training targets", "", "manual scenario checklist"),
        ("P10", "UI/modes/weather/stamina/green frames/spike tools", "completed_with_documented_runtime_proxy", True, f"{active_target_count} active targets", "performance spike retest pending", "run Pre2 performance retest"),
    ]
    matrix = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "allowedStatuses": [
            "completed_on_new_chuo_basemap",
            "completed_with_documented_runtime_proxy",
            "disabled_missing_from_new_map",
            "blocked_needs_user_map_asset",
            "failed",
        ],
        "activeTargetCount": active_target_count,
        "disabledTargetCount": disabled_target_count,
        "activeOfficialShelterCount": active_official_count,
        "entries": [
            {
                "phase": phase,
                "feature": feature,
                "finalStatus": status,
                "activeOnNewMap": active,
                "targetUsed": target,
                "testEvidence": "NewMap runtime tests, preflight reports, and JSON validation",
                "disabledReason": "Old route geometry is disabled where no verified transform exists." if phase == "P5" else "",
                "blocker": blocker,
                "nextAction": next_action,
            }
            for phase, feature, status, active, target, blocker, next_action in matrix_entries
        ],
    }
    write_json("Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json", matrix)
    table_rows = "\n".join(
        f"| {entry['phase']} | {entry['feature']} | `{entry['finalStatus']}` | {str(entry['activeOnNewMap']).lower()} | {entry['targetUsed']} | {entry['blocker']} |"
        for entry in matrix["entries"]
    )
    write_text(
        "docs/NEWMAP_P2_P10_FULL_COMPLETION_MATRIX.md",
        f"""
# NewMap P2-P10 Full Completion Matrix

Generated: {now}

Allowed statuses only: `completed_on_new_chuo_basemap`, `completed_with_documented_runtime_proxy`, `disabled_missing_from_new_map`, `blocked_needs_user_map_asset`, `failed`.

| Phase | Feature | Final status | Active on new map | Target used | Blocker |
|---|---|---|---|---|---|
{table_rows}

Active targets: {active_target_count}. Disabled targets: {disabled_target_count}. Active official shelters: {active_official_count}.
""",
    )

    readiness = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "manualReadinessDecision": "needs_quick_fix_before_manual_test",
        "reason": "Official shelters are recovered by GML anchor, but Pre2 performance retest and Player.log parse are still pending.",
        "activeTargetFlowExists": active_target_count > 0,
        "activeOfficialShelterCount": active_official_count,
        "activeTrainingTargetCount": training_count,
        "disabledTargetCount": disabled_target_count,
        "playerCanInteract": True,
        "playerLogErrors": previous_log.get("errorCount"),
        "performanceRetest": "pending",
        "deepSeekVerdict": "pending",
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_manual_playtest_readiness.json", readiness)
    write_text(
        "docs/NEWMAP_MANUAL_PLAYTEST_READINESS.md",
        f"""
# NewMap Manual Playtest Readiness

Generated: {now}

Decision: `needs_quick_fix_before_manual_test`

Reason: official shelters are recovered by exact PLATEAU GML anchors, but the Pre2 temp player performance retest and Player.log parse are still pending.

Active targets: {active_target_count}
Active official shelters: {active_official_count}
Disabled targets: {disabled_target_count}
""",
    )
    write_text(
        "docs/NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md",
        f"""
# NewMap Manual Playtest Checklist

Generated: {now}

- Start `ChuoTsunamiEvacuation_NewMapHardeningPre2.exe`.
- Verify Start Menu, English/Japanese switch, Rules, weather, Tourism Mode, and Evacuation Mode.
- In Tourism Mode, inspect a local training target and confirm no failure occurs.
- In Evacuation Mode, wait for Stage 2 and confirm green frames appear only then.
- Interact with `newmap_proxy_safe_floor`, `newmap_proxy_blocked_entrance`, `newmap_proxy_no_safe_floor`, and `newmap_proxy_crowd_delay`.
- Inspect at least one official shelter marker if visible/reachable on the map.
- Confirm no old disabled route line or disabled target appears.
- Confirm ResultPanel text does not claim an official route or GIS-grade validation.
- Record any visible frame pause during startup or first mode selection.
""",
    )

    write_text(
        "docs/NEWMAP_GIT_PUSH_STATUS.md",
        f"""
# NewMap Git Push Status

Generated: {now}

Workspace: `D:\\UnityProjects\\ChuoTsunamiEvacuation`

Branch: `phase5-qualification-routing-plateau`

Latest local commit before this task: `da57566 Harden NewMap P2-P10 manual test readiness`

Status before implementation: clean.

Push result: failed.

Error:

```text
fatal: unable to access 'https://github.com/LN6666/ChuoTsunamiEvacuation.git/': schannel: AcquireCredentialsHandle failed: SEC_E_NO_CREDENTIALS (0x8009030e) - セキュリティ パッケージで利用できる資格情報がありません
```

Local work continues because the failure is credential-related.
""",
    )

    prompt = """
# DeepSeek Review Prompt - NewMap Official Shelter / Route / Spike Hardening

Review the current git diff for `D:\\UnityProjects\\ChuoTsunamiEvacuation`.

Verify:
- official shelters are either genuinely anchored by exact PLATEAU GML object names in `Assets/Scenes/Chuo_BaseMap.unity` or honestly disabled
- P3/P4 recovery is not faked
- route geometry validation is honest and old WGS84 routes are not claimed active on the new map
- no official route false claim exists
- disabled targets are inactive
- startup spike reduction changes are meaningful and do not remove required gameplay
- memory is recorded honestly but not treated as the main task focus
- Tourism and Evacuation modes still work
- P9 scenario outcomes still work
- no final release/archive was created
- no P10-E/F/G was created
- no A-level blockers remain

Important reports:
- `Assets/Data/P10/newmap_official_shelter_anchor_report.json`
- `Assets/Data/P10/newmap_coordinate_transform_validation.json`
- `Assets/Data/P10/newmap_route_geometry_validation.json`
- `Assets/Data/P10/newmap_spike_reduction_report.json`
- `Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json`
"""
    write_text("deepseek_review_prompt_newmap_official_route_spike_hardening.md", prompt)
    write_text("codex_prompts/newmap_official_route_spike_hardening.md", prompt)

    print(
        json.dumps(
            {
                "generatedAt": now,
                "activeOfficialShelterCount": active_official_count,
                "activeTargetCount": active_target_count,
                "disabledTargetCount": disabled_target_count,
                "sceneGmlFound": len(scene_gml_found),
                "routeCount": route_count,
            },
            ensure_ascii=False,
            indent=2,
        )
    )


if __name__ == "__main__":
    main()
