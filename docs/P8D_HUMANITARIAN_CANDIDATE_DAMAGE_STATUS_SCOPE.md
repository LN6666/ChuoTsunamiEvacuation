# P8-D Humanitarian Candidate Damage Status Scope

Date: 2026-05-24.

P8-D allocation note only. Do not implement P8-D behavior in the candidate-audit task.

## Scope

The expanded audit data at `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json` may be used by a future P8-D damage/blockage proxy as non-official building candidates.

Allowed future P8-D proxy states:

- building warning;
- entrance blocked proxy;
- low-floor inundation warning;
- restricted/avoid proxy state from hazard data;
- lightweight damage-status marker.

Not allowed:

- real structural collapse simulation;
- official shelter claim;
- safe/approved candidate claim;
- P9 selectable gameplay;
- indoor stair/fire-route scene.

Every candidate must keep `isOfficialShelter=false` and `nonOfficialWarningRequired=true`.
