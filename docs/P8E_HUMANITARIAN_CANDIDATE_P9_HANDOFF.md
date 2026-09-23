# P8-E Humanitarian Candidate P9 Handoff

Date: 2026-05-24.

## Source Artifacts

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `Assets/Data/P8/humanitarian_candidate_hazard_status_config.json`
- `Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`
- `docs/P8D_HUMANITARIAN_CANDIDATE_DAMAGE_STATUS.md`

## P9 Decision

P9 decides whether non-official high-rise / office tower humanitarian candidates become life-first selectable vertical evacuation targets.

P9 must not present any candidate as an official evacuation shelter. P9 must not mark candidates safe/approved by default.

## Gameplay Proxy

Real indoor scene gameplay is cancelled because there is no LOD4/BIM indoor scene.

If P9 uses candidates, use:

- entrance proxy;
- safe-floor proxy;
- evacuation-complete proxy.

P9 should combine these with final gameplay rules, crowd interaction, real/rule-based spawn points, congestion/failure gameplay, and final result flow.
