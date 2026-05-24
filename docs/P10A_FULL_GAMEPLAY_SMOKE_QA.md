# P10-A Full Gameplay Smoke QA

P10-A reuses the P9-D final flow as the automated gameplay smoke baseline.

Covered smoke scenarios:

- normal success
- congestion delay success
- entrance blocked failure
- safe-floor failure
- crowd-delay hazard failure
- collapse/debris deterministic fatality
- collapse/debris disabled success
- life-first non-official candidate selected with warning
- route remains estimated prototype guidance

Additional manual P10-B/P10-D smoke scenarios:

- official shelter selected
- high-detail scene loaded locally
- player spawn is visible and controllable
- candidate marker and entrance marker are visible
- ResultPanel feedback remains readable
- runtime logs do not flood Player.log

Boundary:

- P10-A does not add a new gameplay system.
- P10-A does not add real indoor scenes.
- P10-A does not reimplement P8 or P9.
