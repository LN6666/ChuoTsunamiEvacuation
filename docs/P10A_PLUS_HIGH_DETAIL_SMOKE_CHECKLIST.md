# P10-A+ High-Detail Smoke Checklist

Protected scene:

- `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`

P10-A+ checks:

- scene exists locally
- scene was not touched
- P9 runtime systems remain scene-safe by design
- markers can be generated without direct scene mutation through runtime/proxy systems
- manual smoke checklist is strengthened for P10-B

P10-B manual/runtime smoke checklist:

1. Open `P7_HighDetail_Chuo` without saving.
2. Verify high-detail baseline objects are visible.
3. Verify P9 runtime root/bootstrap can be added or enabled without committing scene changes.
4. Verify candidate markers and official/non-official warnings.
5. Verify spawn/crowd markers.
6. Verify light curtain visibility and marker readability.
7. Verify entrance/safe-floor proxy flow.
8. Verify ResultPanel warning and reason-code text.
9. Collect FPS, memory, loading time, Player.log warnings/errors, NPC count, marker count, and light curtain impact.
10. Revert or discard Unity-generated scene/project churn before commit.

P10-C archive requirement:

- back up or archive the local high-detail scene and import metadata outside normal Git unless explicitly approved.
