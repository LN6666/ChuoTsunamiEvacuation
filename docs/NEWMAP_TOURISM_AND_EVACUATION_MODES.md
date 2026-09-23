# NewMap Tourism And Evacuation Modes

## Tourism Mode

Tourism Mode is map exploration mode:

- No tsunami warning.
- No light curtain.
- No hazard failure.
- No crowd failure.
- No collapse/debris failure.
- Stamina disabled.
- Walk speed 2.0 m/s.
- Sprint speed 10.0 m/s.
- UI labels the mode as `Tourism Mode / 観光モード`.

## Evacuation Mode

Evacuation Mode uses the two-stage tsunami model:

- Stage 1 `Warning`: warning UI/countdown state, light curtain hidden, risk contact ignored.
- Stage 2 `FrontApproaching`: light curtain visible and advancing, hazard checks active.
- Stamina enabled.
- Weather/night speed modifiers apply.
- Crowd delay, entrance, safe-floor, and collapse/debris proxy systems are active.
- Walk speed 1.0 m/s.
- Sprint speed 5.0 m/s.

Final status: `completed_with_documented_runtime_proxy`.
