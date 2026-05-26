# NewMap Manual Playtest Readiness

Generated: 2026-05-27T04:00:17+09:00

Decision: `ready_with_documented_limitations`

Reason: Pre2 temp player build, PlayMode tests, preflight, performance sampling, and Player.log parse passed. Official shelters are recovered by exact PLATEAU GML anchors. The startup max-frame spike remains above 2 seconds, but runtime bootstrap measured 169 ms, so the remaining spike is documented as large Chuo_BaseMap Unity/PLATEAU scene activation.

Active targets: 19
Active official shelters: 15
Disabled targets: 156
Performance retest: 72.72 average FPS, 11273.06 ms max frame, 2 stutter frames over 66 ms
Player.log: 0 errors, 0 warnings
EditMode: total 108, passed 108, failed 0
PlayMode: total 20, passed 20, failed 0
DeepSeek: no A-level blocker
