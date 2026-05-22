# Review Backlog

## Purpose

This document stores curated review results from DeepSeek V4 Pro and human review.

Raw DeepSeek reports are saved locally in:

review_reports/

Raw review reports should not be committed to GitHub.

This file only records actionable review items that should guide Codex fixes.

---

## P7-B Wave 2-B LOD3 Candidate Dry Run

Source report:

Pending DeepSeek review using `deepseek_review_prompt_p7b_wave2b.md`.

Context:

P7-B Wave 2-B is approved only for read-only LOD3 candidate metadata dry-run inspection. It does not import real PLATEAU assets, does not copy external data into Unity, does not modify `Chuo_BaseMap.unity`, and does not integrate with production scenes or gameplay.

Findings to preserve:

- Preferred planning candidate: `53393690`.
- Fallback planning candidates: `53393672` and `53394611`.
- `53393690` remains a planning candidate only, not a real import approval.
- LOD3 is not visually or geometry-quality verified.
- LOD4 path/name hits remain `0`; LOD4 is not assumed available.
- Future real import requires a separate explicit human approval gate.

Overall verdict:

Pending review.

### P7-B Wave 2-B Risks

| ID | Priority | Status | Target | Issue | Required Review |
|---|---|---|---|---|---|
| P7B-W2B-R01 | Medium | Open | Candidate discovery | Candidate file discovery may be incomplete or too broad because it relies on same-mesh-code path/name metadata only. | Confirm docs label discovery as metadata-only and do not treat it as a complete source manifest. |
| P7B-W2B-R02 | High | Open | LOD3 quality | Path/name metadata may not reflect geometry quality or usable Unity output. | Confirm no doc claims visual verification, geometry quality, material correctness, or gameplay suitability. |
| P7B-W2B-R03 | High | Open | Import size | Candidate import size may still be too heavy even for a small-area experiment. | Confirm `53393690` is not treated as small and future import requires narrowing plus approval. |
| P7B-W2B-R04 | Medium | Open | Visual verification | Real visual verification is deferred. | Confirm no screenshots, scene integration, or visual-quality claims are introduced. |
| P7B-W2B-R05 | High | Open | Future approval gate | Wave 2-C or any future import step needs separate approval. | Confirm this package does not approve full PLATEAU import, production scene integration, or real asset import. |

Decision:

Pending DeepSeek review after Wave 2-B preflight passes.

---

## P7-B Wave 2-A Benchmark Skeleton And Metrics Harness

Source report:

Pending DeepSeek review using `deepseek_review_prompt_p7b_wave2a.md`.

Context:

P7-B Wave 2-A is approved only for an isolated benchmark scene skeleton and prototype metrics harness under `P7Benchmark` paths. It does not import real PLATEAU assets, does not modify `Chuo_BaseMap.unity`, and does not change gameplay rules.

Overall verdict:

Pending review.

### P7-B Wave 2-A Risks

| ID | Priority | Status | Target | Issue | Required Review |
|---|---|---|---|---|---|
| P7B-W2A-R01 | Medium | Open | Metrics harness | Metrics recorder FPS and approximate 1 percent low values are sample-based and are not a replacement for Unity Profiler. | Confirm docs and code do not treat recorder output as final profiler evidence. |
| P7B-W2A-R02 | Medium | Open | Benchmark scene skeleton | The skeleton scene has no real PLATEAU geometry yet. | Confirm scene docs and object names make this clear and no visual-quality claims are made. |
| P7B-W2A-R03 | High | Open | Future LOD3 import | Wave 2-B needs explicit approval before importing LOD3 candidate data or copying real assets. | Confirm tasks, docs, and review prompt preserve the approval gate. |
| P7B-W2A-R04 | Medium | Open | Unity scene creation | Unity scene creation may need GUI or batchmode validation depending on local Unity environment behavior. | Confirm preflight and test results record success or explain failures. |
| P7B-W2A-R05 | High | Open | Project settings churn | Unity launches may create ProjectSettings or Packages line-ending/serialization churn. | Confirm any such churn is reverted unless explicitly approved. |

Decision:

Pending DeepSeek review after Wave 2-A preflight and GUI Unity tests pass or failures are documented.

---

## P7-B Area Feasibility And Candidate Selection

Source report:

Pending DeepSeek review using `deepseek_review_prompt_p7b_area.md`.

Context:

P7-B Codex A creates command-line-first feasibility reports and read-only selector tooling for small-area benchmark candidate selection. It does not import assets, modify Unity scenes, modify Unity scripts, modify `Assets/Data`, modify `ProjectSettings`, modify `Packages`, or modify imported PLATEAU assets.

Area feasibility findings to preserve:

- Preferred planning candidate: `53393690` for LOD3 path/name feasibility.
- Fallback candidates: `53393672` for compact bridge/road feasibility and `53394611` for underground feasibility.
- LOD4 path/name hits: `0`.
- Findings are path/name/file-metadata based only, not geometry-quality verified.

Overall verdict:

Pending review.

### P7-B Feasibility Risks

| ID | Priority | Status | Target | Issue | Required Review |
|---|---|---|---|---|---|
| P7B-AREA-R01 | High | Open | LOD3 candidate selection | LOD3 path/name signals may not equal usable geometry or acceptable Unity import output. | Confirm reports label LOD3 findings as unverified path/name/file-metadata inference only. |
| P7B-AREA-R02 | High | Open | LOD4 availability | LOD4 was not found by current path/name scan and must not be assumed available. | Confirm no report or task entry claims LOD4 exists. |
| P7B-AREA-R03 | High | Open | Underground / bridge / road feasibility | Underground, bridge, road, and riverfront feasibility remains unverified until Unity and geometry inspection. | Confirm docs do not claim visual correctness, walkability, route validity, entrance validity, or gameplay suitability. |
| P7B-AREA-R04 | Medium | Open | External PLATEAU source | External PLATEAU source is only enumerated; it must not be modified or committed. | Confirm helper scripts use read-only file metadata commands and do not write into `D:\PLATEAU_DATA\Chuo_2025_CityGML`. |
| P7B-AREA-R05 | Medium | Open | Benchmark decision | The preferred candidate is a planning candidate, not an approved Unity import. | Confirm final benchmark area selection remains pending until human review and a Markdown import/benchmark plan. |

Decision:

Pending DeepSeek review after P7-B area feasibility script and P7 preflight pass.

---

## P7-B Benchmark Harness Prep

Source report:

