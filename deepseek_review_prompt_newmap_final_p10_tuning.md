# DeepSeek Review Prompt: NewMap Final P10 Stamina/Sprint Tuning

Review the current diff for the final P10 tuning before P11.

Verify:
- Evacuation Mode max stamina is exactly `3500`.
- Evacuation sprint speed is reduced by `20%` from the latest previous runtime sprint speed `5.7375 m/s` to `4.59 m/s`.
- Tourism Mode keeps stamina disabled and sprint unchanged at `10.0 m/s`.
- Runtime fallback defaults and JSON config match, so missing config cannot restore old stamina/sprint values.
- UI rules, docs, tests, and tools consistently reference the final values.
- Temporary EXE build/report path is for `NewMapFinalP10TuningPre`, not a final release/archive.
- Player.log parsing checks runtime mode behavior and remains clean.
- P2-P10 smoke behavior is not regressed.
- No map re-import, no final release/archive, no P10-E/F/G, and no move to P11.

Report any A-level blocker first. If none, state that explicitly.
