# P9 Boundaries

P9 focuses on Crowd Interaction + Real/Rule-based Spawn Points + Entrance/Safe-Floor / Vertical Evacuation Proxy + Evacuation Failure Mechanism.

P9-A is scaffold only. It defines plans, schemas, loaders, neutral proxy state, tests, and preflight gates.

## In Scope For P9-A

- P9 stage plan and boundaries.
- Spawn point schema and sample data under `Assets/Data/P9/`.
- Crowd/NPC agent profile schema and sample data under `Assets/Data/P9/`.
- Entrance/safe-floor vertical evacuation proxy schema and sample data under `Assets/Data/P9/`.
- Runtime-neutral P9 loaders and proxy components under `Assets/Scripts/P9/`.
- P8 handoff adapter that safely handles missing P8-D/E data.
- Congestion state and future metrics model.
- EditMode and PlayMode tests that do not require production scenes.
- P9-A preflight and JSON validation tools.

## Out Of Scope For P9-A

- No modification to `Assets/Scenes/Chuo_BaseMap.unity`.
- No modification to `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- No modification to `ProjectSettings` or `Packages`.
- No modification to `Assets/PLATEAU`.
- No modification to `Assets/Data` outside P9-specific data paths.
- No real indoor scene gameplay.
- No stair, fire-route, or interior-template gameplay.
- No real tsunami fluid or hazard system implementation.
- No final P8-D/E damage/collapse integration.
- No direct change to P2-P6 gameplay success/failure rules.
- No NPC/crowd-caused player failure in P9-A.
- No P10 release packaging.

## Baseline Policy

P9 uses the `P7_HighDetail_Chuo` practical baseline after P8 handoff. P9-A does not mutate that scene. P8-D/E will provide future hazard, damage, blockage, and timing handoff signals.

## Indoor Scene Decision

Real indoor shelter scenes are cancelled because the project does not have an available LOD4/BIM interior scene. P9 uses an external/abstracted proxy flow:

- shelter entrance marker,
- safe-floor / vertical evacuation status,
- evacuation complete proxy,
- entrance queue / congestion state,
- future failure mechanism.

Humanitarian high-rise candidates can exist as non-official candidates only. They must not be promoted to official shelters by P9-A data or runtime code.
