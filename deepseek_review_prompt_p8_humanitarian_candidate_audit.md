# DeepSeek Review Prompt: P8 Humanitarian Candidate Expansion Audit

You are reviewing the staged git diff for the Unity + PLATEAU Chuo Tsunami Evacuation project.

Task: review the expanded non-official high-rise humanitarian candidate audit before P8-D.

## Must Check

- Candidate list is expanded beyond the original five P5 sample candidates if local PLATEAU evidence exists.
- Candidate names are not fabricated; source-named candidates come from local project/PLATEAU files and unknown-name candidates use fallback building IDs.
- Unknown names are clearly marked for manual review.
- Every candidate is non-official:
  - `candidateLayer=humanitarian_candidate`
  - `isOfficialShelter=false`
  - `nonOfficialWarningRequired=true`
  - no official shelter/designation claim
- Candidates are not marked safe or approved.
- Candidate audit JSON is data-only and does not change gameplay success/failure rules.
- `humanitarian_candidate_proxy` / `highrise_candidate_marker` remain proxy/data hazard categories only.
- P8-D/P8-E/P9 allocation is documented but not implemented here.
- No P8-D collapse/damage proxy implementation was added.
- No P9 life-first selectable gameplay was added.
- P8 stage count remains exactly P8-A through P8-E; no P8-0/F/G.
- P8-B tsunami evidence wording remains corrected:
  - broader source family is 東京都「首都直下地震等による東京の被害想定」
  - extracted tsunami scenarios are 大正関東地震 and 南海トラフ巨大地震 case 1
  - no 都心南部直下地震 tsunami layer claim
- Protected paths are clean:
  - `Chuo_BaseMap.unity`
  - ProjectSettings
  - Packages
  - Assets/PLATEAU
  - P7 high-detail scene not staged/reset
- Required preflights and Unity tests pass.

## Expected Validation

- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8_humanitarian_candidate_audit_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8bc_consolidation_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8c_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8b_evidence_spatial_gate.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/p8/run_p8a_preflight.ps1`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui`
- `powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui`

## Output Format

# DeepSeek P8 Humanitarian Candidate Audit Review

## A-Level Blockers
List only issues that must block commit.

## B-Level Follow-Ups
List non-blocking issues or risks.

## Candidate Evidence Review
State whether the expanded list is evidence-backed and non-fabricated.

## Non-Official / Gameplay Boundary Review
State whether the non-official and no-gameplay boundaries hold.

## Protected Path Review
State whether protected paths are clean/preserved.

## Verdict
Choose exactly one:
- Safe to commit
- Commit after minor fixes
- Do not commit yet
