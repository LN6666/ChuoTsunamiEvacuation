# P10-B++ Streaming And Loading Audit

## Finding

The current production gameplay path is a high-detail scene plus runtime marker/proxy systems. There is no confirmed full production segmented loading strategy for the final high-detail Chuo scene.

Confirmed or inferred status:

- Addressables: not present in the active P10 runtime path and not added in P10-B++.
- AssetBundles: not used for P10 runtime loading and not added in P10-B++.
- Additive production scene loading: not found in P9/P10 runtime scripts.
- Async production scene loading: not found in P9/P10 runtime scripts.
- Custom production city chunk loading: not implemented for `P7_HighDetail_Chuo`.
- Distance-based production city loading/unloading: not implemented for `P7_HighDetail_Chuo`.
- `Resources.Load`: no P9/P10 heavy Resources loading path was found.
- PLATEAU chunk structure: P7-C has benchmark-only placeholder chunk metadata, not production Chuo streaming.

## Risk

The high-detail scene can still have load-time, memory-peak, and paging risk because most city geometry is scene-resident. P10-B++ does not claim that the final map is streamed or paged by chunks.

## Low-Risk Hardening Applied

- Green frame pooled objects can warm before tsunami start through `warmupPoolBeforeTsunamiStart`.
- Green frame warmup is capped by `warmupFrameCount`, `maxFrameCount`, and target count.
- Debug labels remain disabled by default.
- Runtime layer toggles are available for marker, crowd, light curtain, and debug roots through `P10BPlusPlusRuntimeOptimizer`.

## Deferred Work

Future work may evaluate Addressables, AssetBundles, additive scene chunks, spatial city-cell loading, or PLATEAU-specific tiling. That work belongs after P10-C profiling evidence or a separate approved plan because it can affect packages, build workflow, memory release behavior, scene authoring, and QA scope.

## P10-C Measurements Required

- Unity player startup to first usable frame.
- High-detail scene load time.
- Peak private memory during load.
- Memory after 5 minutes of play.
- Frame spike count when tsunami starts.
- Frame spike count when green frames and light curtain appear.
- Disk usage spikes during load or play.
