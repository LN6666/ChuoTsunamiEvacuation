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

RULEBOOK_PATH = QUALIFICATION_ROOT / "highrise_humanitarian_candidate_rulebook.json"
SCHEMA_PATH = QUALIFICATION_ROOT / "highrise_humanitarian_candidate_schema.json"
SAMPLE_PATH = QUALIFICATION_ROOT / "highrise_humanitarian_candidates_sample.json"
SOURCE_PLAN_PATH = QUALIFICATION_ROOT / "highrise_humanitarian_candidate_sources_plan.json"

REQUIRED_STATUSES = {
    "official_confirmed",
    "official_confirmed_with_review",
    "humanitarian_strong_candidate",
    "humanitarian_candidate_with_review",
    "humanitarian_weak_candidate",
    "unknown",
    "not_recommended",
}

OFFICIAL_CANDIDATE_LAYER = "official"
HUMANITARIAN_CANDIDATE_LAYER = "humanitarian_candidate"
OFFICIAL_SOURCE_STATUSES = {"official_primary", "official_reference"}
OFFICIAL_DESIGNATION_STATUSES = {"official_designated", "official_designated_with_review"}
OFFICIAL_CANDIDATE_STATUSES = {"official_confirmed", "official_confirmed_with_review"}
HUMANITARIAN_STATUSES = {
    "humanitarian_strong_candidate",
    "humanitarian_candidate_with_review",
    "humanitarian_weak_candidate",
}
EXPECTED_CANDIDATE_LAYER_BY_STATUS = {
    "official_confirmed": OFFICIAL_CANDIDATE_LAYER,
    "official_confirmed_with_review": OFFICIAL_CANDIDATE_LAYER,
    "humanitarian_strong_candidate": HUMANITARIAN_CANDIDATE_LAYER,
    "humanitarian_candidate_with_review": HUMANITARIAN_CANDIDATE_LAYER,
    "humanitarian_weak_candidate": HUMANITARIAN_CANDIDATE_LAYER,
    "unknown": HUMANITARIAN_CANDIDATE_LAYER,
    "not_recommended": HUMANITARIAN_CANDIDATE_LAYER,
}


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def test_rulebook_and_source_plan_are_valid_json_with_expected_policy() -> None:
    rulebook = load_json(RULEBOOK_PATH)
    source_plan = load_json(SOURCE_PLAN_PATH)

    assert rulebook["rulebookVersion"] == "p5-f-highrise-humanitarian-candidates-v1"
    assert rulebook["layerSeparation"]["candidateLayerField"] == "candidateLayer"
    assert set(rulebook["layerSeparation"]["candidateLayerValues"]) == {
        OFFICIAL_CANDIDATE_LAYER,
        HUMANITARIAN_CANDIDATE_LAYER,
    }
    assert rulebook["publicAccessPolicy"]["unknownAccessIsHardExclusion"] is False
    assert rulebook["publicAccessPolicy"]["unknownAccessManualReviewRequired"] is True
    assert "Do not label non-official candidate buildings" in rulebook["layerSeparation"]["nonClaim"]
    conservative_policy = rulebook["scoreModel"]["conservativeScoringPolicy"]
    assert "Missing height, floor, capacity, route, seismic, public access, or management evidence" in conservative_policy["missingCriticalEvidenceRule"]
    assert "publicAccessStatus" in conservative_policy["manualReviewEscalationFields"]
    assert "managementAgreementStatus" in conservative_policy["manualReviewEscalationFields"]
    assert "seismicEvidenceLevel" in conservative_policy["manualReviewEscalationFields"]
    assert "can never upgrade candidateLayer to official" in conservative_policy["officialSeparationRule"]

    assert source_plan["planVersion"] == "p5-f-source-plan-v1"
    policy = source_plan["collectionPolicy"]
    assert policy["performCollectionInP5F"] is False
    assert policy["largeDownloadsAllowedInP5F"] is False
    assert policy["aggressiveScrapingAllowed"] is False
    assert policy["rawDataCommitAllowed"] is False


def test_schema_json_is_valid_when_jsonschema_available() -> None:
    schema = load_json(SCHEMA_PATH)
    assert schema["$schema"] == "https://json-schema.org/draft/2020-12/schema"
    assert set(schema["$defs"]["candidateLayer"]["enum"]) == {
        OFFICIAL_CANDIDATE_LAYER,
        HUMANITARIAN_CANDIDATE_LAYER,
    }
    required_fields = set(schema["$defs"]["candidateRecord"]["required"])
    assert "candidateLayer" in required_fields
    assert "reviewRisks" in required_fields
    if Draft202012Validator is None:
        pytest.skip("jsonschema is unavailable; skipping JSON Schema meta-validation.")
    Draft202012Validator.check_schema(schema)


