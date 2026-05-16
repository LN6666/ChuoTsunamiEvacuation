# Tsunami Hazard Data Plan

## Purpose

This document defines the Phase 3 preparation path for future tsunami and water-hazard data.

P3-02 adds a small synthetic hazard fixture and validator. It does not parse official GIS files, CityGML, or PLATEAU data.

## Current P3-02 Components

- `data_pipeline/schemas/tsunami_hazard_schema.json`
- `data_pipeline/samples/sample_tsunami_hazard_zones.json`
- `data_pipeline/scripts/validate_tsunami_hazard.py`
- `data_pipeline/tests/test_tsunami_hazard_schema.py`

## Shelter Points vs Hazard Areas

Shelter data is point-like or address-based. It describes facilities that could become Unity shelter markers.

Hazard data is area-, depth-, height-, mesh-, raster-, or polygon-based. It describes affected zones, inundation areas, depth assumptions, tsunami height assumptions, or disaster-risk areas.

These data families should remain separate. Shelter records should not store hazard polygons, and hazard zones should not be treated as shelter records.

## Hazard Fixture Fields

The sample hazard fixture includes:

- dataset metadata
- coordinate reference system
- source metadata
- zone ID and name
- hazard family
- affected zone
- hazard level
- geometry type
- small synthetic geometry
- inundation area label
- inundation depth in meters or null
- tsunami height in meters or null
- notes

## Paper And Reference Maps

Paper maps, academic figures, and secondary reference maps may help interpret risk and check assumptions.

They are not primary machine-readable pipeline data because they may be simplified, derived, out of date, or unsuitable for automated extraction. Future P3 work should prefer official machine-readable polygon, mesh, or raster data when available.

## Future Work

Future hazard ingestion should:

- review official source license and update policy
- confirm CRS
- parse only a small reviewed local fixture first
- validate geometry type and numeric depth/height fields
- clip or filter to Chuo only after source format is known
- define any Unity visualization hints separately from source geometry
