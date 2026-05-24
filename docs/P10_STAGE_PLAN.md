# P10 Stage Plan

P10 is final QA, Windows EXE release preparation, release packaging, and project closeout.

P10 has exactly four stages. Do not create additional P10 stages unless explicitly approved.

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

Windows EXE Build + Performance Profiling + Stress Test + Optimization Pass.

Scope:

- build Windows x64 player
- smoke and stress test the high-detail scene
- collect FPS, 1 percent low/stutter, CPU, memory, GC allocation when available, loading time, runtime warnings/errors, NPC count, marker count, light curtain impact, and UI/ResultPanel impact
- run Low, Medium, and High quality preset checks
- apply low-risk optimizations only when before/after metrics support them
- export final benchmark results

## P10-C

Release Package + Documentation + Archive.

Scope:

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
