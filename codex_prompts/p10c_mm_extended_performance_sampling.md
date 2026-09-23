# Codex Prompt: P10-C-- Extended Performance Sampling

Implement and validate P10-C-- as an extended pre-release performance gate before official P10-C.

P10-C-- is not official P10-C, not the final release, and not release packaging/archive work. Do not create P10-E, P10-F, or P10-G. Do not commit build artifacts, raw huge logs, final release packages, or archives.

Primary tasks:

- add built-player FPS/frame-time/1 percent low/stutter exporter without high-detail scene mutation
- run or honestly document 3-minute, 5-minute, and 10-minute temporary player process sampling
- validate high-detail scene full-load as much as possible
- parse Player.log and summarize paging/memory evidence
- document scenario coverage and readiness decision
- keep protected paths clean

Validation:

- P10-C-- preflight
- Unity EditMode
- Unity PlayMode
- extended process sampling
- FPS/stutter capture
- DeepSeek review with no A-level blockers
