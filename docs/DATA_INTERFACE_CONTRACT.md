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

## Stability Rules

- P3 processed JSON must remain schema-validated.
- P3 processed CSV should stay aligned with the JSON record fields.
- P3 must keep source metadata in each record.
- P3 sample outputs must remain small and committed for tests.
- Raw, downloaded, intermediate, cache, and large GIS files must remain ignored by Git.

## Manual Handoff Checks

Before P4 consumes real P3 outputs, the team should manually confirm:

- source authority and license
- source update date
- address and coordinate quality
- duplicate handling
- whether records are suitable for gameplay
- whether optional PLATEAU building matching is needed
