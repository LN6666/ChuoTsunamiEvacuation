# P8-B Scene Anchor Report

Date: 2026-05-23.

## Anchor Decision

Scene mutation is deferred for this one-shot P8-B implementation.

`Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is preserved as the user-approved practical baseline and remains locally dirty. P8-B runtime scripts and tests are implemented without opening, resetting, staging, or rewriting the scene.

## Recommended Manual Scene Root

When scene anchoring is performed safely in Unity, create only this isolated root:

`P8_RiskFront_System`

Suggested children:

- `P8_RiskFront_Controller`
- `P8_RiskFront_DebugStatus`
- `P8_RiskFront_VisualRoot`

Attach:

- `P8RiskFrontController` to `P8_RiskFront_Controller`
- `P8RiskFrontTimeDriver` to the controller object
- `P8RiskFrontDebugStatus` to the controller object
- `P8RiskFrontLightCurtainRenderer` to the visual root or allow the controller to add it at runtime

## Protected Scene Status

P8-B does not modify `Chuo_BaseMap.unity`, ProjectSettings, Packages, or PLATEAU assets.

The light curtain remains not physical tsunami height, no real-time fluid simulation, and no official hazard value.
