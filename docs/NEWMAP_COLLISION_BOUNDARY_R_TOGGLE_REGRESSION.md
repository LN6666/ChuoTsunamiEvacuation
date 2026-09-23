# NewMap Collision Boundary R Toggle Regression

Regression coverage expected in EditMode, PlayMode, and player smoke:
- Mouse drag look remains button-gated.
- Random spawn avoids buildings.
- Player and NPCs remain grounded.
- NPC count/distribution/lifecycle remain stable.
- Labels and name cache remain offline.
- Day/night lighting is unchanged.
- Official/non-official shelter semantics are preserved.
- Failure immediately opens result/restart UI.
- Tsunami warning/pre-warning/active state timing is preserved.
- Direct shelter lines, green frames, labels, and hazard visuals remain nonblocking.
- R shows and hides the ranking panel.

Validation result:
- Preflight: passed
- EditMode: passed 123/123
- PlayMode: passed 42/42
- Temporary player build: passed
- Player.log parse: passed, 0 errors / 0 warnings / 0 exceptions
- DeepSeek: unavailable after normal and escalated API retries with `APIConnectionError`
