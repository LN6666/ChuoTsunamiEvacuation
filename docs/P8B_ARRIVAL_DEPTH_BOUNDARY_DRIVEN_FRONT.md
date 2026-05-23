# P8-B Arrival, Depth, Boundary Driven Front

Date: 2026-05-24.

## Front Selection

The active front is selected from hazard-layer records:

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

Warning levels:

- low: below 0.33
- medium: 0.33 to below 0.66
- high: 0.66 and above

## Non-Claims

The current v1 front is not a hydrodynamic simulation, not an official inundation contour, and not final academic tsunami modeling. It is an evidence-shaped, manual-sample hazard-layer driver ready for reviewed source replacement.
