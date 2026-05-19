# P5 Real Chuo OSM Routing Pipeline

## Purpose

P5-B5 adds prototype pedestrian routing to the P5-B4 real Chuo shelter/building qualification outputs.

The pipeline computes OSM walking route samples from controlled origin points to qualified real Chuo shelter/building targets, then writes route outputs, integrated qualification-plus-route outputs, and QGIS QA layers.

No Unity files are modified in B5.

## B4 Spatial QA Handoff

Before B5, the B4 QGIS QA layers were loaded with an OpenStreetMap basemap. Shelter points, building footprints, and match lines aligned normally, and the user confirmed that the shelter/building matching had no obvious issue.

This was sufficient to proceed with B5 routing samples.

## Origin Points

Controlled test origins:

- `data_pipeline/qualification/real_chuo_route_origin_points.json`

The five origins cover:

- Ginza/Yurakucho side
- Nihonbashi
- Hatchobori/Tsukiji
- Tsukishima/Kachidoki
- Harumi waterfront, included as a longer-distance test origin

These origins are controlled route test points only. They are not official evacuation origins and are not gameplay spawn points.

## OSM Source And Attribution

Routing uses OpenStreetMap network data through OSMnx.

Attribution note:

- Route network data is from OpenStreetMap contributors and is used under the Open Database License.

OSMnx cache/download files are local ignored working files and are not committed. Processed route outputs are committed for QA and future Unity-side loading after P5-C approval.

## Method

Builder:

- `data_pipeline/scripts/build_real_chuo_osm_routes.py`

Inputs:

- B5 controlled origin points
- `data_pipeline/processed/qualification/real_chuo_building_qualification.json`
- `data_pipeline/processed/qualification/real_chuo_official_shelters_normalized.json`

Target selection:

- `official_confirmed`
- `official_confirmed_with_review`
- `strong_candidate` if present

B5 found 27 route targets from B4.

Network settings:

- OSMnx network type: `walk`
- source CRS: `EPSG:4326`
- snapping CRS: `EPSG:6677`
- shortest path weight: OSM edge `length`
- walking speed assumption: `1.2 m/s` prototype constant

Routes are shortest-path estimates over the OSM walking network. They are not official evacuation routes, and the route results must not be presented as official evacuation guidance.

`walkingSpeedMetersPerSecond = 1.2` is a P5-B prototype assumption used only to estimate route times. It is not a final evacuation-behavior model and does not account for crowding, age, disability, stair movement, panic, flooding, debris, road closures, or tsunami timing.

## Outputs

Route outputs:

- `data_pipeline/processed/routes/real_chuo_osm_routes_sample.json`
- `data_pipeline/processed/routes/real_chuo_osm_routes_sample.csv`
- `data_pipeline/processed/routes/real_chuo_osm_routes_sample.geojson`

Integrated route/qualification outputs:

- `data_pipeline/processed/qualification/real_chuo_integrated_route_qualification.json`
- `data_pipeline/processed/qualification/real_chuo_integrated_route_qualification.csv`

QGIS route QA layers:

- `data_pipeline/processed/qgis_qa/real_chuo_route_lines.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_route_origins.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_route_failures.geojson`

`real_chuo_route_failures.geojson` is part of the output contract and must remain included even when it contains an empty FeatureCollection.

## Results

- origin points: 5
- target shelter/buildings: 27
- route records: 135
- available routes: 135
- failed routes: 0 for the current controlled 5-origin sample
- integrated qualification records: 31
- integrated records with available routes: 27
- integrated records left `not_evaluated`: 4 broad/unmatched B4 records

Route distance summary:

- minimum: 97.727 m
- mean: 2,288.2 m
- maximum: 5,939.296 m

Estimated travel time summary:

- minimum: 81.439 s
- mean: 1,906.9 s
- maximum: 4,949.413 s

## Validation

Commands run:

- `data_pipeline/.venv/Scripts/python.exe data_pipeline/scripts/build_real_chuo_osm_routes.py`
- `data_pipeline/.venv/Scripts/python.exe data_pipeline/scripts/build_real_chuo_integrated_route_qualification.py`
- `data_pipeline/.venv/Scripts/python.exe -m pytest data_pipeline/tests/test_real_chuo_osm_routing.py`

Result:

- pytest passed: 6 tests

## QGIS QA Steps

Load these layers with an OpenStreetMap basemap:

- B4 shelter points
- B4 matched building footprints
- B5 route origins
- B5 route lines
- B5 route failures

Manual checks:

- origin points are in expected Chuo-area locations
- route lines follow streets/bridges and do not visibly fly away
- route endpoints are near target shelter/building points
- route lengths are plausible for the origin/target pairs
- failure layer is empty or explicitly reviewed

## Final QGIS Route QA Result

The B5 route origin and route line layers were loaded successfully in QGIS with an OpenStreetMap basemap.

The route origins and route lines remained within the Chuo Ward map context. Routes visually followed the street network and did not appear as simple direct flying lines. No obvious far-away coordinate error, origin collapse, or severe CRS shift was observed. The user manually confirmed that route placement showed no obvious issue.

Detailed route spot checks should still be repeated before publication, presentation screenshots, or any user-facing Unity interpretation.

## Limitations

- Routes are estimated prototype pedestrian routes.
- Routes are not official evacuation routes.
- The current 0-failure result applies only to the controlled 5-origin sample.
- Future random, sparse, or waterfront origins may fail because of OSM network gaps, disconnected pedestrian components, tagging gaps, or snapping failures.
- No disaster road closure is modeled.
- No flood or tsunami simulation is modeled.
- OSM completeness and pedestrian access tagging require review.
- Walking speed is a prototype time-estimation constant, not final evacuation behavior.
- P5-B5 does not integrate with Unity or change gameplay rules.

## Next Step

P5-C should load the processed integrated qualification/route outputs into Unity as read-only data, display route/qualification confidence and warnings, and preserve `sourceMode = test` until a later milestone explicitly changes runtime behavior.
