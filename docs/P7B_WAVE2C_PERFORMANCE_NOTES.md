# P7-B Wave 2-C Performance Notes

Date: 2026-05-23

## Import Footprint

Candidate `53393690` sandbox footprint:

- Files: `5,843`
- Bytes: `634,782,243`
- Size: `605.38 MB`

The package is acceptable for this approved sandbox import, but it remains too large to treat casually for production integration.

## Runtime Performance

No runtime rendering performance claim is made in Wave 2-C.

The copied package is CityGML and texture source data. Until a future approved conversion/import step creates renderable Unity geometry, FPS, draw calls, triangle counts, material counts, texture memory, and scene-load impact remain unmeasured.

## Storage

The approved sandbox path uses Git LFS tracking because the package contains large GML files. This is a repository-storage measure, not a gameplay dependency and not a Unity package change.

## Future Measurement Requirements

A later approved visual/import experiment should record:

- imported object count,
- material count,
- texture count,
- triangle count,
- draw calls,
- memory use,
- Editor FPS,
- PlayMode FPS,
- Windows x64 build-size impact if an EXE benchmark is approved.
