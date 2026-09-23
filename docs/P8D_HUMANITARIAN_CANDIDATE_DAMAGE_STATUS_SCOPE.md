# P8-D Humanitarian Candidate Damage Status Scope

Date: 2026-05-24.

P8-D implementation scope. The earlier candidate-audit task produced the input list; P8-D now adds proxy/status support only.

## Scope

The expanded audit data at `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json` is consumed by the P8-D damage/blockage proxy as non-official building candidates.

Allowed P8-D proxy states:

- building warning;
- entrance blocked proxy;
- low-floor inundation warning;
- restricted/avoid proxy state from hazard data;
- lightweight damage-status marker.
- deterministic low-probability `collapsed_proxy_visual` marker.

Not allowed:

- real structural collapse simulation;
- physics collapse, debris, or rigidbody destruction;
- official shelter claim;
- safe/approved candidate claim;
- P9 selectable gameplay;
- indoor stair/fire-route scene.

Every candidate must keep `isOfficialShelter=false` and `nonOfficialWarningRequired=true`.
