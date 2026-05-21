# P5 Real Chuo Building Qualification Pipeline

## Purpose

P5-B4 ingests official Chuo/Tokyo/GSI evacuation-place and shelter data, normalizes Chuo Ward records, extracts a limited local PLATEAU building-footprint subset from existing local CityGML, and produces schema-valid building qualification and shelter-to-building match outputs.

This is a real-data pipeline stage, but it is still a processing/QA milestone. It does not modify Unity, run OSM routing, change gameplay rules, or commit raw source downloads.

## Official Sources

Raw official downloads are local ignored inputs under `data_pipeline/downloads/` and are not committed.

Accepted source manifest:

- `data_pipeline/sources/real_chuo_official_source_manifest.json`

Official sources used:

- Chuo City open data, `指定緊急避難所一覧`: `https://www.city.chuo.lg.jp/documents/984/shiteikinkyuuhinan.csv`
- Tokyo Open Data Catalog, `東京都防災マップ 避難所一覧データCSV`: `https://www.opendata.metro.tokyo.lg.jp/soumu/130001_evacuation_center.csv`
- Tokyo Open Data Catalog, `東京都防災マップ 避難場所一覧データCSV`: `https://www.opendata.metro.tokyo.lg.jp/soumu/130001_evacuation_area.csv`
- GSI Chuo emergency evacuation places: `https://hinanmap.gsi.go.jp/hinanjocp/defaultFtpData/csv/13102_2.csv`
- GSI Chuo designated shelters: `https://hinanmap.gsi.go.jp/hinanjocp/defaultFtpData/csv/13102_1.csv`

The Chuo City open-data file is treated as the primary official evidence source. Tokyo and GSI records are used as official reference cross-checks when facility names match.

## Normalized Shelter Input

Normalizer:

- `data_pipeline/scripts/ingest_official_chuo_shelters.py`

Outputs:

- `data_pipeline/processed/qualification/real_chuo_official_shelters_normalized.json`
- `data_pipeline/processed/qualification/real_chuo_official_shelters_normalized.csv`

Normalized record count: 31.

Each normalized record preserves source owner, source URL, update/download metadata, license/terms notes, address, EPSG:4326 coordinates, disaster-type flags, capacity when parseable, official designation status, manual review flags, and evidence sources.

## PLATEAU Building Input

No committed processed PLATEAU footprint file existed before B4. P5-B4 therefore uses a limited extraction path from existing local PLATEAU CityGML files under:

- `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\bldg`

The extraction is not a full PLATEAU parse. It selects only mesh files needed for the official shelter point mesh codes and only keeps building footprints within a fixed buffer around those points.

Input manifest:

- `data_pipeline/qualification/real_chuo_building_matching_input_manifest.json`

B4 selected 13 local building mesh files, extracted 4,447 nearby candidate building footprints, and matched 26 distinct PLATEAU buildings.

## PLATEAU Attribute Handling

P5-B preserves PLATEAU building attributes such as `usage`, `class`, measured height, and storeys as raw/source attributes when they are available in the parsed local CityGML building elements.

Raw PLATEAU usage codes such as `9999`, `3002`, and `3003` are not fully interpreted in P5-B. They are retained for QA context only. Downstream users should not over-interpret these codes, infer facility type from them, or use them as evacuation-safety evidence without a formal PLATEAU usage-code codebook and a separate review step.

## CRS Strategy

Source/interchange CRS:

- `EPSG:4326`

Metric processing CRS:

- `EPSG:6677`

All distance thresholds and `matchDistanceMeters` values are computed after projection to EPSG:6677. The pipeline does not compute meter distances directly in longitude/latitude.

## Matching Logic

Builder:

- `data_pipeline/scripts/build_real_chuo_building_qualification.py`

Methods:

- `contains`: shelter point is covered by one selected PLATEAU footprint.
- `nearest`: shelter point is outside the footprint but within the B4 nearest threshold.
- `unmatched`: no acceptable building match, or the official record describes a broad evacuation area rather than a single building.

Nearest-match confidence is distance-based but capped below high confidence when multiple selected-building attributes are weak for semantic review, such as missing height, low/uncertain floors, or raw/uninterpreted usage codes. These records include `nearest_match_semantic_review_needed`.

Broad evacuation areas such as park/area districts are not force-matched to a building even if a nearby or containing footprint exists. They remain `unknown` with manual review warnings.

## Qualification Rules

Official designation claims require official evidence from Chuo/Tokyo/GSI records.

PLATEAU geometry supports spatial matching only. It does not independently prove official designation.

Current B4 outputs:

- `official_confirmed`: 24
- `official_confirmed_with_review`: 3
- `unknown`: 4

Match method counts:

- `contains`: 24
- `nearest`: 3
- `unmatched`: 4

Manual review count: 7.

Route fields remain `not_evaluated`; P5-B5 will add routing.

## Outputs

Qualification outputs:

- `data_pipeline/processed/qualification/real_chuo_building_qualification.json`
- `data_pipeline/processed/qualification/real_chuo_building_qualification.csv`

Shelter-building match outputs:

- `data_pipeline/processed/qualification/real_chuo_shelter_building_matches.json`
- `data_pipeline/processed/qualification/real_chuo_shelter_building_matches.csv`

QGIS QA layers:

- `data_pipeline/processed/qgis_qa/real_chuo_shelter_points.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_building_footprints.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_match_lines.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_low_confidence_or_unmatched.geojson`

The GeoJSON QA layers are small processed QA exports. They are not raw PLATEAU or raw official source data.

## Validation

Commands run with `data_pipeline/.venv/Scripts/python.exe`:

- `data_pipeline/scripts/ingest_official_chuo_shelters.py`
- `data_pipeline/scripts/build_real_chuo_building_qualification.py`
- `data_pipeline/scripts/validate_building_qualification.py --input data_pipeline/processed/qualification/real_chuo_building_qualification.json --schema data_pipeline/qualification/evacuation_building_qualification_schema.json`
- `python -m pytest data_pipeline/tests/test_official_chuo_shelter_ingestion.py data_pipeline/tests/test_real_chuo_building_qualification.py`

Result:

- schema validation passed for 31 qualification records
- pytest passed: 9 tests

## Manual QGIS QA

Use QGIS to inspect:

- shelter point alignment against basemap/context
- matched PLATEAU footprints
- match lines from shelter points to matched building centroids
- nearest-match records
- broad evacuation areas left unmatched
- any low-confidence/manual-review point

Manual QA should confirm there is no visible coordinate shift and that nearest matches are plausible before P5-C uses these outputs in Unity.

## Final QGIS QA Result

The B4 shelter/building match layers were loaded successfully in QGIS with an OpenStreetMap basemap.

Reviewed layers:

- shelter points
- matched building footprints
- shelter-to-building match lines
- low-confidence/unmatched records

The layers were located in the Tokyo Chuo Ward context. No obvious CRS offset or severe shelter/building mismatch was observed. The user manually confirmed that the matching has no obvious issue.

Detailed spot checks should still be repeated before publication, presentation screenshots, or any user-facing Unity interpretation.

## Limitations

- P5-B4 does not parse every local PLATEAU building file; it extracts a shelter-focused mesh subset.
- Official source points can represent evacuation places or broad districts, not always buildings.
- Safe floor is not provided by the primary Chuo CSV and remains null.
- Route fields are not evaluated in B4.
- QGIS manual QA is still required before using matches for user-facing interpretation.

## Next Step

P5-B5 should add an OSM routing sample and integrated route-output fields using the existing qualification and match outputs, with OSM attribution/cache policy and all route results labeled as prototype estimates.
