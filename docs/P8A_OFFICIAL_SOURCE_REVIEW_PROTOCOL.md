# P8-A Official Source Review Protocol

## Current P8-A Rule

P8-A must not fetch, download, scrape, or silently import live official data. It only prepares the evidence review structure.

No P8-A JSON value is official unless a reviewed source is attached and recorded in `docs/P8A_EVIDENCE_SOURCE_REGISTRY.md`.

## Review Steps

1. Identify the source owner, title, publication date, URL or local file path, license, and intended use.
2. Confirm whether the source is official, academic, contextual, or manual sample.
3. Record the hazard variables the source actually provides: arrival time, inundation depth, water level, tsunami height, boundary, hazard intensity, or confidence.
4. Record the exact published units and any conversion into Unity JSON fields.
5. Record spatial reference, geometry type, scale, resolution, and transformation assumptions.
6. Check whether the source covers Tokyo Chuo City and the specific P8 area of use.
7. Record uncertainty, limitations, and date sensitivity.
8. Assign or update `evidenceSourceId`.
9. Link every hazard feature that uses the source to that `evidenceSourceId`.
10. Keep science values separate from visual fields such as `visualHeightMeters`.

## Official Claim Gate

A value may be described as official only when all of these are true:

- the source is an official national, prefectural, municipal, or legally recognized public source for the value being used,
- the source is attached or otherwise reviewable by the project,
- the exact value, geometry, unit, and date are traceable,
- the registry entry is marked `reviewed_for_values`,
- the feature has a non-empty `evidenceSourceId`,
- the `sourceMode` is explicit and accepted by the validator for that stage,
- the documentation states the source limitations.

P8-A does not introduce an official runtime source mode. Unknown or official-looking source modes must fail safe until a later approved stage defines them.

## Source Category Checks

Official tsunami/inundation maps:
Review whether the map provides depth, area boundary, arrival time, water level, tsunami height, or only evacuation guidance. Do not infer missing variables.

Tokyo/Chuo hazard maps:
Review map legend, publication date, area coverage, assumptions, and whether the map represents tsunami, storm surge, river flood, or another hazard.

Cabinet Office / MLIT / local government data:
Review dataset scope, CRS, license, update date, and whether the layer is directly hazard-related or only contextual.

Academic tsunami simulation papers:
Review model domain, boundary conditions, resolution, scenario assumptions, and whether values can be localized to Chuo. Academic references are not automatically official local hazard designations.

PLATEAU / CityGML category sources:
Review category coverage and LOD. Treat PLATEAU as geometry/category context unless a specific disaster-risk layer is reviewed.

OSM or route context:
Use only where attribution is handled. Do not use OSM to assert tsunami hazard values.

Manual sample data:
Keep `sourceMode=manual_sample`, `confidence` low, and notes explicit that the data is non-authoritative.

## Review Output

Every completed review should update:

- `docs/P8A_EVIDENCE_SOURCE_REGISTRY.md`,
- relevant hazard JSON `evidenceSources`,
- relevant feature `evidenceSourceId`,
- confidence and limitations,
- validation/test fixtures when semantics change.
