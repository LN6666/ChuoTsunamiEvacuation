# Data Schema

## Purpose

This document defines the data structures used by the ChuoTsunamiEvacuation project.

The first playable prototype uses manually placed test objects in Unity.

Milestone 2-01 moved tsunami event timing, test shelter rules, anti-camping settings, and result metrics into small JSON config/data files.

Milestone 2-04 expanded the test shelter data into a multi-shelter decision fixture while keeping it separate from P3 real-data outputs.

Milestone 2-05 added run-log export fields and a P4 source-mode hook while keeping active gameplay on test data.

## Data Folder

Unity project data folder:

Assets/Data/

External PLATEAU and GIS data folder:

D:\PLATEAU_DATA\

Raw PLATEAU data should not be committed to GitHub.

---

## Shelter Building Data

Suggested file:

Assets/Data/chuo_shelters.csv

Milestone 2 test file:

Assets/Data/test_shelters.json

### Fields

| Field | Type | Description |
|---|---|---|
| id | string | Unique shelter ID |
| name | string | Building or shelter name |
| address | string | Address |
| latitude | float | Latitude in WGS84 |
| longitude | float | Longitude in WGS84 |
| is_official_shelter | bool | Whether this is an official shelter |
| disaster_type | string | Supported disaster type, such as earthquake or tsunami |
| shelter_rank | string | Game rank: S, A, B, C, D |
| can_enter | bool | Whether the player can enter |
| post_earthquake_status | string | usable, damaged, blocked, unknown |
| climb_time_seconds | float | Time required to reach a safe floor |
| capacity | int | Optional capacity value |
| failure_reason | string | Reason shown when shelter cannot be used |
| note | string | Additional notes |

Milestone 2-01 supports a small test shelter config first. Full Chuo City shelter integration remains a later milestone.

### test_shelters.json Fields

| Field | Type | Description |
|---|---|---|
| shelterId | string | ID used to bind scene shelter objects to JSON data |
| shelterName | string | Display name |
| shelterRank | string | Game rank: S, A, B, C, D |
| isOfficialShelter | bool | Whether this is an official shelter |
| canEnter | bool | Whether the player can enter |
| entryDelaySeconds | float | Delay before/while entering shelter |
| climbTimeSeconds | float | Time required to reach a safe floor |
| crowdingDelaySeconds | float | Extra delay caused by crowding |
| failureReason | string | Reason shown when entry is blocked |
| sourceType | string | Current P2 records use "test"; later real records can use a real-data source label |
| facilityType | string | Debug or real facility type, such as debug_shelter |
| layoutPosition | object | Debug platform placement with x, y, and z numeric components |
| realFacilityName | string | Future real facility name, empty for current test data |
| address | string | Future address field, empty for current test data |
| latitude | float | Future WGS84 latitude, 0 for current test data |
| longitude | float | Future WGS84 longitude, 0 for current test data |
| coordinateSystem | string | Current P2 value is debug_platform |
| plateauBuildingId | string | Future PLATEAU building match ID, empty for current test data |
| safeFloor | int | Simplified safe-floor hint for future real-data integration |
| capacity | int | Optional capacity hint |
| dataSource | string | Source label or fixture note |
| sourceUrl | string | Source URL, empty for current test data |
| sourceUpdatedAt | string | Source update date, empty for current test data |
| notes | string | Fixture or data notes |

If a shelterId is missing or unknown, the loader preserves existing scene/Inspector values and logs a warning.

### Current P2-04 Test Shelter Records

The current debug fixture contains:

- test_shelter_001: Near Official Shelter
- test_shelter_far_fast: Far Fast Shelter
- test_shelter_crowded_candidate: Crowded Candidate Shelter
- test_shelter_slow_safe: Slow Safe Shelter
- test_shelter_blocked: Blocked Test Shelter

These records are intentionally not real Chuo facility data. They are decision-test fixtures for the isolated debug platform.

### layoutPosition

layoutPosition drives only debug platform placement.

Example:

```json
"layoutPosition": {
  "x": 8,
  "y": 0,
  "z": -8
}
```

P3/P4 real shelter data should not rely on this debug placement field as a real coordinate system.

### Shelter Rank

S:
Official shelter. Highest reliability.

A:
Modern mid-rise or high-rise building likely usable after an earthquake.

B:
Potential shelter with incomplete information.

C:
Lower reliability or uncertain post-earthquake usability.

D:
Low-rise, wooden, underground, damaged, or unsuitable building.

---

## Risk Zone Data

Suggested file:

Assets/Data/chuo_risk_zones.json

