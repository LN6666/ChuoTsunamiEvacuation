from __future__ import annotations

import argparse
import csv
import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator

SCRIPT_DIR = Path(__file__).resolve().parent
if str(SCRIPT_DIR) not in sys.path:
    sys.path.insert(0, str(SCRIPT_DIR))

from export_unity_shelters import ExportError, row_to_record, validate_dataset, write_csv, write_json


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"

DEFAULT_INPUT = PIPELINE_ROOT / "samples" / "sample_manual_shelter_source.csv"
DEFAULT_MAPPING = PIPELINE_ROOT / "sources" / "shelter_source_mapping_template.json"
DEFAULT_MAPPING_SCHEMA = PIPELINE_ROOT / "schemas" / "source_shelter_mapping_schema.json"
DEFAULT_REAL_SHELTER_SCHEMA = PIPELINE_ROOT / "schemas" / "real_shelter_schema.json"
DEFAULT_JSON_OUTPUT = PIPELINE_ROOT / "processed" / "manual_shelter_ingestion_sample.json"
DEFAULT_CSV_OUTPUT = PIPELINE_ROOT / "processed" / "manual_shelter_ingestion_sample.csv"

EXPORT_FIELDS = [
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
]


def resolve_path(path_value: str | Path) -> Path:
    path = Path(path_value)
    if path.is_absolute():
        return path
    return Path.cwd() / path


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise ExportError(f"JSON file not found: {path}")
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def validate_mapping(mapping: dict[str, Any], schema_path: Path) -> None:
    schema = load_json(schema_path)
    Draft202012Validator.check_schema(schema)
    validator = Draft202012Validator(schema)
    errors = sorted(validator.iter_errors(mapping), key=lambda error: list(error.path))
    if errors:
        first = errors[0]
        location = ".".join(str(part) for part in first.path) or "<root>"
        raise ExportError(f"Mapping does not match schema at {location}: {first.message}")


def load_csv_rows(input_path: Path) -> tuple[list[str], list[dict[str, str]]]:
    if not input_path.exists():
        raise ExportError(f"Input CSV not found: {input_path}")
    with input_path.open("r", encoding="utf-8-sig", newline="") as handle:
        reader = csv.DictReader(handle)
        if not reader.fieldnames:
            raise ExportError(f"Input CSV has no header row: {input_path}")
        rows = list(reader)
    if not rows:
        raise ExportError(f"Input CSV contains no data rows: {input_path}")
    return list(reader.fieldnames), rows


def ensure_source_columns_exist(source_columns: list[str], mapping: dict[str, Any]) -> None:
    field_mappings = mapping["field_mappings"]
    missing = sorted({column for column in field_mappings.values() if column not in source_columns})
    if missing:
        raise ExportError(f"Input CSV is missing mapped source columns: {', '.join(missing)}")


def normalize_value(value: Any, null_markers: set[str]) -> str:
    if value is None:
        return ""
    text = str(value).strip()
    if text.lower() in null_markers:
        return ""
    return text


def mapped_field_value(row: dict[str, str], mapping: dict[str, Any], export_field: str) -> str:
    field_mappings = mapping["field_mappings"]
    defaults = mapping.get("defaults", {})
    null_markers = {str(marker).lower() for marker in mapping.get("null_markers", [])}

    source_column = field_mappings.get(export_field)
    if source_column:
        value = normalize_value(row.get(source_column), null_markers)
        if value != "":
            return value

    if export_field in defaults:
        return normalize_value(defaults[export_field], null_markers)
    return ""


def apply_value_maps(canonical_row: dict[str, str], mapping: dict[str, Any]) -> dict[str, str]:
    type_value_map = {str(key).lower(): value for key, value in mapping.get("type_value_map", {}).items()}
    boolean_value_map = {str(key).lower(): value for key, value in mapping.get("boolean_value_map", {}).items()}

    record_type = canonical_row.get("type", "").lower()
    if record_type in type_value_map:
        canonical_row["type"] = type_value_map[record_type]

    entry_enabled = canonical_row.get("is_entry_enabled", "").lower()
    if entry_enabled in boolean_value_map:
        canonical_row["is_entry_enabled"] = boolean_value_map[entry_enabled]

    return canonical_row


def map_rows_to_records(rows: list[dict[str, str]], mapping: dict[str, Any]) -> list[dict[str, Any]]:
    records = []
    for row_number, row in enumerate(rows, start=2):
        canonical_row = {
            export_field: mapped_field_value(row, mapping, export_field)
            for export_field in EXPORT_FIELDS
        }
        canonical_row = apply_value_maps(canonical_row, mapping)
        records.append(row_to_record(canonical_row, row_number))
    return records


def build_dataset(records: list[dict[str, Any]], mapping: dict[str, Any]) -> dict[str, Any]:
    generated_at = datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")
    return {
        "dataset_id": mapping["dataset_id"],
        "generated_at": generated_at,
        "source_manifest": mapping["source_manifest"],
        "coordinate_reference_system": mapping["coordinate_reference_system"],
        "records": records,
    }


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Map a local manually supplied shelter CSV into the Phase 3 shelter schema.")
    parser.add_argument("--input", default=str(DEFAULT_INPUT), help="Local input CSV path.")
    parser.add_argument("--mapping", default=str(DEFAULT_MAPPING), help="Shelter source mapping JSON path.")
    parser.add_argument("--json-output", default=str(DEFAULT_JSON_OUTPUT), help="Output shelter JSON path.")
    parser.add_argument("--csv-output", default=str(DEFAULT_CSV_OUTPUT), help="Output shelter CSV path.")
    parser.add_argument("--schema", default=str(DEFAULT_REAL_SHELTER_SCHEMA), help="Real shelter JSON Schema path.")
    parser.add_argument("--mapping-schema", default=str(DEFAULT_MAPPING_SCHEMA), help="Mapping JSON Schema path.")
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    input_path = resolve_path(args.input)
    mapping_path = resolve_path(args.mapping)
    mapping_schema_path = resolve_path(args.mapping_schema)
    schema_path = resolve_path(args.schema)
    json_output = resolve_path(args.json_output)
    csv_output = resolve_path(args.csv_output)

    try:
        mapping = load_json(mapping_path)
        validate_mapping(mapping, mapping_schema_path)
        source_columns, rows = load_csv_rows(input_path)
        ensure_source_columns_exist(source_columns, mapping)
        records = map_rows_to_records(rows, mapping)
        dataset = build_dataset(records, mapping)
        validate_dataset(dataset, schema_path)
        write_json(dataset, json_output)
        write_csv(records, csv_output)
    except Exception as exc:
        print(f"[ingest-shelters] ERROR: {exc}", file=sys.stderr)
        return 1

    print(f"[ingest-shelters] Wrote {len(records)} records to {json_output}")
    print(f"[ingest-shelters] Wrote CSV to {csv_output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
