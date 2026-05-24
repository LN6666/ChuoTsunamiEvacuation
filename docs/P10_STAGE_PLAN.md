# P10 Stage Plan

P10 is final QA, manual playtest readiness, Windows EXE release packaging, and project closeout.

P10 has exactly four official stages. P10-A+ is a hardening sprint under P10-A, not an additional official stage. Do not create P10-E, P10-F, or P10-G unless explicitly approved.

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
