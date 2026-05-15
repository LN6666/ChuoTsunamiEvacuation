# Real Data Pipeline

## Purpose

Phase 3 creates a reproducible data engineering foundation for future real Chuo Ward shelter and facility records.

The pipeline is separate from the Unity gameplay project. It prepares processed JSON/CSV outputs that future Phase 4 work may integrate into Unity after review.

## P2 / P3 / P4 Separation

P2 is the current Unity gameplay and data-driven prototype layer. It uses manually placed test shelters and existing gameplay JSON files under `Assets/Data/`, including `test_shelters.json` and `scenario_presets.json`.

P3 is this standalone data pipeline. It reads source data, normalizes records, validates them, and writes processed outputs under `data_pipeline/processed/`.

P4 is future Unity integration. P4 may copy, transform, or load approved P3 outputs into Unity-facing data files and connect them to scene objects.

P3-00 and P3-01 do not modify P2 files and do not perform P4 integration.

## Existing PLATEAU Context

The Unity project already has local PLATEAU Chuo Ward Buildings / bldg / LOD1 imported through PLATEAU SDK. The large generated `Assets/Scenes/Chuo_BaseMap.unity` scene is intentionally local and not committed.

Phase 3 treats PLATEAU as existing Unity context only. It does not download PLATEAU data, re-import CityGML, parse CityGML, or modify Unity scenes. Optional PLATEAU building matching is future Phase 3/4 work.

## Pipeline Structure

```text
data_pipeline/
  sources/source_candidates.json
  sources/source_manifest.json
  schemas/real_shelter_schema.json
  samples/sample_raw_shelters.csv
  scripts/export_unity_shelters.py
  scripts/validate_real_shelters.py
  processed/real_chuo_shelters_sample.json
  processed/real_chuo_shelters_sample.csv
  tests/test_real_shelter_schema.py
  run_pipeline.ps1
```

## Current P3-00 Scope

P3-00 uses a small local synthetic CSV only. The records are placeholders for pipeline testing and are not official Chuo Ward shelter data.

The export script converts the sample CSV to:

- `data_pipeline/processed/real_chuo_shelters_sample.json`
- `data_pipeline/processed/real_chuo_shelters_sample.csv`

The validation script checks JSON Schema compliance and extra sanity rules such as unique IDs, Chuo/Tokyo coordinate ranges, and Unity interface fields.

## Current P3-01 Scope

P3-01 adds `data_pipeline/sources/source_candidates.json`.

This is a planning and metadata registry only. It distinguishes three source families:

- `shelter_facility`
- `tsunami_hazard`
- `paper_or_secondary_reference`

The registry records candidate source metadata, expected data and geometry types, expected fields, official status, license-review flags, manual-download flags, scraping policy, file-size risk, Unity relevance, ingestion priority, and notes.

P3-01 does not ingest any candidate source.

## Shelter Points vs Hazard Areas

Shelter/facility data is usually point-like or address-based. It answers questions such as:

- What is the facility?
- Where is it?
- What disaster types does it support?
- Can it become a Unity shelter marker later?

Tsunami or water-hazard data is usually area-, mesh-, raster-, or polygon-based. It answers questions such as:

- Which areas may be inundated?
- How deep might the inundation be?
- Which zones are affected under a scenario?
- How should future Unity risk zones or overlays be derived?

These data families should not be forced into one schema. P3-00 defines only the shelter export schema. Hazard schemas should be designed later after source format review.

## Official Sources vs Papers / Secondary References

Official sources are produced by government organizations or their official open-data/catalog systems. They are candidates for primary pipeline ingestion after license, update, and field review.

Paper maps, academic figures, third-party summaries, and secondary websites can help review assumptions and find gaps, but they should not be used as primary pipeline records unless separately approved. They may be outdated, simplified, derived from other sources, or unsuitable for automated extraction.

## Run

From the repository root:

```powershell
.\data_pipeline\run_pipeline.ps1
```

The runner performs:

1. Export sample CSV to processed JSON/CSV.
2. Validate the exported JSON.
3. Run pytest tests for the schema, exporter, validator, and sample output contract.

## Intentionally Not Done In P3-00

- No real website scraping.
- No official source download.
- No PLATEAU download or re-import.
- No CityGML parsing.
- No GIS file parsing.
- No source candidate ingestion.
- No Unity integration.
- No gameplay changes.
- No modification to `Assets/Data/test_shelters.json` or `Assets/Data/scenario_presets.json`.

## Future Phases

Future Phase 3 work may add:

- official Chuo Ward shelter source ingestion
- official hazard source ingestion
- source review and license tracking
- geocoding and coordinate verification
- CRS transformation if a source is not EPSG:4326
- duplicate detection across sources
- hazard polygon/depth schema design
- clipping hazard data to the Chuo area
- optional PLATEAU building matching

Future Phase 4 work may integrate approved processed outputs into Unity, bind records to scene objects, and validate Unity-side loading behavior.

## Source Registry To Unity-Ready Data Path

The expected future path is:

1. Review `source_candidates.json`.
2. Select one source family and one official candidate.
3. Confirm license, update policy, fields, geometry, and file size.
4. Add a source-specific ingestion script.
5. Add automated validation for every transformation that can be checked.
6. Export processed JSON/CSV or hazard layers under `data_pipeline/processed/`.
7. Keep Unity integration deferred to P4.

## Manual Checks

P3-00 cannot automate official-source quality checks because no official source is ingested yet. Manual review remains required for:

- confirming official source authority and license terms
- confirming source update date and download method
- verifying real addresses and coordinates
- deciding whether records are suitable for gameplay use
- matching records to PLATEAU buildings or Unity objects in a later phase
