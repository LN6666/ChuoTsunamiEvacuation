from __future__ import annotations

import copy
import json
import subprocess
import sys
from pathlib import Path

from jsonschema import Draft202012Validator, FormatChecker


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
HAZARD_SCHEMA = PIPELINE_ROOT / "schemas" / "tsunami_hazard_schema.json"
HAZARD_SAMPLE = PIPELINE_ROOT / "samples" / "sample_tsunami_hazard_zones.json"
HAZARD_VALIDATOR = PIPELINE_ROOT / "scripts" / "validate_tsunami_hazard.py"


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def test_tsunami_hazard_schema_is_valid_json_schema() -> None:
    schema = load_json(HAZARD_SCHEMA)
    Draft202012Validator.check_schema(schema)


def test_sample_tsunami_hazard_fixture_validates_against_schema() -> None:
    schema = load_json(HAZARD_SCHEMA)
    dataset = load_json(HAZARD_SAMPLE)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(dataset), key=lambda error: list(error.path))
    assert errors == []


def test_tsunami_hazard_validator_passes_on_sample_fixture() -> None:
    result = subprocess.run(
        [sys.executable, str(HAZARD_VALIDATOR)],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )
    assert result.returncode == 0, result.stdout + result.stderr
    assert "[validate-hazard] OK" in result.stdout


def test_sample_tsunami_hazard_fixture_has_required_planning_fields() -> None:
    dataset = load_json(HAZARD_SAMPLE)
    assert dataset["coordinate_reference_system"] == "EPSG:4326"
    assert dataset["source"]["is_official_primary"] is False
    assert dataset["zones"]
    for zone in dataset["zones"]:
        assert zone["geometry_type"]
        assert zone["geometry"]["type"]
        assert "inundation_area" in zone
        assert zone["inundation_depth_m"] is None or isinstance(zone["inundation_depth_m"], (int, float))
        assert zone["tsunami_height_m"] is None or isinstance(zone["tsunami_height_m"], (int, float))


def test_paper_reference_hazard_source_cannot_be_marked_official_primary(tmp_path: Path) -> None:
    dataset = copy.deepcopy(load_json(HAZARD_SAMPLE))
    dataset["source"]["source_family"] = "paper_or_secondary_reference"
    dataset["source"]["official_status"] = "secondary_reference_only"
    dataset["source"]["is_official_primary"] = True

    invalid_path = tmp_path / "invalid_hazard_reference.json"
    invalid_path.write_text(json.dumps(dataset), encoding="utf-8")

    result = subprocess.run(
        [sys.executable, str(HAZARD_VALIDATOR), "--input", str(invalid_path)],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )
    assert result.returncode != 0
    assert "cannot be official primary" in result.stderr
