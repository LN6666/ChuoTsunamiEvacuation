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

Manual QGIS QA:

- B4 shelter points, matched building footprints, match lines, and low-confidence/unmatched records were loaded with an OpenStreetMap basemap.
- The user confirmed the matching has no obvious issue.

P5-B5 next step:

Use the B4 qualification and match outputs as the input to an OSM routing sample. P5-B5 should define OSM attribution/cache behavior, compute prototype route fields, keep route outputs clearly non-official, and generate route QA layers before any Unity integration.

## P5-B5 OSM Routing Sample And Integrated Output

P5-B5 completed the first OSM walking-route sample stage using B4 qualification/match outputs.

Completed scope:

- recorded five controlled route test origins for Ginza, Nihonbashi, Hatchobori/Tsukiji, Tsukishima/Kachidoki, and Harumi waterfront
- used OSMnx/NetworkX with `walk` network data and local ignored cache files
- routed from 5 origins to 27 B4 qualified shelter/building targets
- generated 135 available estimated pedestrian routes and 0 failed routes
- merged nearest route distance/time back into the 31 B4 qualification records
- produced QGIS QA layers for route origins, route lines, and route failures
- added focused tests for route output semantics, non-official route flags, integrated output fields, and QA layer presence

Completion summary:

- route records: 135
- available routes: 135
- failed routes: 0
- integrated qualification records with available routes: 27
- integrated qualification records left `not_evaluated`: 4 broad/unmatched B4 records
- route distance range: 97.727 m to 5,939.296 m
- estimated travel time range: 81.439 s to 4,949.413 s at 1.2 m/s
- pytest passed: 6 tests

Scope boundaries preserved:

- routes are OSM-based prototype walking estimates, not official evacuation routes
- no flood simulation, disaster road closure, Unity integration, gameplay rule change, scene change, `Assets/Data`, `ProjectSettings`, or `Packages` change was made
- raw/cache OSM files are ignored and not committed
- `sourceMode` remains `test`

Manual QGIS QA:

- B5 route origins and route lines were loaded with an OpenStreetMap basemap.
- The user confirmed route origins and route lines are within the Chuo Ward context with no obvious CRS offset or severe route issue.

P5-C next step:

Prepare Unity-side read-only loading of integrated qualification/route outputs, preserving confidence/warning labels and keeping the committed default `sourceMode = test` unless a later milestone explicitly changes it.

## P5-B ReviewPrep

P5-B ReviewPrep records B4/B5 validation results and prepares the DeepSeek review prompt.

Next sequence:

- run DeepSeek review for P5-B
- resolve any A-level blockers if found
- start P5-C only if P5-B passes or has B-level-only follow-ups

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

## P5-C Implemented Unity Integration

P5-C now integrates copied P5-B static outputs into Unity as read-only prototype evidence data.

Implemented:

- copied integrated qualification, OSM route sample, building qualification, and shelter-building match JSON into `Assets/Data`
- added Unity loaders that reject runtime `data_pipeline`, raw, download, cache, tmp, and `.venv` paths
- preserved status, confidence, manual review, warnings, route distance/time, route geometry metadata, and OSM/ODbL attribution
- added a limited collider-free debug marker overlay for P5-C qualification evidence
- kept current route line rendering disabled because the available geometry is WGS84 and no verified Unity coordinate transform exists
- added concise ResultPanel feedback for non-test shelter sources with an unavailable fallback when no safe `shelterId` mapping exists

P5-C remains informational only:

- no real-time routing
- no flood simulation
- no route-based, hazard-based, or qualification-based success/failure rule
- no change to default `sourceMode = test`
- no `Chuo_BaseMap.unity`, PLATEAU imported asset, `ProjectSettings`, or `Packages` change

## P5-D Implemented Real Qualified Gameplay

P5-D turns the P5-C static evidence data into an opt-in gameplay source without changing the committed default.

Implemented:

- added `sourceMode = real_qualified` while keeping `sourceMode = test` as the default
- loaded only copied static JSON under `Assets/Data`
- mapped P5-B/P5-C integrated records into gameplay-ready shelter records
- made only `official_confirmed` and `official_confirmed_with_review` records playable
- kept `strong_candidate`, `weak_candidate`, `unknown`, and `not_qualified` records non-playable/debug-only
- generated runtime-only proxy shelter targets with marker, entrance trigger, metadata label, and optional debug route-preview root
- reused existing shelter entry, climb, success/failure, and ResultPanel flow
- preserved qualification status, confidence, manual review flag, first warnings, route distance/time, estimated prototype route disclaimer, and OSM/ODbL attribution in feedback
- parsed actual route geometry from the copied route sample JSON
- rejected current WGS84 route geometry for rendering because no verified WGS84-to-Unity/PLATEAU transform exists
- kept route, qualification, and hazard information informational only; they do not determine success/failure

