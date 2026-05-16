from __future__ import annotations

import argparse
import csv
import json
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator, FormatChecker


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"

DEFAULT_INPUT = PIPELINE_ROOT / "samples" / "sample_raw_shelters.csv"
DEFAULT_JSON_OUTPUT = PIPELINE_ROOT / "processed" / "real_chuo_shelters_sample.json"
DEFAULT_CSV_OUTPUT = PIPELINE_ROOT / "processed" / "real_chuo_shelters_sample.csv"
DEFAULT_SCHEMA = PIPELINE_ROOT / "schemas" / "real_shelter_schema.json"
SOURCE_MANIFEST_RELATIVE = "data_pipeline/sources/source_manifest.json"

REQUIRED_COLUMNS = [
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

CSV_OUTPUT_COLUMNS = [
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

NULL_MARKERS = {"", "null", "none", "n/a", "na"}
TRUE_MARKERS = {"true", "1", "yes", "y"}
FALSE_MARKERS = {"false", "0", "no", "n"}


class ExportError(ValueError):
    """Raised when input data cannot be exported safely."""


def resolve_path(path_value: str | Path) -> Path:
    path = Path(path_value)
    if path.is_absolute():
        return path
    return Path.cwd() / path


def clean_string(value: str | None) -> str:
    return "" if value is None else str(value).strip()


def null_if_blank(value: str | None) -> str | None:
    text = clean_string(value)
    if text.lower() in NULL_MARKERS:
        return None
    return text


def parse_float(value: str | None, field: str, row_number: int) -> float:
    text = clean_string(value)
    if text.lower() in NULL_MARKERS:
        raise ExportError(f"Row {row_number}: field '{field}' is required and cannot be blank.")
    try:
        return float(text)
    except ValueError as exc:
        raise ExportError(f"Row {row_number}: field '{field}' must be a number, got '{text}'.") from exc


def parse_nullable_float(value: str | None, field: str, row_number: int) -> float | None:
    text = clean_string(value)
    if text.lower() in NULL_MARKERS:
        return None
    try:
        return float(text)
    except ValueError as exc:
        raise ExportError(f"Row {row_number}: field '{field}' must be a number or blank, got '{text}'.") from exc


def parse_nullable_int(value: str | None, field: str, row_number: int) -> int | None:
    text = clean_string(value)
    if text.lower() in NULL_MARKERS:
        return None
    try:
        parsed = int(text)
    except ValueError as exc:
        raise ExportError(f"Row {row_number}: field '{field}' must be an integer or blank, got '{text}'.") from exc
    if parsed < 0:
        raise ExportError(f"Row {row_number}: field '{field}' must be non-negative.")
    return parsed


def parse_bool(value: str | None, field: str, row_number: int) -> bool:
    text = clean_string(value).lower()
    if text in TRUE_MARKERS:
        return True
    if text in FALSE_MARKERS:
        return False
    raise ExportError(f"Row {row_number}: field '{field}' must be true/false, got '{clean_string(value)}'.")


def require_text(row: dict[str, str], field: str, row_number: int) -> str:
    text = clean_string(row.get(field))
    if text == "":
        raise ExportError(f"Row {row_number}: required field '{field}' is blank.")
    return text


def validate_columns(fieldnames: list[str] | None) -> None:
    if not fieldnames:
        raise ExportError("Input CSV has no header row.")
    missing = [column for column in REQUIRED_COLUMNS if column not in fieldnames]
    if missing:
        raise ExportError(f"Input CSV is missing required columns: {', '.join(missing)}")


def row_to_record(row: dict[str, str], row_number: int) -> dict[str, Any]:
    return {
        "id": require_text(row, "id", row_number),
        "name": require_text(row, "name", row_number),
        "type": require_text(row, "type", row_number),
        "latitude": parse_float(row.get("latitude"), "latitude", row_number),
        "longitude": parse_float(row.get("longitude"), "longitude", row_number),
        "address": clean_string(row.get("address")),
        "capacity": parse_nullable_int(row.get("capacity"), "capacity", row_number),
        "floors_available": parse_nullable_int(row.get("floors_available"), "floors_available", row_number),
        "elevation_m": parse_nullable_float(row.get("elevation_m"), "elevation_m", row_number),
        "source": require_text(row, "source", row_number),
        "source_url": null_if_blank(row.get("source_url")),
        "source_updated_at": null_if_blank(row.get("source_updated_at")),
        "notes": clean_string(row.get("notes")),
        "unity": {
            "prefab_hint": require_text(row, "prefab_hint", row_number),
            "is_entry_enabled": parse_bool(row.get("is_entry_enabled"), "is_entry_enabled", row_number),
            "estimated_stair_floors": parse_nullable_int(
                row.get("estimated_stair_floors"),
                "estimated_stair_floors",
                row_number,
            ),
        },
    }


def load_records(input_path: Path) -> list[dict[str, Any]]:
    if not input_path.exists():
        raise ExportError(f"Input CSV not found: {input_path}")

    with input_path.open("r", encoding="utf-8-sig", newline="") as handle:
        reader = csv.DictReader(handle)
        validate_columns(reader.fieldnames)
        records = [row_to_record(row, index) for index, row in enumerate(reader, start=2)]

    if not records:
        raise ExportError(f"Input CSV contains no data rows: {input_path}")
    return records


def build_dataset(records: list[dict[str, Any]]) -> dict[str, Any]:
    generated_at = datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")
    return {
        "dataset_id": "real_chuo_shelters_sample",
        "generated_at": generated_at,
        "source_manifest": SOURCE_MANIFEST_RELATIVE,
        "coordinate_reference_system": "EPSG:4326",
        "records": records,
    }


def validate_dataset(dataset: dict[str, Any], schema_path: Path) -> None:
    if not schema_path.exists():
        raise ExportError(f"Schema file not found: {schema_path}")
    with schema_path.open("r", encoding="utf-8") as handle:
        schema = json.load(handle)
    Draft202012Validator.check_schema(schema)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(dataset), key=lambda error: list(error.path))
    if errors:
        first = errors[0]
        location = ".".join(str(part) for part in first.path) or "<root>"
        raise ExportError(f"Exported dataset does not match schema at {location}: {first.message}")


def write_json(dataset: dict[str, Any], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(dataset, handle, ensure_ascii=False, indent=2)
        handle.write("\n")


def record_to_csv_row(record: dict[str, Any]) -> dict[str, Any]:
    unity = record["unity"]
    return {
        "id": record["id"],
        "name": record["name"],
        "type": record["type"],
        "latitude": record["latitude"],
        "longitude": record["longitude"],
        "address": record["address"],
        "capacity": "" if record["capacity"] is None else record["capacity"],
        "floors_available": "" if record["floors_available"] is None else record["floors_available"],
        "elevation_m": "" if record["elevation_m"] is None else record["elevation_m"],
        "source": record["source"],
        "source_url": "" if record["source_url"] is None else record["source_url"],
        "source_updated_at": "" if record["source_updated_at"] is None else record["source_updated_at"],
        "notes": record["notes"],
        "prefab_hint": unity["prefab_hint"],
        "is_entry_enabled": str(unity["is_entry_enabled"]).lower(),
        "estimated_stair_floors": "" if unity["estimated_stair_floors"] is None else unity["estimated_stair_floors"],
    }


def write_csv(records: list[dict[str, Any]], output_path: Path) -> None:
    output_path.parent.mkdir(parents=True, exist_ok=True)
    with output_path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=CSV_OUTPUT_COLUMNS, lineterminator="\n")
        writer.writeheader()
        for record in records:
            writer.writerow(record_to_csv_row(record))


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Export Phase 3 sample shelter CSV to Unity-ready JSON/CSV.")
    parser.add_argument("--input", default=str(DEFAULT_INPUT), help="Input shelter CSV path.")
    parser.add_argument("--json-output", default=str(DEFAULT_JSON_OUTPUT), help="Output JSON path.")
    parser.add_argument("--csv-output", default=str(DEFAULT_CSV_OUTPUT), help="Output CSV path.")
    parser.add_argument("--schema", default=str(DEFAULT_SCHEMA), help="JSON Schema path for export validation.")
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    input_path = resolve_path(args.input)
    json_output = resolve_path(args.json_output)
    csv_output = resolve_path(args.csv_output)
    schema_path = resolve_path(args.schema)

    try:
        print(f"[export] Reading {input_path}")
        records = load_records(input_path)
        dataset = build_dataset(records)
        validate_dataset(dataset, schema_path)
        write_json(dataset, json_output)
        write_csv(records, csv_output)
    except Exception as exc:
        print(f"[export] ERROR: {exc}", file=sys.stderr)
        return 1

    print(f"[export] Wrote {len(records)} records to {json_output}")
    print(f"[export] Wrote CSV to {csv_output}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
