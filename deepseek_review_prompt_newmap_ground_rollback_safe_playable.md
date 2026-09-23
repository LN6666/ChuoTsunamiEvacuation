# DeepSeek Review Prompt: NewMap Ground Rollback Safe Playable

Review the current git diff for NewMap GroundRoadMerge rollback and safe playable ground recovery.

Verify:
- GroundRoadMergePre failure is acknowledged.
- The faulty adaptive relief/DEM support grid is disabled or rolled back by default.
- Relief/DEM samples are not used as runtime road/support height evidence.
- A safe invisible playable ground/collision setup is restored.
- Support renderers and blue support/debug surfaces are hidden in normal mode.
- Player cannot fall out of the map and recovery does not spam Player.log warnings.
- Accepted mouse drag, spawn validation, lighting, official/non-official semantics, labels, Tourism/Evacuation modes, and P2-P10 gameplay are preserved.
- No final release/archive is created.
- No P10-E/F/G artifacts are created.
- No official route, terrain accuracy, or GIS-grade validation is claimed.

Focus on A-level blockers: compile errors, runtime NullReference risks, unsafe support/collider behavior, visible support renderers, fall-through regressions, and broken gameplay semantics.
