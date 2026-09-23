# P10-A+ Light Curtain Visual QA

P10-A+ does not reimplement P8 risk front or light curtain logic.

Hardening outcome:

- P8 handoff remains the source for hazard/front timing
- `visualHeightMeters` remains cinematic-only
- visual height is not physical tsunami height
- gameplay hazard outcomes consume timing/status conservatively
- P10-B smoke steps are more explicit

P10-B visual smoke checklist:

- sweep direction
- inundation area coverage
- light curtain alignment with expected risk-front direction
- marker visibility with light curtain enabled
- camera and UI obstruction
- warning copy consistency
- frame-time impact
- Player.log warnings/errors during visual sweep

Remaining limitation:

Final visual and performance proof requires P10-B runtime measurement.