Pending DeepSeek review using `deepseek_review_prompt_p7b_harness.md`.

Context:

P7-B Wave 1 prepares the small-area high-detail benchmark harness design, future Unity change proposal, test plan, rollback plan, task entry, decision entry, and review prompt. It is docs/prompts only and does not mutate Unity scenes, scripts, assets, data, packages, project settings, or imported PLATEAU assets.

Harness prep findings to preserve:

- The isolated benchmark scene is only a future proposal.
- No `Chuo_BaseMap.unity` change is made or approved.
- No Unity mutation occurs in Wave 1.
- Wave 2 needs explicit approval before any Unity scene, script, asset, import, package, data, or `ProjectSettings` change.
- The rollback plan, test plan, benchmark harness design, and Unity change proposal are preserved.

Overall verdict:

Pending review.

### P7-B Harness Risks

| ID | Priority | Status | Target | Issue | Required Review |
|---|---|---|---|---|---|
| P7B-HARNESS-R01 | Medium | Open | Future benchmark scene | Benchmark scene creation may require Unity Editor interaction and cannot be proven by docs-only Wave 1. | Confirm Wave 2 requires explicit approval, Unity tests when Unity files change, and P7 preflight. |
| P7B-HARNESS-R02 | High | Open | Future asset import | Asset import may be heavier than expected even for a small selected cluster. | Confirm the design requires small path-cluster scope, rollback criteria, and no broad Chuo import. |
| P7B-HARNESS-R03 | Medium | Open | LOD3/LOD4 visual quality | LOD3/LOD4 visual quality is not yet verified by imported geometry or screenshots. | Confirm no visual-quality claims are made before Wave 2 benchmark evidence. |
| P7B-HARNESS-R04 | Medium | Open | Editor vs Windows x64 | EXE benchmark may differ from Editor benchmark. | Confirm Editor and optional Windows EXE metrics are recorded separately, with P7-D retaining final EXE profiling responsibility. |

Decision:

Pending DeepSeek review after P7-B Wave 1 preflight.

---

## P7-A Benchmark And Performance Automation Prep

Source report:

Pending DeepSeek review using `deepseek_review_prompt_p7a_perf.md`.

Context:

P7-A Codex B adds docs/tools/prompts only for benchmark record skeletons, Markdown performance-log validation, benchmark preflight orchestration, warning-summary traceability, Windows EXE profiling readiness, and P7-B/C/D performance preparation.

Overall verdict:

Pending review.

### P7-A Performance-Prep Risks

| ID | Priority | Status | Target | Issue | Required Review |
|---|---|---|---|---|---|
| P7A-PERF-R01 | Medium | Open | Benchmark records | Metrics may remain placeholders until a Unity benchmark scene or measurement process exists. | Confirm placeholders cannot support pass/fail decisions without measured evidence. |
| P7A-PERF-R02 | Medium | Open | Windows EXE profiling | EXE profiling depends on a later build stage and cannot be completed by docs/tools-only P7-A prep. | Confirm P7-D remains the mandatory EXE profiling closeout stage. |
| P7A-PERF-R03 | High | Open | Scope guard warning filtering | Warning filtering must not weaken protected-path strictness or hide real unsafe changes. | Confirm protected paths and large-file failures remain strict, and unsafe ordinary-file keyword warnings still appear. |

Decision:

Pending DeepSeek review after benchmark preflight passes.

---

## P7-0 Reference Review And Automation Foundation

Source report:

Pending DeepSeek review using `deepseek_review_prompt_p70.md`.

Context:

P7-0 creates documentation, scope guards, status/preflight tooling, reference review, LOD strategy, benchmark protocol, two-Codex workflow, and review prompt records for the PBL7 high-detail Chuo city foundation. P7-0 is docs/tools/prompts only and must not change Unity gameplay, scenes, assets, ProjectSettings, Packages, PLATEAU imports, or Assets/Data.

Overall verdict:

Pending review.

### P7 Risks To Review

| ID | Priority | Status | Target | Issue | Required Review |
|---|---|---|---|---|---|
| P70-R01 | High | Open | Asset strategy / future P7-A | High LOD asset size may make broad Chuo import impractical. | Confirm P7-A inventory and P7-B benchmark are required before full import. |
| P70-R02 | High | Open | Full Chuo import planning | Full Chuo import can create oversized scenes/assets and unstable Editor performance. | Confirm P7-0 blocks full import until benchmark evidence exists. |
| P70-R03 | High | Open | Dependencies / `Packages/` | Streaming or profiling candidates could require package additions. | Confirm all optional dependencies remain reference-only unless approved. |
| P70-R04 | High | Open | `ProjectSettings/` | Render pipeline, quality, occlusion, batching, or player settings could affect the whole project. | Confirm ProjectSettings remain untouched in P7-0 and future changes need decisions. |
| P70-R05 | High | Open | Windows EXE performance | Editor performance may not match Windows x64 EXE behavior. | Confirm P7 benchmark protocol separates Editor and Windows EXE metrics. |
| P70-R06 | Medium | Open | LOD strategy | LOD popping may harm presentation quality. | Confirm P7 requires visual checks and benchmark records before LOD rollout. |
| P70-R07 | Medium | Open | Collision strategy | High-detail visual geometry can create excessive collision overhead. | Confirm default policy avoids broad MeshCollider use and separates gameplay collision. |
| P70-R08 | Medium | Open | Materials/textures | Texture/material explosion can increase memory, draw calls, and build size. | Confirm material and texture counts are required benchmark metrics. |
| P70-R09 | Medium | Open | Underground/bridge data | Underground, bridge, and road data availability may be uncertain in local PLATEAU data. | Confirm P7-A inventory records category and LOD availability before promises. |
| P70-R10 | High | Open | Phase boundaries | P8/P9 scope creep could add hazard, light curtain, inundation depth, or indoor evacuation work to P7. | Confirm boundaries docs and guard warnings are clear. |
| P70-R11 | Medium | Open | Automation workflow | Manual-only process risks missed protected-path or large-file changes. | Confirm preflight and status report scripts work and are documented. |
| P70-R12 | Medium | Open | Two-Codex workflow | Parallel Codex work can cause merge conflicts in shared docs or protected files. | Confirm file ownership and integration rules are documented. |

Decision:

Pending DeepSeek review after P7-0 preflight.

---

## P7-A Asset Inventory And LOD / Area Selection

Source report:

Pending DeepSeek review using `deepseek_review_prompt_p7a_asset.md`.

Context:

