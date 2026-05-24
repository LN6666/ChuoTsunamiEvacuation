# P10-A+ ResultPanel Visual QA

P10-A+ keeps ResultPanel changes non-invasive.

Verified by existing P9-C/P9-D coverage and P10-A+ checklist:

- non-official warning appears in formatter output
- selected target type can be displayed or logged
- final reason code appears
- entrance/crowd/safe-floor/collapse/hazard explanation is available through result fields and logs
- no official shelter claim is made for humanitarian candidates
- no official route claim is introduced

P10-B visual smoke should inspect:

- long warning text wrapping
- ResultPanel readability at target resolutions
- warning visibility over light curtain / marker overlays
- final reason code readability
- UI layout behavior after repeated runs

Limitation:

P10-A+ does not perform risky UI layout rewrites. Visual proof remains P10-B runtime QA.
