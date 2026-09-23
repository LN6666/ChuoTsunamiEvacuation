# P10-C-Pre Quality Profile Recommendations

P10-C-Pre hardens runtime quality-style profiles without changing ProjectSettings, Packages, URP assets, PLATEAU data, or scenes.

## Low

Purpose: ordinary-PC playable baseline at 1080p.

- NPC cap: 40
- marker cap: 90
- green frame cap: 90
- debug labels: off
- UI debug overlays: off
- route proxy debug visibility: off
- candidate marker labels: off
- collapse/debris debug markers: off
- light curtain: disabled for baseline measurement
- target frame rate: 30

## Medium

Purpose: default demonstration profile if Low passes.

- NPC cap: 80
- marker cap: 140
- green frame cap: 120
- debug labels and overlays: off
- light curtain: enabled at moderate intensity
- target frame rate: 60

## High

Purpose: visual-rich inspection only after Low/Medium pass.

- NPC cap: 120
- marker cap: 180
- green frame cap: 140
- debug labels and overlays: off
- light curtain: enabled
- target frame rate: unchanged by profile

High is not the ordinary-PC target. If High is unstable but Low/Medium pass, P10-C may proceed only with limitations that are visible in the release docs.
