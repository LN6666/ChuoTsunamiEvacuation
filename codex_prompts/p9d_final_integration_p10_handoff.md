# P9-D Prompt Trace

Task: Final Integration + Coordinate-Based Anchoring Closure + P10 Handoff.

Branch: `p9d-final-integration-p10-handoff`

Base: P9-C `4f4f11957b7530220e9ba965c758804c5015f89d`

Required outcomes:

- integrate P9-A/B/C into final P9 gameplay flow
- implement coordinate-based anchoring with confidence, thresholds, fallback, and reports
- apply anchoring to humanitarian candidates, official shelters where data exists, entrance/safe-floor proxies, route proxies, hazard lookup, and spawn relations
- preserve non-official humanitarian warnings
- keep P5 routes estimated and non-official
- validate entrance, queue, congestion, safe-floor, hazard timing, and collapse/debris outcome paths
- document final P9 limitations and P10 handoff
- avoid P9-E/F/G
- avoid protected scene/settings/package/PLATEAU changes

Validation requested:

- `powershell -ExecutionPolicy Bypass -File tools/p9/run_p9d_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p9d.md`
