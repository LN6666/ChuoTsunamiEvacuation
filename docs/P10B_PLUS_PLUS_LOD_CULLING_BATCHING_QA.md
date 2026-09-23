# P10-B++ LOD, Culling, Batching, And Occlusion QA

P10-B++ does not change PLATEAU assets, high-detail scenes, ProjectSettings, Packages, or URP assets.

## Current Readiness

- Production high-detail scene chunk streaming is not confirmed.
- LODGroup coverage for the final high-detail scene is unknown from this P10-B++ code-only pass.
- Static batching, dynamic batching, and occlusion culling status must be verified in Unity.
- Camera clipping distance and marker visibility range must be visually checked in the high-detail scene.

## QA Checklist

- Inspect active camera near/far clip values.
- Check whether distant city geometry dominates rendering cost.
- Verify green frame visibility range is useful but not excessive.
- Verify debug labels are off by default.
- Count active NPCs, markers, green frames, and light curtain objects.
- Check whether static batching is configured and whether it increases memory.
- Check whether dynamic batching applies to marker/frame materials.
- Check whether occlusion culling is baked or absent.
- Check whether relevant imported objects have LODGroups.

## P10-C Profiler Targets

- Rendering module: batches, setpass calls, triangles, vertices.
- CPU main thread and render thread frame time.
- Memory module: textures, meshes, materials, scene objects.
- Frame Debugger: green frames and light curtain draw behavior.
