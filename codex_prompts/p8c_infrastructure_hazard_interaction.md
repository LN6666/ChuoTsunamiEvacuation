# P8-C Infrastructure Hazard Interaction Prompt

Role: one-shot Codex goal for P8-C.

Task name: P8-C Infrastructure Hazard Interaction + P2-P6 Runtime Adaptation on the High-detail Map.

Branch: `p8-tsunami-hazard-risk-front-foundation`.

Baseline scene: `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.

Legacy fallback: `Assets/Scenes/Chuo_BaseMap.unity`.

Required outcome:

- implement infrastructure hazard interaction
- implement P2-P6 runtime compatibility smoke/adaptation gate
- use P8-B official/evidence hazard layer v1
- use P7 high-detail scene as practical baseline
- use proxy/marker targets where true PLATEAU semantic geometry is incomplete
- do not implement P8-D collapse proxy
- do not implement P9 crowd/real spawn/indoor evacuation gameplay
- do not change gameplay success/failure rules

Required infrastructure categories:

- road
- building
- bridge
- underground
- entrance
- waterfront
- open_space
- shelter_proxy
- navigation_target_proxy

Required states:

- safe
- watch
- warning
- inundated_proxy
- restricted_proxy
- avoid_proxy

Hazard fields that must drive state:

- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `inundationBoundary`
- `hazardIntensity`
- `confidence`
- `sourceMode`
- `evidenceSourceId`

Required exclusions:

- `maxTsunamiHeightMeters` must not replace `inundationDepthMeters`
- `visualHeightMeters` must remain cinematic only
- no full real-time fluid simulation
- no official route/hazard claims beyond evidence
- no P8-E/F/G stages

Required validation:

- P8-C preflight
- P8-A/P8-B regression preflights
- GUI EditMode tests
- GUI PlayMode tests
- DeepSeek review
- commit and push only if all gates pass and protected paths are clean
