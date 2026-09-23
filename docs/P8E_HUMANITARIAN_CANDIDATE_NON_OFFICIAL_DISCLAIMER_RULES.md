# P8-E Humanitarian Candidate Non-Official Disclaimer Rules

Date: 2026-05-24.

Every humanitarian high-rise candidate must be presented as non-official.

## Required Wording

Use this wording or a stricter equivalent:

> Non-official humanitarian high-rise candidate. Not an official evacuation shelter.

## Forbidden Wording

Do not state or imply:

- official evacuation shelter;
- officially approved shelter;
- safe building;
- guaranteed refuge;
- official damage/safety approval;
- final selectable evacuation target during P8-E.

## Required Data Flags

- `isOfficialShelter=false`
- `nonOfficialWarningRequired=true`
- `manualReviewNeeded=true` unless future review explicitly verifies access and safe-floor evidence
- `selectableGameplayEnabled=false` in P8-E
- `affectsGameplaySuccessFailure=false` in P8-E

P9 may decide selectable behavior later, but must keep the non-official disclaimer visible.
