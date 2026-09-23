# DeepSeek Review Prompt: P10 NewMap Tsunami Mode Hotfix

Review the current git diff for `D:\UnityProjects\ChuoTsunamiEvacuation`.

Scope:
- Active scene: `Assets/Scenes/Chuo_BaseMap.unity`
- Hotfix only within P10 QA.
- Do not propose P11, P10-E/F/G, data re-imports, or final release/archive work.

Verify:
1. Tsunami failure immediately shows result/restart UI and uses clear reason codes.
2. Retry/reset clears failure state and re-randomizes the player spawn without destroying existing NewMap systems.
3. Spawn randomization is enabled for normal sessions and deterministic seed override remains available for tests.
4. Unexpected playable-area invisible blockers are converted/disabled while boundary air walls remain named/preserved.
5. Player stamina is exactly 100x the previous 100 baseline.
6. Tsunami starts from a configured sea/coastal side, currently `south`, and moves inland.
7. Tsunami warning/pre-alert phase defaults to exactly 300 seconds, remains distinct from active tsunami-front phase, and does not show/activate the tsunami hazard before it ends.
8. Light curtain height/length are large enough to cover the NewMap playable area.
9. Pressing E requires touching/overlapping an eligible building-entry trigger volume, not aiming at a precise entrance.
10. Generic/proxy vertical evacuation flow is clearly documented and does not claim real interiors.
11. Existing accepted fixes are not regressed: lighting/night, mouse drag, ground cover, name cache/labels, NPC amount/collision/lifecycle, official/non-official semantics.
12. No Unity runtime web requests, no fabricated labels, no ProjectSettings/Packages changes.

Flag:
- Any compile/runtime risk.
- Any Unity lifecycle or trigger/collider mistake.
- Any risk of reintroducing invisible air-wall blockers.
- Any false claim of GIS-grade terrain/route/interior accuracy.
- Any A-level blocker for manual playtest.
