# NewMap Building Collision Gameplay Validation

Active scene: `Assets/Scenes/Chuo_BaseMap.unity`

## Checks

- Player cannot pass through sampled building footprint proxies.
- Player can move along sampled road/open-space approach areas near active targets.
- Player can approach official shelter markers.
- Player can approach non-official/humanitarian/proxy candidate markers.
- Spawn remains outside building bounds.
- No large invisible wall remains outside tightened building footprint proxies.
- Player.log has no errors, exceptions, missing config messages, or runtime web requests.

The runtime player smoke and Player.log parser update the JSON status file after the temporary build.
