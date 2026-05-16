# P4 Real Data Integration Plan

## Purpose

This document records the Unity-side handoff plan for future P4 real shelter data integration.

Milestone 2-05 adds only hooks. It does not import real Chuo facility data and does not read P3 outputs directly.

## Current P2 Behavior

- Gameplay uses `Assets/Data/test_shelters.json`.
- `Assets/Data/shelter_source_config.json` defaults to `sourceMode = "test"`.
- `Assets/Data/real_chuo_shelters_sample.example.json` is an inactive example stub.
- PLATEAU geometry remains background/context for the debug platform.

## Future P4 Flow

1. P3 produces processed real shelter data outside the P2 Unity workflow.
2. A selected, reviewed sample is copied or converted into `Assets/Data/`.
3. The Unity schema is checked against `docs/DATA_SCHEMA.md`.
4. Unity-side loading is updated to read the selected sample file.
5. Coordinate conversion and placement are implemented in a dedicated Unity data/placement layer.
6. PLATEAU building matching is added only after the shelter data contract is stable.

## Explicitly Out Of Scope For P2-05

- Reading from `data_pipeline/processed`.
- Copying P3 outputs automatically.
- Importing all real Chuo facilities.
- Coordinate conversion in Unity.
- PLATEAU building matching.
- Real entrance detection.
- Real road navigation.
- Dashboard analytics.

## Open P4 Decisions

- Final active real shelter JSON filename.
- Whether runtime loading should use StreamingAssets or another build-safe path.
- Coordinate conversion responsibility and CRS policy.
- How to represent matched PLATEAU building IDs.
- How to validate real facility availability without inventing unsupported safety judgments.
