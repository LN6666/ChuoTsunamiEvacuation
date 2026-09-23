# P8-E Shallow Depth Slowdown Rules

When arrival-time data is unavailable and fallback onshore speed is required, shallow-depth slowdown reduces visual-front speed in shallow areas.

Rules:

- Enabled by `shallowDepthSlowdownEnabled`.
- Applies when `inundationDepthMeters` is below `shallowDepthThresholdMeters`.
- Uses `depthSpeedFactor` as the minimum shallow-speed factor.
- Clamps speed between configured min/max onshore speed.

The slowdown uses `inundationDepthMeters` only. It does not use `visualHeightMeters` or `maxTsunamiHeightMeters` as physical depth.
