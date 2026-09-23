# P8-E Humanitarian Candidate Persistent Visibility Plan

Date: 2026-05-24.

P8-E confirms the future persistent visibility plan for the expanded humanitarian high-rise candidate audit.

## Accepted Candidate Set

- Total candidates: 110.
- Named candidates: 28.
- ID-only / unknown-name candidates: 82.
- ID-only / unknown-name PLATEAU candidates: 81.
- Existing P5 sample candidates: 5.
- PLATEAU-derived candidates: 105.

The user accepts the ID-only / unknown-name PLATEAU candidates as real high-end office buildings/towers for future game review. One additional unknown-name record comes from the existing sample set and remains a manual-review sample, not a PLATEAU-derived office/tower acceptance. This acceptance does not make any candidate an official shelter, safe, approved, or selectable.

## Persistent Visibility Rule

Candidates may become persistently visible in the future game only with explicit non-official labeling.

Required label:

> Non-official humanitarian high-rise candidate. Not an official evacuation shelter.

Required data constraints:

- `isOfficialShelter=false`
- `nonOfficialWarningRequired=true`
- `manualReviewNeeded=true` where public access, management agreement, entrance, safe floor, or seismic evidence is not verified
- `selectableGameplayEnabled=false` during P8-E

## Handoff

P8-E does not place final persistent scene objects. It hands P9 the data, labels, status tokens, and proxy flow requirements so P9 can decide whether candidates become life-first selectable vertical evacuation targets.