P5-D validation uses GUI/headful automated Unity tests:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Latest GUI/headful automated validation:

- EditMode: 102 passed, 0 failed
- PlayMode: 13 passed, 0 failed

Scope boundaries preserved:

- no live routing or web requests
- no flood simulation
- no NPC or crowd simulation
- no `Chuo_BaseMap.unity`, PLATEAU imported asset, `ProjectSettings`, or `Packages` change
- no runtime reads from `data_pipeline/processed`, `data_pipeline/raw`, `data_pipeline/downloads`, `data_pipeline/cache`, `tmp`, or `.venv`

## P5-E Verified Route Geometry Rendering QA

P5-E strengthens the existing P5-D route-preview safety gate and real_qualified QA.

Implemented:

- inspected the actual route sample schema in `Assets/Data/real_chuo_osm_routes_sample.json`
- confirmed route geometry is GeoJSON-like `LineString` data in WGS84 `EPSG:4326`
- confirmed route coordinate order is `[longitude, latitude]`
- refined Unity route parsing so missing, malformed, unsupported, or invalid WGS84 geometry fails closed
- added WGS84 sanity validation for finite coordinates, broad Chuo bounds, lon/lat order, collapse, and implausible span
- confirmed no verified WGS84-to-Unity/PLATEAU world-coordinate transform exists in the Unity runtime code
- kept route preview line rendering disabled for current real WGS84 data
- preserved route distance/time feedback, estimated prototype route labeling, and OSM/ODbL attribution
- kept selected route preview limited to a small opt-in debug subset, not all 135 route records

Current transform result:

- route geometry format: valid WGS84 lon/lat `LineString`
- Unity/PLATEAU transform: not verified / not configured
- route line rendering: safely disabled for current real data
- gameplay effect: none

P5-E validation uses GUI/headful automated Unity tests:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Latest P5-E GUI/headful automated validation:

- EditMode: 107 passed, 0 failed
- PlayMode: 13 passed, 0 failed

## P5-F High-Rise Humanitarian Candidate Screening

P5-F defines a data-only rulebook, schema, fixture, and source plan for future screening of high-rise humanitarian vertical evacuation candidates.

Implemented in the current P5-F worktree:

- created `docs/P5F_HIGHRISE_HUMANITARIAN_CANDIDATES.md`
- created `data_pipeline/qualification/highrise_humanitarian_candidate_rulebook.json`
- created `data_pipeline/qualification/highrise_humanitarian_candidate_schema.json`
- created `data_pipeline/qualification/highrise_humanitarian_candidate_sources_plan.json`
- created `data_pipeline/qualification/highrise_humanitarian_candidates_sample.json`
- added focused pytest coverage in `data_pipeline/tests/test_highrise_humanitarian_candidates.py`
- created `deepseek_review_prompt_p5f.md`

P5-F preserves this policy:

- official/designated evacuation facilities and humanitarian emergency candidate high-rises are separate layers
- non-official high-rises must not be labeled as official shelters
- unknown public access is not a hard exclusion in the life-first humanitarian emergency scenario
- unknown public access, unknown management agreement, and unknown seismic evidence remain visible review risks
- humanitarian candidate statuses require warnings and manual review

P5-F status taxonomy:

- `official_confirmed`
- `official_confirmed_with_review`
- `humanitarian_strong_candidate`
- `humanitarian_candidate_with_review`
- `humanitarian_weak_candidate`
- `unknown`
- `not_recommended`

P5-F does not perform real Chuo high-rise extraction, large downloads, scraping, Unity gameplay changes, scene changes, `Assets/Data` changes, `ProjectSettings` changes, `Packages` changes, or `sourceMode` changes.

Chronology for P5-G:

- P5-D completed the opt-in `real_qualified` gameplay source first.
- P5-E then validated route geometry parsing and fail-closed WGS84 route-preview behavior.
- P5-F then added the data-only high-rise humanitarian candidate screening foundation.
- P5-G will integrate the P5-E and P5-F outcomes without treating humanitarian candidates as official shelters and without enabling route rendering until a verified Unity/PLATEAU transform exists.

## P5-GH Humanitarian Candidate Unity Integration

P5-GH integrates the P5-F controlled high-rise humanitarian candidate sample into Unity with two explicit opt-in levels.

Implemented:

