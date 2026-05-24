# P10-B Manual Playtest Checklist

Use this after P10-B validation and before P10-C build packaging.

1. Open `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` without saving scene changes.
2. Start the tsunami or hazard warning scenario.
3. Verify green rectangular frames appear only after tsunami start.
4. Verify frames appear for official evacuation targets and non-official humanitarian candidates.
5. Verify non-official candidates still show warning text and are not displayed as official shelters.
6. Verify player spawn bias in coastal or low-elevation scenario.
7. Verify candidate markers and entrance/safe-floor proxy interaction.
8. Verify crowd/congestion delay and ResultPanel reason code.
9. Verify collapse/debris deterministic proxy result when enabled.
10. Verify light curtain visibility and marker readability.
11. Verify ResultPanel long warning text does not block play.
12. Record FPS/stutter feel, memory, loading time, Player.log warnings/errors, NPC count, marker count, and green frame count.

Do not commit the high-detail scene after manual smoke unless there is an explicit safe scene-change request.
