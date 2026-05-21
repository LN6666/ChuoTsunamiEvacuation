# DeepSeek Review Prompt: P6-C Navigation + NPC Integration Validation

You are reviewing the Unity project `ChuoTsunamiEvacuation` on branch `p6c-navigation-npc-integration`.

P6-C is a conservative integration validation stage after P6-A Navigation Guidance Prototype and P6-B NPC Evacuation Prototype were independently implemented, tested, reviewed, committed, and pushed.

This is not a feature expansion phase.

## Review Scope

Please review the current git diff and verify:

1. P6-A navigation guidance files are present under `Assets/Scripts/Navigation/`.
2. P6-B NPC evacuation files are present under `Assets/Scripts/NPC/`.
3. The P6-C documentation file `docs/P6C_NAVIGATION_NPC_INTEGRATION.md` is present and accurately describes the integration boundaries.
4. The P6-C integration tests are minimal and appropriate.

## Protected Files And Paths

Confirm that no protected files or paths were modified:

- `Assets/Scripts/Core/EvacuationGameManager.cs`
- `Assets/Scripts/Result/ResultPanelController.cs`
- `Assets/Scripts/Shelter/BuildingShelter.cs`
- `Assets/Scripts/Data/ShelterSourceConfigLoader.cs`
- `Assets/Data/shelter_source_config.json`
- `Assets/Scenes/Chuo_BaseMap.unity`
- `ProjectSettings/`
- `Packages/`
- PLATEAU imported files
- raw PLATEAU data

## Safety Boundary Checks

Please specifically check:

- `sourceMode` default remains `test`.
- `real_qualified` remains opt-in.
- `enableHumanitarianCandidates` remains default `false`.
- `enableLifeFirstCandidateSelection` remains default `false`.
- runtime does not read `data_pipeline/raw`, `data_pipeline/downloads`, `data_pipeline/cache`, `data_pipeline/tmp`, or `.venv`.
- no live routing, web request, or flood simulation was introduced.
- no success/failure gameplay logic changed.
- route, qualification, hazard, and candidate status remain feedback-only.
- navigation remains display-only and keeps estimated prototype route / not official guidance wording.
- NPCs remain non-blocking and cannot affect player success/failure.
- OSM routes remain labeled estimated prototype routes, not official evacuation routes.
- OSM/ODbL attribution remains preserved.
- humanitarian candidates are not treated as official shelters.

## Test Review

Please check that the P6-C tests verify:

- Navigation and NPC components can coexist in the same PlayMode test scene.
- NPC movement does not affect navigation guidance state.
- Navigation guidance refresh does not affect NPC state.
- P6-A/P6-B sources do not reference player success/failure APIs.
- Tests pass together with the existing suite.

Expected validation commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

## Requested Output

Please provide:

- A-level blockers, if any.
- B-level risks or follow-ups, if any.
- Confirmation that no protected files were modified.
- Confirmation that P6-C does not change player success/failure behavior.
- Confirmation that the P6-D closeout path is clear.