- copied `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json`
- added `enableHumanitarianCandidates = false` and `enableLifeFirstCandidateSelection = false`
- added an Assets/Data-only humanitarian candidate loader
- added display-only candidate markers with no gameplay components
- added life-first selectable candidate proxies behind both flags
- limited selectable statuses to `humanitarian_strong_candidate` and `humanitarian_candidate_with_review`
- kept `humanitarian_weak_candidate`, `unknown`, and `not_recommended` display-only
- added non-official/manual-review/access/management/seismic/life-first labels in metadata and ResultPanel feedback
- added `docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md`
- added `deepseek_review_prompt_p5g.md`

P5-GH preserves:

- `sourceMode = test` as the committed default
- `real_qualified` as opt-in only
- `candidateLayer = humanitarian_candidate` for humanitarian records
- `isOfficialShelter = false` for life-first selectable candidates
- controlled-sample warnings: P5-F sample data is not full real Chuo high-rise screening
- route/qualification/hazard/candidate status as feedback only, not success/failure rules

Latest P5-GH GUI/headful automated validation:

- EditMode: 115 passed, 0 failed
- PlayMode: 17 passed, 0 failed

## Testing And Review Strategy

- P5-A: documentation review, evidence-source review, taxonomy review, and DeepSeek architecture review.
- P5-B: schema validation, fixture tests, CRS and geometry sanity checks, routing graph tests, and QGIS manual spatial QA.
- P5-C: focused EditMode tests for loaders/mappers, PlayMode smoke tests for generated runtime objects, and Unity Editor manual validation.
- P5-D: GUI/headful automated EditMode and PlayMode tests, plus DeepSeek review for gameplay source-mode safety, Unity lifecycle behavior, route-preview fallback, and scope boundaries.
- P5-E: GUI/headful EditMode and PlayMode tests for stricter route geometry parsing, WGS84 transform validation fail-closed behavior, selected route-preview limits, and real_qualified feedback/selection safety.
- P5-F: data-pipeline JSON syntax checks, JSON Schema validation when `jsonschema` is available, pytest semantic checks for official/humanitarian separation, and DeepSeek review of life-first candidate boundaries.
- P5-GH: GUI/headful EditMode and PlayMode tests for candidate loader path guards, official/humanitarian separation, display-only markers, explicit life-first selection, and unchanged success/failure rules.

## DeepSeek Review Checkpoints

- After P5-A1: review official/non-official evidence boundaries and open-source reference decisions.
- After P5-A2: review qualification rulebook risks and schema readiness.
- After P5-B: review CRS handling, matching assumptions, routing assumptions, and reproducibility.
- After P5-C: review Unity lifecycle, scene-safety, loader behavior, and UI/feedback correctness.
- After P5-E: review route geometry parsing, WGS84 validation, route-preview fail-closed behavior, and real_qualified feedback safety.
- After P5-F: review humanitarian candidate taxonomy, non-official warning policy, schema completeness, and source-collection boundaries.
- After P5-GH: review Unity lifecycle, display-only/selectable mode boundaries, non-official labeling, path safety, and result feedback clarity.
- Before Phase 5 closure / P5-G integration: final review for blockers, scope creep, and documentation completeness.

## Known Risks

- Official source licensing or update policy may be unclear.
- Official building records may not align cleanly with PLATEAU geometry.
- Address/name matching may produce ambiguous or false-positive building links.
- Candidate criteria from papers/reports can be overinterpreted if not clearly separated from official designation.
- OSM-derived routing may be incomplete, outdated, or unsuitable for official evacuation guidance.
- Windows geospatial dependency setup can be fragile.
- Unity route visualization can become misleading if confidence and warnings are not visible.
- Humanitarian emergency candidate screening can be misread as permission or official shelter designation unless warnings remain prominent.
- Public access, management agreement, and seismic evidence may be difficult to verify for private high-rises.

## No Scope Creep Boundaries

- No official data download in P5-A0.
- No website scraping in P5-A0.
- No new Python dependency installation in P5-A0.
- No GIS routing implementation in P5-A0.
- No PLATEAU building matching implementation in P5-A0.
- No evacuation building qualification logic implementation in P5-A0.
- No Unity gameplay, scene, `Assets/Data`, `ProjectSettings`, or `Packages` changes in P5-A0.
- No Unity gameplay, scene, `Assets/Data`, `ProjectSettings`, `Packages`, large download, raw GIS commit, or `sourceMode` change in P5-F.
- No `Chuo_BaseMap.unity`, PLATEAU, `ProjectSettings`, `Packages`, raw/cache/download/tmp/.venv, live routing, web request, flood simulation, NPC/crowd simulation, or default source-mode change in P5-GH.
