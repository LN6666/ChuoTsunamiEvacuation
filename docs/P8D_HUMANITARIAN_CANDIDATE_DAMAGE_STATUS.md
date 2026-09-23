# P8-D Humanitarian Candidate Damage Status

Date: 2026-05-24.

P8-D integrates the expanded non-official high-rise humanitarian candidate audit:

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `Assets/Data/P8/humanitarian_candidate_hazard_status_config.json`

Candidates can receive proxy status through `P8HumanitarianCandidateHazardStatus`:

- `safe`
- `watch`
- `warning`
- `low_floor_inundation_warning`
- `entrance_blocked_proxy`
- `building_damaged_proxy`
- `collapsed_proxy_visual`
- `manual_review_required`

All candidates remain non-official. Unknown-name / ID-only PLATEAU candidates remain valid audit records but require manual review.

P8-D does not make any humanitarian candidate selectable gameplay, safe, approved, or official.
