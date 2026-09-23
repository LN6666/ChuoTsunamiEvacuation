# P8-A Science vs Visual Layer

## Science/Data Layer

The science layer contains evidence-bearing or explicitly prototype hazard values:

- Arrival time.
- Inundation depth.
- Water level.
- Tsunami height where supported.
- Inundation boundary.
- Hazard intensity.
- Confidence and evidence source ID.

P8-A sample values are placeholders and are not official hazard claims.

`inundationDepthMeters` is local water depth above ground. `tsunamiHeightMeters` is a source-defined physical tsunami height. `waterLevelMeters` is water surface elevation in a source-specific vertical reference.

## Cinematic Visual Layer

The visual layer contains gameplay and readability parameters, including `visualHeightMeters`.

The future tsunami light curtain may use a kilometer-scale or otherwise exaggerated height for visibility, but that height is cinematic-only. It is not a physical tsunami height, not a water depth, and not an evidence-backed water level.

`visualHeightMeters` can be kilometer-scale for cinematic gameplay. When it is large or exceeds science-layer values, `visualHeightIsCinematicOnly` must be true.

The P8-B risk front visual may be non-linear, wavy, smoothed, or otherwise stylized for readability. That visual shape is not automatically the evidence boundary.

The visual risk front and visual curtain boundary are rendering concepts. The `inundationBoundary` field is the data boundary.

## P8-A Guard

If visual height is large or exceeds science-layer values, `visualHeightIsCinematicOnly` must be true.

P8-A does not implement or render the visual light curtain. It only validates the data and configuration needed by later stages.
