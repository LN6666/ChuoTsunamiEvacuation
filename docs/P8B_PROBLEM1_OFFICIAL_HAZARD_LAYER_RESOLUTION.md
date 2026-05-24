# P8-B Problem 1 Official Hazard Layer Resolution

Date: 2026-05-24.

## Decision

Problem 1 is consolidated for P8-C.

The P8-B hazard layer v1 uses Tokyo Metropolitan Government tsunami spatial CSV datasets associated with the broader Tokyo damage-estimation source family/report context, 東京都「首都直下地震等による東京の被害想定」. The actual extracted Chuo tsunami layers are 大正関東地震 and 南海トラフ巨大地震 case 1 only. The layer is not driven by the Chuo flood hazard map and is not a generic flood proxy.

Current decision: `PASS`.

## Official Tokyo Tsunami Source Check

Primary source:

- `tokyo_damage_estimation_map_tsunami`
- Tokyo Open Data ward-area tsunami distribution CSVs
- Dataset URL recorded in `Assets/Data/P8/tsunami_hazard_evidence_registry.json`
- Source mode/category: `official_tsunami_metropolitan`
- Access method: direct HTTPS CSV, no token or login

Source-family distinction:

- 東京都「首都直下地震等による東京の被害想定」 is the broader official source family/report context used for provenance.
- P8-B extracted tsunami spatial samples only for 大正関東地震 and 南海トラフ巨大地震 case 1.
- P8-B does not claim that 都心南部直下地震-specific tsunami inundation-depth data was extracted.
- In the current extracted tsunami layers, 都心南部直下地震 is treated as not available / not a tsunami scenario in current extracted layers, not as missing project work.

Boundary source:

- `mlit_n03_chuo_admin_boundary`
- MLIT N03 Tokyo administrative boundary, selected by Chuo City (`N03_004=中央区`, `N03_007=13102`)
- Used only to clip official Tokyo tsunami mesh points to Chuo City

Non-source/fallback:

- `flood_proxy_chuo_hazard_map` remains a non-tsunami fallback reference only.
- It is not used by the P8-B hazard layer v1 and must not be described as tsunami data.

## Scenario Extraction Summary

| Scenario | Official CSV layers represented | Chuo samples | Max inundation depth | Max tsunami height | Arrival time exists | Boundary status |
|---|---|---:|---:|---:|---|---|
| 大正関東地震 / Taisho Kanto earthquake | inundation depth, arrival time, tsunami height | 1123 | 2.0436 m | 2.1287 m | yes, earliest valid 30cm arrival = 43383.6 s | derived grid extent bbox, not official contour |
| 南海トラフ巨大地震 case 1 / Nankai Trough megathrust earthquake case 1 | inundation depth, arrival time, tsunami height | 1256 | 2.2629 m | 2.4223 m | yes, earliest valid 30cm arrival = 43383.6 s | derived grid extent bbox, not official contour |

The project evidence search found P8-B generated data for Nankai Trough case 1. Case 5 and case 8 are not present in `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json` or the current extractor scenario list, so they are checked/not-present references and are not claimed as extracted for Chuo in this milestone. 都心南部直下地震 is likewise not claimed as an extracted tsunami layer.

## Field Separation

- `inundationDepthMeters` and `maxInundationDepthMeters` come from the official Tokyo tsunami maximum inundation-depth CSVs clipped to Chuo.
- `arrivalTimeSeconds` comes from the official Tokyo tsunami arrival-time CSVs clipped to Chuo. P8-B uses earliest valid 30cm arrival where available.
- `maxTsunamiHeightMeters` comes from the official Tokyo tsunami-height CSVs and remains separate from inundation depth.
- `visualHeightMeters` is cinematic only and is not physical hazard depth, water level, tsunami height, or inundation depth.
- `inundationBoundary` is derived from extracted Chuo grid points as a grid-extent bbox. It is evidence-backed in the sense that the source points are official extracted samples, but it is not an official inundation contour.

## Remaining Limitations

- The official full inundation contour is not extracted.
- The current P8-B boundary is derived/prototype geometry from official extracted points.
- Only Taisho Kanto and Nankai Trough case 1 are present in generated Chuo data.
- No 都心南部直下地震-specific tsunami inundation-depth, arrival-time, or tsunami-height layer is present in the current extracted P8-B layer.
- P8-C may proceed, but must preserve these provenance labels and must not overclaim official contour precision.
