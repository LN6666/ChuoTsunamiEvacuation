# P11 Final Complete Package Plan

P11 is one integrated final stage for packaging, source upload, release ZIP creation, and second-PC test documentation.

Scope:

- Build the final Windows x64 player from `Assets/Scenes/Chuo_BaseMap.unity`.
- Create one portable folder: `ChuoTsunamiEvacuation_v1.0`.
- Create one release ZIP containing exactly that folder.
- Commit and push source, docs, configs, tools, tests, prompts, and small JSON reports.
- Exclude build artifacts, `Library`, `Temp`, `Logs`, generated player folders, release ZIPs, raw PLATEAU data, and giant generated scenes.
- Run final config check, Unity EditMode/PlayMode, local EXE smoke, Player.log parse, performance summary, and DeepSeek review.

Non-goals:

- No new gameplay systems.
- No map re-import.
- No P10-E/F/G.
- No official route or GIS-grade validation claims.
- No promotion of non-official candidates to official shelters.

Release validation gates:

- Evacuation stamina is exactly `3500`.
- Evacuation sprint speed is `4.59 m/s`, a 20% reduction from the previous P10 value.
- Tourism Mode keeps no stamina restriction.
- Tsunami warning duration remains `180` seconds.
- Circular boundary radius remains `2270` meters.
- Runtime web requests remain disabled.
- Player.log is clean.
- Final package contains EXE, data folder, docs, version, and manifest.
