# P9-C Result Panel And Reason Codes

P9-C adds a formatter and log record rather than rewriting older result UI systems.

Runtime feedback includes:

- selected target id
- selected target type
- official/non-official status
- mandatory non-official warning
- entrance status
- safe-floor status
- queue delay
- crowd delay
- collapse/debris exposure result
- final reason code

`ResultMetrics` now has optional P9-C fields:

- `p9cOutcomeFeedback`
- `p9cFinalReasonCode`

Structured log support is provided by `P9CRunLogRecord`.

Reason code source of truth:

- `Assets/Scripts/P9/P9COutcomeReasonCode.cs`
- `Assets/Data/P9/p9c_reason_code_catalog.json`

Reason codes are prototype gameplay explanations. They are not official hazard, route, mortality, or shelter-designation evidence.
