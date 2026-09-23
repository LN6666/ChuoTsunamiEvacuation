# P10-A Humanitarian Candidate Final QA

P10-A verifies the P9-D humanitarian candidate boundary:

- `humanitarianCandidateTotal = 110`
- `namedHumanitarianCandidateCount = 28`
- `idOnlyHumanitarianCandidateCount = 82`
- `isOfficialShelter = false`
- `nonOfficialWarningRequired = true`
- `safeApprovedByDefault = false`

Required wording remains:

- Non-official humanitarian vertical evacuation candidate
- Life-first candidate
- Not an official evacuation shelter
- Use only when official shelter access is unsafe or unavailable

QA findings:

- Candidate markers are gameplay-usable through coordinate/proxy anchoring.
- Candidate selection remains warning-preserving through P9-C/P9-D logic.
- No humanitarian candidate should be silently treated as safe, approved, or official.
- ResultPanel and logs must preserve the non-official warning when a life-first candidate is selected.

Known limitation:

- Candidate-to-building identity is not exact PLATEAU object binding unless future tests prove it.
