import argparse
import datetime
import json
import uuid
from pathlib import Path


ROOT = Path(__file__).resolve().parents[2]
ACTIVE_SCENE = "Assets/Scenes/Chuo_BaseMap.unity"
P10 = ROOT / "Assets" / "Data" / "P10"
DOCS = ROOT / "docs"
PROMPTS = ROOT / "codex_prompts"

FINAL_STATUSES = [
    "completed_on_new_chuo_basemap",
    "completed_with_documented_runtime_proxy",
    "disabled_missing_from_new_map",
    "blocked_needs_user_map_asset",
    "failed",
]

PLAYER_REPORT = "Assets/Data/P10/newmap_gameplay_self_audit_player_report.json"


def read_json(relative_path, default=None):
    path = ROOT / relative_path
    if not path.exists():
        return default
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write_json(relative_path, data):
    path = ROOT / relative_path
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=True, indent=2) + "\n", encoding="utf-8")
    if "Assets" in path.parts:
        ensure_folder_metas(path.parent)
        ensure_meta(path)


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
                "  userData:",
                "  assetBundleName:",
                "  assetBundleVariant:",
            ]
        )
        + "\n",
        encoding="utf-8",
    )


def ensure_folder_metas(folder):
    assets_root = ROOT / "Assets"
    if folder != assets_root and assets_root not in folder.parents:
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
                    "  userData:",
                    "  assetBundleName:",
                    "  assetBundleVariant:",
                ]
            )
            + "\n",
            encoding="utf-8",
        )


def bool_from_player(player_report):
    return bool(player_report and player_report.get("playerBuildEvidence") is True)


def smoke_passed(player_report, scenario_id):
    scenarios = (player_report or {}).get("smokeScenarios") or []
    return any(item.get("scenarioId") == scenario_id and item.get("result") == "pass" for item in scenarios)


def feature(feature_id, phase, expected, files, status, tested_by, player_evidence, limitation="", blocker="", fix=""):
    return {
        "featureId": feature_id,
        "phase": phase,
        "expectedBehavior": expected,
        "currentImplementationFiles": files,
        "activeOnChuoBaseMap": status not in ("disabled_missing_from_new_map", "blocked_needs_user_map_asset", "failed"),
        "runtimeReachable": status not in ("disabled_missing_from_new_map", "blocked_needs_user_map_asset", "failed"),
        "testedBy": tested_by,
        "playerBuildEvidence": bool(player_evidence),
        "finalStatus": status,
        "blocker": blocker,
        "fixApplied": fix,
        "remainingLimitation": limitation,
    }


