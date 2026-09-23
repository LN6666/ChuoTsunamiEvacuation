# P8-E Light Curtain Progression Model

P8-E hardens the dynamic light curtain model with explicit progression rules in `Assets/Data/P8/risk_front_progression_model_config.json` and `P8RiskFrontProgressionModel`.

Priority order:

1. Use `arrivalTimeSeconds` when available.
2. Use `baseOnshoreSpeedMetersPerSecond` only as fallback interpolation.
3. Apply shallow-depth slowdown when fallback speed is used and `inundationDepthMeters` is below the configured shallow threshold.
4. Use `inundationDepthMeters` and `hazardIntensity` for visual/risk strength.

`visualHeightMeters` remains cinematic only. visualHeightMeters remains cinematic only, is not physical tsunami height, and is not used as physical hazard depth. `maxTsunamiHeightMeters` remains reference metadata and is not used as `inundationDepthMeters`. maxTsunamiHeightMeters remains reference metadata, not physical inundation depth.

When no full official contour is available, P8-E allows derived boundaries from extracted points/prototype hulls, clearly labeled as derived/prototype.
