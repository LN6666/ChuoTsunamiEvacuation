# P5 Controlled Qualification Pipeline

## Purpose

P5-B1 creates a deterministic controlled sample pipeline for evacuation building qualification outputs.

The goal is to prove that the P5-A2 rulebook and schema can support qualification, placeholder PLATEAU building matching, and placeholder route metadata before any real Chuo data ingestion, CityGML parsing, OSM routing, QGIS QA, or Unity integration begins.

## Controlled Samples

The P5-B1 fixtures are synthetic and deliberately small:

- `data_pipeline/qualification/sample_controlled_shelters_for_qualification.json`
- `data_pipeline/qualification/sample_controlled_buildings_for_matching.json`
- `data_pipeline/qualification/sample_controlled_routes_for_qualification.json`
- `data_pipeline/qualification/controlled_qualification_pipeline_config.json`

The shelter fixture covers official contains-match, official nearest-match review, non-official strong candidate, weak candidate with missing safe floor/capacity, unknown insufficient evidence, conflict/not-qualified, and unmatched shelter/building cases.

The building fixture uses PLATEAU-like identifiers and simplified centroids/bounds only. It is not real PLATEAU data.

The route fixture uses controlled prototype route estimates only. It is not OSM output and it is not an official evacuation route source.

## Pipeline Script

The build script is:

```powershell
python data_pipeline/scripts/build_controlled_qualification_sample.py
```

It uses only the Python standard library. It loads the controlled fixtures, the controlled config, and the P5-A2 rulebook/schema paths, then writes:

- `data_pipeline/qualification/controlled_building_qualification_output.json`
- `data_pipeline/qualification/controlled_building_qualification_output.csv`

## Qualification Behavior

Official designation remains separate from candidate suitability:

- `official_confirmed` requires official evidence and an unambiguous controlled contains match.
- `official_confirmed_with_review` requires official evidence but keeps manual review for nearest-match ambiguity.
- Literature/report candidate evidence can produce `strong_candidate` or `weak_candidate`, but never `official_confirmed`.
- Unmatched shelter/building records remain `unknown` until manual review resolves the building match.
- Conflict or controlled unsuitable evidence produces `not_qualified`.

Nearest, unmatched, manual override, low-confidence, missing-field, candidate-only, unavailable-route, and failed-route examples carry review warnings.

## Schema Validation

The output JSON is shaped to match `data_pipeline/qualification/evacuation_building_qualification_schema.json`.

Use:

```powershell
python data_pipeline/scripts/validate_building_qualification.py --input data_pipeline/qualification/controlled_building_qualification_output.json --schema data_pipeline/qualification/evacuation_building_qualification_schema.json
```

The validator requires the optional `jsonschema` package. In environments without `jsonschema`, use Python's built-in JSON parser as a syntax check:

```powershell
python -m json.tool data_pipeline/qualification/controlled_building_qualification_output.json
```

## P5-B2 Preparation

P5-B1 does not perform real GIS work. It prepares P5-B2 by stabilizing field names, status mapping behavior, manual review warnings, route placeholders, deterministic thresholds, and JSON/CSV output shape.

P5-B2 should replace controlled matching and route placeholders with controlled real Chuo source ingestion, CRS-aware PLATEAU building matching decisions, OSM routing decisions, and QGIS spatial QA.

QGIS should be used in P5-B2 as a manual spatial QA tool for inspecting building matches, CRS sanity, route outputs, and ambiguous cases. QGIS project state should not become a runtime dependency or the only reproducible source of truth.

## Known Limitations

- No official data is downloaded or scraped.
- No real Chuo source is ingested.
- No CityGML or full PLATEAU data is parsed.
- No OSM data is downloaded.
- No real GIS routing is performed.
- No Unity assets, scenes, `ProjectSettings`, or `Packages` are modified.
- The fixture geometry is simplified and not spatially authoritative.
