# DeepSeek Review Prompt - P6-0 Reference Review And Technical Selection

You are DeepSeek V4 Pro performing a P6-0 documentation review for the Unity project `ChuoTsunamiEvacuation`.

## Review Goal

Review the P6-0 reference review and technical selection docs for the upcoming:

- P6-A Navigation Guidance Prototype
- P6-B NPC Evacuation Prototype
- P6-C Integration

This is a documentation and technical selection task only. No navigation gameplay, NPC behavior, package/dependency change, Unity scene change, ProjectSettings change, PLATEAU import change, source-mode change, or gameplay success/failure logic change should be introduced.

Branch:

`p6a-navigation-guidance-prototype`

## Files To Review

Review the git diff and especially:

- `docs/P6_OPEN_SOURCE_REFERENCE_REVIEW.md`
- `docs/P6_TECHNICAL_SELECTION.md`
- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`, if changed
- `deepseek_review_prompt_p60.md`

Also check that the diff does not modify:

- `Assets/**` implementation files
- Unity scenes, including `Assets/Scenes/Chuo_BaseMap.unity`
- `ProjectSettings/**`
- `Packages/**`
- PLATEAU imported files
- `data_pipeline/raw/**`
- `data_pipeline/download/**`
- `data_pipeline/downloads/**`
- `data_pipeline/cache/**`
- `data_pipeline/tmp/**`
- `data_pipeline/.venv/**`

## Specific Review Questions

1. P5 safety boundaries:
   - Do the P6-0 docs correctly preserve `sourceMode = test` as the default?
   - Do they preserve `real_qualified` as opt-in?
   - Do they preserve `enableHumanitarianCandidates = false` by default?
   - Do they preserve `enableLifeFirstCandidateSelection = false` by default?
   - Do they keep real route line rendering fail-closed until WGS84 to Unity/PLATEAU transform validation exists?
   - Do they preserve OSM estimated prototype route labeling and OSM/ODbL attribution?
   - Do they keep humanitarian candidates non-official?

2. Documentation-only scope:
   - Was any implementation introduced?
   - Was any package or dependency introduced?
   - Were `ProjectSettings`, `Packages`, Unity scenes, PLATEAU files, or protected data-pipeline paths modified?
   - Were any source-mode defaults or gameplay success/failure rules changed?

3. Technical selection:
   - Is P6-A conservatively scoped to lightweight navigation UI, target/direction/distance/estimated-time feedback, and disclaimers?
   - Does P6-A correctly avoid live routing, runtime web requests, route rendering, package changes, and success/failure changes?
   - Is P6-B conservatively scoped to 10-30 NPCs, simple prototype scoring, simple steering/transform movement, and optional state labels?
   - Does P6-B correctly reject large crowd simulation, complex social-force models, real congestion physics, ML agents, and NPC influence on player success/failure?
   - Is P6-C integration gated until P6-A and P6-B are separately reviewed and tested?

4. Reference project categorization:
   - Are Unity-Technologies/NavMeshComponents, Unity AI Navigation / `com.unity.ai.navigation`, recastnavigation/recastnavigation, A* Pathfinding Project, JR-Morgan/Crowd-Evacuation-Simulation, and keijiro/unity-crowd-simulation categorized appropriately?
   - Are optional references such as ECS samples, ML-Agents, JuPedSim, and educational pathfinding references treated conservatively?
   - Are `reference_only`, `optional_dependency`, `direct_integration`, and `not_recommended` categories used clearly?

5. License/dependency risks:
   - Are license and usage constraints documented enough for a pre-implementation selection review?
   - Are no-license or unclear-license public repos treated as inspiration only?
   - Are package/project-settings pollution risks documented?
   - Are Unity 6 compatibility risks called out where known or unknown?

6. Next-step clarity:
   - Are P6-A, P6-B, and P6-C next steps clear?
   - Are deferred risks and decisions visible?
   - Should any deferred risk be moved into `docs/REVIEW_BACKLOG.md` before commit?

## Output Format

Please provide:

- Overall verdict: PASS, PASS WITH B-LEVEL FOLLOW-UPS, or BLOCKED.
- Any A-level blockers before commit, listed first.
- B-level follow-ups with file paths and concrete fixes.
- Confirmation that P6-0 is documentation-only.
- Confirmation that no implementation/dependency/package/scene/ProjectSettings/PLATEAU protected files were modified.
- Confirmation that the technical selections are conservative and appropriate for P6-A/P6-B.

Do not modify files directly.
