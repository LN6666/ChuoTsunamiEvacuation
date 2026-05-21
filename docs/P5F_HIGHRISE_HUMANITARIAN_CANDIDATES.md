# P5-F High-Rise Humanitarian Vertical Evacuation Candidate Screening

## Status

P5-F is a data, schema, rulebook, and planning milestone.

It does not modify Unity gameplay, Unity scenes, PLATEAU imported files, `Assets/Data`, `ProjectSettings`, or `Packages`.

P5-F is not a substitute for official evacuation facility designation. Humanitarian candidates are not official shelters, and no real Chuo high-rise screening has been performed yet.

## Purpose

P5-F defines a conservative pipeline for identifying high-rise buildings in Chuo Ward that may have humanitarian emergency vertical evacuation potential during tsunami risk.

This is a life-first emergency access concept. In a tsunami emergency, immediate life safety may require considering tall offices, towers, commercial buildings, and apartments even when ordinary public access, owner permission, and management agreements have not been verified.

The pipeline must still be explicit about uncertainty. A non-official building must never be presented as an official evacuation shelter.

Life-first emergency access is a scenario assumption for planning and decision-support review. It is not a legal permission, owner agreement, public access guarantee, or official evacuation designation.

## Layer Separation

P5-F separates two layers:

| Layer | Meaning | Allowed claim |
|---|---|---|
| Official/designated evacuation facilities | Facilities or buildings confirmed by official Chuo/Tokyo/administrative sources. | May use `official_confirmed` or `official_confirmed_with_review` when official evidence exists. |
| Humanitarian emergency candidate high-rises | Non-official or unverified high-rise buildings that may provide vertical refuge potential under life-first emergency screening. | May use `humanitarian_strong_candidate`, `humanitarian_candidate_with_review`, or `humanitarian_weak_candidate`, but must not claim official designation. |

Official designation is an evidence claim. Humanitarian candidacy is a screening judgment for emergency decision support and later manual review.

The schema uses the machine-readable `candidateLayer` field to preserve this boundary:

- `candidateLayer = "official"` is allowed only for records with official designation evidence.
- `candidateLayer = "humanitarian_candidate"` is required for non-official, unknown, not evaluated, conflicting, or not-recommended screening records, even when physical suitability is strong.

## Candidate Status Taxonomy

| Status | Meaning | Official evidence required | Manual review |
|---|---|---:|---:|
| `official_confirmed` | Official source confirms the facility/building and the building identity is sufficiently clear. | Yes | Usually no |
| `official_confirmed_with_review` | Official source exists, but match, source date, facility extent, or metadata ambiguity remains. | Yes | Yes |
| `humanitarian_strong_candidate` | Non-official high-rise candidate with strong physical suitability evidence and manageable uncertainty. | No | Yes |
| `humanitarian_candidate_with_review` | Non-official candidate with promising physical signals but unresolved access, seismic, routing, management, or source uncertainty. | No | Yes |
| `humanitarian_weak_candidate` | Non-official candidate with limited positive evidence or weak vertical refuge margin. | No | Yes |
| `unknown` | Insufficient evidence to recommend or reject. | No | Yes |
| `not_recommended` | Evidence suggests the building should not be treated as a vertical evacuation candidate. | No | Usually yes before final exclusion |

Non-official statuses must carry warnings that they are not official shelters and require review before public use.

## Evidence Model

Candidate records are validated by:

- `data_pipeline/qualification/highrise_humanitarian_candidate_schema.json`
- controlled sample fixture: `data_pipeline/qualification/highrise_humanitarian_candidates_sample.json`

Core fields:

- candidate identity: `candidateId`, `buildingId`, `plateauBuildingId`
- location and building metadata: `buildingName`, `address`, `latitude`, `longitude`, `heightMeters`, `floorsAboveGround`, `usageType`, `buildingUse`
- capacity and risk context: `estimatedCapacityProxy`, `tsunamiRiskContext`
- accessibility context: `distanceToOfficialShelter`, `routeDistanceMeters`, `routeTimeSeconds`
- uncertainty fields: `seismicEvidenceLevel`, `seismicEvidenceSource`, `publicAccessStatus`, `managementAgreementStatus`
- official/candidate separation: `officialDesignationStatus`, `candidateLayer`, `humanitarianCandidateStatus`
- review metadata: `confidence`, `manualReviewNeeded`, `warnings`, `reviewTriggers`, `reviewRisks`, `sourceRefs`

## Conservative Scoring Method

The rulebook uses two scores:

- `physicalSuitabilityScore`: higher is better, 0 to 100.
- `operationalUncertaintyScore`: higher means more uncertainty, 0 to 100.

