# Phase 5 Workplan

## P5-A0 Current Scope

P5-A0 establishes the Phase 5 workspace and documentation baseline only.

Completed scope for this milestone should be limited to:

- create the `phase5-qualification-routing-plateau` branch from `master`
- document Phase 5 goals, P5-A/B/C structure, and evidence boundaries
- create the open-source reference candidate registry
- update project progress and task tracking
- reserve an optional qualification directory for future documentation/schema outputs

P5-A0 does not download data, install dependencies, implement routing, match PLATEAU buildings, modify Unity gameplay, modify scenes, change `Assets/Data`, or alter `ProjectSettings` / `Packages`.

## P5-A1 Planned Next Task

P5-A1 should review official evidence sources and open-source reference candidates before implementation.

Expected outputs:

- official evidence registry for evacuation buildings, shelters, disaster facilities, and relevant hazard/disaster source families
- open-source reference decision notes for each candidate
- source authority, license, update-date, manual-download, and reproducibility notes
- evidence confidence levels
- manual review flag definitions
- draft field list for future building qualification outputs

P5-A1 should not ingest official data unless a later prompt explicitly authorizes it.

P5-A1 completion criteria:

- official evidence families and literature/report evidence categories are documented
- official designation and candidate qualification boundaries are explicit
- manual review triggers are listed
- preliminary open-source reference decisions are recorded
- source family, tool decision, and qualification rulebook planning JSON files validate with Python's built-in JSON parser
- no official data download, scraping, dependency installation, routing, PLATEAU matching, Unity gameplay changes, scene changes, `ProjectSettings`, `Packages`, or `Assets/Data` changes are made

P5-A2 next step:

P5-A2 should implement the first concrete qualification rulebook/schema foundation based on P5-A1 decisions. It should formalize official confirmation rules, candidate criteria, confidence levels, manual review flags, and output schema validation before P5-B begins matching/routing work.

## P5-A2 Rulebook Foundation

P5-A2 should turn the P5-A1 evidence review into a qualification rulebook foundation.

Expected work:

- define official confirmation rules
- define candidate suitability rules
- define disqualification and unknown handling
- define manual review triggers
- define schema requirements for qualification outputs
- define how official, candidate, and unknown statuses are represented without overstating certainty

P5-A2 completion criteria:

- concrete evacuation building qualification rulebook JSON is created
- JSON Schema for future qualification outputs is created
- synthetic sample fixture covers official, review, candidate, unknown, not-qualified, and unmatched cases
- validator script validates a fixture against the schema when `jsonschema` is available
- pytest coverage checks taxonomy, official/candidate boundaries, manual review triggers, and fixture shape
- no official data download, scraping, dependency installation, routing, PLATEAU matching, Unity gameplay changes, scene changes, `ProjectSettings`, `Packages`, or `Assets/Data` changes are made

P5-B1 next step:

P5-B1 should implement a controlled sample qualification/matching/routing pipeline using this schema. It should start with small fixtures and explicit CRS/match assumptions, not official downloads or Unity integration.

## P5-B1 Controlled Qualification Pipeline

P5-B1 completion criteria:

- controlled shelter, building, and route fixtures are created with only synthetic sample data
- controlled pipeline config records deterministic thresholds and output paths
- standard-library build script writes schema-shaped JSON and CSV qualification outputs
- output covers official confirmed, official with review, strong candidate, weak candidate, unknown, not qualified, and unmatched cases
- nearest/unmatched/manual review warnings are visible
- route fields are labeled as controlled prototype estimates, not official evacuation routes
- JSON syntax checks pass for controlled inputs, config, and output
- schema validation passes when `jsonschema` is available, or the missing package is documented as an environment warning
- focused tests are added without requiring GeoPandas, Shapely, pyproj, NetworkX, or OSMnx
- no official download, scraping, CityGML parsing, full PLATEAU matching, OSM routing, Unity change, scene change, `ProjectSettings`, `Packages`, or `Assets/Data` change is made

P5-B2 planned next step:

P5-B2 should move from controlled synthetic fixtures to controlled real Chuo data ingestion planning and implementation. It should define the approved source collection workflow, CRS policy, QGIS QA checklist, actual PLATEAU/OSM routing decisions, and reproducible outputs before any Unity integration.

## P5-B2 Real Chuo Ingestion Readiness

P5-B2 defines readiness plans for controlled real Chuo ingestion without downloading, scraping, installing dependencies, parsing CityGML, performing real routing, performing real PLATEAU matching, or touching Unity.

Completion criteria:

- dependency/environment plan records the current Python package availability and the project-local environment strategy
- real Chuo ingestion plan defines source families, staged ingestion, provenance requirements, raw-data policy, processed-output policy, and validation expectations
- CRS/QGIS QA plan defines projected-CRS rules, Unity coordinate boundaries, planned QA layers, and manual spatial checks
- machine-readable planning JSON files validate with Python's built-in JSON parser
- standard-library planning tests are added and syntax-checked
- `sourceMode` remains `test` and no Unity, `Assets/Data`, scene, `ProjectSettings`, `Packages`, PLATEAU, raw data, or large GIS files are changed

P5-B3 next step:

P5-B3 should create or verify a project-local Python GIS environment, then create a small controlled real-source fixture after manual source/license review. It should not process full datasets until validation, provenance, and CRS checks pass on the controlled fixture path.

## P5-B3 Environment And Source Readiness

P5-B3 prepares Phase 5 environment and source review readiness without installing dependencies, downloading data, scraping, parsing CityGML, routing, matching real PLATEAU buildings, or modifying Unity.

Completion criteria:

