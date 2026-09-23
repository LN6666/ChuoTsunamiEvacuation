# P9-B Collapse Risk Zone Prototype

P9-B adds collapse/debris proxy zones as runtime markers only.

Implemented script:

- `P9CollapseDebrisRiskZone`

Data file:

- `Assets/Data/P9/p9b_collapse_debris_risk_zones_sample.json`

Supported behavior:

- marker generation
- deterministic exposure-event calculation hook
- risk level metadata
- warning-only status
- no physics collapse
- no debris simulation
- no player kill
- no player outcome mutation

The deterministic exposure hook exists so P9-C can later add final configured failure behavior. P9-B deliberately does not implement the later fatality probability, result reason, result panel explanation, or gameplay mutation.
