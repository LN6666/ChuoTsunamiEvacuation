# DeepSeek Review Prompt: P8-B Tsunami Evidence Wording Correction

You are reviewing the staged git diff for the Unity + PLATEAU Chuo Tsunami Evacuation project.

Task: verify the P8-B tsunami evidence wording correction before commit.

## Must Check

- The diff distinguishes 東京都「首都直下地震等による東京の被害想定」 as the broader official source family/report context.
- The actual extracted Chuo tsunami spatial layers are only:
  - 大正関東地震
  - 南海トラフ巨大地震 case 1
- The diff does not claim 都心南部直下地震-specific tsunami inundation-depth, arrival-time, or tsunami-height data was extracted.
- If 都心南部直下地震 is mentioned, it is labeled not available / not a tsunami scenario in current extracted layers, not missing project work.
- Nankai Trough case 5 and case 8 remain checked/not-present and are not falsely claimed as extracted Chuo layers.
- `inundationDepthMeters` remains sourced from official Tokyo tsunami inundation-depth CSVs where extracted.
- `arrivalTimeSeconds` remains sourced from official Tokyo tsunami arrival-time CSVs where extracted.
- `maxTsunamiHeightMeters` remains separate from `inundationDepthMeters`.
- `visualHeightMeters` remains cinematic only.
- `inundationBoundary` remains derived from extracted points/grid extent and is not claimed as a complete official inundation contour.
- Chuo flood hazard map material is not used as tsunami data.
- No P8-D collapse/damage proxy or P9 gameplay implementation was added.
- No gameplay success/failure rules changed.
- `Chuo_BaseMap.unity` is untouched.
- `P7_HighDetail_Chuo.unity` is preserved and not staged.
- ProjectSettings and Packages are clean.
- P8-B evidence spatial gate, P8-C preflight, EditMode tests, and PlayMode tests passed.

## Output Format

# DeepSeek P8-B Tsunami Evidence Wording Review

## A-Level Blockers
List only issues that must block commit.

## B-Level Follow-Ups
List non-blocking issues or review notes.

## Evidence Wording Verdict
State whether the source-family/scenario distinction is correct.

## Protected Path Review
State whether protected paths are clean/preserved.

## Verdict
Choose exactly one:
- Safe to commit
- Commit after minor fixes
- Do not commit yet
