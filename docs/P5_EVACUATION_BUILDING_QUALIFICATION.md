# Phase 5 Evacuation Building Qualification

## Phase 5 Goal

Phase 5 builds a prototype chain for evidence-based evacuation building qualification, GIS routing, PLATEAU building matching, and Unity route/building integration for map-based shelter decision validation.

The phase must keep official evidence, candidate inference, routing estimates, and Unity visualization clearly separated.

## P5 Structure

- P5-A: official evidence registry and evacuation building qualification rulebook.
- P5-B: PLATEAU building qualification/matching and GIS routing pipeline.
- P5-C: Unity route and qualified building integration.

## P5-A Research Question

Which PLATEAU buildings in Tokyo Chuo Ward can be considered evacuation buildings, and based on what evidence?

P5-A answers this by defining source families, evidence confidence, qualification statuses, and manual review rules before any matching or routing implementation begins.

## Evidence Priority

Evidence should be evaluated in this order:

1. Official evacuation shelter, evacuation building, disaster facility, or related administrative facility data.
2. Official hazard/disaster maps and administrative disaster documents.
3. PLATEAU building geometry and attributes.
4. Academic papers, policy reports, and technical reports.
5. OSM, road network, and routing data as auxiliary sources.

Papers and reports may define candidate criteria or review logic, but they must not be used to claim that a non-official building is officially designated.

## Qualification Status Taxonomy

- `official_confirmed`: the building is directly confirmed by official source evidence.
- `official_confirmed_with_review`: official evidence likely confirms the building, but spatial matching, naming, address, geometry, or update-date ambiguity requires review.
- `strong_candidate`: non-official or derived evidence strongly suggests suitability under documented criteria, but the building is not claimed as officially designated.
- `weak_candidate`: limited or partial evidence suggests possible suitability, with significant uncertainty.
- `unknown`: available evidence is insufficient to qualify or reject the building.
- `not_qualified`: evidence or rule checks indicate the building should not be treated as an evacuation building candidate.

## Required Future P5-A1 Outputs

P5-A1 should produce:

- an official evidence registry
- an evacuation building qualification rulebook
- source family definitions
- evidence confidence levels
- manual review flags
- a schema plan for future building qualification outputs

The schema plan should identify fields needed by P5-B matching/routing and P5-C Unity integration, but P5-A1 should remain documentation and evidence-review focused unless separately approved.

## P5-A1 Review Outputs

P5-A1 records the first review/planning layer for official evidence, candidate evidence, and open-source tool decisions:

- `docs/P5_OFFICIAL_EVIDENCE_REVIEW.md`: evidence hierarchy, official source families, literature/report categories, official/non-official claim boundaries, and manual review needs.
- `docs/P5_OPEN_SOURCE_REFERENCE_DECISIONS.md`: preliminary use-mode decisions for PLATEAU, QGIS, routing, and geospatial processing candidates.
- `data_pipeline/qualification/source_family_plan.json`: source family plan for future rulebook and schema work.
- `data_pipeline/qualification/tool_decision_matrix.json`: machine-readable tool decision matrix for later P5-B planning.
- `data_pipeline/qualification/qualification_rulebook_plan.json`: planned status taxonomy, confidence levels, rule groups, output fields, and non-claims.

These files are planning artifacts only. They do not download official data, install dependencies, parse CityGML, implement matching/routing, or modify Unity.

## P5-A2 Rulebook And Schema Foundation

P5-A2 converts the P5-A1 plans into concrete validation artifacts for future P5-B outputs:

- Rulebook: `data_pipeline/qualification/evacuation_building_qualification_rulebook.json`
- Output schema: `data_pipeline/qualification/evacuation_building_qualification_schema.json`
- Sample fixture: `data_pipeline/qualification/sample_building_qualification_fixture.json`
- Validator: `data_pipeline/scripts/validate_building_qualification.py`
- Tests: `data_pipeline/tests/test_building_qualification_schema.py`

The rulebook preserves the status taxonomy: `official_confirmed`, `official_confirmed_with_review`, `strong_candidate`, `weak_candidate`, `unknown`, and `not_qualified`.

Evidence rules:

- Official designation can only come from official evidence families.
- Literature/report criteria may support `strong_candidate`, `weak_candidate`, warnings, and manual review, but cannot make a building official.
- PLATEAU geometry and attributes support matching and candidate review, not official designation by themselves.
- Routing results are estimated prototype routes, not official evacuation routes.
- Hazard context is not flood simulation.