def test_sample_fixture_validates_against_schema_when_jsonschema_available() -> None:
    if Draft202012Validator is None or FormatChecker is None:
        pytest.skip("jsonschema is unavailable; skipping fixture schema validation.")

    schema = load_json(SCHEMA_PATH)
    fixture = load_json(SAMPLE_PATH)
    validator = Draft202012Validator(schema, format_checker=FormatChecker())
    errors = sorted(validator.iter_errors(fixture), key=lambda error: list(error.path))
    assert errors == []


def test_required_statuses_exist_in_schema_rulebook_and_sample() -> None:
    schema = load_json(SCHEMA_PATH)
    rulebook = load_json(RULEBOOK_PATH)
    fixture = load_json(SAMPLE_PATH)

    schema_statuses = set(schema["$defs"]["humanitarianCandidateStatus"]["enum"])
    rulebook_statuses = {entry["status"] for entry in rulebook["statusTaxonomy"]}
    sample_statuses = {record["humanitarianCandidateStatus"] for record in fixture["records"]}

    assert REQUIRED_STATUSES.issubset(schema_statuses)
    assert REQUIRED_STATUSES.issubset(rulebook_statuses)
    assert REQUIRED_STATUSES.issubset(sample_statuses)


def test_official_and_humanitarian_layers_are_distinct_in_rulebook() -> None:
    rulebook = load_json(RULEBOOK_PATH)
    by_status = {entry["status"]: entry for entry in rulebook["statusTaxonomy"]}

    for status in {"official_confirmed", "official_confirmed_with_review"}:
        assert by_status[status]["layer"] == "official"
        assert by_status[status]["candidateLayer"] == OFFICIAL_CANDIDATE_LAYER
        assert by_status[status]["requiresOfficialEvidence"] is True

    for status in HUMANITARIAN_STATUSES:
        entry = by_status[status]
        assert entry["layer"] == "humanitarian_candidate"
        assert entry["candidateLayer"] == HUMANITARIAN_CANDIDATE_LAYER
        assert entry["requiresOfficialEvidence"] is False
        assert "official_designated" not in entry["allowedOfficialDesignationStatuses"]

    assert by_status["unknown"]["candidateLayer"] == HUMANITARIAN_CANDIDATE_LAYER
    assert by_status["not_recommended"]["candidateLayer"] == HUMANITARIAN_CANDIDATE_LAYER


def test_official_statuses_require_official_designation_and_official_source() -> None:
    fixture = load_json(SAMPLE_PATH)
    for record in fixture["records"]:
        status = record["humanitarianCandidateStatus"]
        if status in OFFICIAL_CANDIDATE_STATUSES:
            assert record["candidateLayer"] == OFFICIAL_CANDIDATE_LAYER
            assert record["officialDesignationStatus"] in OFFICIAL_DESIGNATION_STATUSES
            source_statuses = {source["officialStatus"] for source in record["sourceRefs"]}
            assert source_statuses.intersection(OFFICIAL_SOURCE_STATUSES)


def test_candidate_layer_field_separates_official_and_humanitarian_records() -> None:
    fixture = load_json(SAMPLE_PATH)
    for record in fixture["records"]:
        status = record["humanitarianCandidateStatus"]
        assert record["candidateLayer"] == EXPECTED_CANDIDATE_LAYER_BY_STATUS[status]

        if record["candidateLayer"] == OFFICIAL_CANDIDATE_LAYER:
            assert status in OFFICIAL_CANDIDATE_STATUSES
            assert record["officialDesignationStatus"] in OFFICIAL_DESIGNATION_STATUSES
            continue

        assert status not in OFFICIAL_CANDIDATE_STATUSES
        assert record["officialDesignationStatus"] not in OFFICIAL_DESIGNATION_STATUSES


def test_non_official_humanitarian_candidates_do_not_claim_official_designation() -> None:
    fixture = load_json(SAMPLE_PATH)
    for record in fixture["records"]:
        if record["humanitarianCandidateStatus"] in HUMANITARIAN_STATUSES:
            assert record["candidateLayer"] == HUMANITARIAN_CANDIDATE_LAYER
            assert record["officialDesignationStatus"] not in OFFICIAL_DESIGNATION_STATUSES
            source_statuses = {source["officialStatus"] for source in record["sourceRefs"]}
            assert not source_statuses.intersection(OFFICIAL_SOURCE_STATUSES)


