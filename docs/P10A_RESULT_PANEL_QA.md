# P10-A ResultPanel QA

P10-A uses formatter/tests first and avoids risky UI layout rewrites.

QA checks:

- selected target type can be displayed or logged
- official vs non-official warning is preserved
- entrance status can be shown or logged
- queue/congestion delay can be shown or logged
- safe-floor status can be shown or logged
- collapse/debris exposure result can be shown or logged
- final reason code can be shown or logged
- P5 route status remains estimated prototype guidance and not official

P10-A conclusion:

- P9-C/P9-D formatter coverage is meaningful enough for P10-A.
- Long non-official warning text remains a layout risk for P10-B visual smoke.
- No invasive ResultPanel layout change is made in P10-A.

P10-B visual smoke should inspect:

- text wrapping
- non-official warning visibility
- reason-code clarity
- UI obstruction from light curtain or marker overlays
