# P8-B Hazard Layer V1 Final Status

Date: 2026-05-24.

## Status

`Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json` is the P8-B official metropolitan tsunami hazard-layer v1 for Chuo.

Gate status: `PASS` for P8-C.

The layer is based on Tokyo Metropolitan Government tsunami damage-estimation spatial CSVs and MLIT N03 Chuo boundary clipping. It is not based on Chuo flood hazard map proxy data.

## Included Official Tokyo Tsunami Scenarios

| Scenario | Extracted for Chuo | Spatial sample count | Max inundation depth | Max tsunami height | Arrival-time data |
|---|---:|---:|---:|---:|---|
| 大正関東地震 / Taisho Kanto earthquake | yes | 1123 | 2.0436 m | 2.1287 m | yes |
| 南海トラフ巨大地震 case 1 / Nankai Trough megathrust earthquake case 1 | yes | 1256 | 2.2629 m | 2.4223 m | yes |
| 南海トラフ巨大地震 case 5 | no current generated Chuo feature found | 0 | n/a | n/a | not present |
| 南海トラフ巨大地震 case 8 | no current generated Chuo feature found | 0 | n/a | n/a | not present |

## Science And Visual Field Rules

- `arrivalTimeSeconds`: available where extracted from the official Tokyo arrival-time CSVs.
- `inundationDepthMeters`: available where extracted from the official Tokyo inundation-depth CSVs.
- `maxTsunamiHeightMeters`: stored separately as tsunami-height metadata; it must not be used as `inundationDepthMeters`.
- `inundationBoundary`: derived from official extracted Chuo grid points as a grid-extent bbox; not an official inundation contour.
- `hazardIntensity`, `confidence`, `sourceMode`, `sourceCategory`, and `evidenceSourceId`: available for P8-C hazard-state provenance.
- `visualHeightMeters`: cinematic only; it must not be treated as physical hazard depth.

## Final P8-B Gate

P8-B Problem 1 is consolidated. P8-C may rely on the official metropolitan hazard-layer v1 while keeping all boundary and field-separation limitations visible.

P8-B does not implement P8-D damage/collapse proxy, P8-E persistent candidate visibility, P9 final gameplay, or P10 packaging.
