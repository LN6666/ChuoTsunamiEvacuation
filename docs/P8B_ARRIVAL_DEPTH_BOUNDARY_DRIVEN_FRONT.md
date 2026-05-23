# P8-B Arrival, Depth, Boundary Driven Front

Date: 2026-05-24.

## Front Selection

The active front is selected from hazard-layer records. In the current P8-B layer, some arrival values come from reviewed Tokyo/Chuo report references while exact spatial extraction remains pending:

- before a record arrives, the next upcoming `arrivalTimeSeconds` record is selected;
- after all records have arrived, the latest arrived record remains selected;
- missing hazard data fails safe and hides the visual.

## Boundary Use

When `inundationBoundary` has points, P8-B converts the points to a local visual boundary. This supports prototype point/polyline/polygon records.

When boundary points are missing, P8-B uses a procedural fallback line so development builds can still show a fail-soft visual. The fallback is explicitly reported as `fallback_procedural_boundary` and must not be treated as evidence.

## Depth And Intensity Use

P8-B combines:

- `inundationDepthMeters / 2.5`
- `hazardIntensity`

The larger normalized value becomes `visualIntensity01`. This drives warning level and light-curtain color/alpha. It does not change gameplay success/failure.

For Chuo evidence-layer v1, `maxTsunamiHeightMeters` records maximum tsunami-height references around 2.4m to 2.46m where available. These references are not a full inundation-depth grid. Until actual spatial depth extraction is complete, `inundationDepthMeters` values remain placeholder/pending and the front uses `hazardIntensity` plus explicit provenance labels for visualization.

Warning levels:

- low: below 0.33
- medium: 0.33 to below 0.66
- high: 0.66 and above

## Non-Claims

The current v1 front is not a hydrodynamic simulation, not an official inundation contour, and not final academic tsunami modeling. It is an evidence-aware hazard-layer driver prioritizing Tokyo metropolitan tsunami evidence while spatial extraction remains pending.

Do not claim:

- the current prototype boundary is official inundation geometry;
- the current pending depth is official inundation depth;
- Chuo lacks tsunami evidence because it lacks a standalone tsunami map;
- a maximum tsunami-height reference is a spatial inundation-depth grid.
