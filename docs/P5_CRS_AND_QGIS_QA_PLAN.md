# P5 CRS And QGIS QA Plan

## Purpose

P5-B2 defines the CRS and manual QGIS QA strategy needed before controlled real Chuo data ingestion and spatial processing begin.

CRS mistakes can make shelter points, PLATEAU building footprints, hazard layers, and route lines appear aligned while producing wrong distances or mismatched buildings. P5-B must make CRS handling explicit before real spatial decisions are trusted.

## Expected Source CRS Possibilities

- Official shelter/facility point tables may use EPSG:4326 latitude/longitude.
- OSM sources normally use WGS84 longitude/latitude coordinates.
- PLATEAU and CityGML-derived data may use source-specific coordinate conventions that must be verified before processing.
- Official hazard/disaster GIS layers may use a local or national projected CRS.
- Metric distance calculations should use a local projected CRS suitable for Tokyo/Chuo, not raw latitude/longitude.

## Target CRS Strategy

- Record CRS metadata in every processed output.
- Treat EPSG:4326 as interchange/source coordinate metadata, not a metric distance CRS.
- All metric spatial operations must use a projected CRS suitable for Tokyo/Chuo, such as a reviewed Japan Plane Rectangular CRS candidate.
- Never compute meter distances directly in unprojected latitude/longitude.
- Convert source layers into the agreed processing CRS before contains, nearest, buffer, route length, or distance-threshold logic.
- Preserve original source coordinates when useful for traceability.

## Unity Coordinate Caution

GIS CRS to Unity `Vector3` mapping is a separate P5-C concern.

Unity should consume processed outputs with explicit coordinates, IDs, status fields, confidence, and warnings. Unity should not perform raw CRS operations, parse raw GIS archives, or read raw PLATEAU/CityGML data at runtime.

## QGIS QA Role

QGIS is a manual visual QA tool, not a runtime dependency and not the only source of truth.

Planned QA layers:

- shelter points
- candidate/qualified building footprints
- building match lines
- route lines
- unmatched/low-confidence points
- hazard context layers

Manual checks:

- no visible coordinate shift between layers
- routes are not flying, disconnected, or crossing impossible gaps in obvious ways
- shelter points are near or inside matched buildings
- nearest-match records are visually plausible
- unmatched and low-confidence points are easy to inspect
- hazard context layers align with known basemap/reference layers
- source CRS and processed CRS are visible in layer metadata

## QGIS Output Policy

QGIS outputs should remain small QA exports, screenshots, or notes. Do not commit raw large GIS files, QGIS caches, full source archives, full CityGML, or full OSM extracts.

Useful future QA outputs may include small GeoJSON layers, CSV review tables, PNG screenshots, or Markdown QA notes, only when explicitly approved by the milestone.

## P5-B4 QA Layer Status

P5-B4 created real processed QGIS QA layers from official Chuo/Tokyo/GSI shelter evidence and a limited local PLATEAU building mesh subset.

CRS choices:

- source/interchange CRS: `EPSG:4326`
- metric processing CRS: `EPSG:6677`

Generated QA layers:

- `data_pipeline/processed/qgis_qa/real_chuo_shelter_points.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_building_footprints.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_match_lines.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_low_confidence_or_unmatched.geojson`

Manual B4 QA checklist:

- verify shelter points align with expected Chuo locations
- verify matched building footprints are plausible for `contains` records
- inspect the 3 `nearest` records before treating them as accepted matches
- inspect the 4 broad evacuation-area `unmatched` records and confirm they should not be assigned to a single building automatically
- verify no visible coordinate shift between shelter points, building footprints, and match lines
- keep route QA deferred until P5-B5, because B4 route fields are `not_evaluated`

After B4, the QGIS QA layers were loaded with an OpenStreetMap basemap. Shelter points, building footprints, and match lines aligned normally, and the user confirmed that the shelter/building matching had no obvious issue. This cleared B5 to proceed with route QA outputs.

## P5-B5 Route QA Layer Status

P5-B5 created processed route QA layers:

- `data_pipeline/processed/qgis_qa/real_chuo_route_lines.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_route_origins.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_route_failures.geojson`

CRS choices:

- source/interchange CRS: `EPSG:4326`
- OSM route snapping CRS: `EPSG:6677`

Manual B5 QA checklist:

- load route origins, route lines, B4 shelter points, and B4 matched building footprints with an OpenStreetMap basemap
- verify route origins are located in the intended Ginza, Nihonbashi, Hatchobori/Tsukiji, Tsukishima/Kachidoki, and Harumi waterfront areas
- verify route lines follow plausible street/bridge paths and do not visibly fly away
- verify route endpoints terminate near expected shelter/building targets
- inspect the route failures layer; B5 generated zero failed routes
- keep route labels clear that routes are estimated OSM pedestrian routes, not official evacuation routes

## P5-B4/B5 QA Completion Note

P5-B4 and P5-B5 manual QGIS QA were completed with an OpenStreetMap basemap.

Observed result:

- B4 shelter points, matched building footprints, match lines, and low-confidence/unmatched records loaded in the Tokyo Chuo Ward context.
- B4 matching had no obvious CRS offset or severe shelter/building mismatch on user inspection.
- B5 route origins and route lines loaded in the Chuo Ward map context.
- B5 routes visually followed the street network and did not show obvious far-away coordinate errors, origin collapse, direct flying lines, or severe CRS shift.

This is sufficient for DeepSeek P5-B review and P5-C planning. Detailed spot checks should still be repeated before final publication, presentation screenshots, or user-facing Unity use.

## Risks

- Axis-order mistakes can swap latitude and longitude.
- Geographic CRS distances can silently produce wrong meter thresholds.
- PLATEAU-derived building footprints may not share the same CRS as source points.
- QGIS visual alignment can hide data-field or source-provenance errors.
- Unity coordinate conversion can introduce a second alignment problem if handled before processed GIS outputs are stable.

## Next Action

P5-B3 should define the exact processing CRS candidate, verify it with known Chuo reference points, and add CRS metadata checks before controlled real-source ingestion output is accepted.
