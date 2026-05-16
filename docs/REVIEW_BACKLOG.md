# Review Backlog

## Purpose

This document stores curated review results from DeepSeek V4 Pro and human review.

Raw DeepSeek reports are saved locally in:

review_reports/

Raw review reports should not be committed to GitHub.

This file only records actionable review items that should guide Codex fixes.

---

## Why This File Exists

The project uses a semi-automatic AI development workflow:

1. Codex writes or modifies code.
2. Unity is used for compile and Play testing.
3. DeepSeek V4 Pro reviews the current git diff.
4. The review result is summarized into this document.
5. The human developer decides which issues should be fixed.
6. A focused Codex fix prompt is created from the selected issues.
7. Codex fixes the selected issues.
8. Unity is tested again.
9. Stable changes are committed to Git.

This file is the bridge between DeepSeek review and Codex fixing.

---

## Review Workflow

1. Codex implements or modifies code.
2. Run Unity compile test.
3. Run Unity Play test when possible.
4. Run DeepSeek review:

   python tools\deepseek_review.py

5. Read the generated report in:

   review_reports/

6. Summarize only actionable issues here.
7. Convert selected issues into a Codex fix prompt.
8. Let Codex fix only the selected issues.
9. Re-test in Unity.
10. Commit only after the project is stable.

---

## Status Labels

Use these status labels:

- Open
- In Progress
- Fixed
- Deferred
- Rejected

Meaning:

Open:
The issue is accepted but not fixed yet.

In Progress:
The issue is currently being fixed by Codex or manually.

Fixed:
The issue has been fixed and tested.

Deferred:
The issue is valid but will be handled later.

Rejected:
The issue is not accepted because it is out of scope or not relevant.

---

## Priority Labels

Use these priority labels:

- Critical
- High
- Medium
- Low

Meaning:

Critical:
Must fix before commit. Usually compile errors, runtime crashes, broken Unity references, or core gameplay failure.

High:
Should fix before commit if possible. Usually serious maintainability, state flow, or Unity setup problems.

Medium:
Useful improvement, but not blocking the current milestone.

Low:
Minor improvement, style issue, or future refactor item.

---

## Review Entry Template

### Review YYYY-MM-DD-XX

Source report:

review_reports/deepseek_review_YYYYMMDD_HHMMSS.md

Context:

Briefly describe what was reviewed.

Example:

First playable gameplay scripts after Codex generated the core loop.

Overall verdict:

Safe to commit / Commit after minor fixes / Do not commit yet

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| R-001 | High | Open | Assets/Scripts/... | Describe issue here | Describe required fix here |

### Codex Fix Prompt

Plain-text prompt to send to Codex:

Please fix the following reviewed issues.

Target files:

- Assets/Scripts/...

Rules:

- Do not modify unrelated files.
- Do not commit.
- Do not touch PLATEAU imported scene files.
- Do not touch raw PLATEAU data.
- Keep the fix focused and minimal.

Issues:

1. ...
2. ...

After fixing:

- summarize changed files
- explain how to test
- do not commit

---

## Active Reviews

No active blocking review items.

---

### Review Pending 2026-05-16-01

Source report:

Pending DeepSeek review for Milestone 2-05.

Context:

Milestone 2-05 - Evaluation, Decision Feedback & Real-Data Integration Hooks 1.0 implementation.

Overall verdict:

Pending review.

### Known Non-Blocking Risks To Review

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-05-B01 | Medium | Deferred | Assets/Scripts/Result/ResultExportService.cs | Result export uses a local run_logs/ folder derived from Application.dataPath. This is acceptable for the Editor/debug prototype but may not be writable on all build targets. | Revisit export path policy before player builds or CI-driven evaluation runs; consider Application.persistentDataPath or a configurable output directory. |
| M2-05-B02 | Medium | Deferred | Assets/Scripts/Data/ShelterSourceConfigLoader.cs / Assets/Data/shelter_source_config.json | The real-data source mode is a documented P4 hook only. It does not load real data yet. | Implement active real sample loading only during P4 after P3 output format is reviewed. |
| M2-05-B03 | Low | Deferred | Assets/Scripts/Result/ResultMetrics.cs / Assets/Scripts/Result/ResultPanelController.cs | ResultPanel remains a fixed prototype/debug UI. Very long advice or failure reason text may still need a later scroll view. | Revisit ResultPanel layout only if UI polish becomes a milestone. |
| M2-05-B04 | Medium | Deferred | Assets/Data/scenario_presets.json | scenario_presets.json should be saved as UTF-8 without BOM. Current .NET reading may tolerate a BOM, but future loaders or external tools may not. | Keep Unity JSON files BOM-free when editing or regenerating them. |
| M2-05-B05 | Medium | Deferred | Assets/Scripts/Result/ResultMetrics.cs | Blocked-shelter advice currently depends on failureReason substring matching, such as unavailable or not available text. If failure reason wording changes, advice may fall back to generic text. | Consider structured failure codes or enums instead of string matching in a future result-reason pass. |
| M2-05-B06 | Medium | Deferred | Assets/Scripts/Core/EvacuationGameManager.cs | Result export now uses a one-shot guard, but future result-state changes could accidentally bypass or duplicate finalization paths. | Preserve explicit one-export-per-run behavior when changing success/failure finalization. |

