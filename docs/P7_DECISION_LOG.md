# P7 Decision Log

This file stores P7 decisions that affect scope, dependencies, assets, Unity settings, import size, benchmark acceptance, or phase boundaries.

## Decision Entry Template

### P7-DL-000 - Short Decision Title

| Field | Value |
|---|---|
| Date | YYYY-MM-DD |
| Stage | P7-0 / P7-A / P7-B / P7-C / P7-D |
| Decision |  |
| Options considered |  |
| Reason |  |
| Risk |  |
| Verification |  |
| Follow-up |  |
| Approved by |  |

## Active Decisions

### P7-DL-001 - P7-0 Adopts Documentation And Automation Only

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-0 |
| Decision | P7-0 creates docs, tools, prompts, and review gates only. It does not modify Unity assets, scenes, scripts, data, packages, project settings, or gameplay. |
| Options considered | Start with asset import; start with dependency/streaming tool adoption; start with docs/tools/reference review. |
| Reason | Full Chuo high-detail import and streaming decisions are high risk without inventory, benchmark protocol, and scope guards. |
| Risk | P7-A may still uncover asset sizes or missing LOD/category data that require scope reduction. |
| Verification | Run `tools/p7/run_p7_preflight.ps1`; verify protected paths are untouched; request DeepSeek review. |
| Follow-up | P7-A asset inventory and area/LOD selection. |
| Approved by | User after P7-0 DeepSeek PASS. |

### P7-DL-002 - P7 Has Exactly Five Stages

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-0 |
| Decision | P7 stages are P7-0, P7-A, P7-B, P7-C, and P7-D only. |
| Options considered | Continuing to P7-E/P7-F/P7-G; using a five-stage plan. |
| Reason | User instruction fixed P7 to five stages. |
| Risk | Future prompts or generated docs may accidentally create extra stages. |
| Verification | Scope guard warns on P7-E/P7-F/P7-G keywords; DeepSeek review checks stage count. |
| Follow-up | Keep all P7 task records aligned to five stages. |
| Approved by | User after P7-0 DeepSeek PASS. |

### P7-DL-003 - P7-A Uses Command-Line-First Asset Inventory

| Field | Value |
|---|---|
| Date | 2026-05-22 |
| Stage | P7-A |
| Decision | P7-A inventories local assets with PowerShell scripts that run without opening Unity and write Markdown reports for asset, LOD, and benchmark-candidate review. |
| Options considered | Manual Unity inspection; direct asset import; command-line metadata scan first. |
| Reason | P7 needs file counts, extension summaries, large-file risks, PLATEAU category hints, and benchmark candidates before any import or scene work. |
| Risk | Path/name inference can miss actual CityGML LOD details and cannot verify imported geometry quality. |
| Verification | Run `tools/p7/run_p7_asset_inventory.ps1`, then verify reports and P7 preflight result. |
| Follow-up | Use the inventory to narrow P7-B benchmark area planning before any Unity import work. |
| Approved by | P7-A prompt. |
