# P7-B Wave 2-B Known Limitations

Date: 2026-05-23

## Limitations

- `53393690` is a planning candidate only.
- Fallback candidates `53393672` and `53394611` are planning candidates only.
- LOD3 is not visually verified.
- LOD3 geometry quality is not verified.
- LOD4 is not available based on current path/name evidence.
- A zero LOD4 path/name result is not geometry parsing proof.
- Path/name metadata may not reflect actual CityGML geometry quality.
- Candidate discovery may be incomplete or too broad depending on same-mesh-code paths.
- Same-mesh-code category hits include non-core categories such as `frn`, `fld`, and `veg`.
- Candidate import size may still be too heavy even for a small-area experiment.
- No full Chuo import is allowed.
- No real asset import was performed in this dry run.
- No external PLATEAU data was copied into Unity.
- `Chuo_BaseMap.unity` is untouched.
- `ProjectSettings` and `Packages` are untouched.
- `Assets/Data` and `Assets/PLATEAU` are untouched.
- P8 hazard systems are excluded.
- P9 crowd and interior-shelter systems are excluded.
- This is a P7Benchmark feasibility step, not production integration.

## Deferred Work

A later Wave 2-C or future import step needs separate explicit approval before:

- importing real PLATEAU assets,
- copying candidate files into Unity,
- creating generated Unity assets,
- modifying benchmark scenes,
- collecting visual screenshots,
- making any production scene integration decision.