---

## Notes

DeepSeek review should not be followed blindly.

The human developer must decide:

- which issues are valid
- which issues are urgent
- which issues should be deferred
- which issues are outside the current milestone

Codex should receive only focused, selected fix tasks.

Do not send the entire raw review report to Codex unless necessary.

---

### Review 2026-05-15-04

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-04 - Multi-Shelter Decision Gameplay 1.0.

Overall verdict:

No A-level blocking issues. Non-blocking risks deferred.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-04-B01 | Medium | Deferred | Assets/Scripts/Data/ShelterDataLoader.cs | GetAllShelters currently iterates over ShelterIdsInLoadOrder from the base JSON. Current scenarios only override existing IDs, so this is safe now. If a future scenario introduces a brand-new shelter ID, it will not appear in the generated field unless loader behavior is extended. | If future scenarios need runtime-only shelters, extend GetAllShelters and scenario override handling to include safely validated new shelter IDs. |
| M2-04-B02 | Medium | Deferred | Assets/Scripts/Editor/FirstPlayableSceneBuilder.cs | BuildFirstPlayableTestSetup creates marker materials for official/candidate/blocked shelters. This is harmless for the current small debug field, but repeated editor invocations may clutter editor material instances. | Cache, reuse, or clean up generated editor materials if setup generation becomes frequent or material clutter becomes visible. |
| M2-04-B03 | Medium | Deferred | Assets/Tests/EditMode/ChuoTsunamiEvacuation.EditModeTests.asmdef | The EditMode test asmdef references ChuoTsunamiEvacuation.Editor. This is valid for EditMode tests. If future tests are moved to PlayMode assemblies, maintainers must avoid editor-only dependencies. | Keep editor-only references limited to EditMode tests; do not reference ChuoTsunamiEvacuation.Editor from PlayMode test assemblies. |
| M2-04-B04 | Medium | Deferred | Assets/Scripts/Shelter/ShelterEntranceTrigger.cs | ShelterEntranceTrigger.Update calls gameManager?.ApplyShelterConfig(shelter) every frame while the player is inside the trigger. This is currently idempotent and harmless for the small debug-platform prototype, but it creates minor runtime overhead and slightly deviates from an event-driven trigger design. | Future optimization could apply shelter config only on trigger enter, shelter change, or state change. |

### Decision

No immediate Codex fix is required before this commit for the remaining B-level items.

Reason:

- The review found no A-level blockers.
- Current scenarios only override existing shelter IDs.
- Material lifecycle and test assembly boundaries are acceptable for the current debug-platform prototype.

---

### Review 2026-05-15-03

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-02 - Scenarioized Gameplay Rules 1.0.

Overall verdict:

No A-level blocking issues. Non-blocking risks deferred.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-02-B01 | Medium | Deferred | Assets/Data/scenario_presets.json | scenario_presets.json may contain a UTF-8 BOM. Manual editing in BOM-unaware tools may cause confusion. | Save scenario_presets.json as UTF-8 without BOM. |
| M2-02-B02 | Medium | Deferred | Assets/Scripts/ChuoTsunamiEvacuation.Runtime.asmdef / Assets/Tests/EditMode/ChuoTsunamiEvacuation.EditModeTests.asmdef / Assets/Tests/PlayMode/ChuoTsunamiEvacuation.PlayModeTests.asmdef | EditMode and PlayMode test assemblies reference ChuoTsunamiEvacuation.Runtime. Future test-needed runtime scripts must stay inside the runtime assembly or tests may fail to compile. Risk is low, but maintainers should be aware of the assembly boundary. | Keep runtime scripts that tests need under the runtime assembly, or update asmdef references when adding new assembly boundaries. |
| M2-02-B03 | Medium | Deferred | Assets/Scripts/Result/ResultPanelController.cs / Assets/Scripts/Result/ResultMetrics.cs | Current ResultPanel is readable, but very long scenario names or verbose failure reasons may still clip. This is acceptable for prototype/debug UI. | Revisit ResultPanel sizing or add a simple ScrollView if UI polish becomes a milestone. |

### Decision

No immediate Codex fix is required before this commit for the remaining B-level items.

Reason:

- The review found no A-level blockers.
- The UTF-8 BOM cleanup was handled.
- The assembly and ResultPanel items are maintainability/UI risks for future milestones.

---

### Review 2026-05-15-02

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-03 - Unity Test Automation Foundation.

Unity testing confirmed:
- EditMode tests ran successfully.
- PlayMode smoke tests ran successfully.

Overall verdict:

