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
