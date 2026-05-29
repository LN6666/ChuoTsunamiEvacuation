# NewMap NPC Lifecycle Deadlock Audit

Findings:

- No intentional global respawn loop was found in the NPC prototype.
- The likely visual refresh/freeze source was far-NPC throttling using the original spawn center instead of the current player position.
- Player-NPC contact had no local NPC sidestep, so nearby NPCs could end up with zero-distance targets and appear stopped.
- Stuck recovery could record `stoppedWithoutReason` instead of forcing an explainable recovery state.

Fixes applied:

- The crowd receives the live player transform as update reference.
- Player-NPC contact is local only and does not set global pause/stop flags.
- Contacted NPCs sidestep or enter `Repathing` / `WaitingAtCrossingOrCrowd`.
- Tourism all-stop deadlock has a bounded recovery kick back into `Repathing`.
