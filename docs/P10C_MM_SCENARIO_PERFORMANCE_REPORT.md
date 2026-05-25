# P10-C-- Scenario Performance Report

Generated: 2026-05-25 19:53:46 local time.

| Scenario | Status | Evidence |
|---|---|---|
| `baseline_idle` | measured | 3/5/10 minute process sampling and FPS exporter use baseline idle unless manual interaction changes runtime state. |
| `tsunami_start` | prepared_not_automated | Exporter records green-frame state and frame spikes, but this script does not synthesize the T key or gameplay event. |
| `green_frames_markers` | not_observed | Runtime exporter inspects P10BGreenGroundFrameRuntime and candidate marker components. |
| `light_curtain` | not_observed | Runtime exporter inspects P8RiskFrontController and active LightCurtain/RiskFront objects. |
| `crowd_congestion` | not_observed | Runtime exporter counts P9CrowdRuntimeAgent components. |
| `result_panel_ui` | observed | Runtime exporter detects active ResultPanel scene objects. |
| `night_rain` | prepared_not_automated | Weather/night label is passed to exporter; runtime night overlay is detected if active. |

Untriggered rows are honest limitations, not pass claims. The exporter is present for built-player capture, but gameplay activation still needs manual or future input automation where marked.
