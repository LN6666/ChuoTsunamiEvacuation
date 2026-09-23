# P7 Reference Review

Date: 2026-05-22

Phase: PBL7 / P7-0

Decision rule: references are reviewed for planning only. P7-0 adopts no optional dependency and makes no `Packages/`, `ProjectSettings/`, scene, asset, or gameplay changes.

## Primary Sources Checked

- Project PLATEAU SDK for Unity: https://github.com/Project-PLATEAU/PLATEAU-SDK-for-Unity
- Project PLATEAU SDK Toolkits for Unity: https://github.com/Project-PLATEAU/PLATEAU-SDK-Toolkits-for-Unity
- Project PLATEAU streaming tutorial: https://github.com/Project-PLATEAU/plateau-streaming-tutorial
- PLATEAU distribution service docs: https://docs.plateauview.mlit.go.jp/
- Cesium for Unity repository: https://github.com/CesiumGS/cesium-unity
- Cesium for Unity docs: https://cesium.com/learn/unity/
- GeoTileLoader: https://mhama.github.io/GeoTileLoader/
- Unity Addressables docs: https://docs.unity.cn/Packages/com.unity.addressables%401.22/manual/index.html
- Unity AssetBundle loading guidance: https://learn.unity.com/tutorial/assets-resources-and-assetbundles
- Unity LODGroup manual: https://docs.unity3d.com/Manual/class-LODGroup.html
- Unity occlusion culling manual: https://docs.unity3d.com/Manual/OcclusionCulling.html
- Unity GPU occlusion culling docs: https://docs.unity.cn/Packages/com.unity.render-pipelines.high-definition%4017.0/manual/gpu-culling.html
- Unity GPU instancing manual: https://docs.unity3d.com/Manual/GPUInstancing.html
- Unity draw call batching manual: https://docs.unity3d.com/Manual/DrawCallBatching.html
- Unity manually combining meshes manual: https://docs.unity.cn/2020.3/Documentation/Manual/combining-meshes.html
- Unity CullingGroup API manual: https://docs.unity.cn/Manual/CullingGroupAPI.html
- Unity Profiler manual: https://docs.unity3d.com/Manual/ProfilerWindow.html
- Unity Frame Debugger manual: https://docs.unity.cn/Manual/frame-debugger-window.html
- Unity Memory Profiler package docs: https://docs.unity.cn/Packages/com.unity.memoryprofiler%400.4/manual/index.html
- Unity texture compression manual: https://docs.unity3d.com/Manual/texture-compression-formats.html

## Review Table

