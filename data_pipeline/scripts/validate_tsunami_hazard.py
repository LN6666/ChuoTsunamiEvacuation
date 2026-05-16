from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any

from jsonschema import Draft202012Validator, FormatChecker


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"

DEFAULT_INPUT = PIPELINE_ROOT / "samples" / "sample_tsunami_hazard_zones.json"
DEFAULT_SCHEMA = PIPELINE_ROOT / "schemas" / "tsunami_hazard_schema.json"

REQUIRED_SOURCE_FIELDS = [
    "source_id",
    "source_family",
    "source_name",
    "organization",
    "official_status",
    "is_official_primary",
    "source_url",
    "source_updated_at",
    "notes",
]


class HazardValidationError(ValueError):
    """Raised when tsunami hazard fixture validation fails."""


def resolve_path(path_value: str | Path) -> Path:
    path = Path(path_value)
    if path.is_absolute():
        return path
    return Path.cwd() / path


def load_json(path: Path) -> Any:
    if not path.exists():
        raise HazardValidationError(f"File not found: {path}")
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def validate_schema(dataset: dict[str, Any], schema: dict[str, Any]) -> None:
    Draft202012Validator.check_schema(schema)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(dataset), key=lambda error: list(error.path))
    if errors:
        messages = []
        for error in errors[:10]:
            location = ".".join(str(part) for part in error.path) or "<root>"
            messages.append(f"{location}: {error.message}")
        raise HazardValidationError("JSON Schema validation failed:\n" + "\n".join(messages))


def validate_extra_sanity(dataset: dict[str, Any]) -> None:
    if not dataset.get("coordinate_reference_system"):
        raise HazardValidationError("coordinate_reference_system is required.")

    source = dataset.get("source")
    if not isinstance(source, dict):
        raise HazardValidationError("source metadata object is required.")
    for field in REQUIRED_SOURCE_FIELDS:
        if field not in source:
            raise HazardValidationError(f"source metadata field missing: {field}")

    if source["source_family"] == "paper_or_secondary_reference" and source["is_official_primary"] is True:
        raise HazardValidationError("paper_or_secondary_reference sources cannot be official primary data.")
    if source["official_status"] == "secondary_reference_only" and source["is_official_primary"] is True:
        raise HazardValidationError("secondary reference sources cannot be marked official primary.")

    zones = dataset.get("zones")
    if not isinstance(zones, list) or not zones:
        raise HazardValidationError("zones array is empty or missing.")

    seen_zone_ids: set[str] = set()
    for zone in zones:
        zone_id = zone.get("zone_id", "<unknown>")
        if zone_id in seen_zone_ids:
            raise HazardValidationError(f"duplicate zone_id found: {zone_id}")
        seen_zone_ids.add(zone_id)

        if not zone.get("geometry_type"):
            raise HazardValidationError(f"zone {zone_id}: geometry_type is required.")
        geometry = zone.get("geometry")
        if not isinstance(geometry, dict) or not geometry.get("type"):
            raise HazardValidationError(f"zone {zone_id}: geometry type is required.")

        for field in ["inundation_depth_m", "tsunami_height_m"]:
            value = zone.get(field)
            if value is not None and not isinstance(value, (int, float)):
                raise HazardValidationError(f"zone {zone_id}: {field} must be numeric or null.")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Validate Phase 3 tsunami hazard fixture JSON.")
    parser.add_argument("--input", default=str(DEFAULT_INPUT), help="Input tsunami hazard JSON path.")
    parser.add_argument("--schema", default=str(DEFAULT_SCHEMA), help="Tsunami hazard JSON Schema path.")
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    input_path = resolve_path(args.input)
    schema_path = resolve_path(args.schema)

    try:
        dataset = load_json(input_path)
        schema = load_json(schema_path)
        validate_schema(dataset, schema)
        validate_extra_sanity(dataset)
    except Exception as exc:
        print(f"[validate-hazard] ERROR: {exc}", file=sys.stderr)
        return 1

    print(f"[validate-hazard] OK: {len(dataset['zones'])} hazard zones passed schema and sanity checks.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
