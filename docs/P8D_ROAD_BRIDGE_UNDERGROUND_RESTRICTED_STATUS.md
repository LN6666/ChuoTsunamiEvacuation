# P8-D Road / Bridge / Underground Restricted Status

Date: 2026-05-24.

P8-D adds category-specific restriction statuses on top of P8-C hazard states.

## Road

Road proxies can receive `road_restricted_proxy` when local depth or the P8-C hazard state indicates restriction/inundation.

## Bridge

Bridge proxies can receive `bridge_restricted_proxy` when configured depth/intensity thresholds are exceeded.

## Underground

Underground proxies can receive `underground_avoid_proxy` for any meaningful inundation warning or P8-C avoid state.

## Gameplay Boundary

These statuses are data/visual proxies only. They do not implement route blocking, crowd congestion, final failure, or P9 navigation behavior.
