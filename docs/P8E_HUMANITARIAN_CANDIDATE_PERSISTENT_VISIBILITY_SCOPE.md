# P8-E Humanitarian Candidate Persistent Visibility Scope

Date: 2026-05-24.

P8-E is reserved for final P8 closeout and handoff verification before P9.

## Expanded Candidate Audit Input

P8-E should verify the expanded audit list:

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`
- P8-D proxy status config at `Assets/Data/P8/humanitarian_candidate_hazard_status_config.json`

Audit v1 is expanded beyond the old five sample candidates. It includes PLATEAU-derived Chuo high-rise / office / commercial / hotel / mixed-use screening candidates where local attributes exist.

## Visibility Requirement

Future persistent visibility must:

- make candidates visible only as non-official humanitarian candidates;
- show explicit warnings/disclaimers;
- avoid official shelter styling;
- avoid safe/approved wording;
- preserve manual review status;
- include P8-D status markers such as low-floor warning, entrance blocked proxy, building damage proxy, and collapsed proxy visual only as non-official proxy states;
- tell P9 how to use entrance/safe-floor/evacuation-complete proxies if P9 makes any candidate selectable.

P8-E must not implement P9 gameplay or real indoor scenes.
