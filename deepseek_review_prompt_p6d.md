# DeepSeek Review Prompt: P6-D Playable Behavior Validation

Please review the P6-D diff on branch `p6d-playable-behavior-validation`.

Project: ChuoTsunamiEvacuation / PBL-ALL / PBL6

Stage: P6-D Playable Integration / Behavior Validation

Review focus:

- Confirm no P5/P6 safety boundary violation.
- Confirm no `ProjectSettings`, `Packages`, PLATEAU imported file, or raw PLATEAU data modification.
- Confirm no unapproved `Assets/Scenes/Chuo_BaseMap.unity` modification.
- Confirm no live routing, runtime web request, route rendering expansion, flood simulation, NavMesh adoption, A* import, Recast integration, ECS/DOTS migration, ML-Agents use, or full crowd simulation.
- Confirm `sourceMode` default remains `test`.
- Confirm `real_qualified` remains opt-in.
- Confirm `enableHumanitarianCandidates` remains default false.
- Confirm `enableLifeFirstCandidateSelection` remains default false.
- Confirm navigation remains display-only.
- Confirm NPCs remain non-blocking and do not affect player success/failure.
- Confirm P6-D runtime scripts do not reference or modify `EvacuationGameManager`, `ResultPanelController`, result metrics/export, shelter source config, or player success/failure APIs.
- Confirm warning text includes `Not official navigation` and `Not official evacuation guidance`.
- Confirm OSM route wording remains estimated prototype route, not official evacuation route.
- Confirm humanitarian candidates are still treated as non-official shelters.
- Confirm generated/test-only integration is conservative and reversible.
- Confirm behavior validation is automated-testable in EditMode/PlayMode.
- Confirm P6-D does not start P7 work.
- Confirm the P6-E final closeout path is clear.

Files expected in this stage:

- `Assets/Scripts/Simulation/P6DBehaviorValidationResult.cs`
- `Assets/Scripts/Simulation/P6DBehaviorValidationRunner.cs`
- `Assets/Scripts/Simulation/P6DPlayableBehaviorScenarioBuilder.cs`
- `Assets/Tests/EditMode/P6DBehaviorValidationTests.cs`
- `Assets/Tests/PlayMode/P6DPlayableBehaviorValidationPlayModeTests.cs`
- `docs/P6D_PLAYABLE_BEHAVIOR_VALIDATION.md`
- `docs/TASKS.md`
- `deepseek_review_prompt_p6d.md`

Please report:

- A-level blockers, if any.
- B-level risks or missing tests, if any.
- NullReferenceException risks.
- Unity lifecycle risks.
- Safety boundary violations.
- Whether P6-D is ready for P6-E final closeout after GUI tests pass.
