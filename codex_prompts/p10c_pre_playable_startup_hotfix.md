# Codex Prompt: P10-C-Pre Playable Startup Hotfix

Objective: make the temporary P10-C-Pre EXE open into a visible playable startup flow by default, while keeping profiling/exporter behavior optional.

Constraints:

- Do not create final P10-C release package/archive.
- Do not create P10-E/F/G.
- Do not commit build artifacts.
- Treat the P7 source worktree as read-only.
- Do not mutate `Chuo_BaseMap.unity`, `ProjectSettings/`, `Packages/`, or `Assets/PLATEAU/`.

Validation:

- Run playable hotfix preflight.
- Run Unity EditMode and PlayMode tests.
- Rebuild the temporary P10CPre EXE.
- Run or manually perform actual P7 scene integration verification.
- Run DeepSeek review and address A-level blockers.
