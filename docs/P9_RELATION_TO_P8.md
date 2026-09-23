# P9 Relation To P8

P9 starts from the current P8 foundation branch while P8-C supplemental work and P8-D/E continue in parallel.

## Responsibility Split

P8 owns:

- tsunami hazard/front foundation,
- evidence-backed hazard layer,
- infrastructure hazard state,
- road/bridge/underground state,
- building warning and damage/collapse proxy handoff,
- entrance blocked state,
- low-floor inundation warnings,
- hazard source metadata and timing status.

P9 owns:

- spawn point data and loader foundation,
- crowd/NPC profile foundation,
- entrance/safe-floor vertical evacuation proxy data,
- entrance queue and congestion proxy state,
- future evacuation failure mechanics after the P8 handoff is available.

P9 does not solve the P8 hazard/front/infrastructure foundation. P9-A must not assume P8-D/E outputs are final.

## Current P9-A Behavior

P9-A uses null/fallback/no-effect behavior when P8 handoff data is missing. The runtime adapter records missing state but does not affect success or failure.

Final gameplay failure waits until P9-C. P9-A only prepares the contract and safe scaffold.

## Baseline

P9 uses the `P7_HighDetail_Chuo` practical baseline after P8 handoff. P9-A does not modify `P7_HighDetail_Chuo` or `Chuo_BaseMap`.
