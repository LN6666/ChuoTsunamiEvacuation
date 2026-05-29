# Codex Prompt: NewMap Airwall NPC Label Fix

Task summary:

- Hard-audit unexpected invisible air walls near buildings and valid walking areas.
- Preserve boundary/invalid-zone blockers.
- Add player-NPC soft blocking with near NPC capsules, avoiding rigid-body crowd instability.
- Preserve NPC 100x distribution, building avoidance, lifecycle, lighting, mouse drag, blue ground, ground cover, spawn, and official/non-official semantics.
- Expand preprocessing-only building/road name enrichment.
- Generate and load `Assets/Data/P10/newmap_name_cache.json`.
- Keep Unity runtime/player offline-only.
- Hide full addresses, coordinate names, GML IDs, and `bldg_` IDs in normal mode.
- Build temporary player at `D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapAirwallNpcLabelFixPre\ChuoTsunamiEvacuation_NewMapAirwallNpcLabelFixPre.exe`.

Do not create a final release/archive, move to P11, create P10-E/F/G, re-import map data, or rework accepted lighting/mouse/ground systems unless a regression is detected.