- `data_pipeline/requirements-p5.txt` records P5 validation/GIS/routing dependencies
- `data_pipeline/setup_p5_environment.ps1` creates and verifies a project-local `data_pipeline/.venv` when run with user approval
- source provenance/license review template is created with placeholder entries marked `template_not_verified`
- controlled real-source fixture plan defines P5-B4 preconditions, provenance, license, CRS, validation, output, and review requirements
- standard-library tests validate the new planning files and compile without requiring installed pytest
- existing planning JSON files still pass Python built-in JSON syntax checks
- setup script is not run automatically, no dependencies are installed, and no Unity or raw data files are changed

P5-B4 next step:

P5-B4 should run or verify the project-local P5 environment with user approval, complete provenance/license review for selected source candidates, and create a small controlled real-source fixture plus schema-shaped outputs. It should still avoid full datasets, full PLATEAU parsing, real routing, and Unity integration.

## P5-B4 Official Data Ingestion And PLATEAU Matching

P5-B4 completed the first real-data building qualification and local PLATEAU matching pipeline.

Completed scope:

- downloaded official Chuo/Tokyo/GSI shelter and evacuation-place source files into ignored `data_pipeline/downloads/` paths
- recorded source provenance in `data_pipeline/sources/real_chuo_official_source_manifest.json`
- normalized 31 official Chuo records into processed JSON/CSV outputs
- used a limited local PLATEAU CityGML building mesh subset from `D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\bldg`
- computed shelter-to-building `contains`, `nearest`, and `unmatched` matches using EPSG:6677 for meter distances
- generated schema-valid building qualification JSON/CSV and shelter-building match JSON/CSV
- generated small QGIS QA GeoJSON layers for shelter points, matched footprints, match lines, and low-confidence/unmatched records
- added pytest coverage for official source ingestion, output schema validation, official evidence rules, B4 routing scope, and QA layer creation

Completion summary:

- shelter records processed: 31
- candidate PLATEAU buildings extracted near shelter points: 4,447
- distinct matched PLATEAU buildings: 26
- match methods: 24 `contains`, 3 `nearest`, 4 `unmatched`
- qualification statuses: 24 `official_confirmed`, 3 `official_confirmed_with_review`, 4 `unknown`
- manual review records: 7
- schema validation passed for 31 records
- pytest passed: 9 tests

Scope boundaries preserved:

- raw official downloads are ignored and not committed
- raw PLATEAU CityGML remains local and unmodified
- no OSM routing, Unity integration, Unity scene change, `Assets/Data`, `ProjectSettings`, or `Packages` change was made
- `sourceMode` remains `test`

P5-B5 next step:

Use the B4 qualification and match outputs as the input to an OSM routing sample. P5-B5 should define OSM attribution/cache behavior, compute prototype route fields, keep route outputs clearly non-official, and generate route QA layers before any Unity integration.

## P5-B Planned Pipeline

P5-B should build the reproducible PLATEAU qualification/matching and GIS routing pipeline after P5-A rule definitions are approved.

Planned pipeline concerns:

- PLATEAU building identifier and geometry handling
- CRS discipline and coordinate transforms
- official evidence to building matching
- candidate qualification output generation
- prototype pedestrian route estimation
- route confidence and warning metadata
- QGIS spatial QA outputs
- Unity-ready processed files

P5-B routes must be labeled as estimated prototype routes unless they come from an approved official route source.

## P5-C Planned Unity Integration

P5-C should integrate processed P5-B outputs into Unity without runtime raw GIS parsing.

Planned Unity concerns:

- display qualified buildings and their status/confidence
- display estimated routes and route warnings
- show official/non-official/candidate distinctions in debug UI
- connect route/building context to decision feedback
- preserve existing `sourceMode = test` behavior unless a later milestone changes the committed default
- avoid scene and PLATEAU asset churn unless explicitly approved

## Testing And Review Strategy

- P5-A: documentation review, evidence-source review, taxonomy review, and DeepSeek architecture review.
- P5-B: schema validation, fixture tests, CRS and geometry sanity checks, routing graph tests, and QGIS manual spatial QA.
- P5-C: focused EditMode tests for loaders/mappers, PlayMode smoke tests for generated runtime objects, and Unity Editor manual validation.

## DeepSeek Review Checkpoints

- After P5-A1: review official/non-official evidence boundaries and open-source reference decisions.
- After P5-A2: review qualification rulebook risks and schema readiness.
- After P5-B: review CRS handling, matching assumptions, routing assumptions, and reproducibility.
- After P5-C: review Unity lifecycle, scene-safety, loader behavior, and UI/feedback correctness.
- Before Phase 5 closure: final review for blockers, scope creep, and documentation completeness.

## Known Risks

- Official source licensing or update policy may be unclear.
- Official building records may not align cleanly with PLATEAU geometry.
- Address/name matching may produce ambiguous or false-positive building links.
- Candidate criteria from papers/reports can be overinterpreted if not clearly separated from official designation.
- OSM-derived routing may be incomplete, outdated, or unsuitable for official evacuation guidance.
- Windows geospatial dependency setup can be fragile.
- Unity route visualization can become misleading if confidence and warnings are not visible.

## No Scope Creep Boundaries

- No official data download in P5-A0.
- No website scraping in P5-A0.
- No new Python dependency installation in P5-A0.
- No GIS routing implementation in P5-A0.
- No PLATEAU building matching implementation in P5-A0.
- No evacuation building qualification logic implementation in P5-A0.
- No Unity gameplay, scene, `Assets/Data`, `ProjectSettings`, or `Packages` changes in P5-A0.
