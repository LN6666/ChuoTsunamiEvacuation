# NewMap Ground Visual Alignment Round 3

Generated: 2026-05-29T00:00:00+09:00

Goal: player, buildings, and visible road/playable ground should appear aligned while the collision support remains invisible.

Changes:
- Runtime samples likely road/ground renderers where identifiable.
- Runtime detected transport/road surfaces at Y=0.00 and an imported building-base offset of 1.97m.
- Building renderer transforms receive a runtime-only 1.97m correction to align with the transport ground reference.
- Player spawn is placed at support Y plus the existing skin offset.
- Support-to-road and support-to-building-base deltas are logged.
- Active target height offsets remain logged for manual classification.

No PLATEAU geometry was reimported or randomly shifted.

Player smoke:
- Support Y: 0.00m
- Road sample Y: 0.00m
- Corrected building base Y: 0.00m
- Player spawn Y: 0.04m
- Support-to-road delta: 0.00m
- Support-to-building-base delta: 0.00m
- Active target height-offset violations: 8, documented for manual classification

Status: `completed_on_new_chuo_basemap_manual_visible_confirmation_remaining`
