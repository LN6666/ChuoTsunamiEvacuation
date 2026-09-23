# P10-B++ Next Steps To P10-C

P10-C remains the Windows EXE build, release package, documentation, and archive stage.

Before building:

- Run the P10-B++ preflight.
- Confirm EditMode and PlayMode pass.
- Confirm DeepSeek has no A-level blockers.
- Confirm protected paths are clean.
- Confirm no final EXE, release, or archive artifacts are staged.

P10-C must profile the built player for:

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
