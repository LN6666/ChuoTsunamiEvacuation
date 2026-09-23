# P8-E Humanitarian Candidate Persistent Marker Readiness

P8-E creates `Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json` from the accepted 110-record audit.

Readiness summary:

- total candidates: 110
- named marker records: 28
- ID-only marker records in the marker dataset: 82
- accepted PLATEAU ID-only / unknown-name candidates: 81
- note: the marker dataset has 82 ID-only records because one existing P5 sample candidate is also unnamed.
- data-only pending records: 0
- persistent scene objects created in P8-E hardening: false
- selectable gameplay enabled: false

All candidates remain non-official. Every record keeps `isOfficialShelter=false`, `nonOfficialWarningRequired=true`, and `selectableGameplayEnabled=false`.

The dataset is ready for persistent marker rendering in a future game pass, but P8-E does not mutate the high-detail scene. P9 must render these with an explicit non-official label and must not mark them safe, approved, or official.