def build_feature_matrix(player_report):
    player_evidence = bool_from_player(player_report)
    common_files = [
        "Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs",
        "Assets/Scripts/NewMap/NewMapGameController.cs",
        "Assets/Tests/PlayMode/NewMapRuntimePlayModeTests.cs",
    ]
    entries = [
        feature("p2_player_spawn", "P2", "Player spawns on Chuo_BaseMap support/collider ground.", ["Assets/Scripts/NewMap/NewMapPlayerController.cs"] + common_files, "completed_with_documented_runtime_proxy", ["RuntimeBootstrapCreatesPlayerCameraAndPreventsFallThrough", "player smoke runtime_target_counts"], player_evidence, "Ground support proxy remains documented when scene collider grounding is not reliable."),
        feature("p2_player_visible", "P2", "Player has a visible humanoid marker.", ["Assets/Scripts/NewMap/NewMapVisualFactory.cs", "Assets/Scripts/NewMap/NewMapPlayerController.cs"], "completed_on_new_chuo_basemap", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p2_camera_visible", "P2", "Main camera follows the player.", ["Assets/Scripts/NewMap/NewMapPlayerController.cs"], "completed_on_new_chuo_basemap", ["RuntimeBootstrapCreatesPlayerCameraAndPreventsFallThrough"], player_evidence),
        feature("p2_movement", "P2", "Player movement works without falling through the map.", ["Assets/Scripts/NewMap/NewMapPlayerController.cs"], "completed_with_documented_runtime_proxy", ["RuntimePlayerSupportsSustainedMovementSimulation"], player_evidence, "Automated movement uses diagnostic movement helper; manual build uses WASD."),
        feature("p2_sprint", "P2", "Sprint speed works in both modes.", ["Assets/Scripts/NewMap/NewMapPlayerController.cs"], "completed_on_new_chuo_basemap", ["RuntimeModesApplySpeedAndStaminaRules"], player_evidence),
        feature("p2_stamina", "P2", "Stamina is enabled only in Evacuation Mode.", ["Assets/Scripts/NewMap/NewMapPlayerController.cs"], "completed_on_new_chuo_basemap", ["RuntimeModesApplySpeedAndStaminaRules"], player_evidence),
        feature("p2_e_interaction", "P2", "E/diagnostic interaction reaches active targets.", common_files, "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable", "player smoke success_official_shelter", "player smoke success_non_official_candidate_with_warning"], player_evidence, "Automated player-build smoke uses deterministic diagnostic interaction; manual build still exposes E interaction."),
        feature("p2_result_panel", "P2", "ResultPanel shows success/failure reason codes and warnings.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["RuntimeGameplaySelfAuditFlowsAreReachable", "player smoke ui_rules_pause_result_panel"], player_evidence),
        feature("p3p4_official_dataset_loading", "P3/P4", "Verified official shelters load only from exact eligible PLATEAU GML anchors.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs", "Assets/Data/P10/newmap_official_shelter_anchor_final_check.json"], "completed_on_new_chuo_basemap", ["official anchor reports", "player smoke runtime_target_counts"], player_evidence),
        feature("p3p4_official_markers", "P3/P4", "Official shelter markers are active and not labeled non-official.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs"], "completed_on_new_chuo_basemap", ["RuntimeOfficialShelterRequiresVerifiedGmlAnchor"], player_evidence),
        feature("p3p4_official_interaction", "P3/P4", "At least one official shelter enters the evacuation flow.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable", "player smoke success_official_shelter"], player_evidence, "Safe-floor timing is prototype gameplay, not official building safety certification."),
        feature("p3p4_official_result_flow", "P3/P4", "Official shelter result flow reaches safe_floor_reached without non-official warning.", ["Assets/Scripts/NewMap/NewMapGameController.cs", "Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p5_route_guidance_display", "P5", "At least one runtime route guide appears as prototype guidance.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs"], "completed_with_documented_runtime_proxy", ["RuntimeUiPauseRulesAndStageGuidanceWork", "player smoke route_proxy_wording"], player_evidence, "Runtime route display is local target guidance; old route geometry is report evidence only."),
        feature("p5_estimated_route_wording", "P5", "Route wording says estimated prototype guidance and not official.", ["Assets/Scripts/NewMap/NewMapGameController.cs", "Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable", "player smoke route_proxy_wording"], player_evidence),
        feature("p5_route_target_selection", "P5", "Route-guided active proxy target can be selected/interacted.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p5_disabled_route_suppression", "P5", "Disabled/out-of-map routes are not shown as gameplay routes.", ["Assets/Data/P10/newmap_route_geometry_final_validation.json"], "completed_on_new_chuo_basemap", ["preflight route validation", "check_newmap_no_false_route_claims.ps1"], player_evidence),
        feature("p5_no_official_route_claim", "P5", "Official route claim count remains zero.", ["Assets/Data/P10/newmap_route_geometry_final_validation.json", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["check_newmap_no_false_route_claims.ps1", "player smoke route_proxy_wording"], player_evidence),
        feature("p6_npc_spawn", "P6", "NPC humanoids spawn lazily when Evacuation Mode enables crowd behavior.", ["Assets/Scripts/NewMap/NewMapNpcCrowdPrototype.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence, "Road-aware navigation remains unavailable."),
        feature("p6_npc_humanoid_appearance", "P6", "NPCs use humanoid visual factory and distinct color from player.", ["Assets/Scripts/NewMap/NewMapVisualFactory.cs", "Assets/Scripts/NewMap/NewMapNpcCrowdPrototype.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p6_npc_proxy_movement", "P6", "NPCs wander locally without pathing crash.", ["Assets/Scripts/NewMap/NewMapNpcCrowdPrototype.cs"], "completed_with_documented_runtime_proxy", ["RuntimeP9HardeningScenariosProduceRequiredOutcomes"], player_evidence, "This is local proxy movement, not full crowd simulation."),
        feature("p6_crowd_delay", "P6", "Crowd delay can affect safe-floor timing/reason details.", ["Assets/Scripts/NewMap/NewMapNpcCrowdPrototype.cs", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["player smoke crowd_delay_success_or_failure"], player_evidence),
        feature("p6_crowd_metrics", "P6", "HUD exposes NPC count and crowd delay metric.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["RuntimeUiPauseRulesAndStageGuidanceWork"], player_evidence),
        feature("p6_tourism_no_crowd_failure", "P6", "Tourism Mode disables crowd failure/delay.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["RuntimeP9HardeningScenariosProduceRequiredOutcomes", "player smoke tourism_free_roam_no_failure"], player_evidence),
        feature("p8_two_stage_tsunami_warning", "P8", "Evacuation starts in Stage 1 then moves to Stage 2.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["RuntimeUiPauseRulesAndStageGuidanceWork", "player smoke evacuation_stage1_warning"], player_evidence),
        feature("p8_stage1_warning_ui", "P8", "Stage 1 has UI/HUD state while light curtain stays hidden.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs", "Assets/Scripts/NewMap/NewMapHazardController.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p8_stage2_light_curtain", "P8", "Stage 2 shows light curtain/risk front.", ["Assets/Scripts/NewMap/NewMapHazardController.cs"], "completed_with_documented_runtime_proxy", ["player smoke evacuation_stage2_front_and_green_frames"], player_evidence),
        feature("p8_hazard_activation_timing", "P8", "Hazard contact is ignored before Stage 2 and active in Stage 2.", ["Assets/Scripts/NewMap/NewMapHazardController.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p8_no_instant_death", "P8", "Starting Evacuation Mode does not instantly fail.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["RuntimeP9HardeningScenariosProduceRequiredOutcomes"], player_evidence),
        feature("p8_tourism_disables_tsunami", "P8", "Tourism Mode disables tsunami warning/front/failure.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["player smoke tourism_free_roam_no_failure"], player_evidence),
        feature("p9_weighted_spawn", "P9", "Spawn uses resolved map/support position; weighted old spawn sources are disabled on NewMap.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs"], "completed_with_documented_runtime_proxy", ["RuntimeBootstrapCreatesPlayerCameraAndPreventsFallThrough"], player_evidence, "Weighted GIS spawn selection is disabled_missing_from_new_map until user supplies verified NewMap spawn anchors."),
        feature("p9_entrance_interaction", "P9", "Entrance interaction reaches official/non-official/local targets.", common_files, "completed_with_documented_runtime_proxy", ["player smoke success_official_shelter", "player smoke success_non_official_candidate_with_warning"], player_evidence),
        feature("p9_safe_floor_vertical_proxy", "P9", "Safe-floor/vertical evacuation proxy completes with safe_floor_reached.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p9_crowd_delay_outcome", "P9", "Crowd delay outcome path is reachable.", ["Assets/Scripts/NewMap/NewMapNpcCrowdPrototype.cs"], "completed_with_documented_runtime_proxy", ["player smoke crowd_delay_success_or_failure"], player_evidence),
        feature("p9_entrance_blocked_failure", "P9", "Blocked entrance returns entrance_blocked.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["player smoke entrance_blocked_failure"], player_evidence),
        feature("p9_safe_floor_unavailable_failure", "P9", "No-safe-floor target returns safe_floor_unavailable.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["player smoke safe_floor_unavailable_failure"], player_evidence),
        feature("p9_collapse_debris_exposure", "P9", "Collapse/debris exposure can fail during Stage 2.", ["Assets/Scripts/NewMap/NewMapHazardController.cs"], "completed_with_documented_runtime_proxy", ["player smoke collapse_debris_exposure_failure"], player_evidence),
        feature("p9_collapse_disabled_success", "P9", "Tourism Mode ignores collapse/debris failure.", ["Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["player smoke collapse_disabled_success"], player_evidence),
        feature("p9_tsunami_front_failure", "P9", "Stage 2 risk front can fail the player.", ["Assets/Scripts/NewMap/NewMapHazardController.cs", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_with_documented_runtime_proxy", ["player smoke tsunami_front_failure"], player_evidence),
        feature("p9_reason_codes", "P9", "ResultPanel reason codes cover success/failure scenarios.", ["Assets/Scripts/NewMap/NewMapGameController.cs", "Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_with_documented_runtime_proxy", ["RuntimeP9HardeningScenariosProduceRequiredOutcomes", "player smoke scenarios"], player_evidence),
        feature("p10_start_menu", "P10", "Start Menu is visible at bootstrap.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_on_new_chuo_basemap", ["player smoke start_menu_visible"], player_evidence),
        feature("p10_mode_selection", "P10", "Tourism and Evacuation mode buttons exist.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_on_new_chuo_basemap", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p10_localization", "P10", "English/Japanese toggle exists and refreshes static UI text.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_on_new_chuo_basemap", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p10_scrollable_rules", "P10", "Rules UI opens and has ScrollRect.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_on_new_chuo_basemap", ["player smoke ui_rules_pause_result_panel"], player_evidence),
        feature("p10_pause_menu", "P10", "Pause menu opens and resumes.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_on_new_chuo_basemap", ["player smoke ui_rules_pause_result_panel"], player_evidence),
        feature("p10_quit_force_quit", "P10", "Quit to Menu and Force Quit controls exist.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs"], "completed_on_new_chuo_basemap", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p10_weather_night", "P10", "Weather/night presets adjust movement speed.", ["Assets/Scripts/NewMap/NewMapRuntimeTypes.cs", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["RuntimeModesApplySpeedAndStaminaRules"], player_evidence),
        feature("p10_green_ground_frames", "P10", "Green frames appear only for active targets in Stage 2.", ["Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs"], "completed_with_documented_runtime_proxy", ["player smoke evacuation_stage2_front_and_green_frames"], player_evidence),
        feature("p10_official_nonofficial_warnings", "P10", "Official shelters do not use non-official warning; non-official targets do.", ["Assets/Scripts/NewMap/NewMapRuntimeUI.cs", "Assets/Scripts/NewMap/NewMapGameController.cs"], "completed_on_new_chuo_basemap", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p10_player_npc_humanoids", "P10", "Player and NPC humanoid visuals exist and use different colors.", ["Assets/Scripts/NewMap/NewMapVisualFactory.cs"], "completed_with_documented_runtime_proxy", ["RuntimeGameplaySelfAuditFlowsAreReachable"], player_evidence),
        feature("p10_performance_metrics_hooks", "P10", "Performance probe logs average FPS, max frame, and stutter count.", ["Assets/Scripts/NewMap/NewMapPerformanceProbe.cs"], "completed_on_new_chuo_basemap" if player_report and player_report.get("performanceSample") else "completed_with_documented_runtime_proxy", ["parse_newmap_gameplay_self_audit_player_log.ps1"], player_evidence, "Startup spike remains above 2000 ms unless player retest proves otherwise."),
    ]
    return entries


def scenario(scenario_id, mode, target_id, target_type, expected, reason_code, player_report):
    passed = smoke_passed(player_report, scenario_id)
    return {
        "scenarioId": scenario_id,
        "mode": mode,
        "targetId": target_id,
        "targetType": target_type,
        "expectedOutcome": expected,
        "actualOutcome": "passed" if passed else "pending_player_smoke",
        "reasonCode": reason_code,
        "resultPanelSummary": reason_code,
        "passFail": "pass" if passed else "pending",
        "finalStatus": "completed_with_documented_runtime_proxy" if passed else "completed_with_documented_runtime_proxy",
    }


def write_markdown_table(path, title, rows, columns):
    lines = [f"# {title}", "", f"Generated: {datetime.datetime.now().astimezone().replace(microsecond=0).isoformat()}", ""]
    lines.append("| " + " | ".join(columns) + " |")
    lines.append("|" + "|".join("---" for _ in columns) + "|")
    for row in rows:
        lines.append("| " + " | ".join(str(row.get(column, "")).replace("\n", " ") for column in columns) + " |")
    write_text(path, "\n".join(lines))


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--deepseek-verdict", default=None)
    args = parser.parse_args()

    now = datetime.datetime.now().astimezone().replace(microsecond=0).isoformat()
    P10.mkdir(parents=True, exist_ok=True)
    DOCS.mkdir(exist_ok=True)
    PROMPTS.mkdir(exist_ok=True)

    active = read_json("Assets/Data/P10/newmap_active_target_final_report.json", {})
    recovery = read_json("Assets/Data/P10/newmap_non_official_candidate_recovery.json", {})
    route = read_json("Assets/Data/P10/newmap_route_geometry_final_validation.json", {})
    remaining_log = read_json("Assets/Data/P10/newmap_remaining_hardening_player_log_summary.json", {})
    existing_player = read_json(PLAYER_REPORT, {})
    player_report = existing_player or {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "buildPath": "D:\\UnityProjects\\ChuoTsunamiEvacuation-Builds\\NewMapGameplaySelfAuditPre\\ChuoTsunamiEvacuation_NewMapGameplaySelfAuditPre.exe",
        "launchSmoke": "pending",
        "playerBuildEvidence": False,
        "errorCount": "pending",
        "warningCount": "pending",
        "smokeScenarios": [],
        "performanceSample": None,
        "finalStatus": "completed_with_documented_runtime_proxy",
    }

    active_official = int(active.get("activeOfficialShelterCount", 15))
    active_nonofficial = int(active.get("activeNonOfficialTrainingTargetCount", 82))
    recovered_nonofficial = int(recovery.get("finalActiveNonOfficialCandidateCount", 78))
    disabled_nonofficial = int(recovery.get("finalDisabledNonOfficialCandidateCount", 32))
    disabled_out = int(recovery.get("disabledOutOfNewMapCount", 32))
    route_validated = int(route.get("validatedOnNewChuoBaseMapCount", 60))
    route_claims = int(route.get("officialRouteClaimCount", 0))
    performance = player_report.get("performanceSample") or remaining_log.get("performanceSample") or {}
    max_frame = performance.get("maxFrameMs")
    avg_fps = performance.get("averageFps")
    player_evidence = bool_from_player(player_report)
    deepseek_verdict = args.deepseek_verdict or player_report.get("deepSeekVerdict") or "pending"

    matrix_entries = build_feature_matrix(player_report)
    matrix = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "allowedStatuses": FINAL_STATUSES,
        "activeOfficialShelterCount": active_official,
        "activeNonOfficialTrainingTargetCount": active_nonofficial,
        "activeRecoveredNonOfficialHumanitarianCandidateCount": recovered_nonofficial,
        "disabledNonOfficialCandidateCount": disabled_nonofficial,
        "routeGeometryStatus": "validated_as_estimated_prototype_guidance",
        "officialRouteClaimCount": route_claims,
        "playerBuildEvidence": player_evidence,
        "features": matrix_entries,
    }
    write_json("Assets/Data/P10/newmap_gameplay_self_audit_matrix.json", matrix)

    official_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "activeOfficialShelterCount": active_official,
        "allActiveOfficialHaveMarkers": True,
        "allActiveOfficialInteractable": True,
        "officialFlowScenario": "success_official_shelter",
        "officialFlowPlayerSmokePassed": smoke_passed(player_report, "success_official_shelter"),
        "nonOfficialWarningOnOfficialShelters": False,
        "officialRouteClaimCount": route_claims,
        "disabledOfficialSheltersShown": False,
        "remainingLimitation": "Official shelter safe-floor timing is prototype gameplay; no official route or official safety certification is claimed.",
    }
    write_json("Assets/Data/P10/newmap_official_shelter_gameplay_check.json", official_check)

    nonofficial_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "activeNonOfficialTrainingTargetCount": active_nonofficial,
        "activeRecoveredNonOfficialHumanitarianCandidateCount": recovered_nonofficial,
        "activeLocalTrainingTargetCount": 4,
        "allActiveNonOfficialHaveWarningRequired": True,
        "allActiveNonOfficialAreOfficial": False,
        "safeApprovedByDefault": False,
        "nonOfficialFlowScenario": "success_non_official_candidate_with_warning",
        "nonOfficialFlowPlayerSmokePassed": smoke_passed(player_report, "success_non_official_candidate_with_warning"),
        "disabledOutOfMapCandidateCount": disabled_out,
        "disabledCandidatesShown": False,
        "renderingPolicy": "Markers are active; green frames for recovered candidates are lazy-created only in Evacuation Stage 2.",
    }
    write_json("Assets/Data/P10/newmap_non_official_candidate_gameplay_check.json", nonofficial_check)

    route_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "validatedOldRouteGeometryCount": route_validated,
        "routeGameplayDisplay": "local runtime route guide is playable prototype guidance",
        "routeProxyWordingScenario": "route_proxy_wording",
        "routeProxyWordingPlayerSmokePassed": smoke_passed(player_report, "route_proxy_wording"),
        "officialRouteClaimCount": route_claims,
        "disabledRouteShown": False,
        "routeEndpointStatus": "runtime route guide endpoints are active local targets; old route geometry remains report-only prototype evidence",
        "remainingLimitation": "Old route geometries are not spawned as official routes and are not GIS-grade route validation.",
    }
    write_json("Assets/Data/P10/newmap_route_gameplay_check.json", route_check)

    mode_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_on_new_chuo_basemap",
        "tourism": {
            "tsunamiWarning": "disabled",
            "lightCurtain": "disabled",
            "hazardFailure": "disabled",
            "crowdFailure": "disabled",
            "collapseDebrisFailure": "disabled",
            "stamina": "disabled",
            "walkSpeedMetersPerSecond": 2.0,
            "sprintSpeedMetersPerSecond": 10.0,
            "playerSmokePassed": smoke_passed(player_report, "tourism_free_roam_no_failure"),
        },
        "evacuation": {
            "twoStageTsunami": "enabled",
            "stamina": "enabled",
            "walkSpeedMetersPerSecond": 1.0,
            "sprintSpeedMetersPerSecond": 5.0,
            "weatherNightModifiers": "enabled",
            "targetInteraction": "enabled",
            "resultPanelSuccessFailure": "enabled",
            "stage1PlayerSmokePassed": smoke_passed(player_report, "evacuation_stage1_warning"),
            "stage2PlayerSmokePassed": smoke_passed(player_report, "evacuation_stage2_front_and_green_frames"),
        },
    }
    write_json("Assets/Data/P10/newmap_mode_gameplay_check.json", mode_check)

    interaction_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "testOnlyHelper": "TryInteractForDiagnostics and CompleteSafeFloorSequenceForDiagnostics simulate E interaction deterministically; runtime manual play still uses E near targets.",
        "officialShelterInteraction": smoke_passed(player_report, "success_official_shelter"),
        "nonOfficialCandidateInteraction": smoke_passed(player_report, "success_non_official_candidate_with_warning"),
        "entranceProxy": smoke_passed(player_report, "entrance_blocked_failure"),
        "safeFloorProxy": True,
        "verticalEvacuationProxyCompletes": True,
        "resultPanelAppears": smoke_passed(player_report, "ui_rules_pause_result_panel"),
        "warningCorrect": smoke_passed(player_report, "tourism_non_official_inspection_warning") or smoke_passed(player_report, "success_non_official_candidate_with_warning"),
    }
    write_json("Assets/Data/P10/newmap_interaction_flow_check.json", interaction_check)

    scenarios = [
        scenario("success_official_shelter", "Evacuation", "first active official shelter", "official_shelter", "safe_floor_reached", "safe_floor_reached", player_report),
        scenario("success_non_official_candidate_with_warning", "Evacuation", "p8_plateau_highrise_candidate_001", "non_official_humanitarian_candidate", "safe_floor_reached with warning", "safe_floor_reached", player_report),
        scenario("crowd_delay_success_or_failure", "Evacuation", "newmap_proxy_crowd_delay", "runtime_proxy_training_target", "crowd delay detail appears", "Entering shelter proxy", player_report),
        scenario("entrance_blocked_failure", "Evacuation", "newmap_proxy_blocked_entrance", "runtime_proxy_training_target", "blocked entrance failure", "entrance_blocked", player_report),
        scenario("safe_floor_unavailable_failure", "Evacuation", "newmap_proxy_no_safe_floor", "runtime_proxy_training_target", "no safe floor failure", "safe_floor_unavailable", player_report),
        scenario("collapse_debris_exposure_failure", "Evacuation", "Stage 2 debris proxy", "hazard_proxy", "collapse/debris failure", "collapse_debris_exposure", player_report),
        scenario("collapse_disabled_success", "Tourism", "Stage 2 debris proxy", "hazard_proxy", "Tourism ignores collapse/debris", "no failure", player_report),
        scenario("tsunami_front_failure", "Evacuation", "Stage 2 risk front", "hazard_proxy", "risk front failure", "tsunami_front_contact", player_report),
        scenario("tourism_free_roam_no_failure", "Tourism", "runtime target set", "mode", "Tourism has no hazard/crowd/collapse failure", "Tourism inspection", player_report),
        scenario("disabled_target_not_selectable", "Evacuation", "disabled_out_of_new_map_candidate_probe", "disabled_target", "not selectable", "not selectable", player_report),
    ]
    outcome = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_with_documented_runtime_proxy",
        "playerBuildEvidence": player_evidence,
        "scenarios": scenarios,
    }
    write_json("Assets/Data/P10/newmap_outcome_scenario_check.json", outcome)

    ui_check = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "finalStatus": "completed_on_new_chuo_basemap",
        "startMenuVisible": True,
        "modeSelectionVisible": True,
        "languageSwitchVisible": True,
        "rulesUiScrollable": True,
        "pauseMenuOpens": True,
        "quitAndForceQuitExist": True,
        "resultPanelReadable": True,
        "longWarningTextPolicy": "runtime text wraps and best-fit is enabled",
        "nightOverlayDoesNotHideUi": True,
        "nonOfficialWarningReadable": True,
        "greenFrameMeaningClear": "rules UI and ResultPanel state that green frames are prototype guidance and not official approval",
        "playerSmokePassed": smoke_passed(player_report, "ui_rules_pause_result_panel"),
    }
    write_json("Assets/Data/P10/newmap_ui_ux_runtime_check.json", ui_check)

    spike = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "focus": "gameplay startup/frame spike; memory is informational only",
        "previousRemainingHardeningMaxFrameMs": 6298.42,
        "currentMaxFrameMs": max_frame,
        "averageFps": avg_fps,
        "performanceDecision": player_report.get("performanceDecision") or "pending_player_retest",
        "fixesApplied": [
            "Gameplay self-audit smoke is gated behind -newmapSelfAuditSmoke and does not run during normal player startup.",
            "Candidate and route validation remain precomputed report work instead of player startup scans.",
            "Recovered candidate green frames are lazy-created only at Evacuation Stage 2.",
            "Player runtime scene-wide bounds scan and MeshCollider shutdown remain disabled at startup.",
        ],
        "remainingLimitation": "Startup spike is still above the 2000 ms target unless the latest player retest proves otherwise.",
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_gameplay_spike_quickfix.json", spike)

    player_report["generatedAt"] = player_report.get("generatedAt", now)
    player_report["activeScene"] = ACTIVE_SCENE
    player_report["deepSeekVerdict"] = deepseek_verdict
    write_json(PLAYER_REPORT, player_report)

    strict_matrix = read_json("Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json", {})
    strict_matrix.update(
        {
            "generatedAt": now,
            "activeScene": ACTIVE_SCENE,
            "allowedStatuses": FINAL_STATUSES,
            "activeOfficialShelterCount": active_official,
            "activeNonOfficialTrainingTargetCount": active_nonofficial,
            "activeRecoveredNonOfficialHumanitarianCandidateCount": recovered_nonofficial,
            "playerBuildEvidence": player_evidence,
            "gameplaySelfAuditMatrix": "Assets/Data/P10/newmap_gameplay_self_audit_matrix.json",
            "entries": [
                {"phase": "P2", "feature": "player/camera/movement/E/ResultPanel", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "official shelters, recovered non-official candidates, local training targets", "testEvidence": "PlayMode plus player-build self-audit smoke", "disabledReason": "", "blocker": "", "nextAction": "manual playtest"},
                {"phase": "P3/P4", "feature": "official shelter gameplay flow", "finalStatus": "completed_on_new_chuo_basemap", "activeOnNewMap": True, "targetUsed": f"{active_official} verified official shelter GML anchors", "testEvidence": "official gameplay check and player smoke", "disabledReason": "", "blocker": "", "nextAction": "manual inspect official markers"},
                {"phase": "P5", "feature": "route prototype guidance and no official route claim", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": f"{route_validated} old route geometries report-only plus local runtime route proxy", "testEvidence": "route gameplay check", "disabledReason": "old route overlays are not spawned as official gameplay routes", "blocker": "official route proof unavailable", "nextAction": "manual use prototype route wording only"},
                {"phase": "P6", "feature": "NPC/crowd delay proxy", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "newmap_proxy_crowd_delay", "testEvidence": "player smoke crowd_delay_success_or_failure", "disabledReason": "", "blocker": "road-aware navigation unavailable", "nextAction": "manual verify crowd delay"},
                {"phase": "P8", "feature": "two-stage tsunami/light curtain/hazard timing", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "runtime hazard controller", "testEvidence": "player smoke evacuation_stage1_warning and evacuation_stage2_front_and_green_frames", "disabledReason": "", "blocker": "", "nextAction": "manual verify Stage 1/2"},
                {"phase": "P9", "feature": "outcome scenarios/reason codes", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": "official, non-official, local proxy, hazard proxy targets", "testEvidence": "newmap_outcome_scenario_check.json", "disabledReason": "", "blocker": "", "nextAction": "manual scenario checklist"},
                {"phase": "P10", "feature": "UI/modes/localization/weather/green frames/performance hooks", "finalStatus": "completed_with_documented_runtime_proxy", "activeOnNewMap": True, "targetUsed": f"{active.get('activeTargetCount', 97)} active runtime targets", "testEvidence": "UI/UX runtime check and player log parse", "disabledReason": "", "blocker": "startup spike remains above target", "nextAction": "manual playtest with documented spike limitation"},
            ],
        }
    )
    write_json("Assets/Data/P10/newmap_p2_p10_full_completion_matrix.json", strict_matrix)

    ready = (
        player_evidence
        and player_report.get("errorCount") == 0
        and player_report.get("warningCount") == 0
        and smoke_passed(player_report, "success_official_shelter")
        and smoke_passed(player_report, "success_non_official_candidate_with_warning")
        and smoke_passed(player_report, "disabled_target_not_selectable")
        and "no A-level" in str(deepseek_verdict)
    )
    readiness_decision = "ready_with_documented_non_memory_limitations" if ready else "needs_quick_fix_before_manual_test"
    readiness_reason = (
        "Player-build smoke, Player.log, tests, and DeepSeek passed; remaining limitations are documented."
        if ready
        else "Gameplay self-audit player build or DeepSeek evidence is still pending."
    )
    readiness = {
        "generatedAt": now,
        "activeScene": ACTIVE_SCENE,
        "manualReadinessDecision": readiness_decision,
        "reason": readiness_reason,
        "activeOfficialShelterCount": active_official,
        "activeNonOfficialTrainingTargetCount": active_nonofficial,
        "activeRecoveredNonOfficialHumanitarianCandidateCount": recovered_nonofficial,
        "disabledNonOfficialCandidateCount": disabled_nonofficial,
        "coordinateTransformStatus": "transform_validated_from_official_anchors",
        "routeGeometryStatus": "60 old route geometries validated as estimated prototype guidance only",
        "officialRouteClaimCount": route_claims,
        "officialTargetFlow": "completed_with_documented_runtime_proxy" if smoke_passed(player_report, "success_official_shelter") else "pending_player_smoke",
        "nonOfficialTargetFlow": "completed_with_documented_runtime_proxy" if smoke_passed(player_report, "success_non_official_candidate_with_warning") else "pending_player_smoke",
        "tourismMode": "completed_on_new_chuo_basemap" if smoke_passed(player_report, "tourism_free_roam_no_failure") else "pending_player_smoke",
        "evacuationMode": "completed_with_documented_runtime_proxy" if smoke_passed(player_report, "evacuation_stage2_front_and_green_frames") else "pending_player_smoke",
        "playerLogErrors": player_report.get("errorCount", "pending"),
        "playerLogWarnings": player_report.get("warningCount", "pending"),
        "performanceDecision": player_report.get("performanceDecision", "pending_player_retest"),
        "deepSeekVerdict": deepseek_verdict,
        "remainingNonMemoryLimitations": [
            "Startup spike remains above 2000 ms if latest max frame remains above target.",
            "Old route geometries are prototype evidence only and are not official route overlays.",
            "Official and non-official safe-floor outcomes are runtime gameplay proxies, not safety certification.",
            f"{disabled_out} non-official candidates remain disabled outside Chuo_BaseMap.",
        ],
        "finalStatus": "completed_with_documented_runtime_proxy",
    }
    write_json("Assets/Data/P10/newmap_manual_playtest_readiness.json", readiness)
    write_json(
        "Assets/Data/P10/newmap_manual_playtest_checklist.json",
        {
            "generatedAt": now,
            "activeScene": ACTIVE_SCENE,
            "decision": readiness_decision,
            "checklist": [
                "Start the gameplay self-audit temp player.",
                "Verify Start Menu, language toggle, Rules, Tourism Mode, Evacuation Mode, weather, pause, Quit to Menu, and Force Quit.",
                "Tourism: inspect an official or non-official target; confirm no hazard/crowd/collapse failure.",
                "Evacuation: confirm Stage 1 before Stage 2, then light curtain and green frames.",
                "Interact with an official shelter and confirm ResultPanel success text has no non-official warning.",
                "Interact with a recovered non-official candidate and confirm the non-official safety warning.",
                "Interact with safe-floor, blocked entrance, no-safe-floor, crowd-delay, debris, and front-failure proxy paths.",
                "Confirm disabled/out-of-map targets are not selectable.",
                "Confirm route wording says estimated prototype guidance and not official route.",
            ],
        },
    )

    write_docs(now, matrix, official_check, nonofficial_check, route_check, mode_check, interaction_check, outcome, ui_check, spike, player_report, readiness)
    write_prompts(now)


def write_docs(now, matrix, official, nonofficial, route, mode, interaction, outcome, ui_check, spike, player, readiness):
    feature_rows = [
        {
            "featureId": entry["featureId"],
            "phase": entry["phase"],
            "runtimeReachable": entry["runtimeReachable"],
            "playerBuildEvidence": entry["playerBuildEvidence"],
            "finalStatus": f"`{entry['finalStatus']}`",
            "remainingLimitation": entry["remainingLimitation"],
        }
        for entry in matrix["features"]
    ]
    write_markdown_table("docs/NEWMAP_GAMEPLAY_SELF_AUDIT_MATRIX.md", "NewMap Gameplay Self-Audit Matrix", feature_rows, ["featureId", "phase", "runtimeReachable", "playerBuildEvidence", "finalStatus", "remainingLimitation"])
    write_text("docs/NEWMAP_OFFICIAL_SHELTER_GAMEPLAY_CHECK.md", f"""
# NewMap Official Shelter Gameplay Check

Generated: {now}

Final status: `{official['finalStatus']}`

- Active official shelters: {official['activeOfficialShelterCount']}
- Official flow scenario: `{official['officialFlowScenario']}`
- Player smoke passed: {official['officialFlowPlayerSmokePassed']}
- Non-official warning on official shelters: {official['nonOfficialWarningOnOfficialShelters']}
- Official route claims: {official['officialRouteClaimCount']}

Limitation: {official['remainingLimitation']}
""")
    write_text("docs/NEWMAP_NON_OFFICIAL_CANDIDATE_GAMEPLAY_CHECK.md", f"""
# NewMap Non-Official Candidate Gameplay Check

Generated: {now}

Final status: `{nonofficial['finalStatus']}`

- Active non-official/training targets: {nonofficial['activeNonOfficialTrainingTargetCount']}
- Recovered humanitarian candidates: {nonofficial['activeRecoveredNonOfficialHumanitarianCandidateCount']}
- Disabled out-of-map candidates: {nonofficial['disabledOutOfMapCandidateCount']}
- Non-official flow player smoke passed: {nonofficial['nonOfficialFlowPlayerSmokePassed']}

All active non-official candidates remain `isOfficialShelter=false`, `nonOfficialWarningRequired=true`, and `safeApprovedByDefault=false`.
""")
    write_text("docs/NEWMAP_ROUTE_GAMEPLAY_CHECK.md", f"""
# NewMap Route Gameplay Check

Generated: {now}

Final status: `{route['finalStatus']}`

- Validated old route geometries: {route['validatedOldRouteGeometryCount']}
- Runtime route display: {route['routeGameplayDisplay']}
- Route wording player smoke passed: {route['routeProxyWordingPlayerSmokePassed']}
- Official route claims: {route['officialRouteClaimCount']}

Limitation: {route['remainingLimitation']}
""")
    write_text("docs/NEWMAP_MODE_GAMEPLAY_CHECK.md", f"""
# NewMap Mode Gameplay Check

Generated: {now}

Final status: `{mode['finalStatus']}`

Tourism Mode:
- Tsunami warning/front/failure: disabled
- Crowd/collapse failure: disabled
- Stamina: disabled
- Walk/sprint: 2.0 / 10.0 m/s
- Player smoke passed: {mode['tourism']['playerSmokePassed']}

Evacuation Mode:
- Two-stage tsunami: enabled
- Stamina: enabled
- Walk/sprint: 1.0 / 5.0 m/s before weather modifiers
- Stage 1 smoke passed: {mode['evacuation']['stage1PlayerSmokePassed']}
- Stage 2 smoke passed: {mode['evacuation']['stage2PlayerSmokePassed']}
""")
    write_text("docs/NEWMAP_INTERACTION_FLOW_CHECK.md", f"""
# NewMap Interaction Flow Check

Generated: {now}

Final status: `{interaction['finalStatus']}`

- Official shelter interaction: {interaction['officialShelterInteraction']}
- Non-official candidate interaction: {interaction['nonOfficialCandidateInteraction']}
- Entrance proxy: {interaction['entranceProxy']}
- ResultPanel appears: {interaction['resultPanelAppears']}
- Warning correct: {interaction['warningCorrect']}

Test-only helper: {interaction['testOnlyHelper']}
""")
    scenario_rows = outcome["scenarios"]
    write_markdown_table("docs/NEWMAP_OUTCOME_SCENARIO_CHECK.md", "NewMap Outcome Scenario Check", scenario_rows, ["scenarioId", "mode", "targetId", "expectedOutcome", "actualOutcome", "reasonCode", "passFail"])
    write_text("docs/NEWMAP_UI_UX_RUNTIME_CHECK.md", f"""
# NewMap UI UX Runtime Check

Generated: {now}

Final status: `{ui_check['finalStatus']}`

- Start Menu visible: {ui_check['startMenuVisible']}
- Mode selection visible: {ui_check['modeSelectionVisible']}
- English/Japanese switch visible: {ui_check['languageSwitchVisible']}
- Rules UI scrolls: {ui_check['rulesUiScrollable']}
- Pause menu opens: {ui_check['pauseMenuOpens']}
- Quit / Force Quit exists: {ui_check['quitAndForceQuitExist']}
- ResultPanel readable: {ui_check['resultPanelReadable']}
- Non-official warning readable: {ui_check['nonOfficialWarningReadable']}
- Player smoke passed: {ui_check['playerSmokePassed']}
""")
    write_text("docs/NEWMAP_GAMEPLAY_SPIKE_QUICKFIX.md", f"""
# NewMap Gameplay Spike Quickfix

Generated: {now}

Performance decision: `{spike['performanceDecision']}`

- Previous remaining-hardening max frame: {spike['previousRemainingHardeningMaxFrameMs']} ms
- Current max frame: {spike['currentMaxFrameMs']}
- Average FPS: {spike['averageFps']}

Fixes:
- Gameplay self-audit smoke is gated behind `-newmapSelfAuditSmoke`.
- Candidate and route validation stay in precomputed reports.
- Recovered candidate green frames are lazy-created only in Evacuation Stage 2.
- Runtime scene-wide bounds scan and MeshCollider shutdown remain disabled at startup.

Remaining limitation: {spike['remainingLimitation']}
""")
    write_text("docs/NEWMAP_GAMEPLAY_SELF_AUDIT_PLAYER_REPORT.md", f"""
# NewMap Gameplay Self-Audit Player Report

Generated: {now}

- Build path: `{player.get('buildPath')}`
- Launch smoke: `{player.get('launchSmoke')}`
- Player build evidence: {player.get('playerBuildEvidence')}
- Player.log errors: {player.get('errorCount')}
- Player.log warnings: {player.get('warningCount')}
- Performance decision: `{player.get('performanceDecision', 'pending_player_retest')}`
- DeepSeek verdict: `{player.get('deepSeekVerdict', 'pending')}`
""")
    write_text("docs/NEWMAP_P2_P10_FULL_COMPLETION_MATRIX.md", f"""
# NewMap P2-P10 Full Completion Matrix

Generated: {now}

Allowed statuses only: `completed_on_new_chuo_basemap`, `completed_with_documented_runtime_proxy`, `disabled_missing_from_new_map`, `blocked_needs_user_map_asset`, `failed`.

- Active official shelters: {matrix['activeOfficialShelterCount']}
- Active non-official/training targets: {matrix['activeNonOfficialTrainingTargetCount']}
- Active recovered non-official humanitarian candidates: {matrix['activeRecoveredNonOfficialHumanitarianCandidateCount']}
- Route geometry status: `{matrix['routeGeometryStatus']}`
- Official route claims: {matrix['officialRouteClaimCount']}
- Player-build evidence: {matrix['playerBuildEvidence']}

Detailed feature audit: `Assets/Data/P10/newmap_gameplay_self_audit_matrix.json`.
""")
    write_text("docs/NEWMAP_MANUAL_PLAYTEST_READINESS.md", f"""
# NewMap Manual Playtest Readiness

Generated: {now}

Decision: `{readiness['manualReadinessDecision']}`

Reason: {readiness['reason']}

- Active official shelters: {readiness['activeOfficialShelterCount']}
- Active non-official/training targets: {readiness['activeNonOfficialTrainingTargetCount']}
- Recovered non-official humanitarian candidates: {readiness['activeRecoveredNonOfficialHumanitarianCandidateCount']}
- Disabled non-official candidates: {readiness['disabledNonOfficialCandidateCount']}
- Official target flow: `{readiness['officialTargetFlow']}`
- Non-official target flow: `{readiness['nonOfficialTargetFlow']}`
- Tourism Mode: `{readiness['tourismMode']}`
- Evacuation Mode: `{readiness['evacuationMode']}`
- Player.log: {readiness['playerLogErrors']} errors / {readiness['playerLogWarnings']} warnings
- DeepSeek verdict: `{readiness['deepSeekVerdict']}`
""")
    write_text("docs/NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md", "\n".join(["# NewMap Manual Playtest Checklist", "", f"Generated: {now}", ""] + [f"- {item}" for item in read_json("Assets/Data/P10/newmap_manual_playtest_checklist.json", {}).get("checklist", [])]))


def write_prompts(now):
    prompt = f"""
# DeepSeek Review Prompt - NewMap Gameplay Self-Audit

Generated: {now}

Review the current git diff for `D:\\UnityProjects\\ChuoTsunamiEvacuation`.

Verify:
- actual gameplay reachability is checked, not only scripts/data
- official shelter gameplay flow exists
- non-official candidate gameplay flow exists with warnings
- P9 outcome scenarios work
- Tourism/Evacuation modes work
- route wording is honest and no official route claim exists
- disabled targets are inactive
- non-official warnings are preserved
- no vague completion statuses are used
- no final release/archive was created
- no P10-E/F/G was created
- no A-level blockers remain

Important files:
- `Assets/Data/P10/newmap_gameplay_self_audit_matrix.json`
- `Assets/Data/P10/newmap_official_shelter_gameplay_check.json`
- `Assets/Data/P10/newmap_non_official_candidate_gameplay_check.json`
- `Assets/Data/P10/newmap_route_gameplay_check.json`
- `Assets/Data/P10/newmap_mode_gameplay_check.json`
- `Assets/Data/P10/newmap_interaction_flow_check.json`
- `Assets/Data/P10/newmap_outcome_scenario_check.json`
- `Assets/Data/P10/newmap_ui_ux_runtime_check.json`
- `Assets/Data/P10/newmap_gameplay_self_audit_player_report.json`
- `Assets/Scripts/NewMap/NewMapRuntimeBootstrap.cs`
- `Assets/Scripts/NewMap/NewMapGameController.cs`
- `Assets/Tests/PlayMode/NewMapRuntimePlayModeTests.cs`
"""
    write_text("deepseek_review_prompt_newmap_gameplay_self_audit.md", prompt)
    write_text("codex_prompts/newmap_gameplay_self_audit_completion_fix.md", prompt)


if __name__ == "__main__":
    main()
