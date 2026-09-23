# NewMap NPC Double Distribution Regression

Regression targets after doubling NPCs:

- NPCs stay inside the `2270m` circular boundary.
- NPCs are distributed across multiple rings/sectors, not clustered only near the player.
- NPCs snap to the raised ground/support.
- Building avoidance remains enabled.
- Player/NPC soft blocking remains active.
- Tourism mode NPCs do not cause failure.
- Evacuation crowd delay remains bounded.
- No rapid global refresh, all-stop state, or post-contact deadlock.

Runtime state counts and performance samples are written to `newmap_npc_double_count_report.json` and `newmap_npc_double_distribution_regression.json`.

Latest runtime retest:
- NPCs inside/outside boundary: `1600/0`
- Sector/ring coverage: `48` sectors, `8` rings, `98.18%` coverage
- No global refresh, no all-stop event, no stopped-without-reason NPCs
- Active performance sample passes with documented stutters: max frame `1021.96ms`, stutter frames over 66ms `2`
