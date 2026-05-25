# DeepSeek Review Prompt: P10-C-Pre Performance Gate

Review the current git diff for ChuoTsunamiEvacuation P10-C-Pre.

P10-C-Pre is a pre-release performance gate before official P10-C. It is not the final Windows EXE release build, not release packaging, not archive work, and not P10-E/P10-F/P10-G.

Please verify:

1. P10-C-Pre remains a performance gate, not final release packaging.
2. Temporary build artifacts are not committed.
3. No final release package/archive was created.
4. No P10-E, P10-F, or P10-G artifacts were created.
5. CPU, memory, GC, stutter, loading, disk paging, and Player.log risks are addressed meaningfully.
6. Optimizations are evidence-driven where possible and low/medium risk.
7. No risky ProjectSettings, Packages, Assets/PLATEAU, Chuo_BaseMap, or P7 high-detail scene changes exist.
8. Chunk streaming status is honest: no true production chunk streaming, Addressables, AssetBundles, or additive city chunking is claimed.
9. AA status is honest: AA is not confirmed unless built-player evidence says so.
10. P10-C readiness decision is clear and conservative.
11. Protected paths are clean.
12. No official route claim, no official/safe claim for humanitarian candidates, and no real indoor scene claim was introduced.
13. Unity lifecycle and compile risks are acceptable.
14. There are no A-level blockers.

Focus especially on:

- whether the temporary build/profile scripts can accidentally create committed release artifacts
- whether the quality profile applier changes global settings or scene objects too aggressively
- whether the green-frame redundant-refresh fix is behaviorally safe
- whether JSON/docs overclaim readiness without measured built-player data
- whether P10-C remains the only official release package/archive stage
