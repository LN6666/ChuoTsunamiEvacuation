# Data Interface Contract

## Purpose

This document defines the boundary between the existing P2 Unity gameplay/dataization layer, the P3 real data pipeline, and future P4 Unity integration.

## P2 Unity Layer

P2 contains the current first playable and data-driven gameplay prototype.

P2 currently consumes Unity gameplay data under `Assets/Data/`, including:

- `Assets/Data/test_shelters.json`
- `Assets/Data/scenario_presets.json`

P3 must not modify these files.

## P3 Data Pipeline Layer

P3 produces standalone processed outputs only:

- `data_pipeline/processed/real_chuo_shelters_sample.json`
- `data_pipeline/processed/real_chuo_shelters_sample.csv`

P3 does not modify Unity scenes, Unity scripts, Unity project settings, PLATEAU imported files, or `Assets/Data` gameplay JSON files.

P3-00 uses synthetic sample data only. The outputs are contract examples, not official Chuo Ward shelter records.

P3-01 adds source planning metadata in:

- `data_pipeline/sources/source_candidates.json`

This registry is not a Unity input. It is a review queue for future official shelter/facility sources, hazard sources, and secondary references.

P3-02 adds sample handoff packaging in:

- `data_pipeline/processed/release/`

The release package is a P4 review input only. It is not copied into Unity and is not official gameplay data.

## P4 Future Unity Integration Layer

P4 may later copy, transform, or load approved P3 outputs into Unity-facing data files. P4 is responsible for deciding:

- whether P3 record `id` maps directly to a Unity `shelterId`
- where processed files should live in Unity
- how WGS84 coordinates are converted to Unity world positions
- how records bind to scene objects or generated markers
- how optional PLATEAU building matching is represented

## Existing PLATEAU Context

The Unity project already has local PLATEAU Chuo Ward Buildings / bldg / LOD1 imported through PLATEAU SDK. `Chuo_BaseMap.unity` is a large generated local scene and is not committed.

P3-00 does not download PLATEAU, re-import CityGML, parse CityGML, or modify `Chuo_BaseMap.unity`. Future PLATEAU building matching is optional Phase 3/4 work.

## Expected Future Unity Consumer Fields

Future Unity integration can expect P3 processed records to include:

- `id`
- `name`
- `latitude`
- `longitude`
- `type`
- `capacity`
- `floors_available`
- `unity.prefab_hint`
- `unity.is_entry_enabled`
- `unity.estimated_stair_floors`

P4 should not assume that P3-00 records already have Unity world positions or PLATEAU building IDs.

## Future Hazard Interface

Hazard data is separate from shelter/facility point data.

Future P3 hazard outputs may represent:

- inundation areas
- inundation depth zones
- tsunami height or water-level assumptions
- affected zones
- disaster-risk areas

P4 must not treat hazard polygons or meshes as shelter records. A future hazard schema should define how risk zones, depth categories, timing assumptions, and Unity visualization hints are represented.

P3-02 provides the first small sample hazard schema and fixture:

- `data_pipeline/schemas/tsunami_hazard_schema.json`
- `data_pipeline/samples/sample_tsunami_hazard_zones.json`

## Official vs Reference Source Contract

Future P4 integration should consume only processed outputs derived from approved official sources.

`paper_or_secondary_reference` candidates may support design review and manual checks, but they are not primary pipeline sources. They should not be copied into `Assets/Data` or used as authoritative gameplay data.

## Stability Rules

- P3 processed JSON must remain schema-validated.
- P3 processed CSV should stay aligned with the JSON record fields.
- P3 must keep source metadata in each record.
- P3 sample outputs must remain small and committed for tests.
- Raw, downloaded, intermediate, cache, and large GIS files must remain ignored by Git.
- Source candidates must not enable scraping or downloads without a later explicit milestone.

## Manual Handoff Checks

Before P4 consumes real P3 outputs, the team should manually confirm:

- source authority and license
- source update date
- address and coordinate quality
- duplicate handling
- whether records are suitable for gameplay
- whether optional PLATEAU building matching is needed
