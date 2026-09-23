# Codex Prompt: NewMap Building Collision Precision

Task: implement and validate the NewMap precise building collision footprint fix.

Key requirements:

- Keep buildings blocking, but constrain collision to tight approximate footprints.
- Do not remove building collision.
- Do not keep huge inflated renderer/root bounds that block roads or sidewalks.
- Keep only ground/support, tight building footprint proxies, NPC body/soft-blocking, and circular boundary as movement blockers.
- Keep route lines, green frames, labels, markers, and hazard visuals nonblocking.
- Preserve 3.5km circular boundary, ground/fall prevention, NPC behavior, tsunami hotfix behavior, and R ranking toggle.
- Build temporary player only at `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapBuildingCollisionPrecisionPre\ChuoTsunamiEvacuation_NewMapBuildingCollisionPrecisionPre.exe`.

Required validation:

```powershell
powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_building_collision_precision_preflight.ps1
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_building_collision_precision_player.ps1
powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_building_collision_precision_player_log.ps1
python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_building_collision_precision.md
```