The score is an audit aid, not an automatic public safety certification.

### Physical Suitability Signals

Physical suitability may increase when evidence supports:

- height of about 30 meters or 10 or more floors for strong high-rise candidacy
- height of about 15 meters or 5 or more floors for weak or review-level candidacy
- likely usable upper floors above expected tsunami inundation context
- reinforced concrete, steel, post-1981, retrofit, or other positive structural hints
- large floor area or capacity proxy
- road or pedestrian route access that is not obviously impossible
- location near tsunami-risk context where vertical evacuation potential matters
- location far from official facilities, when this is used only as a humanitarian access need signal

### Operational Uncertainty Signals

Operational uncertainty increases when:

- official designation is absent or unknown
- public access is unknown or restricted
- management agreement status is unknown or not found
- seismic evidence is estimated or unknown
- building use is private residential, office-only, industrial, or otherwise not designed for emergency public intake
- route distance/time is long, missing, or based on prototype routing
- source date, building identity, height, floors, or capacity are missing

Public access is not a hard exclusion in this humanitarian emergency screening. It must remain visible as uncertainty and usually triggers manual review.

Unknown public access, unknown management agreement status, and unknown seismic evidence may require manual review before any operational interpretation. `publicAccessStatus = "unknown"` does not automatically exclude a physically plausible humanitarian candidate, but it must trigger `manualReviewNeeded = true` with review triggers, warnings, and review risk codes.

### Classification Rules

`official_confirmed`:

- requires official designation evidence
- requires clear building identity
- should have high confidence

`official_confirmed_with_review`:

- requires official designation evidence
- used when official evidence exists but building match, source date, or metadata needs review

`humanitarian_strong_candidate`:

- not official
- strong height/floor evidence, such as 30 meters or 10 or more floors
- positive or estimated structural/seismic hint
- route/access not obviously impossible
- warnings must state that this is not an official shelter

`humanitarian_candidate_with_review`:

- not official
- physically promising, but access, management, seismic evidence, route, or capacity is unknown or unresolved
- public access unknown may remain in this category instead of becoming `not_recommended`

`humanitarian_weak_candidate`:

- not official
- only partial height/floor/capacity evidence, or weak vertical refuge margin
- may remain useful for review when official shelters are distant, but should not be promoted as a strong candidate

`unknown`:

- insufficient evidence to classify
- must not be treated as a recommended candidate

`not_recommended`:

- low-rise or insufficient vertical refuge margin
- known seismic concern or unsuitable structure
- underground-only or inaccessible vertical refuge
- route/access evidence suggests practical emergency use is not plausible
- conflicting evidence indicates candidate use would be misleading

## Source Plan

Future source collection is planned in:

- `data_pipeline/qualification/highrise_humanitarian_candidate_sources_plan.json`

Planned source families:

- PLATEAU building geometry and attributes for height, floors, use, area, and building identity
- existing P5-B/P5-C official shelter and qualified building outputs for designated facilities
- Chuo/Tokyo official disaster prevention, evacuation facility, hazard map, and agreement information
- public seismic diagnosis, retrofit, or construction-era information where available
- public building height, floor, use, capacity, or registry-like information where available
- OSM or approved routing data for prototype route estimates only
- manual review lists for access, management agreement, and conflict resolution

P5-F does not download large raw GIS files, perform broad scraping, or change runtime Unity data.

## Validation

Focused pytest coverage should verify:

- the JSON Schema validates the controlled sample
- official and humanitarian statuses remain separate
- non-official candidates are not labeled official
- `publicAccessStatus = "unknown"` does not automatically exclude a humanitarian candidate
- unknown public access or unknown seismic evidence triggers `manualReviewNeeded = true`
- uncertainty is captured with machine-readable `reviewRisks`
- non-official humanitarian candidates carry warnings

## Boundaries

P5-F must not:

- claim a non-official high-rise is an official shelter
- treat life-first emergency access as a legal, owner, or official public access guarantee
- modify `Assets/Scenes/Chuo_BaseMap.unity`
- modify raw or imported PLATEAU data
- modify Unity gameplay scripts
- change `sourceMode` default
- download large GIS datasets
- aggressively scrape websites
- commit raw/cache/download/tmp/.venv/large data

## Known Limitations

- No real Chuo high-rise screening is completed in P5-F.
- Seismic, access, and management agreement evidence is only modeled, not collected.
- Public access, management agreement, and seismic evidence may require manual review before a record can support any operational decision.
- Capacity is a proxy and does not represent certified intake capacity.
- Route distance/time fields are placeholders until a reviewed route pipeline is approved for this use case.
- Humanitarian candidate status is not permission, legal advice, or an official evacuation designation.
