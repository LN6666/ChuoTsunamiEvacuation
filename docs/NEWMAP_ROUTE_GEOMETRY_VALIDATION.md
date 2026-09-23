# NewMap Route Geometry Validation

Generated: 2026-05-27T03:13:20+09:00

Route geometry validation status: `blocked_transform_unknown`

## Result

- P5 OSM route records checked: 135
- Finite WGS84 route records: 135
- Routes validated on `Chuo_BaseMap`: 0
- Routes blocked by missing transform: 135
- Invalid geometry records: 0
- Official route claims: 0

The old OSM route lines remain EPSG:4326 prototype route geometry. Because no verified WGS84-to-Unity transform exists, no old route line is spawned on the new map and no official evacuation route is claimed.
