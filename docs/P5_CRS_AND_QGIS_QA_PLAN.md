# P5 CRS And QGIS QA Plan

## Purpose

P5-B2 defines the CRS and manual QGIS QA strategy needed before controlled real Chuo data ingestion and spatial processing begin.

CRS mistakes can make shelter points, PLATEAU building footprints, hazard layers, and route lines appear aligned while producing wrong distances or mismatched buildings. P5-B must make CRS handling explicit before real spatial decisions are trusted.

## Expected Source CRS Possibilities

- Official shelter/facility point tables may use EPSG:4326 latitude/longitude.
- OSM sources normally use WGS84 longitude/latitude coordinates.
- PLATEAU and CityGML-derived data may use source-specific coordinate conventions that must be verified before processing.
- Official hazard/disaster GIS layers may use a local or national projected CRS.
- Metric distance calculations should use a local projected CRS suitable for Tokyo/Chuo, not raw latitude/longitude.

## Target CRS Strategy

- Record CRS metadata in every processed output.
- Treat EPSG:4326 as interchange/source coordinate metadata, not a metric distance CRS.
- All metric spatial operations must use a projected CRS suitable for Tokyo/Chuo, such as a reviewed Japan Plane Rectangular CRS candidate.
- Never compute meter distances directly in unprojected latitude/longitude.
- Convert source layers into the agreed processing CRS before contains, nearest, buffer, route length, or distance-threshold logic.
- Preserve original source coordinates when useful for traceability.

## Unity Coordinate Caution

GIS CRS to Unity `Vector3` mapping is a separate P5-C concern.

Unity should consume processed outputs with explicit coordinates, IDs, status fields, confidence, and warnings. Unity should not perform raw CRS operations, parse raw GIS archives, or read raw PLATEAU/CityGML data at runtime.

## QGIS QA Role

QGIS is a manual visual QA tool, not a runtime dependency and not the only source of truth.

Planned QA layers:

- shelter points
- candidate/qualified building footprints
- building match lines
- route lines
- unmatched/low-confidence points
- hazard context layers

Manual checks:

- no visible coordinate shift between layers
- routes are not flying, disconnected, or crossing impossible gaps in obvious ways
- shelter points are near or inside matched buildings
- nearest-match records are visually plausible
- unmatched and low-confidence points are easy to inspect
- hazard context layers align with known basemap/reference layers
- source CRS and processed CRS are visible in layer metadata

## QGIS Output Policy

QGIS outputs should remain small QA exports, screenshots, or notes. Do not commit raw large GIS files, QGIS caches, full source archives, full CityGML, or full OSM extracts.

Useful future QA outputs may include small GeoJSON layers, CSV review tables, PNG screenshots, or Markdown QA notes, only when explicitly approved by the milestone.

## P5-B4 QA Layer Status

P5-B4 did not create real QGIS QA layers because the required real inputs are missing:

- approved real Chuo official shelter/evacuation facility input
- approved processed PLATEAU building footprint/attribute input

The local environment is ready for CRS-aware processing, but QGIS QA layer generation must wait until those inputs exist. Synthetic P3 shelter samples and raw CityGML are not acceptable substitutes for B4 completion.

## Risks

- Axis-order mistakes can swap latitude and longitude.
- Geographic CRS distances can silently produce wrong meter thresholds.
- PLATEAU-derived building footprints may not share the same CRS as source points.
- QGIS visual alignment can hide data-field or source-provenance errors.
- Unity coordinate conversion can introduce a second alignment problem if handled before processed GIS outputs are stable.

## Next Action

P5-B3 should define the exact processing CRS candidate, verify it with known Chuo reference points, and add CRS metadata checks before controlled real-source ingestion output is accepted.
