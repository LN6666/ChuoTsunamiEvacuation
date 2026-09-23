# DeepSeek Review Prompt: P9-B Runtime Prototype

Review the current git diff for P9-B:

`feat(p9-b): add new-map spawn and crowd runtime prototype`

Verify:

- P9-B stays within runtime prototype scope.
- No P9-C final failure gameplay was introduced.
- No NPC, crowd, collapse, or debris code directly causes player failure.
- No P9-B code mutates player success/failure outcomes.
- No real indoor scene, building interior, staircase, fire escape interior, representative indoor template, or BIM/LOD4 indoor gameplay was introduced.
- Humanitarian high-rise candidates remain non-official and warning-required.
- The P8-E 110-candidate handoff is consumed rather than reimplemented.
- Weighted spawn is deterministic and biased toward coastal, low-elevation, river/canal, high-inundation, underground/metro, and dense activity zones.
- Exclusion zones, already flooded zones, invalid positions, and unsafe zero positions are handled.
- Collapse/debris risk zones are prototype markers only.
- P5 routes remain estimated prototype guidance, not official routes.
- P8-E semantic binding is used as proxy/gameplay-usable metadata only, not full scene semantic coverage.
- P2-P6 compatibility is strengthened through adapters/runtime components without rewriting old systems.
- Protected files are clean: `Chuo_BaseMap.unity`, `ProjectSettings`, `Packages`, `Assets/PLATEAU`, and `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Tests are meaningful and not superficial.
- P9-B preflight checks the important boundaries.
- There are no A-level blockers.

Report:

- verdict
- A-level blockers, if any
- B-level follow-ups
- test and preflight concerns
- protected-path concerns
