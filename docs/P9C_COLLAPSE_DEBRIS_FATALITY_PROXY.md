# P9-C Collapse Debris Fatality Proxy

P9-C implements collapse/debris failure as an exposure-event proxy.

Correct design:

- a route or position entering a configured risk zone creates one exposure event
- each exposure event is evaluated once
- the default fatality probability is `0.35`
- probability is scenario-configurable
- the deterministic seed and event id control the result
- `maxCollapseDebrisFatalEventsPerRun` caps fatal outcomes
- disabled scenarios never kill the player

The implementation does not use an every-frame or every-second random death check.

Primary reason codes:

- `collapse_debris_exposure_event`
- `collapse_debris_fatality_proxy`
- `killed_by_building_collapse_proxy`
- `survived_collapse_debris_exposure`
- `collapse_debris_proxy_disabled`

Result text states that this is gameplay-level collapse/debris proxy behavior, not real structural simulation.
