from __future__ import annotations

import json
from pathlib import Path

import pytest

try:
    from jsonschema import Draft202012Validator, FormatChecker
except ImportError:  # pragma: no cover - exercised only when dependency is absent.
    Draft202012Validator = None  # type: ignore[assignment]
    FormatChecker = None  # type: ignore[assignment]


REPO_ROOT = Path(__file__).resolve().parents[2]
QUALIFICATION_ROOT = REPO_ROOT / "data_pipeline" / "qualification"

RULEBOOK_PATH = QUALIFICATION_ROOT / "evacuation_building_qualification_rulebook.json"
SCHEMA_PATH = QUALIFICATION_ROOT / "evacuation_building_qualification_schema.json"
FIXTURE_PATH = QUALIFICATION_ROOT / "sample_building_qualification_fixture.json"

REQUIRED_STATUSES = {
    "official_confirmed",
    "official_confirmed_with_review",
    "strong_candidate",
    "weak_candidate",
    "unknown",
    "not_qualified",
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


def test_rulebook_json_is_valid() -> None:
    assert RULEBOOK_PATH.exists()
    rulebook = load_json(RULEBOOK_PATH)
    assert rulebook["rulebookVersion"] == "p5-a2-qualification-rulebook-v1"
    assert isinstance(rulebook["ruleGroups"], list)
    assert len(rulebook["ruleGroups"]) == 7


def test_schema_json_is_valid() -> None:
    assert SCHEMA_PATH.exists()
    schema = load_json(SCHEMA_PATH)
    assert schema["$schema"] == "https://json-schema.org/draft/2020-12/schema"
    if Draft202012Validator is None:
        pytest.skip("jsonschema is unavailable; skipping JSON Schema meta-validation.")
    Draft202012Validator.check_schema(schema)


def test_sample_fixture_matches_schema_when_jsonschema_available() -> None:
    if Draft202012Validator is None or FormatChecker is None:
        pytest.skip("jsonschema is unavailable; skipping fixture schema validation.")
    schema = load_json(SCHEMA_PATH)
    fixture = load_json(FIXTURE_PATH)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(fixture), key=lambda error: list(error.path))
    assert errors == []


def test_status_taxonomy_includes_required_values() -> None:
    rulebook = load_json(RULEBOOK_PATH)
    statuses = {entry["status"] for entry in rulebook["statusTaxonomy"]}
    assert REQUIRED_STATUSES.issubset(statuses)

    schema = load_json(SCHEMA_PATH)
    schema_statuses = set(schema["$defs"]["qualificationStatus"]["enum"])
    assert REQUIRED_STATUSES.issubset(schema_statuses)

    fixture = load_json(FIXTURE_PATH)
    fixture_statuses = {record["qualificationStatus"] for record in fixture["records"]}
    assert REQUIRED_STATUSES.issubset(fixture_statuses)


def test_literature_report_criteria_cannot_directly_confirm_official_status() -> None:
    rulebook = load_json(RULEBOOK_PATH)
    candidate_families = set(rulebook["candidateEvidenceFamilies"])
    assert "literature_vertical_evacuation_criteria" in candidate_families
    assert "literature_accessibility_criteria" in candidate_families

    for group in rulebook["ruleGroups"]:
        if "literature_vertical_evacuation_criteria" in group["evidenceSources"]:
            if group["groupId"] == "safe_floor_or_height":
                assert group["canConfirmOfficialStatus"] is False
        if "literature_accessibility_criteria" in group["evidenceSources"]:
            assert group["canConfirmOfficialStatus"] is False

    fixture = load_json(FIXTURE_PATH)
    for record in fixture["records"]:
        evidence_families = {source["sourceFamily"] for source in record["evidenceSources"]}
        if evidence_families and evidence_families.issubset(candidate_families):
            assert record["qualificationStatus"] != "official_confirmed"


def test_official_confirmed_requires_official_evidence_family() -> None:
    fixture = load_json(FIXTURE_PATH)
    for record in fixture["records"]:
        if record["qualificationStatus"] in {"official_confirmed", "official_confirmed_with_review"}:
            evidence_families = {source["sourceFamily"] for source in record["evidenceSources"]}
            assert evidence_families.intersection(OFFICIAL_EVIDENCE_FAMILIES)


def test_manual_review_true_for_uncertain_nearest_unmatched_low_confidence_examples() -> None:
    fixture = load_json(FIXTURE_PATH)
    for record in fixture["records"]:
        uncertain_match = record["matchMethod"] in {"nearest", "unmatched", "manual_override"}
        uncertain_confidence = record["confidence"] in {"low", "unknown"}
        if uncertain_match or uncertain_confidence:
            assert record["manualReviewNeeded"] is True


def test_all_sample_records_include_qualification_status_and_confidence() -> None:
    fixture = load_json(FIXTURE_PATH)
    assert len(fixture["records"]) >= 7
    for record in fixture["records"]:
        assert record["qualificationStatus"] in REQUIRED_STATUSES
        assert record["confidence"] in {"high", "medium", "low", "unknown"}
