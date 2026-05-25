# P10-C-Pre CPU Optimization Gate

## Checked

- Existing P10-B++ runtime optimizer already provides bounded layer toggles and caps.
- Existing P10-B++ frame spike detector uses a ring buffer for bounded frame samples.
- Green ground frames use pooling and warmup before tsunami start.
- The P10-C-Pre quality applier maps Low/Medium/High profiles to the existing P10-B++ optimizer instead of introducing a parallel system.

## Fix Applied

`P10BGreenGroundFrameRuntime.SetTsunamiStarted` now skips redundant frame regeneration when the tsunami state and target count have not changed. This avoids unnecessary release/acquire work if the same state is set repeatedly by UI, hazard, or scene bootstrap logic.

## Remaining CPU Risks

- Full high-detail scene rendering cost is unknown until the temporary built player is measured.
- No true production chunk streaming exists, so scene-resident geometry may dominate CPU/GPU work.
- Manual smoke should still compare green frames, light curtain, crowd, UI, and ResultPanel separately.

## Gate Decision

No risky CPU architecture rewrite is introduced. P10-C should remain blocked if Low profile has persistent freezes or severe frame spikes in the temporary built player.