def test_strong_humanitarian_candidate_cannot_be_labeled_official() -> None:
    fixture = load_json(SAMPLE_PATH)
    strong_candidate = next(
        record
        for record in fixture["records"]
        if record["humanitarianCandidateStatus"] == "humanitarian_strong_candidate"
    )

    assert strong_candidate["candidateLayer"] == HUMANITARIAN_CANDIDATE_LAYER
    assert strong_candidate["officialDesignationStatus"] == "not_official"
    assert "non_official_status" in strong_candidate["reviewRisks"]
    source_statuses = {source["officialStatus"] for source in strong_candidate["sourceRefs"]}
    assert not source_statuses.intersection(OFFICIAL_SOURCE_STATUSES)


def test_unknown_public_access_does_not_automatically_exclude_humanitarian_candidate() -> None:
    rulebook = load_json(RULEBOOK_PATH)
    fixture = load_json(SAMPLE_PATH)
    assert rulebook["publicAccessPolicy"]["unknownAccessIsHardExclusion"] is False
    assert rulebook["publicAccessPolicy"]["unknownAccessManualReviewRequired"] is True

    review_candidate = next(
        record
        for record in fixture["records"]
        if record["candidateId"] == "p5f_sample_humanitarian_review_004"
    )
    assert review_candidate["publicAccessStatus"] == "unknown"
    assert review_candidate["humanitarianCandidateStatus"] == "humanitarian_candidate_with_review"

    unknown_access_candidates = [
        record
        for record in fixture["records"]
        if record["publicAccessStatus"] == "unknown"
        and record["humanitarianCandidateStatus"].startswith("humanitarian_")
    ]

    assert unknown_access_candidates
    for record in unknown_access_candidates:
        assert record["humanitarianCandidateStatus"] != "not_recommended"
        assert record["manualReviewNeeded"] is True
        assert "public access unknown" in record["reviewTriggers"]
        assert "public_access_unknown" in record["reviewRisks"]
        assert "public access unknown" in " ".join(record["warnings"]).lower()


def test_manual_review_true_when_access_or_seismic_evidence_is_unknown() -> None:
    fixture = load_json(SAMPLE_PATH)
    for record in fixture["records"]:
        has_unknown_access = record["publicAccessStatus"] == "unknown"
        has_unknown_seismic = record["seismicEvidenceLevel"] == "unknown"
        if has_unknown_access or has_unknown_seismic:
            assert record["manualReviewNeeded"] is True
            assert record["reviewTriggers"]
            assert record["reviewRisks"]


def test_review_risks_are_machine_readable_and_match_rulebook() -> None:
    schema = load_json(SCHEMA_PATH)
    rulebook = load_json(RULEBOOK_PATH)
    fixture = load_json(SAMPLE_PATH)
    schema_risk_codes = set(schema["$defs"]["reviewRiskCode"]["enum"])
    rulebook_risk_codes = set(rulebook["reviewRiskCodes"])

    assert schema_risk_codes == rulebook_risk_codes
    for record in fixture["records"]:
        risk_codes = set(record["reviewRisks"])
        assert risk_codes.issubset(schema_risk_codes)
        if record["publicAccessStatus"] == "unknown":
            assert "public_access_unknown" in risk_codes
        if record["managementAgreementStatus"] == "unknown":
            assert "management_agreement_unknown" in risk_codes
        if record["seismicEvidenceLevel"] == "unknown":
            assert "seismic_evidence_unknown" in risk_codes
        if record["candidateLayer"] == HUMANITARIAN_CANDIDATE_LAYER and record["humanitarianCandidateStatus"] in HUMANITARIAN_STATUSES:
            assert "life_first_access_not_legal_guarantee" in risk_codes


def test_non_official_humanitarian_candidates_have_clear_warnings() -> None:
    fixture = load_json(SAMPLE_PATH)
    for record in fixture["records"]:
        if record["humanitarianCandidateStatus"] in HUMANITARIAN_STATUSES:
            warnings_text = " ".join(record["warnings"]).lower()
            assert record["warnings"]
            assert "not an official shelter" in warnings_text
            assert record["manualReviewNeeded"] is True
