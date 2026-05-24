# P9-D Coordinate-Based Anchoring

P9-D uses coordinate-based proxy anchoring because full Unity scene-object metadata coverage is not available.

Supported anchor categories:

- humanitarian high-rise candidates
- official shelter markers where data exists
- entrance proxies
- safe-floor / vertical evacuation proxy targets
- estimated route guidance proxies
- P8 hazard lookup proxies
- spawn-zone relation proxies

Anchoring fields:

- coordinate source
- coordinate validation
- local linear WGS84-to-Unity proxy projection
- nearest-match binding
- distance threshold
- confidence score
- fallback reason
- anchoring status

Confidence levels:

- `exact`
- `high`
- `medium`
- `low`
- `fallback`
- `rejected`

Binding statuses:

- `anchored_to_coordinate`
- `anchored_to_nearest_building_proxy`
- `anchored_to_nearest_road_proxy`
- `anchored_to_entrance_proxy`
- `anchored_to_hazard_grid`
- `fallback_marker_only`
- `rejected_invalid_coordinate`
- `rejected_out_of_bounds`
- `rejected_distance_threshold_exceeded`

P9-D does not claim exact PLATEAU object identity. Coordinate anchors are gameplay-usable proxy anchors.
Route proxy anchors are not official evacuation routes, and humanitarian candidate anchors are not official shelters.
