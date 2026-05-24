# P10 Stage Plan

P10 is final QA, manual playtest readiness, Windows EXE release packaging, and project closeout.

P10 has exactly four official stages. P10-A+, P10-B+, and P10-B++ are polish/hardening sprints under the existing P10 flow, not additional official stages. Do not create P10-E, P10-F, or P10-G unless explicitly approved.

## P10-A

Remaining Gap Closure + High-Detail Scene QA.

Scope:

- classify remaining P7/P8/P9 gaps
- verify high-detail scene smoke readiness without scene mutation
- confirm coordinate anchoring and humanitarian warning boundaries
- prepare P10-B build, stress-test, profiling, and low-risk optimization plan
- run preflight, Unity tests, and DeepSeek review

P10-A does not build the final Windows EXE, package release artifacts, archive high-detail scenes, or add new gameplay systems.

## P10-B

High-Detail Runtime Smoke + Performance Profiling + Stress Test + Optimization + Manual Playtest Preparation.

Scope:

- prepare high-detail runtime smoke and manual playtest checklist
- add tsunami-start green ground frame markers for official and non-official evacuation-related building targets
- collect FPS, 1 percent low/stutter, CPU, memory, GC allocation when available, loading time, runtime warnings/errors, NPC count, marker count, light curtain impact, and UI/ResultPanel impact
- run Low, Medium, and High quality preset checks
- apply low-risk optimizations only when before/after metrics support them
- keep Windows EXE build deferred to P10-C

## P10-B+

UI / Localization / Weather / Stamina / Manual Playtest Polish.

Scope:

- add lightweight English/Japanese localization
- add runtime-ready start, pause, options, and rules UI
- export English and Japanese game rules
- prepare safe background image policy without committing unlicensed images
- add weather/night movement modifiers
- add deterministic stamina/sprint rules and HUD support
- keep avatar presentation separate from mobility profile
- keep optional gender speed modifier disabled by default and documented as a scenario assumption
- keep Windows EXE build deferred to P10-C

P10-B+ is not an official additional stage and does not create P10-E/F/G.

### P10-B++ Hardening Sprint

Final Optimization Attempt Before P10-C.

Scope:

- audit streaming/chunk loading status honestly
- audit anti-aliasing and quality status without changing ProjectSettings or URP assets
- inspect CPU, memory, GC, stutter, and disk paging risks
- apply only low-risk runtime optimization hardening
- strengthen metrics, profiler checklist, and P10-C readiness
- keep Windows EXE build, release packaging, and archive work deferred to P10-C

P10-B++ is not an official additional stage and does not create P10-E/F/G.

## P10-C

Windows EXE Build + Release Package + Documentation + Archive.

Scope:

- build Windows x64 player after user manual playtest and quick P10-B fixes
- assemble release package
- prepare player-facing and review-facing documentation
- archive or back up local high-detail scene and import metadata
- ensure large build/archive artifacts are handled outside normal Git unless explicitly approved

## P10-D

Final DeepSeek Review + Release Candidate Closeout.

Scope:

- final release-candidate review
- verify protected paths and known limitation wording
- confirm no additional stages were opened
- close remaining B-level items or document release limitations
