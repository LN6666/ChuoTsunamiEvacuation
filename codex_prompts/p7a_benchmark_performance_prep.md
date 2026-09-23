# Codex Prompt: P7-A Codex B — Benchmark + Performance Automation Prep

You are working in:
D:\UnityProjects\ChuoTsunamiEvacuation-P7Perf

Branch:
p7a-benchmark-performance-prep

Phase:
PBL7 / P7-A

Role:
Codex B — Automation / Benchmark / Performance / EXE direction.

Main objective:
Strengthen P7 command-line automation for benchmark preparation, performance recording, status reporting, and P7-B/C/D readiness.

P7 total stage count:
P7 has exactly five stages:
- P7-0
- P7-A
- P7-B
- P7-C
- P7-D

Do not create P7-E, P7-F, or P7-G.

Strict scope:
This is docs/tools/prompts work only.
Do not modify Unity scenes.
Do not modify ProjectSettings.
Do not modify Packages.
Do not modify existing PLATEAU imported files.
Do not modify gameplay scripts.
Do not modify Assets/Data.
Do not import assets.
Do not download large data.
Do not add dependencies.
Do not implement P8 tsunami hazard systems.
Do not implement P9 crowd/spawn/indoor evacuation systems.

Allowed new files:
- tools/p7/new_p7_benchmark_record.ps1
- tools/p7/validate_p7_performance_log.ps1
- tools/p7/run_p7_benchmark_preflight.ps1
- docs/P7_BENCHMARK_AUTOMATION_PLAN.md
- docs/P7_EXE_PROFILING_PREP.md
- docs/P7_PERFORMANCE_LOG_SCHEMA.md
- codex_prompts/p7a_benchmark_performance_prep.md
- deepseek_review_prompt_p7a_perf.md

Allowed updates:
- docs/P7_AUTOMATION_WORKFLOW.md
- docs/P7_BENCHMARK_PROTOCOL.md
- docs/P7_PERFORMANCE_METRICS_TEMPLATE.md
- docs/P7_TWO_CODEX_WORKFLOW.md
- docs/P7_DECISION_LOG.md
- docs/TASKS.md
- docs/REVIEW_BACKLOG.md
- tools/p7/check_p7_scope.ps1 only to reduce expected warning noise from boundary/prompt/review files
- tools/p7/write_p7_status_report.ps1 only to optionally capture warning summaries
- tools/p7/run_p7_preflight.ps1 only if needed to support warning summary output

Do not edit:
- docs/P7_ASSET_INVENTORY_REPORT.md
- docs/P7_LOD_AVAILABILITY_REPORT.md
- docs/P7_BENCHMARK_AREA_CANDIDATES.md
- tools/p7/scan_p7_assets.ps1
- tools/p7/write_p7_asset_inventory_report.ps1
- tools/p7/run_p7_asset_inventory.ps1

These are owned by Codex A.

Implementation requirements:

1. Improve scope guard warning behavior
The P7-0 DeepSeek review noted that forbidden keyword warnings are expected in boundary/prompt/review files but may obscure real accidental additions.

Update tools/p7/check_p7_scope.ps1 carefully so that:
- protected paths still fail
- large files still fail
- actual unsafe files still warn
- docs whose purpose is boundary/prompt/review can be excluded from forbidden keyword warnings or reported as expected-context warnings
- no reduction in protected path strictness
- no protected file whitelist expansion

2. Improve status report traceability
Update tools/p7/write_p7_status_report.ps1 so the status report can include warning summaries when available.
Do not make warning text mandatory if no warning file exists.

3. tools/p7/new_p7_benchmark_record.ps1
Create a command-line helper that creates a timestamped Markdown benchmark record skeleton.
It should not run Unity.
It should generate a file under:
docs/p7_benchmark_records/

The record should include:
- timestamp
- branch
- machine specs placeholder
- scene placeholder
- asset scope
- LOD level
- graphics settings
- Editor metrics placeholder
- Windows EXE metrics placeholder
- average FPS
- 1% low FPS
- RAM
- VRAM or texture memory
- draw calls
- triangles
- batches
- loading time
- build size
- bottlenecks
- decision
- notes

4. tools/p7/validate_p7_performance_log.ps1
Create a validator for Markdown benchmark records.
It should check that required fields exist.
It does not need to parse numeric values yet.

5. tools/p7/run_p7_benchmark_preflight.ps1
Create an orchestration script:
- runs tools/p7/run_p7_preflight.ps1
- creates or validates a benchmark record skeleton if requested
- prints PASS/FAIL clearly

6. docs/P7_BENCHMARK_AUTOMATION_PLAN.md
Document benchmark automation plan from P7-B onward:
- Editor benchmark
- Windows EXE benchmark
- when GUI automated tests are required
- what remains manual only if unavoidable
- Markdown-first performance record

7. docs/P7_EXE_PROFILING_PREP.md
Document Windows x64 EXE profiling readiness:
- why EXE matters
- what to record
- target metrics
- how to compare Editor vs EXE
- P7-B optional EXE benchmark
- P7-D mandatory EXE profiling

8. docs/P7_PERFORMANCE_LOG_SCHEMA.md
Define required fields for benchmark logs.

9. docs/P7_DECISION_LOG.md
Mark P7-DL-001 and P7-DL-002 as approved by user after P7-0 DeepSeek PASS.
Add a P7-A decision entry for benchmark/performance automation prep.

10. docs/TASKS.md
Update only the P7-A automation/performance-prep subsection.

11. docs/REVIEW_BACKLOG.md
Record P7-A performance-prep risks:
- metrics may be placeholders until Unity benchmark exists
- EXE profiling depends on later build stage
- warning filtering must not weaken guard strictness

12. deepseek_review_prompt_p7a_perf.md
Create a review prompt that checks:
- no protected files changed
- no scenes/scripts/assets changed
- no Packages/ProjectSettings changed
- guard strictness preserved
- warning filtering does not hide real unsafe changes
- benchmark records are Markdown-first
- no Unity tests are required yet unless Unity files changed
- P7 still has exactly five stages

After implementation:
Run:
powershell -ExecutionPolicy Bypass -File tools/p7/run_p7_benchmark_preflight.ps1

Then show:
- preflight result
- benchmark preflight result
- git diff --name-only
- protected path check
- whether Unity tests were intentionally not run
- recommended commit message
