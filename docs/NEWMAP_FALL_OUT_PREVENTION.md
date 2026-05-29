# NewMap Fall-Out Prevention

The player controller now has a safe recovery target from the validated spawn.

Safety behavior:
- Recover if player Y falls below `-8.0`.
- Recover if player leaves playable bounds.
- Recover to the safe spawn on the invisible support surface.
- Recovery does not emit `Debug.LogWarning` in normal mode.

Normal play should not repeatedly trigger recovery. Repeated recovery means the support or visible map still needs review.
