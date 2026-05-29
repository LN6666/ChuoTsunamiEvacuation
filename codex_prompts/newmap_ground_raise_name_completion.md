# Codex Prompt: NewMap Ground Cover Raise + Name Cache Runtime Label Completion

Implement and validate the latest NewMap pass:
- raise current gameplay ground cover/support to sampled building bases
- keep imported buildings fixed
- resnap player, NPCs, targets, green frames, interaction zones, and air walls to raised ground
- finish preprocessing name cache generation and runtime local-cache labels
- keep runtime offline-only
- preserve NPC 100x distribution while fixing player/NPC building collision and NPC continuous movement

Do not re-import map data, rework lighting, rework mouse drag, create a release/archive, move to P11, create P10-E/F/G, claim GIS-grade accuracy, or make runtime web requests.

Validation:
- run `tools/map/run_newmap_ground_raise_name_preflight.ps1`
- run Unity EditMode and PlayMode tests
- build `NewMapGroundRaiseNamePre`
- parse Player.log
- run DeepSeek review using `deepseek_review_prompt_newmap_ground_raise_name_completion.md`
