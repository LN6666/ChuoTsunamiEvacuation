# Phase 3 Real Data Pipeline

## Purpose

`data_pipeline/` is the standalone Phase 3 data engineering layer for future real Chuo Ward shelter and facility data.

It does not modify Unity gameplay, Unity scenes, Unity project settings, PLATEAU imported files, or existing gameplay JSON files. Phase 3 produces processed JSON/CSV outputs that future Phase 4 Unity integration can consume after a separate review and integration step.

## Current Scope

Phase 3-00 uses only local synthetic sample data:

- `samples/sample_raw_shelters.csv`
- `sources/source_manifest.json`
- `schemas/real_shelter_schema.json`

The generated sample outputs are:

- `processed/real_chuo_shelters_sample.json`
- `processed/real_chuo_shelters_sample.csv`

These files are pipeline contract examples. They are not official Chuo Ward shelter records.

Phase 3-01 adds a planning-only source candidate registry:

- `sources/source_candidates.json`

The registry records future candidate sources for shelter/facility points and tsunami or water-hazard areas. It does not trigger downloads, scraping, GIS parsing, CityGML parsing, or Unity integration.

Phase 3-02 adds preparation fixtures for:

- local manual shelter ingestion mapping
- tsunami hazard schema validation
- small sample release package creation
- P3 to P4 handoff documentation

## What Phase 3-00 Does Not Do

- No website scraping.
- No external dataset download.
- No PLATEAU download or re-import.
- No CityGML parsing.
- No Unity scene modification.
- No gameplay integration.

The Unity project already has local PLATEAU Chuo Ward Buildings / bldg / LOD1 context from Phase 2. Future phases may optionally match shelter records to PLATEAU building IDs, but Phase 3-00 does not attempt that.

## Folder Structure

```text
data_pipeline/
  README.md
  requirements.txt
  run_pipeline.ps1
  samples/
    sample_raw_shelters.csv
  schemas/
    real_shelter_schema.json
    source_shelter_mapping_schema.json
    tsunami_hazard_schema.json
  scripts/
    build_release_package.py
    export_unity_shelters.py
    ingest_shelters_from_manual_source.py
    validate_tsunami_hazard.py
    validate_real_shelters.py
  sources/
    source_candidates.json
    source_manifest.json
    shelter_source_mapping_template.json
  processed/
    release/
    real_chuo_shelters_sample.json
    real_chuo_shelters_sample.csv
  samples/
    sample_manual_shelter_source.csv
    sample_tsunami_hazard_zones.json
  tests/
    test_manual_shelter_ingestion.py
    test_release_package.py
    test_real_shelter_schema.py
    test_tsunami_hazard_schema.py
```

## Run

From the repository root:

```powershell
.\data_pipeline\run_pipeline.ps1
```

The runner exports the sample data, validates the exported JSON, and runs pytest.

## Future Work

Later Phase 3 work may add official source ingestion, source-specific parsers, geocoding, coordinate verification, CRS transformation, and optional PLATEAU building matching. Phase 4 may then copy or transform approved P3 outputs into Unity-facing `Assets/Data` files.
