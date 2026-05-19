from __future__ import annotations

import json
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[2]
DATA_PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
QUALIFICATION_ROOT = DATA_PIPELINE_ROOT / "qualification"

REQUIREMENTS_P5_PATH = DATA_PIPELINE_ROOT / "requirements-p5.txt"
SETUP_SCRIPT_PATH = DATA_PIPELINE_ROOT / "setup_p5_environment.ps1"
PROVENANCE_TEMPLATE_PATH = QUALIFICATION_ROOT / "source_provenance_review_template.json"
FIXTURE_PLAN_PATH = QUALIFICATION_ROOT / "controlled_real_source_fixture_plan.json"

REQUIRED_PROVENANCE_FIELDS = {
    "sourceId",
    "sourceName",
    "sourceFamily",
    "officialOrReference",
    "sourceUrl",
    "sourceOwner",
    "licenseOrTerms",
    "sourceUpdatedAt",
    "accessDate",
    "plannedUse",
    "canConfirmOfficialStatus",
    "canSupportCandidateStatus",
    "dataSensitivity",
    "attributionRequired",
    "manualReviewStatus",
    "notes",
    "approvalStatus",
}


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def requirement_names(path: Path) -> set[str]:
    names: set[str] = set()
    for raw_line in path.read_text(encoding="utf-8").splitlines():
        line = raw_line.strip()
        if not line or line.startswith("#"):
            continue
        names.add(line.split("==")[0].split(">=")[0].split("<")[0].strip().lower())
    return names


def test_requirements_p5_includes_phase5_dependencies() -> None:
    assert REQUIREMENTS_P5_PATH.exists()
    names = requirement_names(REQUIREMENTS_P5_PATH)
    assert "jsonschema" in names
    assert "pytest" in names
    assert "geopandas" in names
    assert "shapely" in names
    assert "pyproj" in names
    assert "networkx" in names
    assert "osmnx" in names


def test_setup_p5_environment_script_exists() -> None:
    assert SETUP_SCRIPT_PATH.exists()
    script_text = SETUP_SCRIPT_PATH.read_text(encoding="utf-8")
    assert "data_pipeline/.venv" not in script_text
    assert "requirements-p5.txt" in script_text
    assert "pip install" in script_text
    assert "osmnx" in script_text


def test_source_provenance_review_template_is_valid_json() -> None:
    template = load_json(PROVENANCE_TEMPLATE_PATH)
    assert template["status"] == "template_not_verified"
    assert isinstance(template["entries"], list)
    assert template["entries"]


def test_controlled_real_source_fixture_plan_is_valid_json() -> None:
    plan = load_json(FIXTURE_PLAN_PATH)
    assert plan["planVersion"] == "p5-b3-controlled-real-source-fixture-plan-v1"
    assert plan["planningOnly"] is True


def test_provenance_template_entries_include_required_fields() -> None:
    template = load_json(PROVENANCE_TEMPLATE_PATH)
    for entry in template["entries"]:
        assert REQUIRED_PROVENANCE_FIELDS.issubset(entry)
        assert entry["status"] == "template_not_verified"


def test_fixture_plan_includes_crs_and_license_requirements() -> None:
    plan = load_json(FIXTURE_PLAN_PATH)
    crs_requirements = " ".join(plan["CRSRequirements"])
    license_requirements = " ".join(plan["licenseReviewRequirements"])
    assert "Do not compute meter distances in EPSG:4326." in plan["CRSRequirements"]
    assert "projected CRS" in crs_requirements
    assert "license" in license_requirements.lower()
    assert "attribution" in license_requirements.lower()


def test_no_provenance_template_entry_claims_final_approval() -> None:
    template = load_json(PROVENANCE_TEMPLATE_PATH)
    final_approval_values = {"approved", "final_approved", "verified", "officially_approved"}
    for entry in template["entries"]:
        assert entry["approvalStatus"] not in final_approval_values
        assert entry["manualReviewStatus"] == "not_reviewed"
