# P5 Dependency Environment Plan

## Purpose

P5-B2 records the current Python environment and defines the dependency strategy needed before controlled real Chuo data ingestion, PLATEAU building matching, OSM route preparation, and schema validation can move beyond synthetic fixtures.

This milestone does not install dependencies, modify the global Python environment, or update `data_pipeline/requirements.txt`.

## Current Availability

Checked on 2026-05-19:

| Package | Result | P5 role |
|---|---|---|
| Python | `3.12.10` available | base runtime |
| `json` | available | standard-library JSON parsing |
| `jsonschema` | missing | schema validation |
| `pytest` | missing | Python test execution |
| `geopandas` | missing | future vector GIS processing |
| `shapely` | missing | future geometry operations |
| `pyproj` | missing | future CRS transforms |
| `networkx` | missing | future graph/routing support |
| `osmnx` | missing | future OSM pedestrian network preparation |

## Required For Validation

P5 validation needs:

- `jsonschema` for JSON Schema validation of qualification outputs.
- `pytest` for focused pipeline and planning tests.

Until those are available, P5-B can still use Python's built-in `json` module for syntax checks and `py_compile` for test-file syntax checks.

## Likely GIS Packages

Likely P5-B3/B4/B5 dependencies:

- `geopandas` for CRS-aware vector tables, spatial joins, and processed output tables.
- `shapely` for contains, nearest, distance, intersection, and geometry validation.
- `pyproj` for CRS transforms and explicit axis-order handling.
- `networkx` for graph operations when direct graph checks are needed.
- `osmnx` for controlled OSM pedestrian network retrieval and route graph preparation, only after download/cache/attribution policy is approved.
- GDAL/Fiona/pyogrio stack if official source formats require GIS file IO beyond GeoJSON/CSV.

## Recommended Approach

- Prefer a project-local virtual environment for P5 GIS work.
- Avoid global installs.
- Update `data_pipeline/requirements.txt` only in a dedicated dependency milestone.
- Pin or document package versions before running real GIS processing.
- Verify Windows compatibility before committing to a GeoPandas/GDAL/Fiona/pyogrio path.
- Keep OSM download/cache behavior disabled until a routing milestone approves it.

## Risks

- Windows GIS package compatibility can fail because of native dependencies.
- Fiona/GDAL installation can be complex and may need a different IO backend.
- OSMnx can trigger network downloads and cache changes if not controlled.
- OSM data requires license and attribution review.
- Package version drift can change geometry or IO behavior.

## Next Action

P5-B3 should create or verify a project-local Python GIS environment before ingesting real data. That milestone should decide whether to update `data_pipeline/requirements.txt`, record exact package versions, and rerun schema/test checks before any real source ingestion.
