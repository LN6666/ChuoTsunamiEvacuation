# P5 Environment And Source Readiness

## Purpose

P5-B3 prepares the project for local Phase 5 validation and GIS work without downloading real data, installing global dependencies, running OSM network downloads, parsing CityGML, matching real PLATEAU buildings, routing, or touching Unity.

The milestone creates a project-local dependency file, a rerunnable setup helper, source provenance review templates, and a controlled real-source fixture plan for P5-B4.

## Project-Local Virtual Environment

P5 GIS work should run in a project-local virtual environment under:

```powershell
data_pipeline/.venv
```

This keeps P5 validation and GIS packages separate from the global Python installation. The repository `.gitignore` already ignores `.venv` directories, including `data_pipeline/.venv`.

## Requirements File

P5-specific dependencies are listed in:

```powershell
data_pipeline/requirements-p5.txt
```

This file layers P5 validation/GIS/routing packages on top of the existing `data_pipeline/requirements.txt` baseline.

The P5 file includes:

- `jsonschema` and `pytest` for validation and tests.
- `geopandas`, `shapely`, and `pyproj` for future CRS-aware GIS processing and geometry operations.
- `networkx` and `osmnx` for future controlled routing graph work.

GDAL, Fiona, or pyogrio may be installed transitively or may need later Windows-specific handling.

## Setup Script

The helper script is:

```powershell
data_pipeline/setup_p5_environment.ps1
```

It creates or reuses `data_pipeline/.venv`, upgrades pip inside that environment, installs `data_pipeline/requirements.txt` when present, installs `data_pipeline/requirements-p5.txt`, and runs import checks for the P5 packages.

Run it only with explicit user approval:

```powershell
powershell -ExecutionPolicy Bypass -File data_pipeline/setup_p5_environment.ps1
```

The script does not install packages globally, does not download OSM network data, and does not run GIS processing.

## Dependency Risks

- Windows GIS packages can fail because of native dependencies.
- GeoPandas may require compatible GDAL/Fiona/pyogrio wheels.
- OSMnx can download network data if later routing code calls download APIs.
- OSM output requires license and attribution review before use.
- Dependency versions should be recorded before real GIS processing begins.

## Source Provenance Readiness

The source provenance/license template is:

```powershell
data_pipeline/qualification/source_provenance_review_template.json
```

It contains placeholder entries marked `template_not_verified`. These are not final official source claims.

The template records fields such as source owner, license/terms, access date, update date, official/reference status, planned use, attribution requirements, manual review state, and approval status.

## Controlled Real-Source Fixture Plan

The P5-B4 fixture plan is:

```powershell
data_pipeline/qualification/controlled_real_source_fixture_plan.json
```

It describes how P5-B4 should create a small manually reviewed real-source fixture after source and license review. The plan requires provenance fields, license review, CRS metadata, validation checks, and explicit manual review warnings.

## No Real Data In B3

P5-B3 does not download official data, scrape websites, download OSM network data, parse CityGML, parse full PLATEAU data, implement routing, implement building matching, or modify Unity.

The next real-source step must remain small and manually reviewed.

## Next Step

P5-B4 should run or verify the local P5 environment with user approval, complete source provenance review for selected source candidates, and create a small controlled real-source fixture before any expanded processing.
