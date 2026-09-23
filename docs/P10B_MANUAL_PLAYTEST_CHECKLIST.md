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
13. Select English and Japanese from the start menu.
14. Open rules from start and pause menus and verify scroll/wrap behavior.
15. Press `Esc` to pause and resume.
16. Switch language from pause/options and verify text refresh.
17. Verify weather modes: clear day, rainy day, night clear, night rain.
18. Verify stamina appears when sprinting, drains, locks sprint after exhaustion, and recovers.
19. Verify avatar presentation does not impose a hard-coded gender speed penalty by default.
20. Record FPS before tsunami start, at tsunami start, when green frames appear, when light curtain appears, and when ResultPanel opens.
21. Record FPS and visible stutter in crowd/congestion, rain, night, and night rain scenarios.
22. Record memory after scene load, after 5 minutes, and after scenario restart if supported.
23. Watch Task Manager or Resource Monitor for CPU, memory, disk active time, hard faults/sec, or paging symptoms.
24. Confirm anti-aliasing status visually in the built player during P10-C; do not assume TAA, FXAA, or MSAA is active from P10-B++.
25. Confirm debug labels and debug layers remain off unless explicitly testing them.

Do not commit the high-detail scene after manual smoke unless there is an explicit safe scene-change request.
