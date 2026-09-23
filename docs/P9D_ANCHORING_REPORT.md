# P9-D Anchoring Report

Runtime report source:

- `P9DNearestAnchorMatcher.BuildFinalAnchoringReport`
- `Assets/Data/P9/p9d_coordinate_anchoring_config.json`
- `Assets/Data/P8/humanitarian_candidate_persistent_marker_v1.json`

Expected report summary:

- humanitarian candidates: 110
- named humanitarian candidates: 28
- ID-only humanitarian candidates: 82
- humanitarian candidates remain non-official: yes
- humanitarian candidates require warning: yes
- official shelter sample anchor: present
- entrance proxy anchors: present
- route proxy anchor: present
- hazard lookup anchor: present
- exact PLATEAU object identity claim: no
- official route claim: no

The checked-in `p9d_anchoring_report_sample.json` records the expected sample totals. Tests regenerate the detailed report from the P8-E handoff and P9-D config.
