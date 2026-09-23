# Codex Prompt: NewMap Name Enrichment Completion

Task: complete the NewMap building/road name enrichment pass.

Requirements:
- Actually run preprocessing-stage coordinate/network name matching for missing building/road labels.
- Generate/update `Assets/Data/P10/newmap_name_cache.json`.
- Keep Unity runtime offline-only; it must never call web APIs.
- Runtime labels must load the local cache.
- Prefer Japanese/Kanji main names only.
- Hide full addresses, postal-address strings, coordinates, GML/building IDs, and low-confidence names in normal mode.
- Do not fabricate names or machine-translate.
- Preserve official shelter names and non-official candidate warnings.
- Do not rework lighting, mouse, ground, NPC distribution, or gameplay flow unless a regression is detected.
- Do not create a final release/archive, P11, or P10-E/F/G.

Expected validation:
- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_name_enrichment_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/enrich_newmap_names_from_coordinates.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_name_enrichment_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_name_enrichment_player_log.ps1`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_newmap_name_enrichment_completion.md`
