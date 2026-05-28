# NewMap Night Lighting Correction

Generated: 2026-05-29T00:00:00+09:00

Status: `validated_by_tests_and_player_smoke_manual_night_visual_confirmation_remaining`

Round 2 separates sky darkness from building readability:

- clear day keeps a bright sky and 1.25 directional light
- night uses a dark camera sky color
- night keeps moderate ambient fill for building readability
- night adds a low-intensity non-shadowed fill light
- rain and night rain use separate profiles
- switching night back to day reapplies the clear-day profile

The target is darker sky, readable buildings, and unchanged UI/marker readability. Final acceptance still requires manual visual confirmation.

Player smoke:

- night sky brightness: 0.042
- night building readability score: 0.517
- day restored after night: true
- Player.log: 0 errors, 0 warnings

Config: `Assets/Data/P10/newmap_lighting_profiles.json`

Status JSON: `Assets/Data/P10/newmap_night_lighting_correction.json`
