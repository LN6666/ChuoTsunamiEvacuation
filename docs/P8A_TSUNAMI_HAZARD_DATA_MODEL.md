# P8-A Tsunami Hazard Data Model

## Model Purpose

The P8 hazard layer records science/data fields separately from cinematic visual fields.

## Scenario Fields

- `scenarioId`
- `sourceMode`
- `hazardLayerVersion`
- `timeOriginSeconds`

## Hazard Feature Fields

- `arrivalTimeSeconds`
- `inundationDepthMeters`
- `waterLevelMeters`
- `tsunamiHeightMeters`
- `inundationBoundary`
- `hazardIntensity`
- `confidence`
- `evidenceSourceId`
- `notes`
- `geometryType`

## Visual Fields

- `visualHeightMeters`
- `visualHeightIsCinematicOnly`
- `boundaryIsEvidenceBasedOrPrototype`

Large visual front heights must be marked cinematic-only and must not be read as real tsunami physical height.

## Infrastructure Fields

- `affectedInfrastructureTypes`: roads, buildings, bridges, underground, entrances, waterfront, open_space.
- `buildingDamageState`
- `collapseProxyState`
- `collapseProbability`
- `collapseRandomSeed`
- `hazardDrivenCollapse`

In P8-A these fields are parsed and validated only. They do not trigger gameplay damage, collapse, success, or failure behavior.
