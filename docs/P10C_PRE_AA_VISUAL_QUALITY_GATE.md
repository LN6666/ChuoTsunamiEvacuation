# P10-C-Pre AA And Visual Quality Gate

AA is not confirmed for the final built player.

## Current Policy

- Do not change ProjectSettings or render pipeline assets in P10-C-Pre.
- Read runtime settings where available, but do not claim MSAA, FXAA, TAA, or camera AA is active without built-player evidence.
- Visual inspection remains required in P10-C.

## Risks

- If AA is off, edges may look jagged in the high-detail city scene.
- Enabling AA later can reduce FPS or increase memory bandwidth cost.
- Render scale or camera-level settings may matter even when `QualitySettings.antiAliasing` is readable.

## Recommendation

- Low: keep AA minimal/off until FPS and stutter pass.
- Medium: enable only if built-player FPS and 1 percent low are stable.
- High: visual-rich AA settings only after Low/Medium pass.

This task does not create the final release build, so final visual AA verification remains P10-C work.
