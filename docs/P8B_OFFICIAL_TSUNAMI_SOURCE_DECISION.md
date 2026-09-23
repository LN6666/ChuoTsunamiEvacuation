# P8-B Official Tsunami Source Decision

Date: 2026-05-24.

## Decision

P8-B uses Tokyo Metropolitan Government tsunami Open Data as the official metropolitan source for Chuo tsunami spatial hazard v1.

Gate decision: `PASS`.

## Rationale

- Chuo City does not appear to publish a standalone tsunami hazard map equivalent to its flood hazard maps.
- That absence is not evidence absence, because Chuo City references Tokyo Metropolitan Government damage estimation for tsunami numerical simulation results.
- Tokyo provides tsunami estimation layers relevant to Chuo, including maximum tsunami height, maximum inundation depth, and arrival-time data.
- Tokyo Open Data CSV files were directly accessible without token, login, API key, registration, G-Spatial token, real-estate-library token, or browser-only manual download.
- MLIT N03 official administrative boundary data was directly accessible and used to filter Tokyo mesh points to Chuo.

## Claims Allowed

- The P8-B hazard layer is an official metropolitan extracted spatial layer v1.
- The risk front driver is `official_spatial`.
- Maximum inundation depth is spatially extracted from Tokyo official CSV mesh points clipped to Chuo.
- Maximum tsunami height is recorded separately from inundation depth.

## Claims Not Allowed

- Do not claim a full real-time tsunami fluid simulation.
- Do not claim the derived grid-extent bbox is an official inundation contour.
- Do not claim `maxTsunamiHeightMeters` is the inundation-depth grid.
- Do not claim the cinematic curtain height equals real tsunami height.
- Do not implement P8-C infrastructure interaction in P8-B.
- Do not implement P8-D collapse proxy or P9/P10 systems.

## Source Categories

- `official_tsunami_metropolitan`: Tokyo Open Data tsunami CSVs.
- `official_tsunami_report_reference`: Tokyo report and Chuo reference pages.
- `official_admin_boundary`: MLIT N03 administrative boundary.
- `official_flood_proxy`: Chuo flood materials, fallback only and not used for P8-B tsunami front.
- `manual_extraction_required` / `evidence_planned`: retained for future evidence that still needs review or manual handling.

Maximum tsunami height is not the inundation-depth grid; the extracted depth CSV is the depth source.
