# P8-A Evidence Source Registry

## Purpose

This registry defines the evidence source categories and review fields that P8 hazard data must use before any value is treated as evidence-backed.

P8-A does not attach or review official tsunami values. The committed sample data is manual placeholder data for schema, loader, and validation tests only.

## Claim Policy

- Do not claim `arrivalTimeSeconds`, `inundationDepthMeters`, `waterLevelMeters`, `tsunamiHeightMeters`, `hazardIntensity`, or `inundationBoundary` values are official unless the source is attached, reviewed, and recorded here.
- Manual sample and `evidence_planned` records must carry `evidenceSourceId`, `sourceMode`, low or provisional `confidence`, and notes that state they are not official claims.
- OSM may be used only for route/context support where attribution and license handling are documented. It must not become an official hazard source.
- Academic papers may inform model choices or scenario ranges only after the assumptions, domain, and limits are recorded.

## Registry Fields

| Field | Required | Description |
|---|---:|---|
| evidenceSourceId | Yes | Stable identifier used by JSON hazard features and configs. |
| sourceCategory | Yes | One of the source categories below. |
| sourceMode | Yes | `manual_sample`, `evidence_planned`, or another future approved mode. P8-A accepts no official runtime mode. |
| title | Yes | Human-readable source title. |
| ownerOrPublisher | Yes for non-manual sources | Agency, local government, academic publisher, or data owner. |
| urlOrLocalPath | Yes when attached | URL, local reviewed path, or citation locator. |
| reviewedStatus | Yes | `not_attached`, `attached_unreviewed`, `reviewed_for_planning`, or `reviewed_for_values`. |
| variablesSupported | Yes | Which hazard variables the source can support. |
| geometryType | Yes when spatial | `grid`, `polygon`, `polyline`, `point`, or `synthetic`. |
| coordinateReference | Yes when spatial | CRS, datum, and transform notes. |
| units | Yes when numeric | Units exactly as published and converted units used by Unity JSON. |
| licenseOrAttribution | Yes | License, attribution, or usage constraints. |
| reviewedBy | Yes when reviewed | Human or agent reviewer and review date. |
| limitations | Yes | Known uncertainty, coverage, age, scale, and applicability limits. |

## Source Categories

### Official Tsunami/Inundation Maps

Includes official inundation area, depth, water level, arrival-time, or evacuation-planning maps from national, prefectural, or municipal sources.

P8-A status: planned category only. No official values are attached or claimed.

### Tokyo/Chuo Hazard Maps

Includes Tokyo Metropolitan Government and Chuo City hazard maps or disaster-prevention publications relevant to tsunami, storm surge, river flooding, or evacuation context.

P8-A status: planned category only. No values are attached or claimed.

### Cabinet Office / MLIT / Local Government Data

Includes Cabinet Office disaster planning material, MLIT or GSI geospatial layers, and local government disaster datasets.

P8-A status: planned category only. Use requires explicit review of date, scope, license, variables, and CRS before value claims.

### Academic Tsunami Simulation Papers

Includes peer-reviewed or institutional tsunami simulation papers. These may support modeling assumptions, uncertainty framing, or comparative ranges.

P8-A status: planning references only. Academic values must not be copied into the sample data without citation, scope review, and explicit notes that they are not official local hazard designations unless the paper itself is the accepted authority for that use.

### PLATEAU / CityGML Category Sources

Includes PLATEAU CityGML category metadata such as buildings, roads, bridges, underground structures, terrain, water, and disaster-risk related layers.

P8-A status: PLATEAU is context geometry and category evidence. It does not by itself define tsunami arrival time, water depth, water level, or tsunami height.

### OSM or Route Context

Includes OpenStreetMap or derived routing context where attribution is handled and the data is used only for route or urban-context support.

P8-A status: route/context only. OSM must not be treated as official hazard evidence.

### Manual Sample Data

Includes hand-authored placeholder data for schema, validation, and Unity test coverage.

P8-A status: active sample category. Values are non-authoritative and must remain clearly marked as manual sample or prototype data.

## P8-A Registry Entries

| evidenceSourceId | sourceCategory | sourceMode | title | reviewedStatus | variablesSupported | Limit |
|---|---|---|---|---|---|---|
| p8a_manual_placeholder | manual_sample | manual_sample | P8-A manual sample hazard placeholder | reviewed_for_planning | All schema fields for loader tests | Not official, not evidence-backed, and not suitable for hazard claims. |

## Future Review Gate

Before a future P8 stage upgrades any hazard value from placeholder to evidence-backed:

- add or update a registry entry,
- attach or cite the source,
- record variable semantics and units,
- record CRS and transformation notes,
- record license/attribution,
- set confidence according to review quality,
- link every feature to `evidenceSourceId`,
- keep visual-only height separate from science-layer values.