### Fields

| Field | Type | Description |
|---|---|---|
| id | string | Unique risk zone ID |
| name | string | Risk zone name |
| area_group | string | Example: Harumi, Kachidoki, Tsukishima |
| activation_time_seconds | float | Time when the zone becomes dangerous |
| risk_level | int | Risk level from 1 to 5 |
| failure_on_enter | bool | Whether entering this zone causes failure |
| warning_message | string | Message shown to player |
| note | string | Additional notes |

---

## Spawn Point Data

Suggested file:

Assets/Data/chuo_spawn_points.csv

### Fields

| Field | Type | Description |
|---|---|---|
| id | string | Unique spawn point ID |
| name | string | Spawn point name |
| latitude | float | Latitude |
| longitude | float | Longitude |
| area_name | string | Area name |
| difficulty | string | easy, normal, hard |
| note | string | Additional notes |

---

## Tsunami Boundary Data

Suggested file:

Assets/Data/tsunami_boundary_config.json

### Fields

| Field | Type | Description |
|---|---|---|
| id | string | Boundary ID |
| name | string | Boundary name |
| start_position | Vector3 | Unity world start position |
| end_position | Vector3 | Unity world end position |
| duration_seconds | float | Time required for the wall to move |
| color | string | Visual color |
| alpha | float | Transparency |
| failure_on_contact | bool | Whether contact causes failure |

---

## Tsunami Event Config

Implemented file:

Assets/Data/tsunami_event_config.json

### Fields

| Field | Type | Description |
|---|---|---|
| manualStartEnabled | bool | Whether manual T debug start is enabled |
| randomStartEnabled | bool | Whether random warning timing is enabled |
| randomStartMinSeconds | float | Minimum random warning delay |
| randomStartMaxSeconds | float | Maximum random warning delay |
| evacuationCountdownSeconds | float | Evacuation countdown duration after warning starts |
| wallMoveDurationSeconds | float | Time required for the visual wall/front to move |
| warningMessage | string | UI message shown when warning starts |

Default behavior:
- manualStartEnabled is true
- randomStartEnabled is false
- countdown starts only after warning

If tsunami_event_config.json is missing or invalid, gameplay uses hard-coded safe defaults and logs a warning.

---

## Anti-Camping Config

Implemented file:

Assets/Data/anti_camping_config.json

### Fields

| Field | Type | Description |
|---|---|---|
| antiCampingEnabled | bool | Whether anti-camping rules are active |
| preWarningCampingThresholdSeconds | float | Time allowed near the same shelter before warning |
| blockCampedShelterForRound | bool | Whether a camped shelter is blocked after warning starts |

antiCampingEnabled is false by default.

---

## Scenario Presets

Implemented file:

Assets/Data/scenario_presets.json

### Fields

| Field | Type | Description |
|---|---|---|
| activeScenarioId | string | Active scenario for Editor-stage testing; should be "default" before commit unless intentionally testing another scenario |
| presets | array | Named scenario presets |
| scenarioId | string | Scenario identifier |
| displayName | string | Human-readable scenario name |
| description | string | Scenario purpose |
| overrideTsunamiEventConfig | bool | Whether the scenario overrides tsunami timing in memory |
| tsunamiEventConfig | object | Optional in-memory tsunami config override |
| overrideAntiCampingConfig | bool | Whether the scenario overrides anti-camping config in memory |
| antiCampingConfig | object | Optional in-memory anti-camping config override |
| shelterOverrides | array | Optional in-memory shelter data overrides by shelterId |

Scenario overrides are applied at runtime only. They must not rewrite base JSON config files.

Current P2-04 scenarios:

- default
- normal_success
- random_warning
- blocked_shelter
- anti_camping
- late_failure

Unknown scenario IDs fall back to default behavior with a warning.

Unknown shelter IDs in scenario overrides are ignored with a warning and do not corrupt in-scene shelter values.

---

## Shelter Source Config

Implemented file:

Assets/Data/shelter_source_config.json

Purpose:

Provide a small Unity-side hook for future P4 real-data integration without changing current P2 gameplay.

| Field | Type | Description |
|---|---|---|
| sourceMode | string | Current default is "test"; future option may be "real_sample" |
| realSamplePath | string | Future path for an active real-data sample under Assets/Data |
| enableRealSampleLoading | bool | Reserved hook flag; false by default |
| notes | string | Human-readable scope note |

P2-05 always keeps sourceMode as "test" by default and continues to load test_shelters.json.

Example-only file:

Assets/Data/real_chuo_shelters_sample.example.json

