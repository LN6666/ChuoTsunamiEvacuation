# Real Shelter Data Schema

## Scope

`data_pipeline/schemas/real_shelter_schema.json` defines the Phase 3 processed shelter export contract.

The P3-00 sample records are synthetic placeholders only. They are not official Chuo Ward shelter records.

## Top-Level JSON Object

| Field | Type | Required | Description |
|---|---|---|---|
| `dataset_id` | string | yes | Stable dataset identifier. P3-00 uses `real_chuo_shelters_sample`. |
| `generated_at` | string date-time | yes | UTC export timestamp. |
| `source_manifest` | string | yes | Relative path to the source manifest used by the export. |
| `coordinate_reference_system` | string | yes | Must be `EPSG:4326`. |
| `records` | array | yes | Shelter records. Must contain at least one record. |

## Record Fields

| Field | Type | Required | Description |
|---|---|---|---|
| `id` | string | yes | Stable pipeline record ID. Future P4 may map this to Unity shelter IDs. |
| `name` | string | yes | Shelter or facility display name. |
| `type` | enum string | yes | One of `evacuation_shelter`, `temporary_shelter`, `tsunami_evacuation_building`, `welfare_shelter`, or `unknown`. |
| `latitude` | number | yes | WGS84 latitude, valid global range -90 to 90. |
| `longitude` | number | yes | WGS84 longitude, valid global range -180 to 180. |
| `address` | string | yes | Address or location text from source. |
| `capacity` | integer or null | yes | Non-negative capacity when known. |
| `floors_available` | integer or null | yes | Non-negative available floor count when known. |
| `elevation_m` | number or null | yes | Elevation in meters when known. |
| `source` | string | yes | Source name or source manifest label. |
| `source_url` | string or null | yes | Source URL when available. Local samples may use null. |
| `source_updated_at` | string or null | yes | Source update date when known. |
| `notes` | string | yes | Free-text notes, including sample/offical-data warnings. |
| `unity` | object | yes | Future Unity-facing hints. |

## Unity Sub-Object

| Field | Type | Required | Description |
|---|---|---|---|
| `prefab_hint` | string | yes | Future Unity marker/prefab hint. |
| `is_entry_enabled` | boolean | yes | Whether a future Unity consumer may treat the shelter as enterable by default. |
| `estimated_stair_floors` | integer or null | yes | Estimated stair floors for future climb timing. |

## Coordinate Policy

P3-00 exports coordinates in `EPSG:4326` latitude/longitude.

Unity world coordinates are not produced in P3-00. Future P4 integration must decide how to transform WGS84 records into Unity scene positions.

## Null Policy

Unknown numeric values should be represented as JSON `null`, not as `0`, unless zero is the actual value.

Unknown source URLs may be `null`. `source` must be a non-empty string.

## Validation Rules

The automated validator checks:

- JSON Schema compliance.
- non-empty `records`.
- unique `id` values.
- latitude and longitude global ranges through JSON Schema.
- sample Chuo/Tokyo sanity range: latitude 35.60 to 35.75, longitude 139.70 to 139.90.
- required Unity interface fields.
- source metadata fields.

Manual validation is still required for future official source authority, licensing, geocoding quality, and PLATEAU building matching.
