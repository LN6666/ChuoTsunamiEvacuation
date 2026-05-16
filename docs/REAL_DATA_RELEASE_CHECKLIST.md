# Real Data Release Checklist

## Purpose

Use this checklist before committing or handing off any Phase 3 processed data package.

## Required Checks

- `.\data_pipeline\run_pipeline.ps1` passes.
- All pytest tests pass.
- Release manifest exists.
- Release manifest identifies whether data is sample, synthetic, official, or secondary.
- No raw source files are committed unless explicitly approved.
- No large GIS files are committed.
- No files are copied into `Assets/Data`.
- No Unity scenes, scripts, ProjectSettings, or PLATEAU imports are modified.
- Source license and attribution are reviewed before using official data.
- CRS is declared.
- Shelter outputs validate against `real_shelter_schema.json`.
- Hazard outputs validate against `tsunami_hazard_schema.json`.
- Paper or secondary references are not marked as official primary data.

## Current P3-02 Release

The current package is sample-only and synthetic:

- `data_pipeline/processed/release/p3_sample_release_manifest.json`

It is suitable for P4 interface review, not gameplay use.

## Timestamp Note

Some generated files include `generated_at` timestamps. If a pipeline rerun changes only timestamps and no source data or code changed, review the diff before committing and decide whether to keep or revert the timestamp-only change.
