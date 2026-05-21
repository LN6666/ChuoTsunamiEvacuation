# DeepSeek Review Prompt: P6-E Final Closeout

Please review the P6-E final closeout diff on branch `p6e-final-closeout`.

Project: ChuoTsunamiEvacuation / PBL-ALL / PBL6

Stage: P6-E Final Review / Closeout

P6 context:

- P6-0 Open-source Reference Review + Technical Selection is complete.
- P6-A Navigation Guidance Prototype is complete.
- P6-B NPC Evacuation Prototype is complete.
- P6-C Navigation + NPC Integration validation is complete.
- P6-D Playable Behavior Validation is complete.
- P6-D DeepSeek review had no A-level blockers in closeout context.
- P6-E is the final closeout stage for PBL6.

Final GUI validation:

- EditMode: 142 total / 142 passed / 0 failed / 0 skipped / 0 inconclusive.
- PlayMode: 27 total / 27 passed / 0 failed / 0 skipped / 0 inconclusive.
- XML outputs:
  - `test-results/editmode-results.xml`
  - `test-results/playmode-results.xml`

Please check:

1. All P6 stages are represented accurately.
2. No P6-F is created or implied.
3. The P7 boundary is explicit.
4. No protected files are modified.
5. No `ProjectSettings`, `Packages`, `Assets/Scenes/Chuo_BaseMap.unity`, PLATEAU imported file, raw PLATEAU data, or generated large scene changes are present.
6. `sourceMode` remains `test`.
7. `real_qualified` remains opt-in.
8. Humanitarian flags remain false by default:
   - `enableHumanitarianCandidates = false`
   - `enableLifeFirstCandidateSelection = false`
9. Navigation remains display-only and not official navigation.
10. Route rendering remains disabled/fail-closed unless coordinate transform validation is approved later.
11. NPC behavior remains non-blocking and cannot affect player success/failure.
12. NPC placement/spawning remains generated deterministic prototype placement, not real population modeling.
13. NPC target scoring remains prototype scoring, not official evacuation modeling.
14. No live routing, runtime web request, route rendering expansion, flood simulation, NavMesh adoption, A* import, Recast integration, ECS/DOTS migration, ML-Agents use, or full crowd simulation is introduced.
15. Final EditMode and PlayMode test counts are recorded accurately.
16. `docs/P6_FINAL_CLOSEOUT.md`, `docs/TASKS.md`, and `docs/REVIEW_BACKLOG.md` are accurate and do not overclaim.
17. P7 begins later and is reserved for Full Chuo Asset Loading + Underground/Bridge + LOD Upgrade + Game Optimization.

Expected P6-E documentation files:

- `docs/P6_FINAL_CLOSEOUT.md`
- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`
- `deepseek_review_prompt_p6e.md`

Please report:

- A-level blockers, if any.
- B-level risks or missing documentation/tests, if any.
- Any inaccurate or unsafe claims in the final closeout docs.
- Any protected-file boundary concern.
- Whether P6 is ready for final commit/push after this closeout.
