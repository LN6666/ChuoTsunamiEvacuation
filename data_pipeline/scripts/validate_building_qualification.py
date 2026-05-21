from __future__ import annotations

import argparse
import json
import sys
from pathlib import Path
from typing import Any

try:
    from jsonschema import Draft202012Validator, FormatChecker
except ImportError:  # pragma: no cover - exercised only when dependency is absent.
    Draft202012Validator = None  # type: ignore[assignment]
    FormatChecker = None  # type: ignore[assignment]


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"

DEFAULT_INPUT = PIPELINE_ROOT / "qualification" / "sample_building_qualification_fixture.json"
DEFAULT_SCHEMA = PIPELINE_ROOT / "qualification" / "evacuation_building_qualification_schema.json"

OFFICIAL_EVIDENCE_FAMILIES = {
    "official_shelter_facility",
    "official_evacuation_building",
    "official_hazard_map",
    "official_disaster_prevention_plan",
}

REVIEW_MATCH_METHODS = {"nearest", "unmatched", "manual_override"}


class ValidationError(ValueError):
    """Raised when building qualification data fails validation."""


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


def ensure_jsonschema_available() -> None:
    if Draft202012Validator is None or FormatChecker is None:
        raise ValidationError(
            "JSON Schema validation requires the optional 'jsonschema' Python package. "
            "Install project-approved dependencies in a later environment setup step; P5-A2 does not install dependencies."
        )


def validate_json_schema(dataset: dict[str, Any], schema: dict[str, Any]) -> None:
    ensure_jsonschema_available()
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

    for record in records:
        qualification_id = record.get("qualificationId")
        if qualification_id in seen_ids:
            duplicate_ids.add(str(qualification_id))
        seen_ids.add(str(qualification_id))

        status = record.get("qualificationStatus")
        evidence_sources = record.get("evidenceSources", [])
        evidence_families = {
            source.get("sourceFamily")
            for source in evidence_sources
            if isinstance(source, dict)
        }

        if status in {"official_confirmed", "official_confirmed_with_review"}:
            if not evidence_families.intersection(OFFICIAL_EVIDENCE_FAMILIES):
                raise ValidationError(
                    f"Record {qualification_id}: {status} requires at least one official evidence family."
                )

        if status == "official_confirmed" and record.get("manualReviewNeeded") is True:
            raise ValidationError(
                f"Record {qualification_id}: official_confirmed should not require manual review in this foundation."
            )

        if record.get("matchMethod") in REVIEW_MATCH_METHODS and record.get("manualReviewNeeded") is not True:
            raise ValidationError(
                f"Record {qualification_id}: {record.get('matchMethod')} match requires manualReviewNeeded=true."
            )

        if record.get("confidence") in {"low", "unknown"} and record.get("manualReviewNeeded") is not True:
            raise ValidationError(
                f"Record {qualification_id}: low/unknown confidence requires manualReviewNeeded=true."
            )

        if status in {"strong_candidate", "weak_candidate"}:
            official_status = record.get("officialDesignationStatus")
            if official_status not in {"not_official", "unknown", "not_evaluated", None}:
                raise ValidationError(
                    f"Record {qualification_id}: candidate records must not claim official designation."
                )

    if duplicate_ids:
        raise ValidationError(f"Duplicate qualification ids found: {', '.join(sorted(duplicate_ids))}")


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Validate Phase 5 evacuation building qualification JSON.")
    parser.add_argument("--input", default=str(DEFAULT_INPUT), help="Building qualification JSON path.")
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

    print(f"[validate] OK: {len(dataset['records'])} building qualification records passed validation.")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
