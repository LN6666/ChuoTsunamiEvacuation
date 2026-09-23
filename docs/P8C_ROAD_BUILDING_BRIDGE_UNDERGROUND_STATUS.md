# P8-C Road Building Bridge Underground Status

Date: 2026-05-24.

## Implemented Status Categories

P8-C implements hazard-state evaluation for:

- road
- building
- bridge
- underground
- entrance
- waterfront
- open_space
- shelter_proxy
- navigation_target_proxy

## Current Evidence Level

The high-detail map baseline has renderable PLATEAU objects, but true semantic coverage for roads, bridges, underground objects, entrances, and shelter entry points is incomplete. P8-C therefore uses proxy/marker targets where direct semantic geometry is incomplete.

## Status Rules

Roads and bridges can become `restricted_proxy` after contact or arrival when depth/intensity is moderate or high.

Buildings can become `warning` or `inundated_proxy`, but P8-C does not implement structural damage or collapse.

Underground and waterfront targets can become `avoid_proxy` at moderate/high hazard.

Entrances and shelter proxies can become `warning` or `restricted_proxy` without changing real shelter success/failure rules.

Navigation target proxies can be marked `restricted_proxy` or `avoid_proxy` for display/smoke-test routing context only.

## Deferred

True PLATEAU semantic binding and lightweight damage/collapse proxy are deferred. P8-D implements the damage/collapse proxy; P9 remains the crowd/real spawn/indoor gameplay phase.
