# Official Shelter Ingestion

## Purpose

This document defines the Phase 3 preparation path for future official Chuo/Tokyo shelter and evacuation facility ingestion.

P3-02 still uses fixtures only. It does not download official data, scrape websites, or integrate with Unity.

## Current P3-02 Components

- `data_pipeline/sources/shelter_source_mapping_template.json`
- `data_pipeline/schemas/source_shelter_mapping_schema.json`
- `data_pipeline/samples/sample_manual_shelter_source.csv`
- `data_pipeline/scripts/ingest_shelters_from_manual_source.py`
- `data_pipeline/tests/test_manual_shelter_ingestion.py`

The adapter maps a local manually supplied CSV into the existing shelter export schema.

## Manual Download Policy

Future official source files must be downloaded manually by the human developer after source and license review.

Do not add automatic network download behavior without a separate approved milestone.

## No-Scraping Policy

Shelter ingestion scripts must not scrape websites. If an official machine-readable file exists, the future flow should use a manually supplied local copy or an explicitly approved download step.

## Mapping Flow

1. Review `data_pipeline/sources/source_candidates.json`.
2. Select one official `shelter_facility` candidate.
3. Manually review license, update date, fields, and file size.
4. Place a small local fixture under `data_pipeline/samples/` for tests.
5. Create or update a mapping JSON.
6. Run the ingestion adapter.
7. Validate output against `real_shelter_schema.json`.
8. Keep Unity integration deferred to P4.

## Official vs Secondary Sources

Official government open data may become primary pipeline input after review.

Paper maps, third-party pages, and secondary summaries may help manual QA, but they should not be treated as authoritative shelter records.

## Current Limitations

- No geocoding.
- No duplicate merging.
- No PLATEAU building matching.
- No Unity world-position conversion.
- No update automation.
