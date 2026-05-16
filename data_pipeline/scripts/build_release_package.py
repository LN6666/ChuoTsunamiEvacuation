from __future__ import annotations

import argparse
import json
import shutil
import sys
from datetime import datetime, timezone
from pathlib import Path
from typing import Any


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"

DEFAULT_RELEASE_DIR = PIPELINE_ROOT / "processed" / "release"
DEFAULT_SHELTER_JSON = PIPELINE_ROOT / "processed" / "real_chuo_shelters_sample.json"
DEFAULT_SHELTER_CSV = PIPELINE_ROOT / "processed" / "real_chuo_shelters_sample.csv"
DEFAULT_HAZARD_JSON = PIPELINE_ROOT / "samples" / "sample_tsunami_hazard_zones.json"

MANIFEST_NAME = "p3_sample_release_manifest.json"


class ReleaseBuildError(ValueError):
    """Raised when the P3 sample release package cannot be built."""


def resolve_path(path_value: str | Path) -> Path:
    path = Path(path_value)
    if path.is_absolute():
        return path
    return Path.cwd() / path


def repo_relative(path: Path) -> str:
    try:
        return path.resolve().relative_to(REPO_ROOT.resolve()).as_posix()
    except ValueError:
        return path.resolve().as_posix()


def load_json(path: Path) -> dict[str, Any]:
    if not path.exists():
        raise ReleaseBuildError(f"Required JSON input not found: {path}")
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def require_file(path: Path) -> None:
    if not path.exists():
        raise ReleaseBuildError(f"Required input file not found: {path}")
    if path.stat().st_size > 1024 * 1024:
        raise ReleaseBuildError(f"Release input is unexpectedly large: {path}")


def copy_release_file(source: Path, release_dir: Path) -> dict[str, Any]:
    require_file(source)
    destination = release_dir / source.name
    shutil.copy2(source, destination)
    return {
        "path": destination.name,
        "source_path": repo_relative(source),
        "size_bytes": destination.stat().st_size,
    }


def build_manifest(
    copied_files: list[dict[str, Any]],
    shelter_dataset: dict[str, Any],
    hazard_dataset: dict[str, Any],
) -> dict[str, Any]:
    generated_at = datetime.now(timezone.utc).replace(microsecond=0).isoformat().replace("+00:00", "Z")
    return {
        "release_id": "p3_02_sample_release",
        "generated_at": generated_at,
        "release_type": "synthetic_sample_fixture",
        "all_data_sample_or_synthetic": True,
        "intended_consumer": "future Phase 4 Unity integration review",
        "not_for_gameplay_without_review": True,
        "unity_integration_performed": False,
        "plateau_or_citygml_used": False,
        "included_files": [
            {
                **copied_file,
                "sample_status": "synthetic_sample",
            }
            for copied_file in copied_files
        ],
        "datasets": {
            "shelter_dataset_id": shelter_dataset.get("dataset_id"),
            "hazard_dataset_id": hazard_dataset.get("dataset_id"),
            "hazard_zone_count": len(hazard_dataset.get("zones", [])),
            "shelter_record_count": len(shelter_dataset.get("records", [])),
        },
        "handoff_notes": [
            "P3 release package is small and Git-safe.",
            "All included data is sample/synthetic unless a future manifest states otherwise.",
            "P4 must not copy these files into Assets/Data without a separate integration milestone.",
            "No PLATEAU, CityGML, GIS archive, scraping, or network download was used to build this package."
        ],
    }


def write_manifest(manifest: dict[str, Any], release_dir: Path) -> Path:
    manifest_path = release_dir / MANIFEST_NAME
    with manifest_path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(manifest, handle, ensure_ascii=False, indent=2)
        handle.write("\n")
    return manifest_path


def build_release_package(release_dir: Path, shelter_json: Path, shelter_csv: Path, hazard_json: Path) -> Path:
    release_dir.mkdir(parents=True, exist_ok=True)

    shelter_dataset = load_json(shelter_json)
    hazard_dataset = load_json(hazard_json)

    copied_files = [
        {**copy_release_file(shelter_json, release_dir), "data_family": "shelter_facility", "media_type": "json"},
        {**copy_release_file(shelter_csv, release_dir), "data_family": "shelter_facility", "media_type": "csv"},
        {**copy_release_file(hazard_json, release_dir), "data_family": "tsunami_hazard", "media_type": "json"},
    ]
    manifest = build_manifest(copied_files, shelter_dataset, hazard_dataset)
    return write_manifest(manifest, release_dir)


def build_parser() -> argparse.ArgumentParser:
    parser = argparse.ArgumentParser(description="Build a small Phase 3 sample release package for future P4 handoff.")
    parser.add_argument("--release-dir", default=str(DEFAULT_RELEASE_DIR), help="Release output directory.")
    parser.add_argument("--shelter-json", default=str(DEFAULT_SHELTER_JSON), help="Processed shelter JSON input.")
    parser.add_argument("--shelter-csv", default=str(DEFAULT_SHELTER_CSV), help="Processed shelter CSV input.")
    parser.add_argument("--hazard-json", default=str(DEFAULT_HAZARD_JSON), help="Sample tsunami hazard JSON input.")
    return parser


def main(argv: list[str] | None = None) -> int:
    parser = build_parser()
    args = parser.parse_args(argv)

    release_dir = resolve_path(args.release_dir)
    shelter_json = resolve_path(args.shelter_json)
    shelter_csv = resolve_path(args.shelter_csv)
    hazard_json = resolve_path(args.hazard_json)

    try:
        manifest_path = build_release_package(release_dir, shelter_json, shelter_csv, hazard_json)
    except Exception as exc:
        print(f"[release] ERROR: {exc}", file=sys.stderr)
        return 1

    print(f"[release] Wrote sample release manifest to {manifest_path}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
