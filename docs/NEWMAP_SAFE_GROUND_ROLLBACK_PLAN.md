# NewMap Safe Ground Rollback Plan

Status: implementation plan for the GroundRoadMergePre failure rollback.

Confirmed failure:
- GroundRoadMergePre is not accepted as a gameplay fix.
- The supplemental source contained relief/DEM renderers but no road/transport renderers.
- Relief-driven adaptive support introduced unsafe local heights and must not remain the default runtime ground.

Rollback approach:
- Disable the adaptive support grid by default and prevent relief/DEM samples from driving runtime player/NPC/target heights.
- Keep the prior accepted spawn building-overlap rejection, mouse drag buttons, lighting profiles, name-cache behavior, and air walls.
- Restore a conservative invisible runtime support platform at the stable gameplay support height.
- Hard-hide blue/debug/support renderers in normal mode, including large blue ground-like planes.
- Add silent fall/out-of-bounds recovery to the player controller so holes cannot eject the player from gameplay.

Validation:
- New rollback JSON/docs/tools will assert adaptive grid off, safe support collider on, support renderers hidden, blue hard-removal active, fall recovery available, air walls active, and manual readiness not overstated.
- Build and player-log parsing will use a new GroundRollbackPre temporary build path.
