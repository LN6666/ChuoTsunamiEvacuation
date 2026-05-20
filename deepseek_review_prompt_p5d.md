# DeepSeek Review Prompt: P5-D Real Qualified Shelter Gameplay + Verified Route Preview 1.0

Review the current git diff for the Unity project `ChuoTsunamiEvacuation`.

Focus milestone:

P5-D - Real Qualified Shelter Gameplay + Verified Route Preview 1.0

## Context

This is a Unity + PLATEAU serious game prototype for tsunami evacuation behavior in Tokyo Chuo City.

P5-D should make copied P5-B/P5-C static real qualification/routing outputs playable as an opt-in prototype source mode while preserving the existing test-mode gameplay.

Automated Unity validation uses GUI/headful mode in this cloud Administrator environment:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Manual Test Runner clicking is not the primary validation path.

## Expected P5-D Behavior

- Default `sourceMode` remains `test`.
- `sourceMode = real_qualified` is opt-in.
- Runtime reads only copied static JSON under `Assets/Data`.
- Real qualified records are built from P5-B/P5-C static JSON.
- Only `official_confirmed` and `official_confirmed_with_review` records are playable/selectable.
- `strong_candidate`, `weak_candidate`, `unknown`, and `not_qualified` remain debug/data-only.
- Runtime proxy shelter targets are generated without modifying scenes.
- Existing E-entry, stair-climb, success/failure, and ResultPanel flow is reused.
- Result feedback preserves qualification status, confidence, manual review, warnings, route distance/time, estimated prototype route disclaimer, and OSM/ODbL attribution.
- Route geometry is parsed, but route lines are rendered only if transform validation proves the geometry is safe.
- Current WGS84 route data should not render route lines because no verified WGS84-to-Unity/PLATEAU transform exists.

## Hard Safety Boundaries

Flag any violation as A-level/blocking:

- Do not modify `Assets/Scenes/Chuo_BaseMap.unity`.
- Do not modify PLATEAU imported files.
- Do not modify `ProjectSettings`.
- Do not modify `Packages`.
- Do not read `data_pipeline/processed`, `data_pipeline/raw`, `data_pipeline/downloads`, `data_pipeline/cache`, `tmp`, or `.venv` at runtime.
- Do not implement live routing or web requests.
- Do not implement flood simulation.
- Do not implement NPCs or crowd simulation.
- Do not let route, qualification, or hazard data directly determine gameplay success/failure.
- Do not present OSM routes as official evacuation routes.
- Preserve OSM/ODbL attribution.

## Review Focus

Please review for:

- Unity compile risks and asmdef/reference risks.
- NullReferenceException risks in runtime generation and ResultPanel feedback.
- Unity lifecycle risks around `RuntimeInitializeOnLoadMethod`, generated objects, and cleanup.
- Whether `sourceMode = test` behavior is preserved.
- Whether `real_qualified` can be enabled safely and remains opt-in.
- Whether generated real qualified shelters are actually playable through the existing entry/climb/result flow.
- Whether non-selectable statuses can accidentally become playable.
- Whether runtime file loading is restricted to `Assets/Data`.
- Whether route transform validation prevents misleading WGS84 route lines.
- Whether OSM/ODbL attribution and estimated-route disclaimer are preserved in UI/metadata.
- Whether tests cover the key behavior and are appropriately scoped.
- Whether documentation accurately describes limitations and enablement.

## Expected Output

Return:

1. Overall verdict: PASS, PASS WITH B-LEVEL FOLLOW-UPS, or BLOCKED.
2. A-level blockers, if any, with file/line references.
3. B-level follow-ups, if any, with concise rationale.
4. Small Codex fix tasks, if any.
5. Confirmation whether P5-D is stable enough to commit after any required fixes and tests.
