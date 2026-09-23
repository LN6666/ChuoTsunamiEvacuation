# P10-B+ Player Profile Policy

Avatar presentation and mobility are separated.

Policy:

- avatar presentation can be assigned male/female at a deterministic 50/50 balance for visual identity
- mobility is controlled by a separate mobility profile such as `standard`, `cautious`, `carrying_load`, `injured`, `elderly`, or `custom_slow`
- no unavoidable sex-based speed penalty is enabled by default
- `enableGenderSpeedModifier=false` by default

Optional scenario assumption:

`femalePresentationSpeedMultiplier=0.85` exists only as a disabled scenario configuration. If enabled for an experiment, it must be described as a game-balance/scenario assumption, not a real-world claim about women.
