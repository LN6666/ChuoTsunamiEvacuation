from __future__ import annotations

import csv
import json
import subprocess
import sys
from pathlib import Path

import pytest
from jsonschema import Draft202012Validator, FormatChecker


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
SCHEMA_PATH = PIPELINE_ROOT / "schemas" / "real_shelter_schema.json"
SAMPLE_CSV_PATH = PIPELINE_ROOT / "samples" / "sample_raw_shelters.csv"
EXPORT_SCRIPT = PIPELINE_ROOT / "scripts" / "export_unity_shelters.py"
VALIDATE_SCRIPT = PIPELINE_ROOT / "scripts" / "validate_real_shelters.py"

REQUIRED_SAMPLE_COLUMNS = {
    "id",
    "name",
    "type",
    "latitude",
    "longitude",
    "address",
    "capacity",
    "floors_available",
    "elevation_m",
    "source",
    "source_url",
    "source_updated_at",
    "notes",
    "prefab_hint",
    "is_entry_enabled",
    "estimated_stair_floors",
}


@pytest.fixture()
def exported_sample(tmp_path: Path) -> tuple[Path, Path, dict]:
    json_output = tmp_path / "real_chuo_shelters_sample.json"
    csv_output = tmp_path / "real_chuo_shelters_sample.csv"

    result = subprocess.run(
        [
            sys.executable,
            str(EXPORT_SCRIPT),
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

    with json_output.open("r", encoding="utf-8") as handle:
        dataset = json.load(handle)
    return json_output, csv_output, dataset


def test_schema_file_exists_and_is_valid_json_schema() -> None:
    assert SCHEMA_PATH.exists()
    with SCHEMA_PATH.open("r", encoding="utf-8") as handle:
        schema = json.load(handle)
    Draft202012Validator.check_schema(schema)


def test_sample_csv_exists_and_has_required_columns() -> None:
    assert SAMPLE_CSV_PATH.exists()
    with SAMPLE_CSV_PATH.open("r", encoding="utf-8-sig", newline="") as handle:
        reader = csv.DictReader(handle)
        assert reader.fieldnames is not None
        assert REQUIRED_SAMPLE_COLUMNS.issubset(set(reader.fieldnames))
        assert sum(1 for _ in reader) >= 4


def test_export_script_can_generate_processed_json_and_csv(exported_sample: tuple[Path, Path, dict]) -> None:
    json_output, csv_output, dataset = exported_sample
    assert json_output.exists()
    assert csv_output.exists()
    assert dataset["dataset_id"] == "real_chuo_shelters_sample"
    assert dataset["coordinate_reference_system"] == "EPSG:4326"
    assert len(dataset["records"]) >= 4


def test_exported_json_validates_against_schema(exported_sample: tuple[Path, Path, dict]) -> None:
    _, _, dataset = exported_sample
    with SCHEMA_PATH.open("r", encoding="utf-8") as handle:
        schema = json.load(handle)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(dataset), key=lambda error: list(error.path))
    assert errors == []


def test_validator_passes_on_exported_sample(exported_sample: tuple[Path, Path, dict]) -> None:
    json_output, _, _ = exported_sample
    result = subprocess.run(
        [sys.executable, str(VALIDATE_SCRIPT), "--input", str(json_output)],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )
    assert result.returncode == 0, result.stdout + result.stderr
    assert "[validate] OK" in result.stdout


def test_exported_records_have_unique_ids(exported_sample: tuple[Path, Path, dict]) -> None:
    _, _, dataset = exported_sample
    ids = [record["id"] for record in dataset["records"]]
    assert len(ids) == len(set(ids))


def test_exported_records_include_unity_interface_fields(exported_sample: tuple[Path, Path, dict]) -> None:
    _, _, dataset = exported_sample
    for record in dataset["records"]:
        unity = record["unity"]
        assert isinstance(unity["prefab_hint"], str)
        assert isinstance(unity["is_entry_enabled"], bool)
        assert "estimated_stair_floors" in unity


def test_exported_sample_coordinates_are_in_chuo_tokyo_sanity_range(
    exported_sample: tuple[Path, Path, dict],
) -> None:
    _, _, dataset = exported_sample
    for record in dataset["records"]:
        assert 35.60 <= record["latitude"] <= 35.75
        assert 139.70 <= record["longitude"] <= 139.90
