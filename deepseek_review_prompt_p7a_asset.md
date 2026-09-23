# DeepSeek Review Prompt: P7-A Asset Inventory

You are reviewing P7-A for the ChuoTsunamiEvacuation Unity + PLATEAU project.

Branch:

`p7a-asset-inventory-lod-area`

Scope:

P7-A is command-line asset inventory and report generation only. It must not import assets, modify Unity scenes, modify gameplay scripts, modify `Assets/Data`, modify `Packages`, modify `ProjectSettings`, or modify existing PLATEAU imported files.

Expected P7-A files:

- `tools/p7/scan_p7_assets.ps1`
- `tools/p7/write_p7_asset_inventory_report.ps1`
- `tools/p7/run_p7_asset_inventory.ps1`
- `docs/P7_ASSET_INVENTORY_REPORT.md`
- `docs/P7_LOD_AVAILABILITY_REPORT.md`
- `docs/P7_BENCHMARK_AREA_CANDIDATES.md`
- `docs/P7_ASSET_INVENTORY_PROTOCOL.md`
- `docs/P7_DECISION_LOG.md`
- `docs/TASKS.md`
- `docs/REVIEW_BACKLOG.md`
- `deepseek_review_prompt_p7a_asset.md`
- `codex_prompts/p7a_asset_inventory_lod_area.md`

Review checks:

1. Confirm no protected files changed:
   - `ProjectSettings/`
   - `Packages/`
   - `Assets/Scenes/`
   - `Assets/PLATEAU/`
   - `Assets/Scripts/`
   - `Assets/Data/`
2. Confirm no assets or Unity scenes were modified.
3. Confirm no package, project setting, or dependency change was introduced.
4. Confirm `tools/p7/scan_p7_assets.ps1` is read-only and uses file metadata/path inference only.
5. Confirm reports clearly state that LOD and category findings are path/name-based inference only and do not verify geometry quality.
6. Confirm benchmark candidates are not presented as final selected benchmark areas.
7. Confirm P7 still has exactly five stages: P7-0, P7-A, P7-B, P7-C, and P7-D.
8. Confirm no P8 hazard implementation or P9 shelter-interior scope was added.
9. Confirm `tools/p7/run_p7_asset_inventory.ps1` exits non-zero when scan, report writing, or preflight fails.
10. Confirm P7 preflight was run after implementation and that any warnings do not represent scope violations.

Please return:

- Overall verdict: PASS / PASS WITH FOLLOW-UPS / FAIL
- A-level blockers, if any
- B-level follow-ups, if any
- Protected path findings
- Read-only scanner assessment
- Report clarity assessment
- Scope boundary assessment
