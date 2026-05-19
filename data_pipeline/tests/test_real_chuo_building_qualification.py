from __future__ import annotations

import csv
import json
import subprocess
import sys
from collections import Counter
from pathlib import Path

import pytest
from jsonschema import Draft202012Validator, FormatChecker


REPO_ROOT = Path(__file__).resolve().parents[2]
SCRIPT = REPO_ROOT / "data_pipeline" / "scripts" / "build_real_chuo_building_qualification.py"
INGEST_SCRIPT = REPO_ROOT / "data_pipeline" / "scripts" / "ingest_official_chuo_shelters.py"
SCHEMA_PATH = REPO_ROOT / "data_pipeline" / "qualification" / "evacuation_building_qualification_schema.json"
INPUT_MANIFEST = REPO_ROOT / "data_pipeline" / "qualification" / "real_chuo_building_matching_input_manifest.json"
NORMALIZED_SHELTERS = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_official_shelters_normalized.json"
OUTPUT_JSON = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_building_qualification.json"
OUTPUT_CSV = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_building_qualification.csv"
MATCHES_JSON = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_shelter_building_matches.json"
MATCHES_CSV = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_shelter_building_matches.csv"
QGIS_LAYERS = [
    REPO_ROOT / "data_pipeline" / "processed" / "qgis_qa" / "real_chuo_shelter_points.geojson",
    REPO_ROOT / "data_pipeline" / "processed" / "qgis_qa" / "real_chuo_building_footprints.geojson",
    REPO_ROOT / "data_pipeline" / "processed" / "qgis_qa" / "real_chuo_match_lines.geojson",
    REPO_ROOT / "data_pipeline" / "processed" / "qgis_qa" / "real_chuo_low_confidence_or_unmatched.geojson",
]
LOCAL_PLATEAU_BLDG_ROOT = Path(r"D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\bldg")


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def official_families(record: dict) -> set[str]:
    return {
        source["sourceFamily"]
        for source in record["evidenceSources"]
        if source.get("sourceFamily", "").startswith("official_")
    }


def ensure_inputs_available() -> None:
    if not LOCAL_PLATEAU_BLDG_ROOT.exists():
        pytest.skip("Local PLATEAU CityGML building root is not present.")
    manifest = load_json(REPO_ROOT / "data_pipeline" / "sources" / "real_chuo_official_source_manifest.json")
    if not all((REPO_ROOT / source["localRawPath"]).exists() for source in manifest["sources"]):
        pytest.skip("Official raw downloads are local ignored inputs and are not present.")


def test_build_script_runs_and_writes_outputs() -> None:
    ensure_inputs_available()
    ingest = subprocess.run(
        [sys.executable, str(INGEST_SCRIPT)],
        cwd=REPO_ROOT,
        check=False,
        text=True,
        capture_output=True,
    )
    assert ingest.returncode == 0, ingest.stderr

    result = subprocess.run(
        [sys.executable, str(SCRIPT)],
        cwd=REPO_ROOT,
        check=False,
        text=True,
        capture_output=True,
    )
    assert result.returncode == 0, result.stderr

    for path in [INPUT_MANIFEST, NORMALIZED_SHELTERS, OUTPUT_JSON, OUTPUT_CSV, MATCHES_JSON, MATCHES_CSV, *QGIS_LAYERS]:
        assert path.exists(), path


def test_input_manifest_has_required_fields() -> None:
    if not INPUT_MANIFEST.exists():
        pytest.skip("Input manifest has not been generated.")
    manifest = load_json(INPUT_MANIFEST)
    assert manifest["shelterInputPath"].endswith("real_chuo_official_shelters_normalized.json")
    assert manifest["buildingInputOrigin"].startswith("existing local PLATEAU")
    assert manifest["buildingInputIsProcessed"] is False
    assert manifest["coordinateReferenceSystem"] == "EPSG:4326"
    assert manifest["metricCoordinateReferenceSystem"] == "EPSG:6677"
    assert manifest["selectedMeshCodes"]
    assert manifest["candidateBuildingCount"] > 0


def test_qualification_output_validates_against_schema() -> None:
    if not OUTPUT_JSON.exists():
        pytest.skip("Qualification output has not been generated.")
    dataset = load_json(OUTPUT_JSON)
    schema = load_json(SCHEMA_PATH)
    Draft202012Validator.check_schema(schema)
    errors = sorted(
        Draft202012Validator(schema, format_checker=FormatChecker()).iter_errors(dataset),
        key=lambda error: list(error.path),
    )
    assert errors == []


def test_qualification_records_preserve_official_rules_and_b4_routing_scope() -> None:
    if not OUTPUT_JSON.exists():
        pytest.skip("Qualification output has not been generated.")
    dataset = load_json(OUTPUT_JSON)
    assert len(dataset["records"]) == 31

    statuses = Counter(record["qualificationStatus"] for record in dataset["records"])
    methods = Counter(record["matchMethod"] for record in dataset["records"])
    assert statuses["official_confirmed"] > 0
    assert statuses["official_confirmed_with_review"] > 0
    assert statuses["unknown"] > 0
    assert methods["contains"] > 0
    assert methods["nearest"] > 0
    assert methods["unmatched"] > 0

    for record in dataset["records"]:
        assert record["qualificationStatus"]
        assert record["confidence"] in {"high", "medium", "low", "unknown"}
        assert record["routeAvailability"] == "not_evaluated"
        assert record["nearestRouteDistanceMeters"] is None
        assert record["estimatedTravelTimeSeconds"] is None
        if record["qualificationStatus"] in {"official_confirmed", "official_confirmed_with_review"}:
            assert "official_shelter_facility" in official_families(record)
        if record["matchMethod"] == "nearest":
            assert isinstance(record["matchDistanceMeters"], (int, float))
            assert record["manualReviewNeeded"] is True
            assert record["warnings"]
        if record["matchMethod"] == "unmatched":
            assert record["qualificationStatus"] in {"unknown", "not_qualified"}
            assert record["manualReviewNeeded"] is True
            assert record["warnings"]


def test_csv_and_qgis_layers_are_created_and_small() -> None:
    if not OUTPUT_CSV.exists() or not MATCHES_CSV.exists():
        pytest.skip("CSV outputs have not been generated.")

    with OUTPUT_CSV.open("r", encoding="utf-8", newline="") as handle:
        qualification_rows = list(csv.DictReader(handle))
    with MATCHES_CSV.open("r", encoding="utf-8", newline="") as handle:
        match_rows = list(csv.DictReader(handle))
    assert len(qualification_rows) == 31
    assert len(match_rows) == 31

    for path in QGIS_LAYERS:
        payload = load_json(path)
        assert payload["type"] == "FeatureCollection"
        assert len(payload["features"]) > 0
        assert path.stat().st_size < 500_000


def test_processed_outputs_do_not_depend_on_raw_runtime_paths() -> None:
    if not OUTPUT_JSON.exists() or not MATCHES_JSON.exists():
        pytest.skip("Processed outputs have not been generated.")
    for path in [OUTPUT_JSON, MATCHES_JSON]:
        text = path.read_text(encoding="utf-8")
        assert "data_pipeline/raw" not in text
        assert "data_pipeline/downloads" not in text
        assert "data_pipeline/cache" not in text
