# P10-B++ Next Steps To P10-C

P10-C remains the official Windows EXE build, release package, documentation, and archive stage.

P10-C-Pre now runs first as a temporary pre-release performance gate. It may create a temporary Windows x64 profiling/test build, but that build is not the final release build and must not be committed or packaged as the P10-C release.

Before official P10-C release packaging:

- Run the P10-C-Pre preflight.
- Confirm EditMode and PlayMode pass.
- Confirm DeepSeek has no A-level blockers.
- Confirm protected paths are clean.
- Confirm no final EXE, release, or archive artifacts are staged.
- Confirm the P10-C-Pre readiness decision is not blocked.

P10-C-Pre or P10-C must profile the built player for:

- loading time and memory peak
- average FPS and 1 percent low FPS
- min/avg/max frame time
- frame spike count
- CPU usage from Windows tools
- managed heap and total allocated memory proxy
- disk paging symptoms
- Player.log warnings/errors
- NPC, marker, green frame, light curtain, UI, weather, language, and stamina state

If severe performance issues remain:

- lower quality preset first
- disable debug layer
- compare green frames on/off
- compare light curtain on/off
- lower bounded NPC/marker/frame caps
- document limits if a risky architecture change would be required

No true production chunk streaming is currently implemented. If Low/Medium cannot pass without freezes, memory growth, or paging-heavy stalls, official P10-C release packaging should stay blocked until a bounded optimization plan is approved.
