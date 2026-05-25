# DeepSeek Review Prompt: P10-C-Pre Playable Startup Hotfix

Review the git diff for P10-C-Pre Playable Startup Hotfix.

Verify:

- Temporary EXE blank-start issue is addressed by a visible playable startup flow.
- Start Menu, English/Japanese language selector, Rules UI, pause UI, and Start Game flow exist.
- Start Game targets `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Runtime diagnostics prevent blank screen when high-detail scene/dependencies are missing.
- Profiling exporter is explicit opt-in and does not auto-quit default playable builds.
- Built-player data path fallback handles copied `*_Data/Data` folders.
- P7 source worktree is not mutated.
- P9 high-detail target scene is not committed as a large generated scene.
- No final release package/archive/build artifacts are committed.
- No P10-E/F/G artifacts are created.
- Protected paths are clean: `Chuo_BaseMap.unity`, `ProjectSettings/`, `Packages/`, `Assets/PLATEAU/`.
- Tests meaningfully cover startup config, exporter defaults, language/rules/start flow, data path fallback, diagnostics, and green-frame/runtime safety.
- Actual P7 scene integration verification honestly documents any blocker or limitation.

Return A-level blockers first. Do not rewrite the codebase.
