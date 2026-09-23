# Codex Prompt Trace - P7-D Final High-Detail Import / Optimization / Closeout

Continue PBL7 using the established PBL4/PBL5/PBL6 workflow.

Project: ChuoTsunamiEvacuation.

Current phase: PBL7 / P7-D.

Task: Complete high-detail PLATEAU import, optimization, P2-P6 compatibility verification, Windows EXE profiling, and P7 final closeout.

Critical requirement:

P7-D must not be documentation-only unless actual high-detail PLATEAU import is impossible to automate safely. If automation is blocked, document the exact blocker and manual SDK checklist, and do not claim P7 final completion.

Guardrails:

- P7 has exactly five stages: P7-0, P7-A, P7-B, P7-C, P7-D.
- Do not create P7-E, P7-F, or P7-G.
- Do not modify `Chuo_BaseMap.unity`.
- Do not modify `ProjectSettings`, `Packages`, `Assets/Data`, or `Assets/PLATEAU`.
- Do not change gameplay success/failure rules.
- Do not implement P8 or P9 systems.
- Do not blindly commit large generated/imported city assets.

Current outcome:

- PLATEAU SDK exposes a code importer, but autonomous full import is blocked by raw-source side-effect risk, large asset/output size, and missing approved archive/LFS workflow.
- P7-D is marked blocked on manual PLATEAU SDK import.
- P7 final closeout is not complete.
