# P9-B Humanitarian Candidate Markers

P9-B consumes the P8-E humanitarian high-rise handoff and prepares runtime marker support for all available candidates.

Loaded P8-E candidate status:

- total candidates: 110
- named candidates: 28
- ID-only or unknown-name marker records: 82
- `isOfficialShelter=false`
- `nonOfficialWarningRequired=true`
- selectable gameplay remains disabled

Runtime support:

- `P9HumanitarianCandidateMarkerRuntime`
- `P9RuntimeMarker`
- `p9b_humanitarian_marker_runtime_config.json`

Marker rules:

- candidates never appear as official evacuation shelters
- candidates display a non-official humanitarian warning
- candidates are not marked safe or approved by default
- candidates are visible/queryable runtime markers
- candidates are not final selectable evacuation targets in P9-B

P9-B uses proxy projection for marker placement when WGS84 coordinates are available. This is enough for runtime prototype anchoring and tests, but not a claim of perfect PLATEAU scene-object binding.
