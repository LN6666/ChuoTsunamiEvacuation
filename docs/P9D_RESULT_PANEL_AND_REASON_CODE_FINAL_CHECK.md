# P9-D Result Panel And Reason Code Final Check

P9-D keeps ResultPanel integration conservative.

Validated feedback content:

- selected target type
- official or non-official status
- non-official candidate warning
- entrance status
- queue delay
- congestion delay
- safe-floor status
- collapse/debris proxy result when triggered
- final reason code

Validated log content:

- final outcome
- final reason code
- selected target id/type
- queue and congestion delay
- collapse/debris exposure and fatality state
- warning-required status

Reason codes remain sourced from P9-C:

- `P9COutcomeReasonCode`
- `Assets/Data/P9/p9c_reason_code_catalog.json`

P9-D adds final flow validation rather than replacing the ResultPanel UI.
