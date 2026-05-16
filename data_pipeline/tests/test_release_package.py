from __future__ import annotations

import json
import subprocess
import sys
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
EXPORT_SCRIPT = PIPELINE_ROOT / "scripts" / "export_unity_shelters.py"
RELEASE_SCRIPT = PIPELINE_ROOT / "scripts" / "build_release_package.py"
HAZARD_SAMPLE = PIPELINE_ROOT / "samples" / "sample_tsunami_hazard_zones.json"


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def test_release_package_builder_creates_small_sample_release(tmp_path: Path) -> None:
    shelter_json = tmp_path / "real_chuo_shelters_sample.json"
    shelter_csv = tmp_path / "real_chuo_shelters_sample.csv"
    release_dir = tmp_path / "release"

    export_result = subprocess.run(
        [
            sys.executable,
            str(EXPORT_SCRIPT),
            "--json-output",
            str(shelter_json),
            "--csv-output",
            str(shelter_csv),
        ],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )
    assert export_result.returncode == 0, export_result.stdout + export_result.stderr

    release_result = subprocess.run(
        [
            sys.executable,
            str(RELEASE_SCRIPT),
            "--release-dir",
            str(release_dir),
            "--shelter-json",
            str(shelter_json),
            "--shelter-csv",
            str(shelter_csv),
            "--hazard-json",
            str(HAZARD_SAMPLE),
        ],
        cwd=REPO_ROOT,
        capture_output=True,
        text=True,
    )
    assert release_result.returncode == 0, release_result.stdout + release_result.stderr

    manifest_path = release_dir / "p3_sample_release_manifest.json"
    assert manifest_path.exists()

    manifest = load_json(manifest_path)
    assert manifest["release_id"] == "p3_02_sample_release"
    assert manifest["all_data_sample_or_synthetic"] is True
    assert manifest["unity_integration_performed"] is False
    assert manifest["plateau_or_citygml_used"] is False
    assert manifest["datasets"]["shelter_record_count"] == 5
    assert manifest["datasets"]["hazard_zone_count"] == 3
    assert len(manifest["included_files"]) == 3

    for included_file in manifest["included_files"]:
        output_file = release_dir / included_file["path"]
        assert output_file.exists()
        assert output_file.stat().st_size < 1024 * 1024
        assert included_file["sample_status"] == "synthetic_sample"
