# P9-C Test Results

Final validation:

- P9-C preflight: PASS
- Unity GUI EditMode: PASS, 241/241
- Unity GUI PlayMode: PASS, 61/61
- DeepSeek: PASS, no A-level blockers

Validated coverage:

- non-official candidates remain non-official and warning-required
- life-first selector can select eligible candidate with warning
- blocked, low-floor-warning, unsafe, and missing safe-floor candidates are rejected
- official shelter selection still works
- entrance open/crowded/blocked states are deterministic
- safe-floor success/failure statuses resolve correctly
- crowd delay can fail when the hazard safety window is exceeded
- collapse/debris fatality uses exposure-event probability
- disabled collapse/debris proxy never kills
- result feedback and run log include reason codes
- route proxy guard keeps routes estimated and non-official
- no real building interior scene is introduced
- DeepSeek reviewed the staged full diff after the new files were staged

Note: The first DeepSeek invocation reached the API but failed while printing Unicode to the local Windows console. The review was rerun with UTF-8 output enabled and completed successfully.
