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

No active review items yet.

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

