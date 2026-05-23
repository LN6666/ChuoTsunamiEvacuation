# P8-B Arrival, Depth, Boundary Driven Front

Date: 2026-05-24.

## Front Selection

The active front is selected from extracted Tokyo metropolitan hazard-layer records clipped to Chuo:

- before a record arrives, the next upcoming `arrivalTimeSeconds` record is selected;
- after all records have arrived, the latest arrived record remains selected;
- missing hazard data fails safe and hides the visual.

## Boundary Use

When `inundationBoundary` has points, P8-B converts the points to a local visual boundary. The current official spatial layer stores a derived bbox around clipped Chuo grid samples. It is an extracted data extent, not an official inundation contour.

When boundary points are missing, P8-B uses a procedural fallback line so development builds can still show a fail-soft visual. The fallback is explicitly reported as `fallback_procedural_boundary` and must not be treated as evidence.

## Depth And Intensity Use

P8-B combines:

- `inundationDepthMeters / 2.5`
- `hazardIntensity`

The larger normalized value becomes `visualIntensity01`. This drives warning level and light-curtain color/alpha. It does not change gameplay success/failure.

For Chuo hazard-layer v1, `inundationDepthMeters` records extracted spatial maximum inundation depth from Tokyo official mesh CSVs. `maxTsunamiHeightMeters` records the separate tsunami-height source field. It is not a full inundation-depth grid and is not used as one.

Warning levels:

- low: below 0.33
- medium: 0.33 to below 0.66
- high: 0.66 and above

## Non-Claims

The current v1 front is not a hydrodynamic simulation, not an official inundation contour, and not final academic tsunami modeling. It is an evidence-backed `official_spatial` hazard-layer driver using Tokyo metropolitan tsunami mesh data clipped to Chuo.

Do not claim:

- the derived bbox boundary is an official inundation contour;
- Chuo lacks tsunami evidence because it lacks a standalone tsunami map;
- a maximum tsunami-height reference is a spatial inundation-depth grid.