P7-A adds command-line-first asset inventory scripts and Markdown reports. The scanner is read-only, runs without Unity, records file metadata, and labels LOD/category findings as path/name-based inference only.

Overall verdict:

Pending review.

### P7-A Risks To Review

| ID | Priority | Status | Target | Issue | Required Review |
|---|---|---|---|---|---|
| P7A-R01 | High | Open | `tools/p7/scan_p7_assets.ps1` | Scanner must remain read-only and must not import, move, delete, or rewrite Unity or PLATEAU assets. | Confirm scanner uses file metadata reads only. |
| P7A-R02 | High | Open | Reports | LOD findings are path/name-based and may miss actual CityGML content details. | Confirm reports clearly say geometry quality and actual LOD contents are unverified. |
| P7A-R03 | High | Open | Git diff / protected paths | P7-A must not change scenes, gameplay scripts, `Assets/Data`, `Packages`, `ProjectSettings`, or existing PLATEAU imports. | Confirm protected paths are untouched after inventory and preflight. |
| P7A-R04 | Medium | Open | Benchmark candidates | Candidate folders/categories are not final benchmark selections. | Confirm docs defer final area choice to P7-B planning and human review. |
| P7A-R05 | Medium | Open | External PLATEAU source scan | Large local source files can make broad import risky. | Confirm large-file summary is used to narrow future benchmark area size. |
| P7A-R06 | Medium | Open | `tools/p7/run_p7_asset_inventory.ps1` | Orchestration must fail non-zero if scan, report writing, or P7 preflight fails. | Confirm exit-code handling is strict enough for command-line use. |

Decision:

Pending DeepSeek review after P7-A inventory and preflight pass.

---

## P6-E Final Closeout Review

Source report:

review_reports/deepseek_review_20260522_010241.md

Context:

P6-E closes PBL6 after P6-0 reference review, P6-A display-only navigation guidance, P6-B lightweight NPC evacuation prototype, P6-C integration validation, and P6-D generated playable behavior validation.

Overall verdict:

PASS. No A-level blockers. P6 is ready for final commit/push after closeout.

Validation:

- EditMode GUI/headful: 142 total / 142 passed / 0 failed / 0 skipped / 0 inconclusive
- PlayMode GUI/headful: 27 total / 27 passed / 0 failed / 0 skipped / 0 inconclusive

### Items To Review

| ID | Priority | Status | Target File | Issue | Required Review |
|---|---|---|---|---|---|
| P6E-R01 | High | Confirmed | `docs/P6_FINAL_CLOSEOUT.md` / `docs/TASKS.md` | Confirm every P6 stage is represented accurately and P6-E does not imply P6-F. | Final review found all expected files present and consistent; no P6-F or future work implied. |
| P6E-R02 | High | Confirmed | Git diff / protected paths | Confirm no `ProjectSettings`, `Packages`, `Assets/Scenes/Chuo_BaseMap.unity`, PLATEAU imported file, raw PLATEAU data, or generated large scene changes are present. | Final review found no protected-file boundary concerns. |
| P6E-R03 | High | Confirmed | `Assets/Data/shelter_source_config.json` / docs | Confirm `sourceMode = test`, `real_qualified` opt-in, and humanitarian flags default false remain protected. | Final review found no source-mode or default-flag regression in the closeout diff; current config was manually confirmed before review. |
| P6E-R04 | High | Confirmed | P6 Navigation/NPC docs | Confirm navigation remains display-only and NPCs remain non-blocking with no player success/failure effect. | Final review found no unsafe official-navigation, simulation, or gameplay-rule overclaim. |
| P6E-R05 | Medium | Confirmed | `docs/P6_FINAL_CLOSEOUT.md` | Confirm known limitations and deferred items preserve route-rendering, NPC, no-live-routing, no-flood, and no-P7 boundaries. | Final review found safety disclaimers, deferred items, and P7 boundary clear. |
| P6E-R06 | Medium | Confirmed | Test results / closeout docs | Confirm final GUI EditMode and PlayMode XML counts are recorded accurately. | Final review confirmed the closeout docs consistently record EditMode 142 passed / 0 failed and PlayMode 27 passed / 0 failed. |

Decision:

Final DeepSeek review passed with no A-level blockers. P6-E remains documentation and validation only; P7 begins later with Full Chuo Asset Loading + Underground/Bridge + LOD Upgrade + Game Optimization.

---

## P6-0 Reference Review Deferred Decisions

Source report:

Pending DeepSeek review of `deepseek_review_prompt_p60.md`.

Context:

P6-0 is a documentation-only reference review and technical selection stage for P6-A navigation guidance and P6-B NPC evacuation. No implementation, dependency import, package change, ProjectSettings change, scene change, PLATEAU change, source-mode change, or gameplay success/failure change is approved by P6-0.

Overall verdict:

Pending review.

### Deferred Risks And Decisions

| ID | Severity | Status | Limitation / Decision | Follow-up |
|---|---|---|---|---|
| P60-B01 | Medium | Deferred | Real route line rendering remains blocked because WGS84 to Unity/PLATEAU transform validation is still unverified. | Keep route rendering fail-closed until a dedicated transform validation plan and tests exist. |
| P60-B02 | Medium | Deferred | NavMesh / AI Navigation workflow adoption could create scene, bake, package, or ProjectSettings risk. | Reconsider only after explicit approval and a package/project-settings/scene risk review. |
| P60-B03 | Medium | Deferred | A* Pathfinding Project has license/package footprint considerations. | Keep `reference_only` until license and dependency approval are complete. |
| P60-B04 | Medium | Deferred | Recast, DOTS/ECS, ML-Agents, and external pedestrian simulation tools are too heavy for P6-A/P6-B. | Treat as future research references only unless a later phase approves a technical spike. |
| P60-B05 | Medium | Deferred | Public crowd/evacuation sample repositories with unclear license files must not be copied. | Use behavior/visual inspiration only and avoid code/assets unless license clearance is documented. |
| P60-B06 | High | Deferred | P6-B NPCs could be misread as validated evacuation behavior or could accidentally influence player success/failure. | Keep NPC state labels/disclaimers clear and ensure future implementation cannot mutate player result logic. |

Decision:

P6-0 selects reference-only review first and custom lightweight prototypes for P6-A/P6-B. DeepSeek should confirm there are no A-level blockers before committing P6-0.

---

## P5 Final Closeout Review

Source report:

review_reports/deepseek_review_20260521_212540.md

Context:

