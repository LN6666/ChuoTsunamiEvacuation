# NewMap Ground Height Realignment Round 2

Generated: 2026-05-29T00:00:00+09:00

Status: `player_log_validated_manual_visible_confirmation_remaining`

Round 2 stops relying on a hardcoded support height. Runtime bootstrap now samples loaded map renderer bounds before player spawn and uses the low-percentile renderer base height as the visual ground/building-base reference. The invisible gameplay support surface is aligned to that sampled height. If no usable renderer sample exists, the fallback support height is documented as 1.2m rather than silently assuming 0m.

Reported fields:

- old support Y: 0.0
- new support Y: 1.97
- player spawn Y: 2.01
- player spawn offset above support: 0.04m
- sampled building base Y: 1.97
- sampled map min Y: -8.52
- visual ground samples: 12754
- support-to-visual delta: 0.0
- active target max height offset: 3.27
- active target height offset violations: 2
- fall recovery warnings in smoke: 0

No imported PLATEAU geometry is moved. A single flat gameplay support plane cannot perfectly match every elevated terrain/building base across the full Chuo map; the spawn support is aligned to the sampled visual base, while 2 active target anchors remain more than 2.5m from the support height and need manual visual classification.

JSON: `Assets/Data/P10/newmap_ground_height_realignment_round2.json`
