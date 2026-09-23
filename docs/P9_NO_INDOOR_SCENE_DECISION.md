# P9 No Indoor Scene Decision

P9 does not build real indoor shelter scenes.

## Decision

Real indoor shelter scenes are cancelled for P9 because no LOD4/BIM interior scene is available in the current project baseline.

## Rationale

The project has a practical high-detail exterior city baseline, but it does not contain verified indoor stairs, corridors, fire routes, refuge floors, or BIM-level shelter interiors. Building indoor gameplay on guessed interiors would create false precision and add fragile scene dependencies.

## Replacement Flow

P9 uses an external/abstracted proxy flow:

- shelter entrance marker,
- safe-floor / vertical evacuation status,
- evacuation complete proxy,
- entrance queue / congestion state,
- future failure mechanism.

The safe-floor proxy is a gameplay abstraction. It does not claim to model real indoor movement.

## Humanitarian Candidate Policy

Humanitarian high-rise candidates can be represented as non-official candidates. They require a non-official warning and must remain distinct from official shelters.

## Scope Boundary

P9-A is scaffold only. It does not implement final NPC-caused failure, final congestion failure, final vertical evacuation failure, P8-D/E damage/collapse integration, or P10 release packaging.
