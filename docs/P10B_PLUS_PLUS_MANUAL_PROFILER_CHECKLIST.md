# P10-B++ Manual Profiler Checklist

Use this checklist in P10-C before and after the Windows EXE build.

## Baseline

1. Open the high-detail scene without saving it.
2. Record load time to playable state.
3. Record average FPS, 1 percent low, min/avg/max frame time, and spike count.
4. Record memory and Player.log warning/error count.

## Scenario Checks

1. FPS before tsunami start.
2. FPS immediately after tsunami start.
3. FPS when green frames appear.
4. FPS when light curtain appears.
5. FPS in bounded crowd/congestion scenario.
6. FPS when ResultPanel opens.
7. FPS in rainy day.
8. FPS in night clear.
9. FPS in night rain.
10. FPS after language switch.
11. FPS after opening rules UI.

## Memory And Paging

1. Memory before scene load if measurable.
2. Memory after scene load.
3. Memory after 5 minutes.
4. Memory after scenario restart if supported.
5. Disk active time during load and spike moments.
6. Hard faults/sec or pages/sec if available.

## Logging

1. Check Player.log warning count.
2. Check Player.log error count.
3. Record shader, missing reference, memory, and asset load warnings.

## Acceptance For P10-C

The build can proceed only if performance limitations are known, reproducible severe spikes are either fixed or documented, and no A-level runtime blockers remain.
