# P5 Real Chuo Data Ingestion Plan

## Purpose

P5-B2 prepares the project for controlled real Chuo data ingestion. It defines source families, provenance requirements, staging, raw-data policy, and validation expectations before any official data download, scraping, CityGML parsing, OSM routing, or Unity integration happens.

This is a planning/readiness milestone only. It does not add real Chuo datasets or processed real GIS outputs.

## Source Families

Later P5-B milestones should review and ingest these source families in a controlled order:

1. Official shelter/facility records.
2. Official evacuation building records if available.
3. Official hazard/disaster maps.
4. Administrative disaster prevention plans.
5. PLATEAU building geometry and attributes.
6. OSM pedestrian road network.
7. Literature/report criteria.

Official source evidence and candidate inference must stay separate. Candidate sources such as literature, PLATEAU attributes, and OSM route estimates can support warnings or `strong_candidate` / `weak_candidate` status, but they must not create unsupported official claims.

## Ingestion Stages

P5-B3: controlled official source fixture ingestion.

- Create a small manually reviewed real-source fixture after source/license review.
- Preserve source provenance and explicit official/candidate boundaries.
- Validate output shape against the P5-A2 schema when dependencies are available.

P5-B4: PLATEAU building footprint/attribute preparation.

- Prepare a controlled, small building footprint/attribute sample.
- Define building ID policy, CRS policy, and match ambiguity handling.
- Do not parse full PLATEAU or CityGML until explicitly approved.

P5-B5: OSM route network sample preparation.

- Decide whether OSMnx is approved and how cache/attribution will be handled.
- Prepare a small route sample only after network download policy is approved.
- Label all route outputs as estimated prototype routes unless an official route source is approved.

P5-B6: QGIS QA and expanded processing.

- Load small processed layers into QGIS for visual QA.
- Review coordinate alignment, building matches, route lines, hazard context, and unmatched/low-confidence records.
- Keep QGIS as a QA tool, not a runtime dependency or the only reproducible source of truth.

## Provenance Requirements

Every real source record or processed source batch should preserve:

- `sourceName`
- `sourceType`
- `sourceUrl`
- `sourceUpdatedAt`
- `downloadedAt`
- `licenseTerms`
- `reliabilityLevel`
- `manualReviewNeeded`

When fields are missing, the pipeline should keep them null or explicitly unknown rather than inventing values.

## Raw Data Policy

Raw large source files must not be committed. This includes CityGML, shapefiles, GeoPackages, raster files, archives, OSM extracts, and other large GIS downloads.

Allowed committed files should be small, reviewed planning files, schemas, fixtures, and processed samples approved for the milestone.

Unity runtime should not read raw GIS archives, raw PLATEAU data, raw CityGML, QGIS project state, or manual-only intermediate files.

## Processed Output Policy

Processed outputs should be small, reproducible, schema-shaped, and suitable for later Unity-side loading only after a P5-C milestone approves integration.

Every processed output should include CRS metadata, source provenance, generation timestamp, rulebook/schema version, qualification status, warnings, and manual review fields.

## Validation Requirements

- JSON syntax checks for every planning/config/output JSON file.
- JSON Schema validation when `jsonschema` is available.
- Focused tests for official/candidate boundary rules.
- CRS metadata checks before spatial output is accepted.
- Manual review flags for ambiguous, unmatched, missing-field, and source-conflict records.

## Risks

- Official source license or attribution terms may block ingestion.
- Official facility points may be address-only or spatially ambiguous.
- Official evacuation building records may not exist as a separate machine-readable source.
- PLATEAU building IDs and source facility points may not align cleanly.
- Hazard map scope may not apply cleanly to Chuo tsunami decision logic.
- OSM route estimates may be incomplete or misleading if treated as official guidance.

## Next Action

P5-B3 should set up or verify the local Python validation/GIS environment, then create a small controlled real-source fixture after manual source/license review. It should not expand to full real datasets until the fixture path, provenance fields, and validation behavior are stable.
