# NewMap Debug Cleanup Round 2

Generated: 2026-05-29T00:00:00+09:00

Status: `regression_check_passed_manual_visual_confirmation_remaining`

Round 2 keeps the prior production/manual cleanup rules:

- `DebugDiagnosticsRoot` is inactive by default.
- Local training proxy targets are diagnostics-only.
- The runtime ground support renderer is hidden.
- NPCs are gameplay objects, not debug clutter.
- Green frames, official markers, non-official warnings, active route guidance, hazard visuals, and UI remain allowed gameplay objects.

JSON: `Assets/Data/P10/newmap_debug_cleanup_round2.json`

Validation:

- Preflight passed.
- Player.log had 0 errors and 0 warnings.
