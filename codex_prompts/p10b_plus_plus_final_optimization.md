# Codex Prompt Trace: P10-B++ Final Optimization Attempt

Task: P10-B++ final optimization hardening after P10-B/P10-B+ and before P10-C.

Branch: `p10b-plus-plus-final-optimization`

Scope:

- P10-B++ is not a new official stage.
- Do not create P10-E, P10-F, or P10-G.
- Do not build the Windows EXE.
- Do not create release package or archive artifacts.
- Protect ProjectSettings, Packages, Assets/PLATEAU, Chuo_BaseMap, and P7 high-detail scene.
- Audit streaming/chunk loading honestly.
- Audit anti-aliasing/quality honestly.
- Inspect CPU, memory, GC, stutter, loading, and disk paging risks.
- Apply only low-risk optimization hardening.
- Strengthen P10-C profiler readiness.

Implementation summary:

- Added P10-B++ optimization config and quality recommendation JSON.
- Added bounded ring-buffer metrics and frame spike detector.
- Added runtime optimizer layer toggles.
- Added quality preset advisor.
- Added green-frame pool warmup and reduced activation allocation.
- Added audit docs, profiler checklist, preflight, JSON validation, and DeepSeek review prompt.
