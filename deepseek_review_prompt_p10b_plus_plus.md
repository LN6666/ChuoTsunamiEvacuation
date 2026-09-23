# DeepSeek Review Prompt: P10-B++ Final Optimization Attempt Before P10-C

Review the current git diff for ChuoTsunamiEvacuation P10-B++.

P10-B++ is not a new official stage. It is the final optimization hardening sprint after P10-B/P10-B+ and before P10-C. The official P10 structure remains P10-A, P10-B, P10-C, and P10-D.

Please verify:

1. P10-B++ remains final optimization hardening, not a new official stage.
2. No Windows EXE build was created.
3. No P10-E, P10-F, or P10-G was created.
4. No risky ProjectSettings, Packages, Assets/PLATEAU, Chuo_BaseMap, or P7 high-detail scene changes exist.
5. Streaming/loading audit is honest and does not falsely claim production chunk streaming, Addressables, AssetBundles, additive loading, or async city streaming.
6. Anti-aliasing audit is honest and does not falsely claim TAA, FXAA, or MSAA is active.
7. CPU and memory optimizations are low-risk, meaningful, and reversible.
8. No large architecture rewrite, ECS/DOTS/Jobs/Burst rewrite, or heavy package was introduced.
9. Metrics/checklists cover FPS, 1 percent low, frame spikes, CPU proxy, memory, GC, loading, Player.log warnings/errors, NPC/marker/green frame counts, light curtain, UI/ResultPanel, weather/night, language, stamina, and paging risk.
10. P10-C readiness is improved and the Windows EXE build remains deferred to P10-C.
11. Protected paths are clean.
12. There are no official route claims, no official/safe claims for humanitarian candidates, and no real indoor scene claim.
13. Unity lifecycle and compile risks are acceptable.
14. There are no A-level blockers.

Focus especially on:

- whether green-frame pool warmup can cause unintended scene mutation or runtime behavior changes
- whether the runtime optimizer accidentally clears scene targets or changes global settings by default
- whether P10-B++ metrics are bounded and avoid per-frame allocations
- whether documentation overclaims performance improvements without measured P10-C built-player data
