# P8-C Humanitarian High-Rise Candidate Expansion Audit

Date: 2026-05-24.

## Result

The P8 humanitarian high-rise candidate audit is no longer sample-only.

Audit v1 combines:

- existing P5-F/P5-GH non-official sample candidates;
- local PLATEAU CityGML building attributes for Chuo (`13102-*`) using measured height, above-ground storeys, usage code, and centroid geometry;
- P8-B official tsunami layer bbox context for hazard-status eligibility only;
- a known-official exclusion set from P5 real Chuo building qualification outputs.

Output data:

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`

## Screening Counts

- Raw local PLATEAU building records matching screening criteria before top-N selection: 4147.
- PLATEAU records included in audit v1: 105.
- Existing sample records retained: 5.
- Total audit records: 110.
- Real source-name records found in PLATEAU: 24.
- Unknown-name PLATEAU records retained by building ID: 81.
- PLATEAU records inside a derived P8 tsunami boundary bbox: 105.

## Selection Rules

PLATEAU candidates were selected only when local evidence had at least one of:

- `measuredHeight >= 45m`;
- `storeysAboveGround >= 10`;
- office/commercial/hotel/mixed-use code with `measuredHeight >= 31m` or `storeysAboveGround >= 8`.

Known official shelter-matched PLATEAU building IDs from P5 qualification outputs were excluded from the non-official candidate layer.

## Candidate Categories

- `existing_sample_candidate`: existing P5-F/P5-GH controlled sample retained for continuity.
- `plateau_height_candidate`: local PLATEAU measured-height screen.
- `plateau_floor_candidate`: local PLATEAU above-ground-storey screen.
- `office_or_commercial_candidate`: local PLATEAU usage code suggests office/commercial/hotel/mixed-use context.
- `highrise_near_hazard_candidate`: centroid is inside a derived P8 tsunami boundary bbox.
- `named_tower_candidate` / `named_office_candidate`: reserved for source-named records; no real local PLATEAU source names were found in this run.
- `manual_review_required`: every audit v1 record requires manual review before persistent gameplay use.
- `insufficient_evidence`: sample/low-confidence records retained only for audit visibility.

## Non-Official Boundary

Every record has:

- `isOfficialShelter=false`;
- `nonOfficialWarningRequired=true`;
- `manualReviewNeeded=true`;
- `hazardStatusEligible=true`;
- `p8cProxyEligible=true`;
- `affectsGameplaySuccessFailure=false`;
- `implementsP8D=false`;
- `implementsP9SelectableGameplay=false`;
- no safe/approved status.

P8-D may use these records only as data-only building warning / entrance blocked / low-floor inundation warning / damage-proxy inputs. P8-E must verify persistent explicit non-official visibility and P9 handoff. P9 decides whether any become life-first selectable vertical evacuation targets using entrance/safe-floor/evacuation-complete proxies, never real indoor scenes.

## Limitations

- No web search was used.
- No candidate names were fabricated.
- Real PLATEAU-derived candidates mostly lack building names; building IDs are used as fallback identifiers.
- Public access, management agreement, seismic evidence, safe-floor assumptions, and entrance location remain unresolved.
- This audit does not claim official shelter status or safety approval.