P5 final closeout reviews the combined P5-D/E/F/GH integration and confirms Phase 5 is complete as a prototype/research integration stage.

Overall verdict:

PASS WITH B-level follow-ups. No A-level blockers.

Validation:

- EditMode GUI/headful: 121 passed, 0 failed
- PlayMode GUI/headful: 18 passed, 0 failed

Final DeepSeek review items:

| ID | Priority | Status | Target File | Issue | Required Review |
|---|---|---|---|---|---|
| P5FINAL-R01 | High | Confirmed | `Assets/Data/shelter_source_config.json` / source loaders | Confirm default `test`, opt-in `real_qualified`, and default-off humanitarian flags. | No source-mode or flag default regression found. |
| P5FINAL-R02 | High | Confirmed | P5-D/P5-GH loaders | Confirm Unity runtime reads copied static `Assets/Data` JSON only. | Path guards reject data_pipeline/raw/download/cache/tmp/.venv and no live routing/web calls exist. |
| P5FINAL-R03 | High | Fixed | Humanitarian candidate loader/generator/metadata | Confirm official vs humanitarian separation. | Loader now warns/diagnoses missing or non-humanitarian `candidateLayer`; candidates remain non-official; life-first mode is double opt-in. |
| P5FINAL-R04 | High | Fixed | Route parser/validator/generator | Confirm route fail-closed behavior. | Added explicit validator summary comment; EPSG:4326 route geometry remains blocked without a verified Unity/PLATEAU transform and distance/time feedback remains informational. |
| P5FINAL-R05 | Medium | Fixed | Docs | Confirm limitations and next phases are documented. | Known limitations now carry severity labels and cross-references; no official navigation claim, no full real high-rise screening claim, and P6/P7 remain deferred. |

### B-Level Closeout Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| P5FINAL-BH01 | Medium | Fixed | `docs/P5_FINAL_CLOSEOUT_REVIEW.md` / `docs/REVIEW_BACKLOG.md` | Known limitations needed dedicated severity-labeled documentation and backlog cross-references. | Added severity-labeled Known Limitations entries for route-coordinate heuristics, WGS84 span precision, `AllFinite` cleanup, QGIS recheck, and manual `real_qualified` smoke. |
| P5FINAL-BH02 | Medium | Fixed | `Assets/Scripts/Data/HumanitarianCandidateDataLoader.cs` | Missing or wrong `candidateLayer` needed a fault-tolerant warning/diagnostic skip path. | Missing layers are skipped with warning/diagnostic; non-`humanitarian_candidate` layers continue to be skipped with warning/diagnostic. |
| P5FINAL-BH03 | Low | Fixed | `Assets/Scripts/Data/P5DRoutePreviewTransformValidator.cs` | Route transform validator needed a top-level comment clarifying fail-closed EPSG:4326 behavior. | Added summary comment: real EPSG:4326 to Unity/PLATEAU conversion is future work, rendering is blocked until verified, and distance/time feedback is informational. |
| P5FINAL-BH04 | Medium | Fixed | `Assets/Scripts/Gameplay/P5GHHumanitarianCandidateRuntimeGenerator.cs` / PlayMode tests | Life-first humanitarian proxies must never look official. | Proxies are explicitly mapped with `isOfficialShelter = false`; tests assert no `Official shelter` display label. |
| P5FINAL-BH05 | Medium | Fixed | `Assets/Tests/PlayMode/P5GHHumanitarianCandidatePlayModeTests.cs` | Double opt-in display-only mode needed explicit verification that selectable proxies remain empty. | Display-only test covers no `BuildingShelter`, no `ShelterEntranceTrigger`, no colliders/Rigidbodies, and no life-first marker/entrance objects. |
| P5FINAL-BH06 | Medium | Fixed | `docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md` | Display-only marker docs needed a prominent disclaimer. | Added bold Display Markers disclaimer that display-only markers are not shelters, cannot be entered, and do not alter evacuation scoring. |

### Known Limitations

| ID | Severity | Status | Limitation | Cross-reference / follow-up |
|---|---|---|---|---|
| P5FINAL-B01 | Medium | Deferred | Route-coordinate heuristic limitations: WGS84 coordinate-order and broad Chuo bounds checks are conservative heuristics, not a verified EPSG:4326 to Unity/PLATEAU transform. | Keep route rendering fail-closed until verified transform work; related to `P5FINAL-R04` and `P5C-B04`. |
| P5FINAL-B02 | Medium | Deferred | WGS84 span precision limitation: current span validation uses approximate latitude/longitude meter conversion for broad plausibility, not survey-grade geodesic distance. | Replace with tighter geodesic validation only if future route rendering/publication QA requires it. |
| P5FINAL-B03 | Low | Deferred | `AllFinite` duplication cleanup: finite-coordinate validation is duplicated across route/data validators. | Cleanup-only refactor if route validation grows. |
| P5FINAL-B04 | Medium | Deferred | QGIS recheck status before publication/demo use: P5-B QGIS QA passed for this milestone, but should be repeated before external/user-facing publication or demo use. | Re-run shelter/building and route QA layers with an OSM basemap; related to `P5C-B03`. |
| P5FINAL-B05 | Medium | Deferred | Manual `real_qualified` smoke follow-up: automated tests cover the path, but one manual gameplay pass remains recommended before public/demo use. | Temporarily enable `real_qualified`, enter a selectable real shelter, complete result flow, verify warnings/route/OSM feedback, then restore `test`; related to `P5D-CLOSE-B05`. |

Decision:

Final DeepSeek review passed with no A-level blockers. GUI/headful automated tests passed for this B-level hardening pass, so Phase 5 is ready for commit/push/merge at human discretion; P6/P7 work remains deferred.

---

## P5-GH Closeout Follow-Ups

Source report:

review_reports/deepseek_review_20260521_203334.md

Context:

P5-GH integrates the P5-F controlled humanitarian high-rise candidate sample into Unity with display-only markers and an explicit life-first selectable mode. DeepSeek review was conditionally approved with moderate B-level follow-ups and no A-level blockers.

Overall verdict:

