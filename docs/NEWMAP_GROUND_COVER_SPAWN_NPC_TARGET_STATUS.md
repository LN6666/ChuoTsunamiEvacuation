# NewMap Ground Cover Spawn NPC Target Status

Generated: 2026-05-29T17:00:00+09:00

- Player spawn uses the ground-cover support Y.
- Random spawn candidates validate against bounds and building avoidance before acceptance.
- NPC placement uses the same support Y, so NPC visual feet should sit near the cover.
- Official shelter anchors, non-official candidates, green frames, and interaction markers align to the same support Y.
- Tourism and Evacuation modes keep their existing semantics and warnings.

This intentionally favors stable gameplay ground over exact PLATEAU visual height matching.
