# DeepSeek Review Prompt: P8-C Infrastructure Hazard Interaction

Review the current git diff for P8-C.

Check A-level blockers:

- P8 has exactly five stages: P8-A, P8-B, P8-C, P8-D, P8-E.
- No P8-0/F/G stages were introduced.
- P2-P6 runtime adaptation gate exists and is tested or honestly marked proxy/pending.
- Hazard layer/risk front drives infrastructure states.
- `arrivalTimeSeconds`, `inundationDepthMeters`, `inundationBoundary`, `hazardIntensity`, `confidence`, `sourceMode`, and `evidenceSourceId` are used.
- `maxTsunamiHeightMeters` is not confused with `inundationDepthMeters`.
- `visualHeightMeters` is not used as physical depth.
- No gameplay success/failure changes were made.
- No P8-D collapse proxy is implemented.
- No P9/P10 systems are implemented.
- `Chuo_BaseMap.unity` is untouched.
- ProjectSettings and Packages are clean.
- `P7_HighDetail_Chuo.unity` baseline is preserved and not staged/reset/lost.
- Tests and preflights pass.

Check B-level follow-ups:

- proxy labels are honest where PLATEAU semantics are incomplete.
- conservative fallback behavior is documented.
- P2-P6 smoke status is clear: passed, pending, proxy-based, blocked.
- P8-D handoff is limited to lightweight damage/collapse proxy and P8 closeout.

Do not suggest rewriting the whole system. Prefer small, actionable fixes.
