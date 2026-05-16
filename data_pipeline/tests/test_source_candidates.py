from __future__ import annotations

import json
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[2]
SOURCE_CANDIDATES_PATH = REPO_ROOT / "data_pipeline" / "sources" / "source_candidates.json"

ALLOWED_SOURCE_FAMILIES = {
    "shelter_facility",
    "tsunami_hazard",
    "paper_or_secondary_reference",
}

REQUIRED_CANDIDATE_FIELDS = {
    "source_id",
    "source_family",
    "source_name",
    "organization",
    "expected_data_type",
    "expected_geometry_type",
    "expected_fields",
    "official_status",
    "license_review_required",
    "manual_download_required",
    "scraping_allowed",
    "large_file_expected",
    "unity_relevance",
    "p3_ingestion_priority",
    "notes",
}


def load_registry() -> dict:
    with SOURCE_CANDIDATES_PATH.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def test_source_candidates_file_exists_and_is_valid_json() -> None:
    assert SOURCE_CANDIDATES_PATH.exists()
    registry = load_registry()
    assert registry["registry_version"] == "p3-01"
    assert isinstance(registry["candidates"], list)
    assert registry["candidates"]


def test_source_candidate_required_fields_exist() -> None:
    registry = load_registry()
    for candidate in registry["candidates"]:
        missing = REQUIRED_CANDIDATE_FIELDS - set(candidate)
        assert missing == set(), f"{candidate.get('source_id', '<unknown>')} missing {sorted(missing)}"
        assert isinstance(candidate["expected_fields"], list)
        assert candidate["expected_fields"]


def test_source_family_uses_allowed_values() -> None:
    registry = load_registry()
    families = {candidate["source_family"] for candidate in registry["candidates"]}
    assert families.issubset(ALLOWED_SOURCE_FAMILIES)
    assert "shelter_facility" in families
    assert "tsunami_hazard" in families
    assert "paper_or_secondary_reference" in families


def test_scraping_is_false_unless_explicitly_justified() -> None:
    registry = load_registry()
    for candidate in registry["candidates"]:
        if candidate["scraping_allowed"] is True:
            assert candidate.get("scraping_justification", "").strip()
        else:
            assert candidate["scraping_allowed"] is False


def test_paper_or_secondary_sources_are_not_official_primary_sources() -> None:
    registry = load_registry()
    for candidate in registry["candidates"]:
        if candidate["source_family"] == "paper_or_secondary_reference":
            assert candidate["official_status"] == "secondary_reference_only"


def test_p3_01_registry_policy_blocks_download_scrape_citygml_and_unity() -> None:
    registry = load_registry()
    policy = registry["policy"]
    assert policy["planning_only"] is True
    assert policy["download_enabled"] is False
    assert policy["scraping_enabled"] is False
    assert policy["citygml_parsing_enabled"] is False
    assert policy["unity_integration_enabled"] is False
