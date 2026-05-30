# DeepSeek Review Prompt: NewMap Final Tuning

Review the current git diff for the Unity project `ChuoTsunamiEvacuation`.

Focus on this final manual tuning pass:

- Ground cover/support gets only a small extra raise (`0.3m`, capped <= `1.0m`) and does not move imported PLATEAU buildings or raw map geometry.
- Building collision final refinement tightens runtime proxy bounds without disabling building blocking entirely.
- Spawn safety rejects building proxy bounds and renderer bounds, requires support ground, uses a final player-capsule overlap check, and has a safe fallback.
- Player stamina is reduced by `35%`: old final `20000`, new final `13000`.
- Evacuation sprint speed is reduced by `15%`: old `6.75m/s`, new `5.7375m/s`.
- Tsunami warning duration is `180s`, while PRE_WARNING_WAIT, WARNING, and active tsunami/front logic remain intact.
- Tourism mode still disables tsunami failure and stamina restriction.
- P2-P10 gameplay flow, NPC behavior, mouse left/right drag look, day/night lighting, labels, official/non-official warnings, route prototype guidance, blue ground/fall prevention, and Player.log cleanliness are not regressed.

Do not suggest moving to P11, creating P10-E/F/G, creating final release/archive artifacts, re-importing map data, reworking lighting, reworking mouse drag, or claiming GIS-grade route/terrain/building-footprint validation.

Please identify:

- A-level blockers, if any.
- Compile risks.
- NullReferenceException risks.
- Unity lifecycle/order risks.
- Spawn-inside-building residual risks.
- Building-collision false-air-wall risks.
- Any case where building blocking could be removed too aggressively.
- Test/report gaps.

Expected verdict format:

- Verdict: PASS / PASS_WITH_NOTES / BLOCKED
- A-level blockers: list or `none`
- Required Codex fixes: small actionable items only
- Residual manual playtest risks: concise list
