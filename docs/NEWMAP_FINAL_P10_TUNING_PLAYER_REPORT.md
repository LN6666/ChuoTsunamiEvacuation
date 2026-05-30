# NewMap Final P10 Tuning Player Report

Temporary build target:

`D:\UnityProjects\ChuoTsunamiEvacuation-Builds\NewMapFinalP10TuningPre\ChuoTsunamiEvacuation_NewMapFinalP10TuningPre.exe`

Validation status is written to:

- `Assets/Data/P10/newmap_final_p10_tuning_player_report.json`
- `Assets/Data/P10/newmap_final_p10_tuning_player_log_summary.json`

Final validation result:

- Temporary build: passed
- Player.log: passed clean, `0` errors, `0` warnings, `0` exceptions
- Evacuation max stamina: `3500`
- Evacuation sprint speed: `4.59 m/s`
- Tourism sprint speed: `10.0 m/s`
- Tourism stamina restriction: disabled
- P2-P10 smoke: passed
- Performance sample: measured, average FPS `1406.92`, stutter frames over 66ms `2`

Required runtime checks:

- Player.log has no errors, warnings, exceptions, missing assets, or runtime web requests.
- Evacuation Mode max stamina is exactly `3500`.
- Evacuation sprint speed is exactly `4.59 m/s`.
- Tourism Mode keeps stamina disabled and sprint at `10.0 m/s`.
- P2-P10 smoke scenarios still pass.
- No final release/archive is created.
