# DeepSeek Review Prompt: NewMap Name Enrichment Completion

Review the current git diff for the NewMap building/road name enrichment completion pass.

Scope:
- Workspace: `D:\UnityProjects\ChuoTsunamiEvacuation`
- Active scene: `Assets/Scenes/Chuo_BaseMap.unity`
- Main outputs:
  - `tools/map/enrich_newmap_names_from_coordinates.py`
  - `Assets/Data/P10/newmap_name_cache.json`
  - `Assets/Data/P10/newmap_name_enrichment_report.json`
  - `Assets/Data/P10/newmap_name_cache_coverage_audit.json`
  - `Assets/Data/P10/newmap_name_enrichment_query_list.json`
  - `Assets/Data/P10/newmap_name_normalization_report.json`
  - `Assets/Scripts/NewMap/NewMapNameLabelController.cs`

Verify:
- Online lookup was actually run during preprocessing or a clear unavailable status is reported.
- Unity runtime/player does not perform web requests and reads only the local cache.
- `runtimeNetworkRequestsAllowed` is false in config/cache/runtime report.
- The local cache is generated and loaded by runtime labels.
- Japanese/Kanji main-name normalization is implemented.
- Full addresses, postal-address strings, coordinate strings, GML IDs, and building IDs are hidden in normal mode.
- Names are not fabricated or machine-translated.
- Trusted project/source names are not overwritten by low-confidence online names.
- Official shelter and non-official candidate warning semantics are preserved.
- Road labels are road-like and building labels are reliable enough for prototype display.
- Label caps, culling, and throttling prevent full-map label scans per frame.
- No final release/archive was created.
- No P10-E/F/G artifacts were created.
- No A-level blockers remain.

Validation already expected:
- `powershell -ExecutionPolicy Bypass -File tools/map/run_newmap_name_enrichment_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/enrich_newmap_names_from_coordinates.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/map/build_newmap_name_enrichment_player.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/map/parse_newmap_name_enrichment_player_log.ps1`

Please report any compile risk, runtime null-reference risk, data-schema risk, or false-completion claim.
