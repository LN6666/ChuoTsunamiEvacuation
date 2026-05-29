# NewMap Name Normalization Rules

Runtime-visible labels keep only trusted Japanese/Kanji main names.

Rules:
- prefer `name:ja`, then `name`, then `official_name`
- use `alt_name` only if it is a better main Japanese name
- hide full addresses and postal addresses
- hide GML IDs, PLATEAU IDs, coordinates, unknown names, and debug IDs
- do not machine translate
- do not fabricate names
- hide low-confidence labels in normal mode

Examples to show: 東京駅, 日本橋三越本店, 昭和通り.

Examples to hide: 東京都中央区〇〇丁目..., bldg_533..., 35.68,139.76.