Manual review triggers include nearest/unmatched/manual building matches, large match distance, multiple building candidates, source conflict, missing safe floor or capacity, low confidence, non-official candidate status, stale/missing source metadata, PDF/manual-only evidence, unclear hazard applicability, and OSM-derived routing.

P5-B should consume this foundation by producing controlled sample qualification/matching/routing outputs that validate against the schema before any Unity integration.

## P5-B1 Controlled Sample Pipeline

P5-B1 adds a controlled sample pipeline that emits schema-shaped qualification outputs with placeholder PLATEAU matching and placeholder route fields:

- Controlled shelter fixture: `data_pipeline/qualification/sample_controlled_shelters_for_qualification.json`
- Controlled building fixture: `data_pipeline/qualification/sample_controlled_buildings_for_matching.json`
- Controlled route fixture: `data_pipeline/qualification/sample_controlled_routes_for_qualification.json`
- Pipeline config: `data_pipeline/qualification/controlled_qualification_pipeline_config.json`
- Build script: `data_pipeline/scripts/build_controlled_qualification_sample.py`
- Output JSON/CSV under `data_pipeline/qualification/controlled_building_qualification_output.*`

The controlled cases cover official contains-match confirmation, official nearest-match review, non-official strong candidate, weak candidate with missing safe floor/capacity, unknown insufficient evidence, conflict/not-qualified, and unmatched shelter/building behavior.

Official evidence is still required for `official_confirmed`. Candidate evidence from literature/report-style sources can support `strong_candidate` or `weak_candidate`, but it cannot create an official designation claim.

Nearest and unmatched matches trigger `manualReviewNeeded` with warnings. Route fields are present as controlled prototype estimates only and are not official evacuation routes.

P5-B2 should replace the controlled placeholders with controlled real Chuo source ingestion, CRS-aware PLATEAU matching decisions, OSM routing decisions, and QGIS QA before Unity integration.

## P5-B2 Real Ingestion Readiness

P5-B2 does not ingest full real Chuo data yet. It creates readiness plans for dependency setup, source provenance, CRS discipline, and QGIS QA before real spatial processing begins.

The dependency plan records that the active environment has Python and built-in JSON support, while `jsonschema`, `pytest`, GeoPandas, Shapely, pyproj, NetworkX, and OSMnx are not currently installed. P5-B3 should resolve this through a project-local environment rather than global installs.

The real Chuo ingestion plan preserves the official/candidate boundary from P5-A2 and P5-B1. Official shelter/facility and evacuation-building records are the only source families that can directly support official designation; PLATEAU attributes, hazard context, OSM routes, and literature/report criteria remain supporting evidence or review context.

The CRS/QGIS QA plan requires metric matching and route distance logic to use a projected CRS suitable for Tokyo/Chuo. QGIS QA will guard against coordinate shifts, incorrect shelter/building matches, route geometry problems, unmatched points, low-confidence points, and hazard-layer misalignment.

Future P5-B3/B4/B5 milestones should start controlled real data ingestion in stages: first a small official-source fixture, then controlled PLATEAU footprint/attribute preparation, then controlled OSM route sample preparation after download/cache/attribution policy is approved.

## P5 Safety Boundaries

- Do not confuse official buildings with non-official candidates.
- Do not make unsupported claims that a candidate building is officially designated.
- Do not parse CityGML at Unity runtime.
- Do not reimport PLATEAU data.
- Do not implement full flood simulation.
- Do not implement real road navigation in P5-A.
- Do not modify Unity scenes in P5-A.
- Do not modify `Assets/Scenes/Chuo_BaseMap.unity`.

## QGIS Role

QGIS is a manual spatial QA and validation tool, not a runtime dependency.

The Python/GIS pipeline must generate reproducible processed outputs. Unity should consume processed outputs only, never raw CityGML, raw GIS archives, QGIS project state, or manual-only intermediate files.

## Manual Validation Preview

- P5-A: evidence review, taxonomy review, official/non-official boundary review, and source confidence review.
- P5-B: QGIS spatial QA for PLATEAU matching, candidate qualification geometry checks, CRS sanity, and routing output inspection.
- P5-C: Unity validation for route/building display, confidence and warning labels, decision feedback, and preservation of existing gameplay boundaries.
