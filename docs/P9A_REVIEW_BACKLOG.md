# P9-A Review Backlog

## B-Level Follow-Ups

- Confirm the exact P8-D/E handoff field names once P8-D/E stabilizes.
- Decide whether P9-B spawn placement should use rule-based points, manual samples, or P8 handoff-derived points first.
- Add build-safe data loading later if P9 data becomes active in player builds.
- Decide P9-C failure reason vocabulary after congestion and vertical evacuation proxy behavior is designed.
- Review whether humanitarian high-rise candidate warnings need UI copy changes in P9-C.
- Consider replacing `Directory.GetCurrentDirectory()` in the P9 EditMode file-path guard with an `Application.dataPath`-based path if Unity test working directories become less stable.
- Avoid hard dependencies on legacy global gameplay type names in future P9 tests if P2-P6 classes are namespaced or renamed.
- Add optional warnings for zero/default entrance or spawn positions before P9-B scene placement.
- Ensure future stages respect `SpawnRuntimeEnabledInP9A`, `EntranceQueueRuntimeEnabledInP9A`, and final-failure disabled flags when enabling runtime prototypes.

## Not For P9-A

- Final NPC-caused failure.
- Final congestion failure.
- Final vertical evacuation failure.
- Real indoor shelter gameplay.
- P8-D/E damage/collapse integration.
- P10 release packaging.
