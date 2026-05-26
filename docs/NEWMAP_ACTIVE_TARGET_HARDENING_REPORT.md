# NewMap Active Target Hardening Report

JSON: `Assets/Data/P10/newmap_active_target_hardening_report.json`

Active runtime targets: 4.

All active targets are local non-official training targets. They validate gameplay mechanics only and do not represent official shelters, official routes, or safety approval.

Verified active flows:

- `newmap_proxy_safe_floor`: safe-floor success path.
- `newmap_proxy_blocked_entrance`: blocked entrance failure path.
- `newmap_proxy_no_safe_floor`: no safe-floor failure path.
- `newmap_proxy_crowd_delay`: local NPC congestion delay path.

Official shelters remain inactive because no official record has a verified anchor in the new `Chuo_BaseMap`.
