# Data Schema

## Purpose

This document defines the data structures used by the ChuoTsunamiEvacuation project.

The first version may use manually placed test objects in Unity.
Later versions should load shelter, risk zone, spawn point, and boundary data from CSV / JSON / GeoJSON files.

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