This file is a P4 stub and is not active gameplay data.

---

## Run Log Export

Implemented output directory:

run_logs/

run_logs/ is ignored by Git.

ResultExportService writes:

- run_logs/run_results.csv
- run_logs/run_<runId>.json

### Export Fields

| Field | Type | Description |
|---|---|---|
| runId | string | Unique run identifier generated when missing |
| timestamp | string | UTC ISO timestamp generated when missing |
| scenarioId | string | Active scenario ID |
| scenarioName | string | Active scenario display name |
| success | bool | Whether the run succeeded |
| outcome | string | success or failure |
| outcomeReason | string | Result reason used for review |
| failureReason | string | Failure reason when outcome is failure |
| successReason | string | Success reason when outcome is success |
| selectedShelterId | string | Selected shelter ID |
| selectedShelterName | string | Selected shelter display name |
| shelterRank | string | Selected shelter rank |
| isOfficialShelter | bool | Whether selected shelter is official |
| entryDelaySeconds | float | Entry delay used in the run |
| climbTimeSeconds | float | Climb time used in the run |
| crowdingDelaySeconds | float | Crowding delay used in the run |
| evacuationCountdownSeconds | float | Countdown setting for the run |
| warningStartTime | float | Unity time when warning started |
| shelterEntryTime | float | Unity time when shelter entry was selected |
| climbStartTime | float | Unity time when climb started |
| climbCompleteTime | float | Unity time when climb completed |
| tsunamiArrivalTime | float | Unity time when risk reached player or active shelter |
| resultTime | float | Unity time when result finalized |
| wasCampingDetected | bool | Whether pre-warning camping was detected |
| wasShelterBlockedByCampingRule | bool | Whether selected shelter was blocked by anti-camping |
| advice | string | Short next-step advice shown in result review and export |

If a runtime value is unavailable, export uses an empty string, false, or 0 instead of throwing.

---

## Result Metrics Data

Suggested runtime/export fields:

| Field | Type | Description |
|---|---|---|
| outcome | string | success or failure |
| selected_shelter_id | string | Shelter selected by the player |
| selected_shelter_name | string | Shelter display name |
| elapsed_seconds | float | Time from warning start to result |
| warning_started_at_seconds | float | Runtime timestamp for warning start |
| climb_started_at_seconds | float | Runtime timestamp for climb start |
| climb_completed_at_seconds | float | Runtime timestamp for climb completion |
| failure_reason | string | Failure reason shown to player |
| reached_by_risk | bool | Whether player was reached by tsunami risk |
| active_shelter_reached_by_risk | bool | Whether active shelter entrance was reached during climb |
| entry_delay_seconds | float | Shelter entry delay used for result explanation |
| climb_time_seconds | float | Shelter climb time used for result explanation |
| crowding_delay_seconds | float | Crowding delay used for result explanation |
| was_camping_detected | bool | Whether pre-warning camping was detected |
| was_shelter_blocked_by_camping_rule | bool | Whether shelter was blocked by anti-camping config |
| run_id | string | Run identifier used for CSV/JSON export |
| timestamp | string | UTC export timestamp |
| scenario_id | string | Active scenario ID |
| scenario_name | string | Active scenario display name |
| advice | string | Short next-step decision advice |

Result metrics should be collected by gameplay/result systems, not by UI text components.

---

## Coordinate Policy

The first playable version may use manually placed Unity objects.

Later versions should support:

- WGS84 latitude / longitude
- conversion to Unity world coordinates
- point-in-polygon checks for boundary logic
- mapping shelter points to nearby buildings

Coordinate conversion should be isolated in a dedicated utility class.

---

## Data Loading Policy

DataLoader scripts should:

- validate required fields
- log missing or invalid rows
- avoid crashing on one bad record
- expose clear error messages
- keep data parsing separate from gameplay logic

Gameplay scripts should not parse CSV / JSON directly.

Current Milestone 2-01 runtime loading uses Application.dataPath + "/Data/..." for the Editor-stage prototype.

Before player builds, data loading should move to StreamingAssets or another build-safe loading path.

---

## Editor Validation Policy

Milestone 2 editor validation should check:

- required config files exist
- required fields are present
- numeric values are non-negative where appropriate
- random warning min is less than or equal to max
- shelter IDs are unique
- referenced shelter IDs and risk IDs are valid
- manual debug key is configured when manual start is enabled

Validation should log clear errors and warnings without modifying PLATEAU imported files, raw PLATEAU data, or Unity scene files unless the user explicitly runs a setup tool.