Ready after closeout hardening and GUI/headful automated validation.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| P5GH-R01 | High | Fixed | Assets/Scripts/Data/HumanitarianCandidateDataLoader.cs | Confirm `candidateLayer = official` records cannot enter the humanitarian layer and non-official candidates cannot become official shelters. | Loader accepts only `candidateLayer = humanitarian_candidate`, skips `official`, records diagnostics, and tests mixed official/humanitarian JSON. |
| P5GH-R02 | High | Fixed | Assets/Scripts/Gameplay/P5GHHumanitarianCandidateRuntimeGenerator.cs | Confirm display-only mode has no `BuildingShelter`, no `ShelterEntranceTrigger`, no colliders, and no success/failure effect. | PlayMode coverage verifies display-only markers have no gameplay components, no collider, no `Rigidbody`, no runtime registration, and no raycast hit. |
| P5GH-R03 | High | Fixed | Assets/Scripts/Gameplay/P5GHHumanitarianCandidateRuntimeGenerator.cs / Assets/Scripts/Core/EvacuationGameManager.cs | Confirm life-first selectable mode is explicit opt-in, non-official, warning-heavy, and does not use candidate status as a success/failure rule. | PlayMode coverage verifies the life-first flag alone creates no markers, first flag alone remains display-only, selectable proxies are non-official, and candidate metadata remains feedback-only for result flow. |
| P5GH-R04 | Medium | Fixed | Assets/Scripts/Data/HumanitarianCandidateDataLoader.cs | Confirm runtime path guards reject `data_pipeline/raw/download/cache/tmp/.venv` and require copied `Assets/Data` JSON. | Loader uses an `Application.dataPath + "/Data/"` whitelist plus forbidden runtime path rejection; EditMode tests cover raw, downloads, cache, tmp, and `.venv` paths. |
| P5GH-R05 | Medium | Fixed | docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md | Confirm docs clearly state controlled sample data, non-official status, uncertainty, and future real Chuo screening gap. | Docs state display-only non-interaction, double opt-in, non-official status, controlled P5-F sample limits, and feedback-only success/failure semantics. |
| P5GH-R06 | Medium | Fixed | Assets/Scripts/Data/HumanitarianCandidateFeedbackFormatter.cs | Result feedback should prominently warn that candidates are not official shelters even with minimal data. | Feedback now starts with `This building is NOT officially designated as an evacuation shelter.` and EditMode coverage verifies the warning with minimal record data. |

Validation:

- EditMode: 121 passed, 0 failed
- PlayMode: 18 passed, 0 failed

Decision:

Closeout hardening stays within P5-GH review scope. Display-only candidates remain non-interactive, life-first selectable candidates require double opt-in, and candidate selection remains feedback-only with respect to success/failure rules.

---

## P5-E Review Preparation

Source report:

Pending DeepSeek review.

Context:

P5-E verified route geometry rendering QA on branch `p5e-route-geometry-rendering`.

Overall verdict:

Pending review after GUI/headful automated validation passed.

### Items To Review

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| P5E-R01 | Medium | Open | Assets/Scripts/Data/P5CStaticDataLoader.cs | Route geometry parser should fail safely for missing, malformed, unsupported, or invalid WGS84 geometry. | Confirm parser leaves `HasGeometry` false without crashing and still preserves route metadata. |
| P5E-R02 | Medium | Open | Assets/Scripts/Data/P5DRoutePreviewTransformValidator.cs / Assets/Scripts/Gameplay/P5DRealQualifiedShelterRuntimeGenerator.cs | Current real routes are WGS84 and must not render unless a verified Unity/PLATEAU transform passes validation. | Confirm current EPSG:4326 route lines render zero objects and selected-route limits remain in place. |
| P5E-R03 | Medium | Open | Assets/Tests/EditMode/P5DRealQualifiedGameplayDataTests.cs / Assets/Tests/PlayMode/P5DRealQualifiedGameplayPlayModeTests.cs | real_qualified QA must preserve selectable-status policy, feedback metadata, OSM/ODbL attribution, and no gameplay rule effect. | Confirm GUI/headful EditMode and PlayMode tests cover these boundaries. |

Decision:

GUI/headful Unity validation passed:

- EditMode: 107 passed, 0 failed
- PlayMode: 13 passed, 0 failed

Send `deepseek_review_prompt_p5e.md` for review.

---

## P5-D Closeout Review Follow-Ups

Source report:

review_reports/deepseek_review_20260521_051736.md

Context:

P5-D real qualified gameplay closeout after DeepSeek PASS with B-level follow-ups only.

Overall verdict:

PASS with B-level follow-ups. No A-level blockers.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| P5D-CLOSE-B01 | Medium | Fixed | Assets/Scripts/Data/RealQualifiedShelterFeedbackFormatter.cs / Assets/Scripts/Core/EvacuationGameManager.cs | P5-D feedback formatting should not throw on missing records, malformed optional fields, null/empty shelter IDs, or missing route/qualification records. | Keep formatter and GameManager fallback exception-safe and use concise unavailable feedback without affecting success/failure. |
| P5D-CLOSE-B02 | Medium | Fixed | Assets/Scripts/Shelter/BuildingShelter.cs | `real_qualified` shelters missing `RealQualifiedShelterMetadata` should warn but continue safely. | Log a warning and keep the base shelter prompt/interaction usable. |
| P5D-CLOSE-B03 | Medium | Confirmed | Assets/Scripts/Data/P5DRoutePreviewTransformValidator.cs / Assets/Scripts/Gameplay/P5DRealQualifiedShelterRuntimeGenerator.cs | WGS84 route geometry must not render until WGS84-to-Unity/PLATEAU transform validation passes. | Current runtime route line rendering remains disabled for `EPSG:4326` data; no implementation change. |
| P5D-CLOSE-B04 | Medium | Fixed | Assets/Tests/EditMode/P5DRealQualifiedGameplayDataTests.cs | `real_qualified` source-mode smoke coverage should verify playable vs debug-only statuses and metadata preservation. | Added focused EditMode smoke coverage. |
| P5D-CLOSE-B05 | Medium | Deferred | Manual Unity validation | A one-run `real_qualified` gameplay smoke remains recommended before public/demo use. | Temporarily enable `real_qualified`, enter a selectable real shelter, complete result flow, confirm qualification/warnings/route/OSM feedback, confirm candidate/unknown records are not playable, then restore `test`. |
| P5D-CLOSE-B06 | Medium | Confirmed | tools/run_unity_tests.ps1 / docs | GUI/headful automated testing is the current approved fallback in this cloud Administrator environment. | Continue using GUI/headful EditMode and PlayMode commands that produce XML and pass/fail counts without manual Test Runner clicking. |

Decision:

Closeout fixes stay within P5-D scope. Route/qualification/hazard metadata remain feedback-only and do not affect gameplay success/failure.

---

## P5-C Unity Integration Follow-Ups

Context:

