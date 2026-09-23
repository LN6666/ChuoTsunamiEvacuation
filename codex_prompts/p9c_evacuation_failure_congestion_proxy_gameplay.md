# P9-C Prompt Trace

Task: Evacuation Failure / Congestion / Vertical Evacuation Proxy Gameplay.

Branch: `p9c-evacuation-failure-congestion-proxy-gameplay`

Base: P9-B `abdd040115b69071e3bfc38c861799a4139fa0db`

Required outcomes:

- implement life-first selectable vertical evacuation target decision
- preserve non-official humanitarian candidate warnings
- implement entrance interaction, queue delay, congestion delay, and safe-floor proxy gameplay
- allow deterministic bounded outcome mutation in P9-C only
- implement collapse/debris exposure-event fatality proxy with default probability near `0.35`
- add ResultPanel-compatible feedback formatting
- add structured run log record
- keep P5 routes estimated prototype guidance
- avoid real building interior scenes and heavy social simulation packages
- consume P8/P9-B handoff data rather than reimplementing earlier stages

Validation requested:

- `powershell -ExecutionPolicy Bypass -File tools/p9/run_p9c_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`
- `python .\tools\deepseek_review.py --prompt-file .\deepseek_review_prompt_p9c.md`
