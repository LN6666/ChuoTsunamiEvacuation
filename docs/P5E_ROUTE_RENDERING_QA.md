# P5-E Route Rendering QA

## Status

P5-E strengthens route geometry parsing, transform validation, and real_qualified QA for the copied P5-B/P5-C static route data.

Current route rendering result:

- route geometry is loaded and validated as WGS84 `EPSG:4326`
- coordinates are GeoJSON-like `LineString` pairs in `[longitude, latitude]` order
- no verified WGS84-to-Unity/PLATEAU world-coordinate transform exists in the Unity runtime code
- selected route preview rendering remains safely disabled for current real data
- route distance/time, prototype-route labeling, and OSM/ODbL attribution remain visible as feedback

## Actual Route Geometry Format

Source file:

- `Assets/Data/real_chuo_osm_routes_sample.json`

Root fields:

- `datasetId`: `p5_b5_real_chuo_osm_routes_sample`
- `coordinateReferenceSystem`: `EPSG:4326`
- `metricCoordinateReferenceSystem`: `EPSG:6677`
- `walkingSpeedMetersPerSecond`: `1.2`
- `osmAttribution`: OpenStreetMap / Open Database License attribution
- `records`: 135 route records

Route record fields used by Unity:

- route id: `routeId`
- origin id: `originId`
- origin name: `originName`
- target shelter id: `shelterId`
- target PLATEAU building id: `plateauBuildingId`
- route availability: `routeAvailability`
- distance: `routeDistanceMeters`
- estimated time: `estimatedTravelTimeSeconds`
- route source/type: `routeSource`, `routeType`
- official-route flag: `isOfficialEvacuationRoute`
- geometry: `geometry.type = LineString`, `geometry.coordinates = [[longitude, latitude], ...]`

Dataset summary from Assets/Data inspection:

- records: 135
- origins: 5 origins x 27 target shelter/buildings
- available routes: 135
- failed routes: 0
- distance range: 97.727 m to 5939.296 m
- estimated time range: 81.439 s to 4949.413 s
- coordinate bounds: longitude 139.7613478 to 139.7925025, latitude 35.649057 to 35.6917845

## Parser Safety

`P5CStaticDataLoader` now keeps route geometry only when:

- the geometry object exists
- `geometry.type` is `LineString`
- at least two coordinate pairs are present
- all parsed coordinates are finite
- `EPSG:4326` coordinates are valid longitude/latitude bounds

Missing, malformed, unsupported, or swapped-order geometry fails closed by leaving `RouteSampleRecord.HasGeometry` false. The route record can still preserve distance/time feedback.

## Transform Validation

`P5DRoutePreviewTransformValidator` validates WGS84 route geometry before any line rendering decision:

- checks finite coordinates
- rejects latitude/longitude swapped order
- rejects coordinates outside broad Chuo WGS84 bounds
- estimates route bounding-box span in meters
- rejects collapsed or implausibly large spans
- rejects rendering because no verified WGS84-to-Unity/PLATEAU transform is configured

The validator still accepts only synthetic `UNITY_DEBUG` coordinates for line rendering in tests. Current real `EPSG:4326` route geometry renders zero route lines.

## Gameplay Safety

P5-E does not change success/failure rules:

- `sourceMode` default remains `test`
- `real_qualified` remains opt-in
- only `official_confirmed` and `official_confirmed_with_review` are playable in real_qualified mode
- candidate, unknown, and not-qualified records remain non-playable/debug-only
- route, qualification, and hazard metadata remain feedback only
- no live routing, web requests, flood simulation, NPC simulation, or official route guidance was added

## Automated Validation

Required GUI/headful commands:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Latest P5-E result:

- EditMode GUI/headful: 107 passed, 0 failed
- PlayMode GUI/headful: 13 passed, 0 failed

XML outputs:

- `test-results/editmode-results.xml`
- `test-results/playmode-results.xml`
