# DeepSeek Review Prompt: NewMap Ground Raise + Name Completion + NPC Collision/Movement

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Verify the latest user decision:
- current gameplay ground cover is the authoritative gameplay ground standard
- ground/cover/support are raised to building-base samples
- imported buildings are not randomly moved in this pass
- player, NPCs, targets, green frames, and air-wall references follow the raised ground
- remaining building floating is honestly documented as gameplay visual alignment, not GIS-grade terrain/PLATEAU correction

Verify name-cache completion:
- `Assets/Data/P10/newmap_name_cache.json` is generated and populated
- runtime loads local cache only
- runtime web requests are disabled
- Japanese/Kanji main-name filtering is implemented
- full addresses, IDs, coordinates, low-confidence names, and fabricated names are hidden
- official shelter and non-official candidate warning semantics are preserved

Verify NPC/player collision and continuous movement:
- player cannot pass through sampled building bounds via gameplay collision proxy
- NPC spawn/movement avoids building bounds without heavy per-NPC pathfinding
- NPC movement has explicit states and no silent permanent stop
- 100x NPC cap/distribution/pooling safeguards remain
- performance impact is measured and not hidden

Also verify:
- blue/ground cover/fall prevention did not regress
- mouse drag and lighting were not reworked
- P2-P10 smoke behavior is not regressed
- Player.log remains 0 errors / 0 warnings
- no final release/archive was created
- no P10-E/F/G artifacts exist
- no A-level blockers remain

Validation commands already run:
- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_ground_raise_name_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_ground_raise_name_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_ground_raise_name_player_log.ps1`

Important runtime results:
- ground raise offset: 3m
- building samples: 10,787
- buildings moved this pass: 0
- Player.log: 0 errors / 0 warnings
- name cache records: 94
- runtime network requests allowed: false
- NPCs: requested/spawned/capped 800 / 800 / 800
- NPC stopped-without-reason count: 0
- 180s FPS sample after spatial building-bound index: 213.19 average FPS

Return findings ordered by severity. If no blockers, say so clearly and identify residual manual visual risk.
