# P8-B Dynamic Risk Front Design

Date: 2026-05-23.

## Purpose

P8-B adds a dynamic tsunami risk-front / cinematic light curtain visualization driven by the P8-A hazard data layer.

The risk front is visual and communicative only. It is not physical tsunami height, no real-time fluid simulation, and no official hazard claim.

## Runtime Components

- `P8RiskFrontController`: loads P8-A hazard/config data, drives the visual boundary, and fails safe when data is missing.
- `P8RiskFrontVisualConfig`: converts P8-A/P8-B config into a validated visual runtime config.
- `P8RiskFrontCurveGenerator`: separates the data boundary from a non-linear visual curtain boundary.
- `P8RiskFrontLightCurtainRenderer`: builds a lightweight vertical mesh strip using built-in material fallback.
- `P8RiskFrontTimeDriver`: advances sample visualization time without moving gameplay objects.
- `P8RiskFrontDebugStatus`: provides the label text: "Cinematic risk-front visualization, not physical tsunami height."

## Data Flow

P8-B reads:

- `Assets/Data/P8/tsunami_hazard_sample_chuo.json`
- `Assets/Data/P8/risk_front_visualization_config.json`

The science/data layer remains:

- arrival time
- inundation depth
- water level
- tsunami height where supported
- data inundation boundary
- hazard intensity and confidence

The visual layer remains:

- cinematic visual height
- cinematic-only flag
- segment count
- wave/noise settings
- visual curtain mesh boundary

## Safety Boundaries

P8-B does not:

- modify gameplay success/failure rules
- move the player, NPCs, shelters, or P2-P6 gameplay objects
- implement P8-C road/building/bridge/underground hazard interaction
- implement P8-D collapse proxy behavior
- implement P9 crowd/spawn/indoor systems
- perform live web requests or live routing
- claim official tsunami route or hazard values

## Failure Behavior

If hazard data, risk-front config, or visual validation is missing/invalid, the controller hides the visual and reports a fail-safe status. It does not throw into gameplay flow and does not trigger success/failure.
