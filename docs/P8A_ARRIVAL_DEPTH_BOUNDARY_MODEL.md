# P8-A Arrival, Depth, and Boundary Model

## Model Purpose

P8 uses a simplified hazard data model for serious-game evacuation decisions. It does not simulate real tsunami fluid dynamics.

P8-A only hardens schema, semantics, tooling, and tests. It does not render the visual curtain and does not apply hazard interactions to roads, buildings, bridges, underground areas, or collapse proxies.

## Arrival Time

`arrivalTimeSeconds` is measured from `timeOriginSeconds`.

For a polygon feature, the value is the representative arrival time for that feature. For future grid or polyline data, the value may be feature-specific or cell-specific, but the semantics must be documented before use.

Manual samples use prototype values only.

## Inundation Depth

`inundationDepthMeters` is water depth above local ground. It is the field most directly related to whether a ground-level location is inundated.

It is not:

- tsunami wave height,
- water surface elevation,
- cinematic curtain height,
- building damage probability.

## Water Level and Tsunami Height

`waterLevelMeters` is water surface elevation in a source-defined vertical reference. It needs a datum and source explanation before official use.

`tsunamiHeightMeters` is the source-defined physical tsunami height. It must not be inferred from `visualHeightMeters`.

## Inundation Boundary

`inundationBoundary` is the data boundary. It may be:

- evidence-based, when reviewed source geometry supports it,
- prototype, when hand-authored for sample/testing.

The `boundaryIsEvidenceBasedOrPrototype` field records that distinction.

## Arrival Front vs Visual Risk Front

The arrival front is a data concept: where and when hazard arrival is represented.

The visual risk front is a future rendered concept: how P8-B makes risk readable to players. It may be non-linear, wavy, smoothed, exaggerated, or otherwise stylized as long as it stays labeled as visual.

## Data Boundary vs Visual Curtain Boundary

The data boundary is the reviewed or prototype hazard geometry.

The visual curtain boundary is the future light-curtain rendering boundary. It can be derived from data, but it is not itself evidence unless the underlying data is reviewed and the visual transform is documented.

## Fail-Safe Behavior

If source mode, evidence source, geometry type, confidence, or visual/science separation is invalid, P8-A validators must fail safe and keep hazard interactions disabled.
