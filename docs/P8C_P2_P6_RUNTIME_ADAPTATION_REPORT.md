# P8-C P2-P6 Runtime Adaptation Report

Date: 2026-05-24.

## Result

P8-C solves the P2-P6 runtime adaptation gate at smoke/proxy level.

No gameplay success/failure rules were changed.

No hard dependency on `Chuo_BaseMap.unity` was introduced.

## P2

Status:

- player movement: passed at source/smoke level
- camera: passed at source/smoke level
- shelter interaction proxy: proxy-based
- ResultPanel / success-failure flow: passed, unchanged

P8-C proxy shelter targets do not call `BuildingShelter`, `ShelterEntranceTrigger`, `EvacuationGameManager`, or `ResultPanelController`.

## P3

Status: passed.

Unity-ready data assumptions remain valid because the P8-B hazard layer v1 loads from `Assets/Data/P8` and includes evidence/provenance fields.

## P4

Status: proxy-based.

Real shelter loading and marker logic can be represented by P8-C marker/proxy components. True high-detail entrance semantics remain pending.

## P5

Status: proxy-based.

Qualified shelter, route, high-rise candidate, and humanitarian candidate metadata can adapt to proxy targets. `real_qualified` remains opt-in/fail-safe. P8-C makes no unsafe official-route claim.

## P6

Status: proxy-based.

Navigation guidance can target P8-C proxy targets without `Chuo_BaseMap.unity`. The NPC prototype can be staged against limited P8-C proxy targets. No P9 crowd, real spawn, congestion, or indoor gameplay system is required.

## Tool

`tools/p8/inspect_p8c_p2_p6_runtime_adaptation.ps1` reports `passed`, `pending`, `proxy-based`, or `blocked` for each P2-P6 category.
