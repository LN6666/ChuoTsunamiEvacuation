# P8-C Humanitarian Candidate Handoff

Date: 2026-05-24.

## Scope

P8-C owns data/proxy representation only.

Expanded audit input:

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`

## P8-C Allocation

- Candidates are represented as `humanitarian_candidate_proxy` / `highrise_candidate_marker` in the hazard interaction model.
- Candidates can receive hazard status at proxy/data level through `P8InfrastructureHazardEvaluator`.
- Candidate name list is for user review before persistent visibility or gameplay use.
- No final selectable gameplay is implemented.
- No gameplay success/failure rule changes are introduced.

## Future Allocation

P8-D may use the audit records for building warning, entrance blocked, low-floor inundation warning, and lightweight damage-proxy status. P8-D must not implement real structural collapse or official shelter claims.

P8-E verifies persistent explicit visibility, non-official warnings/disclaimers, and handoff to P9.

P9 decides whether any candidates become life-first selectable vertical evacuation targets. They remain non-official, use entrance/safe-floor/evacuation-complete proxies, and do not use real indoor scenes.
