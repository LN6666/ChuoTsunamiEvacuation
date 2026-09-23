# NewMap Final Tuning Regression Status

- Status JSON: `Assets/Data/P10/newmap_final_tuning_regression_status.json`

Preserved accepted fixes:
- Blue ground and fall-through fixes remain covered by visible gameplay ground cover/support colliders.
- Player, NPCs, target anchors, green frames, route markers, labels, and interaction zones remain support-ground aligned.
- NPC amount/distribution, lifecycle, anti-clipping, and soft blocking are not reworked.
- Mouse left/right drag look is unchanged.
- Day/night lighting is unchanged.
- Official/non-official target labels and warnings are preserved.
- Route prototype guidance remains nonblocking and does not claim official route validation.
- P2-P10 gameplay flow remains the regression target.

Final validation gates:
- Final preflight
- EditMode tests
- PlayMode tests
- Temporary player build
- Player.log parse
- Repeated spawn safety sample
- Building collision overflow sample
- DeepSeek review
