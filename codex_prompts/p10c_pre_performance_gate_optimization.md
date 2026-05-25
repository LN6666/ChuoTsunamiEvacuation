# Codex Prompt Trace: P10-C-Pre Performance Gate Optimization

Task: P10-C-Pre pre-release performance gate before official P10-C.

Branch: `p10c-pre-performance-gate-optimization`

Scope:

- P10-C-Pre is not P10-C final release packaging.
- Do not create P10-E, P10-F, or P10-G.
- Do not create the final release package or archive.
- A temporary Windows x64 profiling/test build is allowed if feasible.
- Temporary build outputs must stay outside Git and must not be committed.
- Protect `ProjectSettings/`, `Packages/`, `Assets/PLATEAU/`, `Assets/Scenes/Chuo_BaseMap.unity`, and `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Add performance gate configs, quality profiles, build/profile tooling, docs, and review prompt.
- Apply only low/medium-risk runtime hardening.

Implementation summary:

- Added P10-C-Pre performance gate JSON, quality profiles, profile summary, optimization decision, and manual checklist.
- Added runtime gate, quality profile applier, staged activation config, and temporary build builder.
- Reused P10-B++ runtime optimizer and ring buffer.
- Avoided ProjectSettings, Packages, PLATEAU, Chuo_BaseMap, and high-detail scene mutation.
- Added a small green-frame runtime fix to skip redundant same-state refresh.

Validation targets:

- `powershell -ExecutionPolicy Bypass -File tools/p10/run_p10c_pre_preflight.ps1`
- Unity GUI EditMode
- Unity GUI PlayMode
- optional temporary build/profile scripts if safe
- DeepSeek review with `deepseek_review_prompt_p10c_pre.md`
