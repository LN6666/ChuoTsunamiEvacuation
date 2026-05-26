# NewMap Manual Playtest Checklist

Generated: 2026-05-27T04:00:17+09:00

- Start `ChuoTsunamiEvacuation_NewMapHardeningPre2.exe`.
- Verify Start Menu, English/Japanese switch, Rules, weather, Tourism Mode, and Evacuation Mode.
- In Tourism Mode, inspect a local training target and confirm no failure occurs.
- In Evacuation Mode, wait for Stage 2 and confirm green frames appear only then.
- Interact with `newmap_proxy_safe_floor`, `newmap_proxy_blocked_entrance`, `newmap_proxy_no_safe_floor`, and `newmap_proxy_crowd_delay`.
- Inspect at least one official shelter marker if visible/reachable on the map.
- Confirm no old disabled route line or disabled target appears.
- Confirm ResultPanel text does not claim an official route or GIS-grade validation.
- Expect a documented startup pause: Pre2 max frame measured 11273.06 ms, while runtime bootstrap measured 169 ms.
- Record any additional visible frame pause after the first scene activation or during first mode selection.
