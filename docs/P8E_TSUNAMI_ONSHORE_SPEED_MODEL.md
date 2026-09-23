# P8-E Tsunami Onshore Speed Model

The onshore speed model is a fallback for visual front interpolation, not a physical fluid simulation.

Config fields:

- `baseOnshoreSpeedMetersPerSecond`
- `minOnshoreSpeedMetersPerSecond`
- `maxOnshoreSpeedMetersPerSecond`
- `useArrivalTimeWhenAvailable`
- `arrivalTimeOverridePriority`

If `arrivalTimeSeconds` exists for a hazard feature, the model uses arrival-time progress and does not use the fallback speed to override it. Fallback speed exists only for interpolation gaps or future derived-boundary visualization.

This model does not change gameplay success/failure rules, route passability, or official hazard claims.
