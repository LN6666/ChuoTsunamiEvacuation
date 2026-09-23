# NewMap Lighting Visual Fix

Generated: 2026-05-29T00:00:00+09:00

Status: `implemented_pending_player_screenshot_or_manual_confirmation`

Runtime now creates `NewMap_ClearDay_DirectionalLight` and applies explicit ambient settings through `NewMapLightingController`.

- Clear day: brighter directional and trilight ambient setup.
- Rain: slightly dimmer with light fog.
- Night: darker but not blacked out.
- Night rain: darkest preset, still reversible.
- Returning to clear day reapplies clear brightness and disables fog.

This fixes the dark-scene blocker without baking lighting or modifying imported PLATEAU geometry.

JSON: `Assets/Data/P10/newmap_lighting_visual_status.json`
