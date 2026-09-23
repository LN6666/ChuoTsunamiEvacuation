# NewMap Coordinate Transform Validation

Generated: 2026-05-27T03:13:20+09:00

Status: `blocked_missing_coordinate_transform`

Official shelter anchors are recovered by exact PLATEAU GML scene object names. That proves object identity for those shelters, but it does not prove a general WGS84 latitude/longitude to Unity world-position transform.

## Findings

- Official point CRS: `EPSG:4326`
- Route geometry CRS: `EPSG:4326`
- Route metric CRS: `EPSG:6677`
- Scene audit renderer count: 18596
- Scene audit collider count: 18596
- Stored map bounds extents: not available in the current audit
- WGS84-to-Unity transform: not proven

## Decision

Route geometry is not validated on the new map. Local route guidance remains prototype guidance only and no official route claim is made.
