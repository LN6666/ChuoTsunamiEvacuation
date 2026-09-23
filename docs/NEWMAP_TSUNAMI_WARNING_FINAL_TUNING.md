# NewMap Tsunami Warning Final Tuning

- Config: `Assets/Data/P10/newmap_tsunami_mode_hotfix_config.json`
- Report: `Assets/Data/P10/newmap_tsunami_warning_final_tuning_report.json`

The tsunami warning phase is shortened to `180` seconds.

Preserved behavior:
- `PRE_WARNING_WAIT` remains before warning.
- `WARNING` remains before active tsunami.
- `ACTIVE_TSUNAMI` / front approaching begins after the warning duration.
- Hazard failure and tsunami curtain visuals remain inactive before active tsunami.
- Direct shelter guidance remains available.
- Tourism mode keeps tsunami disabled.

This remains a simplified risk-boundary model, not fluid simulation.
