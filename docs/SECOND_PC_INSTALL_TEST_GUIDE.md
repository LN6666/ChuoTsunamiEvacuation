# Second-PC Install Test Guide

## Copy

Copy the entire folder:

`ChuoTsunamiEvacuation_v1.0`

to the other computer.

Do not copy only `ChuoTsunamiEvacuation.exe`.

Keep these together:

- `ChuoTsunamiEvacuation.exe`
- `ChuoTsunamiEvacuation_Data`
- `UnityPlayer.dll`
- `MonoBleedingEdge`

## Launch

Double-click:

`ChuoTsunamiEvacuation.exe`

If Windows SmartScreen appears, choose **More info** and **Run anyway** only if this is your own trusted build.

## Fullscreen Recovery

The build starts in borderless fullscreen by default.

- Press `F11` to toggle borderless fullscreen / windowed mode.
- Press `Alt+Enter` to toggle borderless fullscreen / windowed mode.
- The selected mode is saved on that PC.

If fullscreen displays incorrectly, press `F11`. If the display is unusable, launch the EXE from a shortcut with:

`-screen-fullscreen 0 -screen-width 1280 -screen-height 720`

## Recommended Test

1. Launch EXE.
2. Confirm start menu is visible.
3. Switch language.
4. Open rules.
5. Start Tourism Mode.
6. Start Evacuation Mode.
7. Test movement, sprint, and stamina.
8. Test left/right mouse drag camera.
9. Press `E` near an interaction target.
10. Press `R` for ranking/leaderboard if implemented and visible.
11. Press `F11` and confirm fullscreen/windowed toggle.
12. Press `Alt+Enter` and confirm fullscreen/windowed toggle.
13. Confirm official/non-official target wording.
14. Confirm route wording says estimated prototype guidance, not official route.
15. Confirm green frames are visible in Evacuation Mode and do not imply official approval.
16. Wait for warning/front behavior if doing a longer smoke.
17. Check `Player.log` if anything fails.

## Player.log

Default path:

`%USERPROFILE%\AppData\LocalLow\DefaultCompany\ChuoTsunamiEvacuation\Player.log`

Attach `Player.log` to the second-PC report if there are errors, warnings, crashes, blank screens, or severe performance issues.
