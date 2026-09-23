# P9-D Full Gameplay Flow

`P9DFinalGameplayFlowValidator` validates the final P9 gameplay chain without touching a Unity scene:

- P8 handoff files are present
- P9-D coordinate anchoring report exists
- P9-C target selection runs
- entrance/congestion rules run
- safe-floor proxy rules run
- hazard timing rules run
- collapse/debris exposure-event rules run
- ResultPanel feedback and run log records are generated

Validated scenario fixtures:

- normal delayed success
- congestion delayed success
- entrance blocked failure
- safe-floor failure
- crowd delay hazard-arrival failure
- collapse/debris fatality
- collapse/debris disabled success

This is final P9 gameplay integration coverage, not a release build or P10 performance pass.
