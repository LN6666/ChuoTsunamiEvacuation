# NewMap Final Stamina Sprint Tuning

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

This final P10 tuning changes only Evacuation Mode stamina and sprint tuning:

- Runtime source of truth: `Assets/Data/P10/newmap_player_stamina_config.json`
- Old max stamina: `13000`
- New max stamina: `3500`
- Old Evacuation sprint speed: `5.7375 m/s`
- New Evacuation sprint speed: `4.59 m/s`
- Sprint reduction: `20%`
- Base Evacuation walk speed: unchanged at `1.0 m/s`
- Tourism Mode stamina: still disabled
- Tourism sprint speed: unchanged at `10.0 m/s`

The runtime fallback defaults in `NewMapPlayerStaminaConfig` were updated to the same final values so a missing config cannot silently restore the previous stamina/sprint tuning.

Validation:

- Final P10 preflight: passed
- EditMode: passed, `127/127`
- PlayMode: passed, `43/43`
- Temporary player build: passed
- Player.log parse: passed clean
- DeepSeek: no A-level blockers

This task does not create a final release/archive and does not move the project to P11.
