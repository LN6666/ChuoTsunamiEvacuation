from __future__ import annotations

import json
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[2]
QUALIFICATION_ROOT = REPO_ROOT / "data_pipeline" / "qualification"

REAL_CHUO_PLAN_PATH = QUALIFICATION_ROOT / "real_chuo_ingestion_plan.json"
CRS_QGIS_PLAN_PATH = QUALIFICATION_ROOT / "crs_qgis_qa_plan.json"
DEPENDENCY_PLAN_PATH = QUALIFICATION_ROOT / "dependency_environment_plan.json"


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def test_real_chuo_ingestion_plan_is_valid_json_with_required_keys() -> None:
    plan = load_json(REAL_CHUO_PLAN_PATH)
    required_keys = {
        "planVersion",
        "sourceFamilies",
        "ingestionStages",
        "provenanceFields",
        "rawDataPolicy",
        "processedOutputPolicy",
        "validationRequirements",
        "risks",
        "nextActions",
    }
    assert required_keys.issubset(plan)


def test_crs_qgis_qa_plan_is_valid_json_with_required_keys() -> None:
    plan = load_json(CRS_QGIS_PLAN_PATH)
    required_keys = {
        "planVersion",
        "crsStrategy",
        "metricOperationRules",
        "unityCoordinatePolicy",
        "qgisLayers",
        "manualChecks",
        "outputFormats",
        "risks",
        "nextActions",
    }
    assert required_keys.issubset(plan)


def test_dependency_environment_plan_is_valid_json_with_required_keys() -> None:
    plan = load_json(DEPENDENCY_PLAN_PATH)
    required_keys = {
        "planVersion",
        "pythonVersion",
        "packageChecks",
        "requiredForValidation",
        "requiredForRouting",
        "requiredForBuildingMatching",
        "installPolicy",
        "risks",
        "nextActions",
    }
    assert required_keys.issubset(plan)


def test_source_family_list_includes_required_families() -> None:
    plan = load_json(REAL_CHUO_PLAN_PATH)
    family_ids = {family["familyId"] for family in plan["sourceFamilies"]}
    assert "official_shelter_facility" in family_ids
    assert "official_hazard_map" in family_ids
    assert "plateau_building_geometry" in family_ids
    assert "plateau_building_attribute" in family_ids
    assert "osm_routing_network" in family_ids


def test_crs_plan_requires_projected_crs_for_metric_operations() -> None:
    plan = load_json(CRS_QGIS_PLAN_PATH)
    metric_rules = plan["metricOperationRules"]
    assert metric_rules["mustUseProjectedCrs"] is True
    assert metric_rules["forbidMeterDistancesInUnprojectedLatLon"] is True
    assert "Never compute meter distances directly" in metric_rules["ruleNote"]


def test_dependency_plan_includes_validation_packages() -> None:
    plan = load_json(DEPENDENCY_PLAN_PATH)
    assert "jsonschema" in plan["requiredForValidation"]
    assert "pytest" in plan["requiredForValidation"]
    assert plan["packageChecks"]["jsonschema"]["available"] is False
    assert plan["packageChecks"]["pytest"]["available"] is False


def test_raw_data_policy_forbids_committing_raw_large_files() -> None:
    plan = load_json(REAL_CHUO_PLAN_PATH)
    raw_policy = plan["rawDataPolicy"]
    forbidden_patterns = set(raw_policy["forbiddenCommitPatterns"])
    assert raw_policy["commitRawLargeFilesAllowed"] is False
    assert "*.citygml" in forbidden_patterns
    assert "*.shp" in forbidden_patterns
    assert "*.gpkg" in forbidden_patterns
    assert "*.osm" in forbidden_patterns
    assert "*.pbf" in forbidden_patterns
