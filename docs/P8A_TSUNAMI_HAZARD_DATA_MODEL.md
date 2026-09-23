# P8-A Tsunami Hazard Data Model

## Model Purpose

The P8 hazard layer records science/data fields separately from cinematic visual fields.

## Scenario Fields

- `scenarioId`
- `sourceMode`
- `hazardLayerVersion`
- `timeOriginSeconds`
- `scienceLayerFields`
- `visualLayerFields`

## Hazard Feature Fields

- `sourceMode`
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

Large visual front heights must be marked cinematic-only and must not be read as real tsunami physical height.

`scienceLayerFields` and `visualLayerFields` are explicit schema metadata. Science-layer field lists must not contain visual-only fields, and visual-layer field lists must not contain science fields.

## Evidence Fields

- `evidenceSourceId`
- `sourceMode`
- `sourceCategory`
- `reviewedStatus`
- `boundaryIsEvidenceBasedOrPrototype`

P8-A accepts manual sample and evidence-planned records only as non-authoritative data. Unknown or official-looking source modes fail safe until a later approved stage defines them.

## Infrastructure Fields

- `affectedInfrastructureTypes`: roads, buildings, bridges, underground, entrances, waterfront, open_space.
- `buildingDamageState`
- `collapseProxyState`
- `collapseProbability`
- `collapseRandomSeed`
- `hazardDrivenCollapse`

In P8-A these fields are parsed and validated only. They do not trigger gameplay damage, collapse, success, or failure behavior.
