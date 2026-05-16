# P3 To P4 Handoff

## Purpose

This document describes what Phase 3 can hand to future Phase 4 Unity integration.

P3 does not modify Unity. P4 decides how to consume approved processed outputs.

## Current Safe P3 Outputs

The current P3 sample release package is built under:

`data_pipeline/processed/release/`

It includes:

- `real_chuo_shelters_sample.json`
- `real_chuo_shelters_sample.csv`
- `sample_tsunami_hazard_zones.json`
- `p3_sample_release_manifest.json`

These files are safe for P4 to inspect as shape and contract examples. They are not official data and should not be used as gameplay truth.

## P4 Must Not Assume

P4 must not assume:

- sample records are official
- WGS84 coordinates are already Unity world positions
- shelters are already matched to PLATEAU buildings
- hazard polygons are final risk zones
- paper/reference sources are authoritative

## P4 Future Responsibilities

P4 should decide:

- where approved P3 outputs live inside Unity
- whether records are copied to `Assets/Data` or loaded another way
- how shelter IDs map to Unity objects
- how WGS84 coordinates map to Unity scene coordinates
- how hazard zones become gameplay risk zones or visual overlays
- how PLATEAU building matching is represented, if used

## Handoff Gate

Before P4 consumes real P3 outputs, confirm:

- official source authority
- license and attribution requirements
- source update date
- schema validation
- coordinate quality
- duplicate handling
- file size and Git safety
- manual review of any paper/secondary reference influence
