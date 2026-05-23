# P7 Final Known Limitations

Validation date: 2026-05-23.

## Baseline Decision

The user-approved practical baseline is `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.

The items below are known limitations for P8/P9/P10 planning, not blockers to P7 closeout or baseline handoff.

## B-Level Follow-Ups

- Original average LOD3 target is not achieved; actual evidence is LOD0-LOD2.
- Buildings are renderable but below target.
- Roads/transport are renderable but LOD0-LOD1 only.
- Bridges are renderable but only 13 LOD1 objects were detected.
- Underground evidence is limited to one LOD1 object.
- City furniture, water, vegetation, relief/terrain, disaster risk, land use, and urban planning decision categories are missing from converted scene evidence.
- Windows EXE profiling is prepared but not complete.
- P2-P6 runtime smoke validation on the populated high-detail scene is pending.
- The imported scene is 22.55 GB and local-only unless later archived outside normal Git.

P8/P9 should proceed on the accepted baseline and use data layers, markers, rule-based nodes, proxy colliders, or representative templates where detailed geometry is missing.

## Scope Limits

- No real tsunami fluid simulation.
- No P8 hazard/inundation/light-curtain/flood/risk-front system.
- No P9 crowd, real spawn, indoor evacuation, underground evacuation, congestion, or new failure system.
- No gameplay success/failure rule changes.
- No unsafe official-route claims.
