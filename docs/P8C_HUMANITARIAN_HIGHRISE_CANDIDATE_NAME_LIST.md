# P8-C Humanitarian High-Rise Candidate Name List

Date: 2026-05-24.

## Warning

These are not official evacuation shelters.

They are non-official humanitarian / life-first high-rise candidates from existing project files.

They must not be displayed as official shelters.

Future persistent visibility requires explicit non-official labeling.

The current source is controlled P5-F/P5-GH sample data, not full real Chuo high-rise screening.

## Summary

Total non-official humanitarian candidates found: 5.

Official-layer records found in the same source files and excluded from this humanitarian list: 2.

## Candidate Table

| index | candidateId | buildingName | address or locationText | sourceFile | candidateLayer | publicAccessStatus | manualReviewNeeded | isOfficialShelter / officialFlag | latitude / longitude | notes |
|---:|---|---|---|---|---|---|---:|---|---|---|
| 1 | `p5f_sample_humanitarian_strong_003` | Sample High-Rise Office Tower | Sample waterfront office district address | `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json` | `humanitarian_candidate` | `likely_public_or_lobby_access` | true | `isOfficialShelter=false`; `officialDesignationStatus=not_official` | 35.6592 / 139.7748 | `humanitarian_strong_candidate`; controlled synthetic P5-F/P5-GH sample. |
| 2 | `p5f_sample_humanitarian_review_004` | Sample High-Rise Apartment Building | Sample riverfront apartment address | `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json` | `humanitarian_candidate` | `unknown` | true | `isOfficialShelter=false`; `officialDesignationStatus=unknown` | 35.6615 / 139.7805 | `humanitarian_candidate_with_review`; controlled synthetic P5-F/P5-GH sample. |
| 3 | `p5f_sample_humanitarian_weak_005` | Sample Mid-Rise Commercial Building | Sample inland commercial address | `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json` | `humanitarian_candidate` | `restricted_private` | true | `isOfficialShelter=false`; `officialDesignationStatus=not_official` | 35.6704 / 139.7764 | `humanitarian_weak_candidate`; controlled synthetic P5-F/P5-GH sample. |
| 4 | `p5f_sample_unknown_006` | n/a | n/a | `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json` | `humanitarian_candidate` | `unknown` | true | `isOfficialShelter=false`; `officialDesignationStatus=unknown` | 35.6677 / 139.7822 | `unknown`; insufficient evidence in controlled sample. |
| 5 | `p5f_sample_not_recommended_007` | Sample Low-Rise Building With Seismic Concern | Sample low-rise address | `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json` | `humanitarian_candidate` | `restricted_private` | true | `isOfficialShelter=false`; `officialDesignationStatus=not_official` | 35.6633 / 139.7688 | `not_recommended`; included for audit because it is a non-official candidate-layer record, not a recommended target. |

## Files Searched

- `Assets/Data`
- `docs`
- `data_pipeline`
- `processed` (root path not present)
- `Assets/Scripts`
- `Assets/Tests`

Search terms included:

- humanitarian
- high-rise / highrise
- tower
- office
- life-first
- candidateLayer
- publicAccessStatus
- manualReviewNeeded
- isOfficialShelter
- HumanitarianCandidate
- P5-F / P5-GH / P5G / P5F

## Files Used

- `Assets/Data/p5g_highrise_humanitarian_candidates_sample.json`
- `data_pipeline/qualification/highrise_humanitarian_candidates_sample.json`
- `Assets/Scripts/Data/HumanitarianCandidateDataLoader.cs`
- `Assets/Scripts/Shelter/HumanitarianCandidateMetadata.cs`
- `docs/P5F_HIGHRISE_HUMANITARIAN_CANDIDATES.md`
- `docs/P5G_HUMANITARIAN_CANDIDATE_UNITY_INTEGRATION.md`
- `Assets/Tests/PlayMode/P5GHHumanitarianCandidatePlayModeTests.cs`

## Files Not Found Or Missing Expected Data

- No full real Chuo high-rise screening output was found.
- No real non-official candidate dataset beyond the controlled P5-F/P5-GH sample was found.
- No separate `HumanitarianCandidate*.json` source file was found.
- The root `processed` path requested by the audit is not present; processed project outputs are under `data_pipeline/processed`.

## Audit Notes

`HumanitarianCandidateDataLoader` accepts only `candidateLayer = humanitarian_candidate` and skips `candidateLayer = official` records. P5-GH life-first selectable proxies set `isOfficialShelter = false` and use `sourceType = p5g_humanitarian_candidate`.
