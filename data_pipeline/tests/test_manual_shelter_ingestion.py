from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path

from jsonschema import Draft202012Validator


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
INGEST_SCRIPT = PIPELINE_ROOT / "scripts" / "ingest_shelters_from_manual_source.py"
MAPPING_TEMPLATE = PIPELINE_ROOT / "sources" / "shelter_source_mapping_template.json"
MAPPING_SCHEMA = PIPELINE_ROOT / "schemas" / "source_shelter_mapping_schema.json"
REAL_SHELTER_SCHEMA = PIPELINE_ROOT / "schemas" / "real_shelter_schema.json"
MANUAL_FIXTURE = PIPELINE_ROOT / "samples" / "sample_manual_shelter_source.csv"


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def test_mapping_template_validates_against_mapping_schema() -> None:
    mapping = load_json(MAPPING_TEMPLATE)
    schema = load_json(MAPPING_SCHEMA)
    Draft202012Validator.check_schema(schema)
    errors = sorted(Draft202012Validator(schema).iter_errors(mapping), key=lambda error: list(error.path))
    assert errors == []


def test_manual_fixture_exists() -> None:
    assert MANUAL_FIXTURE.exists()
    text = MANUAL_FIXTURE.read_text(encoding="utf-8-sig")
    assert "manual_id" in text
    assert "facility_name" in text


def test_manual_ingestion_generates_schema_compliant_outputs(tmp_path: Path) -> None:
    json_output = tmp_path / "manual_shelter_ingestion_sample.json"
    csv_output = tmp_path / "manual_shelter_ingestion_sample.csv"

    result = subprocess.run(
        [
            sys.executable,
            str(INGEST_SCRIPT),
            "--json-output",
            str(json_output),
            "--csv-output",
            str(csv_output),
        ],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )

    assert result.returncode == 0, result.stdout + result.stderr
    assert json_output.exists()
    assert csv_output.exists()

    dataset = load_json(json_output)
    shelter_schema = load_json(REAL_SHELTER_SCHEMA)
    errors = sorted(Draft202012Validator(shelter_schema).iter_errors(dataset), key=lambda error: list(error.path))
    assert errors == []
    assert dataset["dataset_id"] == "manual_shelter_ingestion_sample"
    assert len(dataset["records"]) == 3
    assert dataset["records"][0]["type"] == "tsunami_evacuation_building"
    assert dataset["records"][0]["unity"]["is_entry_enabled"] is True
    assert dataset["records"][2]["capacity"] is None
