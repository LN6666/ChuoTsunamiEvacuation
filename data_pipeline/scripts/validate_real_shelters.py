from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator, FormatChecker


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"

DEFAULT_INPUT = PIPELINE_ROOT / "processed" / "real_chuo_shelters_sample.json"
DEFAULT_SCHEMA = PIPELINE_ROOT / "schemas" / "real_shelter_schema.json"

CHUO_LATITUDE_RANGE = (35.60, 35.75)
CHUO_LONGITUDE_RANGE = (139.70, 139.90)
UNITY_REQUIRED_FIELDS = ["prefab_hint", "is_entry_enabled", "estimated_stair_floors"]


class ValidationError(ValueError):
    """Raised when exported shelter data fails validation."""


def resolve_path(path_value: str | Path) -> Path:
    path = Path(path_value)
    if path.is_absolute():
        return path
    return Path.cwd() / path


def load_json(path: Path) -> Any:
    if not path.exists():
        raise ValidationError(f"File not found: {path}")
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def validate_json_schema(dataset: dict[str, Any], schema: dict[str, Any]) -> None:
    Draft202012Validator.check_schema(schema)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(dataset), key=lambda error: list(error.path))
    if errors:
        messages = []
        for error in errors[:10]:
            location = ".".join(str(part) for part in error.path) or "<root>"
            messages.append(f"{location}: {error.message}")
        raise ValidationError("JSON Schema validation failed:\n" + "\n".join(messages))


def validate_extra_sanity(dataset: dict[str, Any]) -> None:
    records = dataset.get("records")
    if not isinstance(records, list) or not records:
        raise ValidationError("records array is empty or missing.")

    seen_ids: set[str] = set()
    duplicate_ids: set[str] = set()

    for index, record in enumerate(records):
        record_id = record.get("id")
        if record_id in seen_ids:
            duplicate_ids.add(str(record_id))
        seen_ids.add(str(record_id))

        latitude = record.get("latitude")
        longitude = record.get("longitude")
        if not CHUO_LATITUDE_RANGE[0] <= latitude <= CHUO_LATITUDE_RANGE[1]:
            raise ValidationError(
                f"Record {record_id}: latitude {latitude} is outside sample Chuo/Tokyo range "
                f"{CHUO_LATITUDE_RANGE[0]}..{CHUO_LATITUDE_RANGE[1]}."
            )
        if not CHUO_LONGITUDE_RANGE[0] <= longitude <= CHUO_LONGITUDE_RANGE[1]:
            raise ValidationError(
                f"Record {record_id}: longitude {longitude} is outside sample Chuo/Tokyo range "
                f"{CHUO_LONGITUDE_RANGE[0]}..{CHUO_LONGITUDE_RANGE[1]}."
            )

        unity = record.get("unity")
        if not isinstance(unity, dict):
            raise ValidationError(f"Record {record_id}: unity object is missing.")
        missing_unity = [field for field in UNITY_REQUIRED_FIELDS if field not in unity]
        if missing_unity:
            raise ValidationError(f"Record {record_id}: unity fields missing: {', '.join(missing_unity)}")

        for field in ["source", "source_url", "source_updated_at"]:
            if field not in record:
                raise ValidationError(f"Record {record_id}: source metadata field missing: {field}")
        if not isinstance(record.get("source"), str) or not record["source"].strip():
            raise ValidationError(f"Record {record_id}: source must be a non-empty string.")

    if duplicate_ids:
        raise ValidationError(f"Duplicate shelter ids found: {', '.join(sorted(duplicate_ids))}")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Validate Phase 3 exported real shelter JSON.")
    parser.add_argument("--input", default=str(DEFAULT_INPUT), help="Exported shelter JSON path.")
    parser.add_argument("--schema", default=str(DEFAULT_SCHEMA), help="JSON Schema path.")
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    input_path = resolve_path(args.input)
    schema_path = resolve_path(args.schema)

    try:
        dataset = load_json(input_path)
        schema = load_json(schema_path)
        validate_json_schema(dataset, schema)
        validate_extra_sanity(dataset)
    except Exception as exc:
        print(f"[validate] ERROR: {exc}", file=sys.stderr)
        return 1

    print(f"[validate] OK: {len(dataset['records'])} records passed schema and sanity checks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