P5-B5 DeepSeek review passed with no A-level blockers, and P5-C Unity read-only integration has been implemented.

Deferred B-level items:

| ID | Priority | Status | Target | Issue | Required Fix |
|---|---|---|---|---|---|
| P5C-B01 | Medium | Deferred | P5 matching pipeline / docs | Nearest-match semantic confidence logic is conservative but can be refined. | Improve semantic confidence rules in a future data-pipeline pass. |
| P5C-B02 | Medium | Deferred | data_pipeline tests | Route tests do not yet include an artificial out-of-network failure case. | Add a controlled route failure fixture/test. |
| P5C-B03 | Medium | Deferred | QGIS QA process | QGIS spot checks should be repeated before publication or user-facing use. | Re-run shelter/building and route QA layers with an OSM basemap before external use. |
| P5C-B04 | Medium | Deferred | Unity visualization | Current route geometry is WGS84 and has no verified Unity/PLATEAU coordinate transform. | Define and validate a coordinate transform before rendering route lines. |

Decision:

No immediate Codex fix is required for these B-level items before P5-C review. They should be included in the next DeepSeek review prompt.

---

### P5-C Closeout Review 2026-05-20

Source report:

DeepSeek V4 Pro P5-C review.

Context:

P5-C read-only Unity integration for qualified building evidence, estimated route metadata, and decision feedback.

Overall verdict:

PASS with B-level issues only. No A-level blockers. P5-C can be marked complete as a read-only informational Unity integration.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| P5C-CLOSE-B01 | Medium | Deferred | ResultPanel / P5-C debug metadata flow | P5-C evidence may currently be visible only through debug, `GetDebugText`, or `M` metadata/debug flow. If intended for all players, the main ResultPanel visibility should be verified. | Future UI pass may move or duplicate P5-C evidence into the main result panel if player-facing visibility is required. |
| P5C-CLOSE-B02 | Medium | Deferred | Assets/Scripts/Core/EvacuationGameManager.cs / Assets/Scripts/Data/P5CDecisionFeedbackFormatter.cs | `EvacuationGameManager` calls `P5CDecisionFeedbackFormatter` methods. Current assembly setup should make this available, but the dependency should remain explicit and safe. | Add a defensive guard or confirm asmdef dependency guarantees availability if assembly boundaries change. |
| P5C-CLOSE-B03 | Medium | Deferred | Route tests / data_pipeline tests | Route tests do not yet include an artificial out-of-network route failure fixture. | Add a controlled out-of-network route failure fixture/test in a future route coverage pass. |
| P5C-CLOSE-B04 | Medium | Deferred | Unity route visualization | WGS84 route geometry to Unity/PLATEAU coordinate transform has not been verified. | Keep route rendering disabled/deferred until the coordinate transform is validated. |
| P5C-CLOSE-B05 | Medium | Deferred | real_sample mapping / P5-C docs | `real_sample` shelter IDs do not safely map to P5-B official shelter IDs. The unavailable evidence fallback is intentional and should stay explicit. | Do not invent unsafe shelter/building mappings; explain the expected fallback in docs and UI/debug text where relevant. |
| P5C-CLOSE-B06 | Medium | Fixed | Assets/Scripts/Data/P5CStaticDataLoader.cs | Loader path safety needed manual confirmation that runtime reads only copied `Assets/Data` files and does not touch pipeline/raw/cache/temp environments. | Manual audit completed on 2026-05-20. Runtime reads use `Application.dataPath + "/Data/"`; no code change required unless loader paths change. |

Decision:

No implementation change is required for P5-C closeout. The remaining items are deferred follow-ups, except the loader path safety audit, which is complete with no code changes.

---

## Why This File Exists

The project uses a semi-automatic AI development workflow:

1. Codex writes or modifies code.
2. Unity is used for compile and Play testing.
3. DeepSeek V4 Pro reviews the current git diff.
4. The review result is summarized into this document.
5. The human developer decides which issues should be fixed.
6. A focused Codex fix prompt is created from the selected issues.
7. Codex fixes the selected issues.
8. Unity is tested again.
9. Stable changes are committed to Git.

This file is the bridge between DeepSeek review and Codex fixing.

---

## Review Workflow

1. Codex implements or modifies code.
2. Run Unity compile test.
3. Run Unity Play test when possible.
4. Run DeepSeek review:

   python tools\deepseek_review.py

5. Read the generated report in:

   review_reports/

6. Summarize only actionable issues here.
7. Convert selected issues into a Codex fix prompt.
8. Let Codex fix only the selected issues.
9. Re-test in Unity.
10. Commit only after the project is stable.

---

## Status Labels

Use these status labels:

- Open
- In Progress
- Fixed
- Deferred
- Rejected

Meaning:

Open:
The issue is accepted but not fixed yet.

In Progress:
The issue is currently being fixed by Codex or manually.

Fixed:
The issue has been fixed and tested.

Deferred:
The issue is valid but will be handled later.

Rejected:
The issue is not accepted because it is out of scope or not relevant.

---

## Priority Labels

Use these priority labels:

- Critical
- High
- Medium
- Low

Meaning:

Critical:
Must fix before commit. Usually compile errors, runtime crashes, broken Unity references, or core gameplay failure.

High:
Should fix before commit if possible. Usually serious maintainability, state flow, or Unity setup problems.

Medium:
Useful improvement, but not blocking the current milestone.

Low:
Minor improvement, style issue, or future refactor item.

---

## Review Entry Template

### Review YYYY-MM-DD-XX

Source report:

review_reports/deepseek_review_YYYYMMDD_HHMMSS.md

Context:

Briefly describe what was reviewed.

Example:

First playable gameplay scripts after Codex generated the core loop.

Overall verdict:

Safe to commit / Commit after minor fixes / Do not commit yet

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| R-001 | High | Open | Assets/Scripts/... | Describe issue here | Describe required fix here |

### Codex Fix Prompt

Plain-text prompt to send to Codex:

Please fix the following reviewed issues.

Target files:

- Assets/Scripts/...

Rules:

- Do not modify unrelated files.
- Do not commit.
- Do not touch PLATEAU imported scene files.
- Do not touch raw PLATEAU data.
- Keep the fix focused and minimal.

Issues:

1. ...
2. ...

After fixing:

- summarize changed files
- explain how to test
- do not commit

---

## Active Reviews

No active blocking review items.

---

### Review Pending 2026-05-21-P5F

Source report:

Pending DeepSeek review for P5-F high-rise humanitarian candidate screening.

