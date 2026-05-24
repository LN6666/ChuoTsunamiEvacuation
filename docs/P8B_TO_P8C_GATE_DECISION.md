# P8-B To P8-C Gate Decision

Date: 2026-05-24.

## Gate Options

P8-C may proceed only if the P8-B evidence gate is one of:

- `PASS`: official/evidence spatial tsunami hazard layer extracted.
- `CONDITIONAL PASS`: Tokyo metropolitan tsunami evidence identified, but spatial extraction is manual/pending; P8-C can use prototype geometry only with explicit user acceptance.
- `BLOCKED`: no acceptable tsunami evidence source identified, or extraction could not be completed and no user override allows proxy/manual geometry.

## Current Decision

Current decision: `PASS`.

Reason:

- Tokyo Metropolitan Government tsunami Open Data CSV layers were available by direct HTTPS download with no token, login, registration, or browser-only manual download.
- The official Tokyo maximum inundation depth, maximum tsunami height, and arrival-time 10m mesh CSVs were inspected and used.
- Official MLIT N03 administrative boundary data was available by direct HTTPS ZIP download and used to clip mesh points to Chuo City (`中央区` / `13102`).
- `Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json` contains extracted Chuo spatial mesh samples for Taisho Kanto earthquake and Nankai Trough case 1.
- `inundationDepthMeters` comes from the official Tokyo tsunami inundation-depth CSVs.
- `arrivalTimeSeconds` comes from the official Tokyo tsunami arrival-time CSVs.
- `maxTsunamiHeightMeters` remains separate from `inundationDepthMeters`; maximum tsunami height is not used as the inundation-depth grid.
- `visualHeightMeters` remains cinematic only.
- The Chuo flood hazard map is not used as tsunami data.

Do not mark BLOCKED solely because Chuo lacks a standalone tsunami hazard map. Tokyo metropolitan evidence is the primary source candidate for Chuo.

## Extracted Scenarios

| Scenario | Chuo samples | Max inundation depth | Max tsunami height | Arrival time exists |
|---|---:|---:|---:|---|
| 大正関東地震 / Taisho Kanto earthquake | 1123 | 2.0436 m | 2.1287 m | yes |
| 南海トラフ巨大地震 case 1 / Nankai Trough megathrust earthquake case 1 | 1256 | 2.2629 m | 2.4223 m | yes |

Nankai Trough case 5 and case 8 are not present in the generated P8-B Chuo layer or extractor scenario list, so they remain not extracted for this gate.

## P8-C Conditions

P8-C may proceed to infrastructure hazard interaction using this extracted hazard layer, with these constraints:

- P8-C may rely on extracted spatial sample records and scenario provenance.
- P8-C must keep the derived grid-extent boundary labeled as derived, not as an official inundation contour.
- P8-C must not claim complete official inundation contour precision.
- P8-C must preserve the no full fluid simulation boundary.
- P8-C must not claim full real-time fluid simulation or academic hydrodynamic modeling.
- P8-C must not use `maxTsunamiHeightMeters` as an inundation-depth grid.
- P8-C must not use `visualHeightMeters` as physical hazard depth.
- P8-C must keep gameplay success/failure rules unchanged.
- P8-C must not implement P8-D collapse proxy behavior, P8-E closeout work, P9 crowd/spawn/indoor systems, or P10 packaging.