| Reference | Classification | Expected P7 Use | Benefit | Risk | P7-0 Adoption Decision | Verify Before Adoption | Packages / ProjectSettings Risk |
|---|---|---|---|---|---|---|---|
| Project-PLATEAU / PLATEAU-SDK-for-Unity | reference_only | Understand existing PLATEAU import, CityGML category handling, LOD import behavior, and expected generated asset structure. | Official PLATEAU Unity workflow reference. | Re-importing can generate large assets/scenes and may disturb current local base map. | Use as reference only; do not update or reinstall SDK in P7-0. | Current installed SDK version, import logs, LOD/category mapping, generated file sizes, rollback path. | Possible if SDK update is attempted; blocked in P7-0. |
| Project-PLATEAU / PLATEAU-SDK-Toolkits-for-Unity | reference_only | Study rendering, LOD, utility, and sample workflow ideas for future P7-C. | Contains PLATEAU-specific visual/LOD tooling ideas. | Requires toolkit package workflow and may assume URP/HDRP/project setup. | Reference only; no toolkit import. | License, Unity version, render pipeline, package installation footprint, generated assets, sample scene side effects. | Likely `Packages/` and possible settings risk; blocked in P7-0. |
| PLATEAU 3D Tiles / streaming references | reference_only | Learn data catalog, tile URL, city tile partition, and streaming concepts for comparison with local chunking. | Official PLATEAU streaming model may inform P7-C architecture. | Network/service dependency, coordinate alignment, runtime streaming complexity, duplicated local/streamed geometry. | Reference only; no runtime streaming. | Offline availability, service terms, Chuo coverage, coordinate transform, cache size, fallback behavior. | Could require packages or network config if adopted; blocked in P7-0. |
| Cesium for Unity | not_recommended_for_now | Compare 3D Tiles streaming, georeferencing, and tile exclusion concepts. | Mature geospatial streaming ecosystem. | Large dependency, package/settings impact, external services, coordinate integration complexity. | Not recommended for P7-0; reference only for concepts. | Unity version, render pipeline, license, offline/local tiles support, build performance, georeference integration, package footprint. | Requires package changes and may affect project settings; blocked in P7-0. |
| GeoTileLoader or similar tiled city loading | not_recommended_for_now | Compare lightweight 3D Tile loading design and tile selection logic. | Smaller conceptual model for tiled loading. | Third-party maintenance/license risk and unclear fit with PLATEAU SDK-generated local assets. | Not recommended for P7-0; reference only. | License, Unity compatibility, supported tile specs, performance, maintenance, local/offline support. | Likely package/source import; blocked in P7-0. |
| Unity Addressables | reference_only | Future candidate for chunked local asset loading in P7-C. | Official asset management layer over AssetBundles with async loading and dependency tracking. | Requires package setup, addressable groups, build content workflow, and generated data. | Reference only; no Addressables package or group changes. | Existing package presence, content profile strategy, local/remote load path, build reproducibility, memory release behavior. | Usually `Packages/`, Addressables settings, and generated group data; blocked in P7-0. |
| AssetBundle / asset streaming | reference_only | Lower-level alternative for chunked city loading. | Built-in runtime asset packaging/loading concept. | Manual dependency management, duplicate bundle load errors, memory spikes, build pipeline complexity. | Reference only; no bundles generated. | Bundle granularity, dependency layout, unload policy, build automation, profiler evidence. | May require editor tooling and generated build artifacts; no settings change by default. |
| Unity LODGroup | reference_only | Candidate component strategy for per-building or per-chunk LOD display. | Official LOD distance switching; direct fit for high/medium/low model variants. | LOD popping, authoring burden, memory cost if all LODs are loaded, mismatch with PLATEAU LOD categories. | Reference only; no LODGroup edits in P7-0. | Screen-relative transition values, cross-fade settings, source LOD availability, benchmark FPS/memory impact. | No package required; scene/prefab/asset changes in later stages. |
| Unity Occlusion Culling | reference_only | Evaluate static occlusion for dense urban canyons or underground spaces. | Can reduce rendering of hidden static geometry. | Bake data, scene static flags, large-scene bake size, weak benefit in open areas, scene changes required. | Reference only; no bake or static flag changes. | Small-area bake result, build size, CPU/GPU impact, compatibility with chunk loading. | Scene and generated occlusion data risk; blocked in P7-0. |
| GPU Occlusion Culling | reference_only | Future comparison for Unity 6 / SRP projects if supported. | May reduce GPU work in occluded high-detail scenes. | Render pipeline/version constraints, GPU Resident Drawer dependency, possible settings changes, uncertain benefit. | Reference only; do not enable. | Unity version, URP/HDRP support, compute shader support, pipeline settings, frame debugger/profiler evidence. | Likely ProjectSettings/render-pipeline asset risk; blocked in P7-0. |
| GPU Instancing | reference_only | Candidate for repeated props, vegetation, facade parts, or shared-material city objects. | Reduces draw calls for repeated mesh/material pairs. | Only helps repeated meshes/materials; shader/material compatibility required; can conflict with batching choices. | Reference only; no material changes. | Repetition count, shared mesh/material candidates, shader instancing support, Frame Debugger evidence. | No package required; material/shader/project setting changes possible later. |
| Draw call batching | reference_only | Baseline rendering optimization strategy for static urban geometry and shared materials. | Reduces CPU render submission overhead. | Static batching can increase memory; dynamic batching is limited; SRP batching depends on shaders/material layout. | Reference only; no batching settings changed. | Current render pipeline, material count, batch reason from Frame Debugger, memory impact. | ProjectSettings or material/shader changes possible later; blocked in P7-0. |
| Mesh slicing / city chunking / culling references | reference_only | Inform chunk size, culling boundaries, and streamed asset granularity. | Can keep loaded/rendered geometry bounded near the player/camera. | Bad chunking can increase draw calls, break culling, duplicate materials, or create visible seams. | Reference only; no mesh processing in P7-0. | Chunk size benchmark, culling behavior, collider policy, material sharing, load/unload timing. | May generate assets/scenes/tools later; no P7-0 changes. |
| Unity Profiler | reference_only | Required measurement tool for CPU, rendering, memory, loading, and frame timing. | Official measurement basis for Editor and Windows EXE profiling. | Editor profiling can differ from player builds; profiler overhead can skew numbers. | Adopt as a measurement reference only; no project changes. | Development build/profiler attachment workflow, sample duration, repeatability, target machine specs. | No package required for built-in profiler. |
| Unity Frame Debugger | reference_only | Required draw call, batch, shader pass, and render event investigation tool. | Explains why batching/instancing/LOD choices work or fail. | Manual GUI workflow; can attach only when build/debug setup supports it. | Measurement reference only. | Capture procedure, target build settings, representative camera views, event count record format. | No package required; development build may be needed later. |
| Unity Memory Profiler | reference_only | Candidate for RAM/Unity object/texture memory snapshots in P7-B/P7-D. | Helps identify duplicate assets, texture memory, and large object categories. | Package may not already be installed; captures can be heavy and can perturb low-memory targets. | Reference only; do not install in P7-0. | Existing package presence, snapshot size, target build compatibility, repeat capture method. | Package change if absent; blocked in P7-0. |

## P7-0 Technical Selection

P7-0 selects a conservative automation-first path:

- Use official/GitHub references for planning only.
- Do not adopt Cesium, GeoTileLoader, Addressables, Memory Profiler, PLATEAU Toolkits, or any new package in P7-0.
- Prefer command-line inventory and small-area benchmark evidence before any asset import or streaming implementation.
- Treat Unity LODGroup, batching, instancing, occlusion, and profiling tools as future evaluation topics, not current changes.

## Follow-Up Verification Questions

- What PLATEAU LOD levels and categories exist locally for Chuo 2025?
- How large are candidate LOD2/LOD3/LOD4 subsets by area and category?
- Which small areas best represent waterfront, dense inland, bridge, road, and underground complexity?
- Does a local chunked workflow outperform a single generated scene for Editor and Windows x64?
- Which optimization gives measurable benefit first: lower LOD, material reduction, texture compression, chunk loading, batching, instancing, or occlusion?