No A-level blocking issues. Non-blocking risks deferred.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-03-B01 | Medium | Deferred | Assets/Tests/EditMode/AntiCampingConfigTests.cs / Assets/Tests/EditMode/ShelterDataLoaderTests.cs | Current tests verify normal config loading and defaults, but do not explicitly test missing anti_camping_config.json or missing test_shelters.json. Risk is low because files exist in normal setup, but future test rounds should cover accidental deletion/fallback behavior. | Add missing-file/fallback EditMode tests for AntiCampingConfig and ShelterDataLoader in a future test coverage pass. |
| M2-03-B02 | Medium | Deferred | Assets/Tests/PlayMode/EvacuationSmokePlayModeTests.cs / Assets/Scripts/Core/EvacuationGameManager.cs | Current PlayMode tests instantiate EvacuationGameManager on an empty GameObject. This is acceptable now, but future milestones may add heavier Awake initialization or required scene references. | Watch smoke test sensitivity to future EvacuationGameManager Awake/init changes; add a minimal fixture setup if needed. |

### Decision

No immediate Codex fix is required before this commit.

Reason:

- The review found no A-level blockers.
- Unity EditMode and PlayMode test runs passed.
- The issues are future test coverage and test fixture robustness improvements.

---

### Review 2026-05-15-01

Source report:

review_reports/deepseek_review_20260515_*.md

Context:

DeepSeek V4 Pro review for Milestone 2-01 - Data-Driven Rules Integration after Unity testing and stabilization fixes.

Unity Play testing confirmed:
- manual T tsunami start works
- countdown starts only after warning
- random warning works when enabled
- shelter JSON can block entry and show failureReason
- climbTimeSeconds and crowdingDelaySeconds affect climb duration
- anti-camping detection and blocking work when enabled
- anti-camping is disabled by default
- missing tsunami config uses safe defaults
- unknown shelterId preserves in-scene shelter values
- ResultPanel is readable
- tsunami risk-front failure catches player bypassing the visible wall
- active shelter entrance fails during climb if the risk front passes it
- MarkSceneDirty no longer errors in Play Mode

Overall verdict:

No A-level blocking issues after fixes. Ready to commit after documenting B/C items.

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| M2-01-B01 | Medium | Deferred | Assets/Scripts/Core/EvacuationGameManager.cs / Assets/Data/tsunami_event_config.json | If both manualStartEnabled and randomStartEnabled are false, the game can stay in PreEvent with no start path. | Later mitigation: warning, fallback to manual, or explicit disabled-event mode. |
| M2-01-B02 | Medium | Deferred | Assets/Scripts/Data/GameConfigLoader.cs / Assets/Scripts/Data/ShelterDataLoader.cs | Runtime loading uses Application.dataPath + "/Data/...". This is acceptable for the Editor prototype but not build-safe. | Later migrate to StreamingAssets or another build-safe loading path. |
| M2-01-C01 | Low | Deferred | Assets/Scripts/Result/ResultPanelController.cs | If ResultPanel detail text grows much longer, fixed text areas may become cramped again. | Consider a simple ScrollView in a future UI pass. |

### Decision

No immediate Codex fix is required before this commit for the B/C items.

Reason:

- A-level blockers were fixed.
- Unity testing confirmed the Milestone 2-01 gameplay loop.
- Remaining issues are deferred design/tooling polish for later Milestone 2 tasks.

---

### Review 2026-05-14-02

Source report:

D:\UnityProjects\ChuoTsunamiEvacuation\review_reports\deepseek_review_20260514_032100.md

Context:

DeepSeek V4 Pro max-thinking review for the first playable evacuation prototype.

Unity Play testing confirmed:
- WASD / arrow-key movement works
- Shift sprint works
- mouse-based third-person camera control works
- E shelter entry works
- climb simulation works
- T starts tsunami test
- tsunami risk can trigger failure
- tsunami reaching the active shelter entrance during climb can trigger failure

Overall verdict:

Safe to commit

### Action Items

| ID | Priority | Status | Target File | Issue | Required Fix |
|---|---|---|---|---|---|
| R-002 | Medium | Deferred | Assets/Scripts/Shelter/ShelterEntranceTrigger.cs | Missing shelter reference could be clearer if setup is broken. | Add null warning in Awake and Update before TryEnterShelter in a future cleanup pass. |
| R-003 | Low | Deferred | Assets/Scripts/Player/SimplePlayerController.cs | fallbackTranslateIfControllerStuck is useful for debugging but can bypass collision. | Add a production-warning comment or revise after movement system stabilizes. |
| R-004 | Low | Deferred | Assets/Scripts/Core/EvacuationGameManager.cs | Public state-changing methods could use clearer summary comments. | Add XML summary comments in a later documentation cleanup. |
| R-005 | Low | Deferred | Assets/Scripts/UI/GameUIManager.cs | Uses legacy UnityEngine.UI.Text instead of TextMeshPro. | Accept for prototype; consider migration after gameplay stabilizes. |

### Decision

No immediate Codex fix is required before this commit.

Reason:

- No critical issues were found.
- Unity Play testing confirmed the first playable loop works.
- The remaining issues are maintainability or future cleanup items.
- The prototype should be committed now as a stable milestone.

