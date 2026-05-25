# P10-C-Pre Temporary EXE Playability Checklist

- Temporary EXE opens to Start Menu by default.
- English and Japanese language buttons are visible.
- Rules panel opens and closes.
- Start Game requests `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
- Runtime bootstrap creates player, camera, Canvas, EventSystem, GameManager, ResultPanel, and at least one shelter entrance.
- Player can move with WASD/arrow keys after Start Game.
- T starts the tsunami warning path.
- E near a shelter entrance reaches the shelter interaction path.
- Player.log contains `P10CPreStartupDiagnostics`.
- If the P9 scene is only the placeholder shell, the UI and Player.log say so.
- Build outputs remain outside the repository.
- No final release package/archive is created.
