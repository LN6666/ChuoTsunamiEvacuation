from __future__ import annotations

import csv
import json
import subprocess
import sys
from pathlib import Path

import pytest


REPO_ROOT = Path(__file__).resolve().parents[2]
SCRIPT = REPO_ROOT / "data_pipeline" / "scripts" / "ingest_official_chuo_shelters.py"
MANIFEST = REPO_ROOT / "data_pipeline" / "sources" / "real_chuo_official_source_manifest.json"
OUTPUT_JSON = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_official_shelters_normalized.json"
OUTPUT_CSV = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_official_shelters_normalized.csv"


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def raw_downloads_available(manifest: dict) -> bool:
    return all((REPO_ROOT / source["localRawPath"]).exists() for source in manifest["sources"])


def test_official_source_manifest_has_provenance() -> None:
    manifest = load_json(MANIFEST)
    assert manifest["manifestVersion"].startswith("p5-b4")
    assert len(manifest["sources"]) >= 3
    for source in manifest["sources"]:
        assert source["sourceId"]
        assert source["sourceName"]
        assert source["sourceOwner"]
        assert source["sourceUrl"].startswith("https://")
        assert source["localRawPath"].startswith("data_pipeline/downloads/")
        assert source["downloadedAt"]
        assert source["licenseOrTerms"]
        assert source["reliabilityLevel"].startswith("official")


def test_ingestion_script_runs_and_writes_normalized_outputs() -> None:
    manifest = load_json(MANIFEST)
    if not raw_downloads_available(manifest):
        pytest.skip("Official raw downloads are local ignored inputs and are not present.")

    result = subprocess.run(
        [sys.executable, str(SCRIPT)],
        cwd=REPO_ROOT,
        check=False,
        text=True,
        capture_output=True,
    )
    assert result.returncode == 0, result.stderr
    assert OUTPUT_JSON.exists()
    assert OUTPUT_CSV.exists()

    payload = load_json(OUTPUT_JSON)
    assert payload["datasetId"] == "p5_b4_real_chuo_official_shelters_normalized"
    assert payload["coordinateReferenceSystem"] == "EPSG:4326"
    assert payload["recordCount"] == 31
    assert len(payload["records"]) == 31

    with OUTPUT_CSV.open("r", encoding="utf-8", newline="") as handle:
        rows = list(csv.DictReader(handle))
    assert len(rows) == 31


def test_normalized_records_include_required_official_fields() -> None:
    if not OUTPUT_JSON.exists():
        pytest.skip("Normalized output has not been generated.")

    payload = load_json(OUTPUT_JSON)
    for record in payload["records"]:
        assert record["shelterId"].startswith("chuo_official_")
        assert record["shelterName"]
        assert record["address"]
        assert isinstance(record["latitude"], float)
        assert isinstance(record["longitude"], float)
        assert record["sourceUrl"].startswith("https://")
        assert record["officialDesignationStatus"] == "official_designated"
        assert record["evidenceSources"]
        assert any(
            source["sourceFamily"] == "official_shelter_facility"
            for source in record["evidenceSources"]
        )
        assert "data_pipeline/raw" not in json.dumps(record, ensure_ascii=False)
        assert "data_pipeline/downloads" not in json.dumps(record, ensure_ascii=False)
