# P8-A Hazard Variable Definitions

## Scope

These definitions separate evidence-bearing hazard data from cinematic visualization settings. P8-A validates the data model only and does not implement the light curtain or infrastructure interactions.

## Variables

| Field | Layer | Type | Definition |
|---|---|---|---|
| arrivalTimeSeconds | Science | number >= 0 | Time after `timeOriginSeconds` when the hazard arrival front reaches the feature or boundary. |
| inundationDepthMeters | Science | number >= 0 | Water depth above local ground surface. This is not tsunami wave height. |
| waterLevelMeters | Science | number | Water surface elevation in the source vertical reference. Requires source-specific datum notes before official use. |
| tsunamiHeightMeters | Science | number >= 0 | Source-defined tsunami height, usually relative to a reference sea level or observation point. It is not the same as inundation depth unless the source says so. |
| hazardIntensity | Science | number 0..1 | Normalized gameplay/data intensity derived from evidence or sample assumptions. It is not an official intensity scale unless a reviewed source defines it. |
| inundationBoundary | Science geometry | array of points | Data boundary for inundated or hazard-affected area. It may be an evidence boundary or a prototype boundary. |
| confidence | Science metadata | number 0..1 | Project confidence in the feature value. Manual samples should remain low. |
| evidenceSourceId | Evidence metadata | string | Registry ID linking the feature to reviewed or planned evidence. Required for manual sample and evidence-planned records. |
| geometryType | Science metadata | enum | `grid`, `polygon`, `polyline`, `point`, or `synthetic`. |
| sourceMode | Evidence metadata | enum | P8-A accepts `test`, `manual_sample`, and `evidence_planned`. Unknown or official-looking modes fail safe. |
| boundaryIsEvidenceBasedOrPrototype | Science metadata | enum | `evidence_based` when reviewed source geometry supports the boundary; `prototype` for sample/manual boundaries. |

## Depth, Height, and Visual Height

`inundationDepthMeters` is local water depth above ground.

`tsunamiHeightMeters` is a source-specific physical tsunami height and needs source/datum interpretation.

`visualHeightMeters` is a cinematic visualization value. It may be much larger than physical hazard values for readability. Large values require `visualHeightIsCinematicOnly=true` and must never be described as physical tsunami height.

## Boundary Terms

Arrival front:
The data or model position where arrival time changes across space.

Visual risk front:
The future P8-B rendered front used for gameplay readability. It may be stylized or wavy and does not need to match a physical water edge exactly.

Data boundary:
The science/evidence or prototype geometry in `inundationBoundary`.

Visual curtain boundary:
The future rendered light-curtain shape. It may derive from the data boundary but remains a visual layer.

## Source Modes

`test`:
Unit-test or validator fixture data.

`manual_sample`:
Development sample data. It must be marked non-authoritative and linked to a manual placeholder source.

`evidence_planned`:
A record prepared for later source review. It must not be treated as official or reviewed.

Any other source mode:
Fail safe in P8-A.
