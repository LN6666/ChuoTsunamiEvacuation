# NewMap Ground Raise Runtime Resnap Status

Runtime uses the raised gameplay ground cover Y as the single support height for player, NPCs, active targets, green frames, interaction zones, and air walls.

Validation gates:
- player spawn Y must match raised cover within tolerance
- NPCs must stay on the same raised support
- target/green-frame height offsets must remain under tolerance
- air walls must remain active and invisible
- no blue/fall-through regression is allowed

The player-log parser updates `Assets/Data/P10/newmap_ground_raise_runtime_resnap_status.json` after the temporary player smoke.
