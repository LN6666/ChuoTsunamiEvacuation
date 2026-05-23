# P8-B To P8-C Gate Decision

Date: 2026-05-24.

## Gate Options

P8-C may proceed only if the P8-B evidence gate is one of:

- `PASS`: actual spatial hazard layer extracted.
- `CONDITIONAL PASS`: Tokyo metropolitan tsunami evidence identified, but spatial extraction is manual/pending; P8-C can use prototype geometry with explicit labels.
- `BLOCKED`: no acceptable evidence source identified.

## Current Decision

Current decision: `CONDITIONAL PASS`.

Reason:

- Tokyo Metropolitan Government tsunami damage estimation map/report sources are identified as primary Chuo evidence candidates.
- Chuo City references Tokyo's damage estimation report for tsunami numerical simulation results.
- Public source review includes Chuo maximum tsunami-height references around 2.4m to 2.46m.
- Exact geospatial extraction for Chuo may require manual GIS/web-map extraction if no direct downloadable dataset/API is available.
- A complete Chuo official spatial inundation-depth raster or polygon layer has not yet been extracted into project data.

Do not mark `BLOCKED` solely because Chuo lacks a standalone tsunami hazard map equivalent to flood hazard maps.

Plain gate rule: Do not mark BLOCKED solely because Chuo lacks a standalone tsunami hazard map.

## P8-C Conditions

P8-C may start infrastructure hazard interaction only under these conditions:

- Treat current boundary/depth as prototype/manual unless extraction is reviewed.
- Display or log that official metropolitan source is known and spatial extraction is pending.
- Do not claim complete official spatial inundation data.
- Do not use maximum tsunami height as an inundation-depth grid.
- Keep gameplay success/failure rules unchanged unless a later approved stage changes them.
