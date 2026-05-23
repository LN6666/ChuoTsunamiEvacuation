# P8-B Light Curtain Visual Rules

Date: 2026-05-23.

## Required Disclaimer

Every P8-B light curtain surface or debug status must carry this meaning:

`Cinematic risk-front visualization, not physical tsunami height.`

The light curtain is not physical tsunami height, no real-time fluid simulation, and no official hazard value.

## Height Rules

- `visualHeightMeters` is a visual readability parameter.
- It can be kilometer-scale for readability.
- It must not be interpreted as `tsunamiHeightMeters`, `waterLevelMeters`, or `inundationDepthMeters`.
- Large visual heights require `visualHeightIsCinematicOnly=true`.

## Shape Rules

The visual curtain boundary can be non-linear, smoothed, wavy, or noisy to improve readability.

The data boundary remains the hazard input boundary. The visual curtain boundary is a generated rendering boundary and must not be treated as official inundation evidence.

## Performance Rules

- Use a bounded segment count.
- Use a generated mesh strip or primitive fallback.
- Avoid unbounded per-frame allocations.
- Rebuild mesh at a bounded interval.
- Do not require new packages, shaders, or ProjectSettings changes.
