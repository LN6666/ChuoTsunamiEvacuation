# P9-C Life-First Vertical Target Rules

P9-C can select either an official shelter proxy or a non-official humanitarian high-rise candidate proxy.

Non-official candidate rules:

- `isOfficialShelter` remains `false`
- `nonOfficialWarningRequired` remains `true`
- candidates are never displayed as official evacuation shelters
- candidates are never marked safe or approved by default
- result/debug text uses "Non-official humanitarian vertical evacuation candidate"
- result/debug text uses "Life-first candidate"
- result/debug text uses "Not an official evacuation shelter"
- result/debug text uses "Use only when official shelter access is unsafe or unavailable"

Eligibility considers:

- entrance status
- safe-floor proxy status
- low-floor inundation warning
- hazard/damage/blockage state
- queue and congestion state
- route proxy availability
- estimated travel time
- hazard arrival window
- scenario preset

Primary reason codes:

- `selected_official_shelter`
- `selected_life_first_vertical_candidate`
- `rejected_non_official_candidate_blocked`
- `rejected_non_official_candidate_low_floor_warning`
- `rejected_non_official_candidate_unsafe_hazard_state`
- `rejected_candidate_missing_safe_floor_proxy`

The selector is deterministic and tie-breaks by score and target id.
