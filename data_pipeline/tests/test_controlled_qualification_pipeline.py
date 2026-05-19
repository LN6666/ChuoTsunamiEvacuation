from __future__ import annotations

import csv
import json
import subprocess
import sys
from pathlib import Path

import pytest

try:
    from jsonschema import Draft202012Validator, FormatChecker
except ImportError:  # pragma: no cover - exercised only when dependency is absent.
    Draft202012Validator = None  # type: ignore[assignment]
    FormatChecker = None  # type: ignore[assignment]


REPO_ROOT = Path(__file__).resolve().parents[2]
QUALIFICATION_ROOT = REPO_ROOT / "data_pipeline" / "qualification"
SCRIPT_PATH = REPO_ROOT / "data_pipeline" / "scripts" / "build_controlled_qualification_sample.py"

SHELTERS_PATH = QUALIFICATION_ROOT / "sample_controlled_shelters_for_qualification.json"
BUILDINGS_PATH = QUALIFICATION_ROOT / "sample_controlled_buildings_for_matching.json"
ROUTES_PATH = QUALIFICATION_ROOT / "sample_controlled_routes_for_qualification.json"
CONFIG_PATH = QUALIFICATION_ROOT / "controlled_qualification_pipeline_config.json"
OUTPUT_JSON_PATH = QUALIFICATION_ROOT / "controlled_building_qualification_output.json"
OUTPUT_CSV_PATH = QUALIFICATION_ROOT / "controlled_building_qualification_output.csv"
SCHEMA_PATH = QUALIFICATION_ROOT / "evacuation_building_qualification_schema.json"

REQUIRED_OUTPUT_FIELDS = {
    "qualificationId",
    "plateauBuildingId",
    "shelterId",
    "shelterName",
    "officialDesignationStatus",
    "qualificationStatus",
    "qualificationReason",
    "evidenceSources",
    "matchMethod",
    "matchDistanceMeters",
    "confidence",
    "manualReviewNeeded",
    "warnings",
    "disasterTypes",
    "safeFloor",
    "capacity",
    "sourceUpdatedAt",
    "routeAvailability",
    "nearestRouteDistanceMeters",
    "estimatedTravelTimeSeconds",
    "notes",
}

OFFICIAL_EVIDENCE_FAMILIES = {
    "official_shelter_facility",
    "official_evacuation_building",
    "official_hazard_map",
    "official_disaster_prevention_plan",
}


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def run_build_script() -> dict:
    result = subprocess.run(
        [sys.executable, str(SCRIPT_PATH)],
        cwd=REPO_ROOT,
        check=True,
        capture_output=True,
        text=True,
    )
    assert "[build] records: 7" in result.stdout
    return load_json(OUTPUT_JSON_PATH)


def test_controlled_input_json_files_are_valid() -> None:
    for path in [SHELTERS_PATH, BUILDINGS_PATH, ROUTES_PATH]:
        payload = load_json(path)
        assert payload["datasetId"].startswith("p5_b1_sample_controlled")
        assert payload["coordinateReferenceSystem"] == "EPSG:4326"


def test_pipeline_config_json_is_valid_and_thresholds_are_deterministic() -> None:
    config = load_json(CONFIG_PATH)
    thresholds = config["defaultThresholds"]
    assert thresholds == {
        "containsMatchConfidence": "high",
        "nearestHighConfidenceDistanceMeters": 10.0,
        "nearestMediumConfidenceDistanceMeters": 30.0,
        "unmatchedDistanceMeters": 75.0,
        "minSafeFloorCandidate": 3,
        "minCapacityCandidate": 50,
    }
    assert config["rulebookPath"].endswith("evacuation_building_qualification_rulebook.json")
    assert config["schemaPath"].endswith("evacuation_building_qualification_schema.json")


def test_build_script_produces_output_json_and_csv() -> None:
    dataset = run_build_script()
    assert OUTPUT_JSON_PATH.exists()
    assert OUTPUT_CSV_PATH.exists()
    assert len(dataset["records"]) == 7

    with OUTPUT_CSV_PATH.open("r", encoding="utf-8", newline="") as handle:
        rows = list(csv.DictReader(handle))
    assert len(rows) == 7
    assert rows[0]["qualificationId"] == "controlled_qualification_001"


def test_output_json_has_expected_top_level_fields() -> None:
    dataset = run_build_script()
    assert set(dataset).issuperset(
        {
            "datasetId",
            "generatedAt",
            "coordinateReferenceSystem",
            "rulebookVersion",
            "notes",
            "records",
        }
    )
    assert dataset["datasetId"] == "p5_b1_controlled_building_qualification_output"
    assert dataset["coordinateReferenceSystem"] == "EPSG:4326"


def test_output_records_include_all_required_qualification_fields() -> None:
    dataset = run_build_script()
    for record in dataset["records"]:
        assert set(record) == REQUIRED_OUTPUT_FIELDS


def test_official_confirmed_appears_only_when_official_evidence_exists() -> None:
    dataset = run_build_script()
    for record in dataset["records"]:
        evidence_families = {source["sourceFamily"] for source in record["evidenceSources"]}
        if record["qualificationStatus"] == "official_confirmed":
            assert evidence_families.intersection(OFFICIAL_EVIDENCE_FAMILIES)
            assert record["manualReviewNeeded"] is False


def test_candidate_examples_are_not_marked_official_confirmed() -> None:
    dataset = run_build_script()
    by_shelter = {record["shelterId"]: record for record in dataset["records"]}
    strong = by_shelter["controlled_candidate_strong_003"]
    weak = by_shelter["controlled_candidate_weak_missing_004"]

    assert strong["qualificationStatus"] == "strong_candidate"
    assert weak["qualificationStatus"] == "weak_candidate"
    assert strong["officialDesignationStatus"] == "not_official"
    assert weak["officialDesignationStatus"] == "not_official"
    assert strong["qualificationStatus"] != "official_confirmed"
    assert weak["qualificationStatus"] != "official_confirmed"


def test_nearest_and_unmatched_examples_trigger_manual_review_and_warnings() -> None:
    dataset = run_build_script()
    by_shelter = {record["shelterId"]: record for record in dataset["records"]}
    nearest = by_shelter["controlled_shelter_official_nearest_002"]
    unmatched = by_shelter["controlled_unmatched_official_007"]

    assert nearest["matchMethod"] == "nearest"
    assert nearest["manualReviewNeeded"] is True
    assert any("nearest match" in warning for warning in nearest["warnings"])

    assert unmatched["matchMethod"] == "unmatched"
    assert unmatched["qualificationStatus"] == "unknown"
    assert unmatched["manualReviewNeeded"] is True
    assert any("unmatched" in warning for warning in unmatched["warnings"])


def test_route_fields_are_present_and_clearly_estimated_not_official() -> None:
    dataset = run_build_script()
    for record in dataset["records"]:
        assert "routeAvailability" in record
        assert "nearestRouteDistanceMeters" in record
        assert "estimatedTravelTimeSeconds" in record
        assert "prototype estimates" in record["notes"]
        assert "not official evacuation routes" in record["notes"]


def test_output_validates_against_schema_when_jsonschema_available() -> None:
    if Draft202012Validator is None or FormatChecker is None:
        pytest.skip("jsonschema is unavailable; skipping controlled output schema validation.")

    dataset = run_build_script()
    schema = load_json(SCHEMA_PATH)
    Draft202012Validator.check_schema(schema)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(dataset), key=lambda error: list(error.path))
    assert errors == []
