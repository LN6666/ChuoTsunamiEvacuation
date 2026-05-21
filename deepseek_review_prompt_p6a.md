# DeepSeek Review Prompt: P6-A Navigation Guidance Prototype

Please review the current git diff for P6-A on branch `p6a-navigation-guidance-prototype`.

Focus on safety, Unity lifecycle risks, compile risks, and whether the implementation stays within the approved lightweight/display-only scope.

## Required Checks

Confirm:

- No P5/P6 safety boundary violation.
- No `ProjectSettings`, `Packages`, scene, `Chuo_BaseMap.unity`, PLATEAU imported file, raw PLATEAU data, or protected data-pipeline path modification.
- No live routing, runtime web request, NavMesh adoption, A* dependency, Recast dependency, AI Navigation package workflow, flood simulation, or route service call.
- No claim that guidance is official navigation, official evacuation guidance, or an official evacuation route.
- OSM route feedback remains labeled as an estimated prototype route and OSM/ODbL attribution is preserved when available.
- Gameplay success/failure logic is not changed.
- Navigation feedback does not mutate shelter validity, tsunami timing, result state, or climb outcome.
- Real WGS84 route line rendering remains fail-closed unless a verified transform explicitly allows it.
- Implementation scripts are under `Assets/Scripts/Navigation/`.
- Tests cover direction, distance, estimated time, warnings/disclaimers, missing target fail-safe behavior, and display-only PlayMode behavior.
- P6-C integration notes are clear and do not require modifying shared core files during P6-A.

## Files Expected In Scope

- `Assets/Scripts/Navigation/`
- `Assets/Tests/EditMode/P6ANavigationGuidanceTests.cs`
- `Assets/Tests/PlayMode/P6ANavigationGuidancePlayModeTests.cs`
- `docs/P6A_NAVIGATION_GUIDANCE_PROTOTYPE.md`
- `deepseek_review_prompt_p6a.md`

## Protected Files That Should Not Be Modified

- `Assets/Scripts/Core/EvacuationGameManager.cs`
- `Assets/Scripts/Result/ResultPanelController.cs`
- `Assets/Scripts/Shelter/BuildingShelter.cs`
- `Assets/Scripts/Data/ShelterSourceConfigLoader.cs`
- `Assets/Data/shelter_source_config.json`
- `Assets/Scenes/Chuo_BaseMap.unity`
- `ProjectSettings/`
- `Packages/`

## Requested Output

Please list:

- A-level blockers, if any.
- B-level risks or follow-up tasks.
- Compile or Unity lifecycle risks.
- Missing or weak tests.
- Any wording that could be misread as official navigation or official evacuation guidance.
- Whether P6-A is safe to proceed to human review after GUI EditMode/PlayMode validation.
