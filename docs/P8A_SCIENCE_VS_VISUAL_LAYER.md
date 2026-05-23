# P8-A Science vs Visual Layer

## Science/Data Layer

The science layer contains evidence-bearing hazard values:

- Arrival time.
- Inundation depth.
- Water level.
- Tsunami height where supported.
- Inundation boundary.
- Hazard intensity.
- Confidence and evidence source ID.

P8-A sample values are placeholders and are not official hazard claims.

## Cinematic Visual Layer

The visual layer contains gameplay and readability parameters, including `visualHeightMeters`.

The future tsunami light curtain may use a kilometer-scale or otherwise exaggerated height for visibility, but that height is cinematic-only. It is not a physical tsunami height, not a water depth, and not an evidence-backed water level.

## P8-A Guard

If visual height is large or exceeds science-layer values, `visualHeightIsCinematicOnly` must be true.