Context:

P5-F creates data-only rulebook, schema, source-plan, sample fixture, and tests for life-first high-rise humanitarian vertical evacuation candidate screening.

Overall verdict:

Pending review.

### Known P5-F Review Questions

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| P5F-RQ01 | Medium | Open | docs/P5F_HIGHRISE_HUMANITARIAN_CANDIDATES.md / data_pipeline/qualification/highrise_humanitarian_candidate_rulebook.json | Confirm the official/designated layer and humanitarian candidate layer are separated clearly enough to prevent non-official candidates being treated as official shelters. | DeepSeek should review taxonomy and warning policy. |
| P5F-RQ02 | Medium | Open | data_pipeline/qualification/highrise_humanitarian_candidate_schema.json / data_pipeline/qualification/highrise_humanitarian_candidates_sample.json | Confirm schema fields are sufficient for future real Chuo high-rise screening without overclaiming seismic, access, or management agreement evidence. | DeepSeek should identify missing evidence fields or unsafe assumptions. |
| P5F-RQ03 | Medium | Open | data_pipeline/tests/test_highrise_humanitarian_candidates.py | Focused pytest exists but was not executed in system Python because `pytest` and `jsonschema` are unavailable. | Run in project-approved Python environment before commit if dependencies are available. |

---

### Review Pending 2026-05-16-01

Source report:

Pending DeepSeek review for Milestone 2-05.

Context:

Milestone 2-05 - Evaluation, Decision Feedback & Real-Data Integration Hooks 1.0 implementation.

Overall verdict:

Pending review.

### Known Non-Blocking Risks To Review

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-05-B01 | Medium | Deferred | Assets/Scripts/Result/ResultExportService.cs | Result export uses a local run_logs/ folder derived from Application.dataPath. This is acceptable for the Editor/debug prototype but may not be writable on all build targets. | Revisit export path policy before player builds or CI-driven evaluation runs; consider Application.persistentDataPath or a configurable output directory. |
| M2-05-B02 | Medium | Deferred | Assets/Scripts/Data/ShelterSourceConfigLoader.cs / Assets/Data/shelter_source_config.json | The real-data source mode is a documented P4 hook only. It does not load real data yet. | Implement active real sample loading only during P4 after P3 output format is reviewed. |
| M2-05-B03 | Low | Deferred | Assets/Scripts/Result/ResultMetrics.cs / Assets/Scripts/Result/ResultPanelController.cs | ResultPanel remains a fixed prototype/debug UI. Very long advice or failure reason text may still need a later scroll view. | Revisit ResultPanel layout only if UI polish becomes a milestone. |
| M2-05-B04 | Medium | Deferred | Assets/Data/scenario_presets.json | scenario_presets.json should be saved as UTF-8 without BOM. Current .NET reading may tolerate a BOM, but future loaders or external tools may not. | Keep Unity JSON files BOM-free when editing or regenerating them. |
| M2-05-B05 | Medium | Deferred | Assets/Scripts/Result/ResultMetrics.cs | Blocked-shelter advice currently depends on failureReason substring matching, such as unavailable or not available text. If failure reason wording changes, advice may fall back to generic text. | Consider structured failure codes or enums instead of string matching in a future result-reason pass. |
| M2-05-B06 | Medium | Deferred | Assets/Scripts/Core/EvacuationGameManager.cs | Result export now uses a one-shot guard, but future result-state changes could accidentally bypass or duplicate finalization paths. | Preserve explicit one-export-per-run behavior when changing success/failure finalization. |

---

## Notes

DeepSeek review should not be followed blindly.

The human developer must decide:

- which issues are valid
- which issues are urgent
- which issues should be deferred
- which issues are outside the current milestone

Codex should receive only focused, selected fix tasks.

Do not send the entire raw review report to Codex unless necessary.

---

### Review 2026-05-15-04

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-04 - Multi-Shelter Decision Gameplay 1.0.

Overall verdict:

No A-level blocking issues. Non-blocking risks deferred.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-04-B01 | Medium | Deferred | Assets/Scripts/Data/ShelterDataLoader.cs | GetAllShelters currently iterates over ShelterIdsInLoadOrder from the base JSON. Current scenarios only override existing IDs, so this is safe now. If a future scenario introduces a brand-new shelter ID, it will not appear in the generated field unless loader behavior is extended. | If future scenarios need runtime-only shelters, extend GetAllShelters and scenario override handling to include safely validated new shelter IDs. |
| M2-04-B02 | Medium | Deferred | Assets/Scripts/Editor/FirstPlayableSceneBuilder.cs | BuildFirstPlayableTestSetup creates marker materials for official/candidate/blocked shelters. This is harmless for the current small debug field, but repeated editor invocations may clutter editor material instances. | Cache, reuse, or clean up generated editor materials if setup generation becomes frequent or material clutter becomes visible. |
| M2-04-B03 | Medium | Deferred | Assets/Tests/EditMode/ChuoTsunamiEvacuation.EditModeTests.asmdef | The EditMode test asmdef references ChuoTsunamiEvacuation.Editor. This is valid for EditMode tests. If future tests are moved to PlayMode assemblies, maintainers must avoid editor-only dependencies. | Keep editor-only references limited to EditMode tests; do not reference ChuoTsunamiEvacuation.Editor from PlayMode test assemblies. |
| M2-04-B04 | Medium | Deferred | Assets/Scripts/Shelter/ShelterEntranceTrigger.cs | ShelterEntranceTrigger.Update calls gameManager?.ApplyShelterConfig(shelter) every frame while the player is inside the trigger. This is currently idempotent and harmless for the small debug-platform prototype, but it creates minor runtime overhead and slightly deviates from an event-driven trigger design. | Future optimization could apply shelter config only on trigger enter, shelter change, or state change. |

### Decision

No immediate Codex fix is required before this commit for the remaining B-level items.

Reason:

- The review found no A-level blockers.
- Current scenarios only override existing shelter IDs.
- Material lifecycle and test assembly boundaries are acceptable for the current debug-platform prototype.

---

### Review 2026-05-15-03

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-02 - Scenarioized Gameplay Rules 1.0.

Overall verdict:

