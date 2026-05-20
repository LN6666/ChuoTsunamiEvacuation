# P5-D Real Qualified Shelter Gameplay

## Status

P5-D adds an opt-in real qualified gameplay mode on top of the copied P5-B/P5-C static outputs.

The committed/default source mode remains:

```json
"sourceMode": "test"
```

To manually enable P5-D in the Unity Editor, temporarily set:

```json
"sourceMode": "real_qualified"
```

in `Assets/Data/shelter_source_config.json`, then restore `test` before committing unless the milestone explicitly asks otherwise.

## Runtime Data Boundary

`real_qualified` reads only copied static JSON files under `Assets/Data`:

- `real_chuo_integrated_route_qualification.json`
- `real_chuo_osm_routes_sample.json`
- `real_chuo_building_qualification.json`
- `real_chuo_shelter_building_matches.json`

Runtime does not read `data_pipeline/processed`, `data_pipeline/raw`, `data_pipeline/downloads`, `data_pipeline/cache`, `tmp`, `.venv`, raw PLATEAU data, OSM cache files, or live routing/web sources.

## Selectable Records

P5-D loads 31 integrated real Chuo records.

Playable/selectable by default:

- `official_confirmed`
- `official_confirmed_with_review`

Current selectable count: 27.

Debug/data-only in P5-D 1.0:

- `strong_candidate`
- `weak_candidate`
- `unknown`
- `not_qualified`

Current non-selectable count: 4 `unknown` records.

## Runtime Generation

When `sourceMode = real_qualified`, Unity creates runtime-only proxy targets:

- shelter marker
- entry trigger
- `BuildingShelter`
- `ShelterEntranceTrigger`
- `RealQualifiedShelterMetadata`
- compact/detailed `TextMesh` metadata label

Existing test shelter scene objects are disabled at runtime only while the real qualified proxies are active. No scene asset is saved or modified.

The proxy layout is deterministic debug placement, not true geospatial placement. The PLATEAU building id and qualification evidence are preserved as metadata, but the marker position is not a verified map coordinate.

## Gameplay Flow

P5-D reuses the existing gameplay path:

1. Player approaches a generated real qualified target.
2. Player presses `E` in the trigger.
3. Existing shelter validation, entry, and climb flow runs.
4. Existing success/failure ResultPanel appears.
5. P5-D feedback is appended to the result detail text.

Qualification status, route, and hazard metadata do not determine success/failure. Success still depends on existing entry/climb/timing/risk flow.

## Feedback Preserved

ResultPanel and metadata preserve:

- shelter name
- qualification status
- confidence
- manual review flag
- first warnings
- route distance/time
- `estimated prototype route, not an official evacuation route`
- OSM/ODbL attribution

OSM routes remain estimated prototype walking routes, not official evacuation routes.

## Route Preview

P5-D parses actual route geometry from `real_chuo_osm_routes_sample.json`.

Route rendering is allowed only when `P5DRoutePreviewTransformValidator` can prove the route geometry is in a safe Unity debug coordinate system and its endpoints match the expected preview origin/target.

Current P5-B/P5-C route geometry is WGS84 (`EPSG:4326`). Because there is no verified WGS84-to-Unity/PLATEAU transform, P5-D does not render route lines for current data. Distance/time feedback remains visible.

This avoids drawing misleading route lines.

## Automated Validation

Use GUI/headful automated Unity tests in this cloud Administrator environment:

```powershell
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode EditMode -LaunchMode Gui
powershell -ExecutionPolicy Bypass -File tools/run_unity_tests.ps1 -Mode PlayMode -LaunchMode Gui
```

Expected XML outputs:

- `test-results/editmode-results.xml`
- `test-results/playmode-results.xml`

Manual Test Runner clicking is not required.

## Manual real_qualified Smoke Checklist

Before public/demo use, run one manual `real_qualified` gameplay pass in Unity:

1. Temporarily set `Assets/Data/shelter_source_config.json` `sourceMode` to `real_qualified`.
2. Start the game.
3. Approach a selectable real shelter proxy.
4. Press `E`.
5. Complete the stair climb and result flow.
6. Confirm ResultPanel/metadata shows qualification, warnings, route distance/time, the estimated prototype route note, and OSM/ODbL attribution.
7. Confirm candidate/unknown buildings are not playable targets.
8. Restore `sourceMode` to `test` before committing unless a milestone explicitly changes the default.

Latest validation:

- EditMode GUI automated run: 105 passed, 0 failed
- PlayMode GUI automated run: 13 passed, 0 failed

## B-Level Follow-Ups

- WGS84 route transform must be verified before route line rendering; current WGS84 routes remain non-rendered by default.
- Manual one-run `real_qualified` gameplay smoke remains recommended before public/demo use.
- GUI/headful automated testing is the current approved fallback in this cloud Administrator environment.
- Replace reflection-based `ShelterEntranceTrigger` setup with a public setup API if this runtime generator becomes long-lived production code.
- Add an explicit manual validation pass in `Chuo_BaseMap.unity` after scene-safe review, without saving the scene.
- Improve player-facing UI layout if P5-D feedback becomes part of a polished classroom/demo build.
