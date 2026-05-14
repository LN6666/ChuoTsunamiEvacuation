# Data Schema

## Purpose

This document defines the data structures used by the ChuoTsunamiEvacuation project.

The first playable prototype uses manually placed test objects in Unity.

Milestone 2: Rules and Dataization 1.0 should move tsunami event timing, shelter rules, anti-camping settings, result metrics, and editor validation inputs into small CSV / JSON config files.

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

Milestone 2 should support a small test shelter config first. Full Chuo City shelter integration can remain a later milestone.

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

Suggested file:

Assets/Data/tsunami_event_config.json

### Fields

| Field | Type | Description |
|---|---|---|
| countdown_seconds | float | Evacuation countdown duration after warning starts |
| manual_start_key | string | Debug key, currently T |
| manual_start_enabled | bool | Whether manual debug start is enabled |
| random_warning_enabled | bool | Whether future random warning timing is enabled |
| random_warning_min_seconds | float | Minimum future random warning delay |
| random_warning_max_seconds | float | Maximum future random warning delay |
| warning_message | string | UI message shown when warning starts |

The current prototype should keep manual T start for debugging. Countdown must not decrease before the tsunami warning starts.

---

## Anti-Camping Config

Suggested file:

Assets/Data/anti_camping_config.json

### Fields

| Field | Type | Description |
|---|---|---|
| enabled | bool | Whether anti-camping rules are active |
| pre_warning_entrance_grace_seconds | float | Time allowed near a shelter before warning |
| reveal_shelter_status_after_warning | bool | Whether shelter usability is hidden before warning |
| randomize_shelter_availability | bool | Whether shelter availability may change after warning |
| camping_failure_reason | string | Result text for anti-camping failure or denial |

Milestone 2 may define and validate this config before fully enforcing all future anti-camping behavior.

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