No A-level blocking issues. Non-blocking risks deferred.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-02-B01 | Medium | Deferred | Assets/Data/scenario_presets.json | scenario_presets.json may contain a UTF-8 BOM. Manual editing in BOM-unaware tools may cause confusion. | Save scenario_presets.json as UTF-8 without BOM. |
| M2-02-B02 | Medium | Deferred | Assets/Scripts/ChuoTsunamiEvacuation.Runtime.asmdef / Assets/Tests/EditMode/ChuoTsunamiEvacuation.EditModeTests.asmdef / Assets/Tests/PlayMode/ChuoTsunamiEvacuation.PlayModeTests.asmdef | EditMode and PlayMode test assemblies reference ChuoTsunamiEvacuation.Runtime. Future test-needed runtime scripts must stay inside the runtime assembly or tests may fail to compile. Risk is low, but maintainers should be aware of the assembly boundary. | Keep runtime scripts that tests need under the runtime assembly, or update asmdef references when adding new assembly boundaries. |
| M2-02-B03 | Medium | Deferred | Assets/Scripts/Result/ResultPanelController.cs / Assets/Scripts/Result/ResultMetrics.cs | Current ResultPanel is readable, but very long scenario names or verbose failure reasons may still clip. This is acceptable for prototype/debug UI. | Revisit ResultPanel sizing or add a simple ScrollView if UI polish becomes a milestone. |

### Decision

No immediate Codex fix is required before this commit for the remaining B-level items.

Reason:

- The review found no A-level blockers.
- The UTF-8 BOM cleanup was handled.
- The assembly and ResultPanel items are maintainability/UI risks for future milestones.

---

### Review 2026-05-15-02

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-03 - Unity Test Automation Foundation.

Unity testing confirmed:
- EditMode tests ran successfully.
- PlayMode smoke tests ran successfully.

Overall verdict:

No A-level blocking issues. Non-blocking risks deferred.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-03-B01 | Medium | Deferred | Assets/Tests/EditMode/AntiCampingConfigTests.cs / Assets/Tests/EditMode/ShelterDataLoaderTests.cs | Current tests verify normal config loading and defaults, but do not explicitly test missing anti_camping_config.json or missing test_shelters.json. Risk is low because files exist in normal setup, but future test rounds should cover accidental deletion/fallback behavior. | Add missing-file/fallback EditMode tests for AntiCampingConfig and ShelterDataLoader in a future test coverage pass. |
| M2-03-B02 | Medium | Deferred | Assets/Tests/PlayMode/EvacuationSmokePlayModeTests.cs / Assets/Scripts/Core/EvacuationGameManager.cs | Current PlayMode tests instantiate EvacuationGameManager on an empty GameObject. This is acceptable now, but future milestones may add heavier Awake initialization or required scene references. | Watch smoke test sensitivity to future EvacuationGameManager Awake/init changes; add a minimal fixture setup if needed. |

### Decision

No immediate Codex fix is required before this commit.

Reason:

- The review found no A-level blockers.
- Unity EditMode and PlayMode test runs passed.
- The issues are future test coverage and test fixture robustness improvements.

---

### Review 2026-05-15-01

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-01 - Data-Driven Rules Integration after Unity testing and stabilization fixes.

Unity Play testing confirmed:
- manual T tsunami start works
- countdown starts only after warning
- random warning works when enabled
- shelter JSON can block entry and show failureReason
- climbTimeSeconds and crowdingDelaySeconds affect climb duration
- anti-camping detection and blocking work when enabled
- anti-camping is disabled by default
- missing tsunami config uses safe defaults
- unknown shelterId preserves in-scene shelter values
- ResultPanel is readable
- tsunami risk-front failure catches player bypassing the visible wall
- active shelter entrance fails during climb if the risk front passes it
- MarkSceneDirty no longer errors in Play Mode

Overall verdict:

No A-level blocking issues after fixes. Ready to commit after documenting B/C items.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-01-B01 | Medium | Deferred | Assets/Scripts/Core/EvacuationGameManager.cs / Assets/Data/tsunami_event_config.json | If both manualStartEnabled and randomStartEnabled are false, the game can stay in PreEvent with no start path. | Later mitigation: warning, fallback to manual, or explicit disabled-event mode. |
| M2-01-B02 | Medium | Deferred | Assets/Scripts/Data/GameConfigLoader.cs / Assets/Scripts/Data/ShelterDataLoader.cs | Runtime loading uses Application.dataPath + "/Data/...". This is acceptable for the Editor prototype but not build-safe. | Later migrate to StreamingAssets or another build-safe loading path. |
| M2-01-C01 | Low | Deferred | Assets/Scripts/Result/ResultPanelController.cs | If ResultPanel detail text grows much longer, fixed text areas may become cramped again. | Consider a simple ScrollView in a future UI pass. |

### Decision

No immediate Codex fix is required before this commit for the B/C items.

Reason:

- A-level blockers were fixed.
- Unity testing confirmed the Milestone 2-01 gameplay loop.
- Remaining issues are deferred design/tooling polish for later Milestone 2 tasks.

---

### Review 2026-05-14-02

Source report:

D:\UnityProjects\ChuoTsunamiEvacuation\review_reports\deepseek_review_20260514_032100.md

Context:

DeepSeek V4 Pro max-thinking review for the first playable evacuation prototype.

Unity Play testing confirmed:
- WASD / arrow-key movement works
- Shift sprint works
- mouse-based third-person camera control works
- E shelter entry works
- climb simulation works
- T starts tsunami test
- tsunami risk can trigger failure
- tsunami reaching the active shelter entrance during climb can trigger failure

Overall verdict:

Safe to commit

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| R-002 | Medium | Deferred | Assets/Scripts/Shelter/ShelterEntranceTrigger.cs | Missing shelter reference could be clearer if setup is broken. | Add null warning in Awake and Update before TryEnterShelter in a future cleanup pass. |
| R-003 | Low | Deferred | Assets/Scripts/Player/SimplePlayerController.cs | fallbackTranslateIfControllerStuck is useful for debugging but can bypass collision. | Add a production-warning comment or revise after movement system stabilizes. |
| R-004 | Low | Deferred | Assets/Scripts/Core/EvacuationGameManager.cs | Public state-changing methods could use clearer summary comments. | Add XML summary comments in a later documentation cleanup. |
| R-005 | Low | Deferred | Assets/Scripts/UI/GameUIManager.cs | Uses legacy UnityEngine.UI.Text instead of TextMeshPro. | Accept for prototype; consider migration after gameplay stabilizes. |

### Decision

No immediate Codex fix is required before this commit.

Reason:

- No critical issues were found.
- Unity Play testing confirmed the first playable loop works.
- The remaining issues are maintainability or future cleanup items.
- The prototype should be committed now as a stable milestone.

