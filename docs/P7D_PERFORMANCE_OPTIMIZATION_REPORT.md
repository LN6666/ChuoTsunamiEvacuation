# P7-D Performance Optimization Report

Validation date: 2026-05-23.

## Status

Performance optimization status: LIMITED EVIDENCE, EXE PROFILING STILL REQUIRED.

The imported high-detail scene is renderable but heavy: 22.55 GB scene text, 117,728 MeshRenderers, 117,728 MeshFilters, 117,728 MeshColliders, and no LODGroup evidence.

## Current Observations

- The import appears to embed converted meshes/material references directly in the scene.
- No converted asset output roots were detected under `Assets/P7HighDetail` or `Assets/PLATEAU`.
- No LODGroup components were detected, so Unity-side distance switching is not evident from the scene text.
- The scene contains many MeshColliders, which may be expensive for runtime loading and physics if enabled broadly.
- Missing water/terrain/disaster-risk categories remain known limitations that P8 should handle with data-layer, proxy, or rule-based approaches where needed.

## Optimization Risks

- Loading time may be high due to the 22.55 GB scene file.
- Draw calls and batches may be high because 117,728 renderers are present.
- Collider cost may be high because MeshCollider count matches renderer count.
- Scene serialization and Git diff tooling are already stressed by file size.
- No EXE data exists yet for FPS, memory, loading time, or build size.

## Recommended P8/P9 Optimization Work

- Profile a Windows x64 development build before adding P8/P9 gameplay systems.
- Consider disabling or simplifying MeshColliders for non-interactable city objects.
- Consider chunking/streaming or additive scenes before expanding content.
- Keep expensive P8/P9 systems staged and measured until load time and memory are known.
- Do not add new dependencies or broad rendering changes in P7-D.
