# P7-C Performance Optimization Report

## Profiling Methodology

P7-C follows a measure-first workflow:

1. Identify likely CPU, GPU, memory, and loading bottleneck categories.
2. Record approximate sandbox telemetry only.
3. Separate target settings from actual scene/renderable evidence.
4. Avoid final optimization claims until Unity Profiler and Windows EXE evidence exists.

`P7BenchmarkMetricsRecorder` records average FPS, approximate 1 percent low FPS, sample count, elapsed time, active chunk count, imported file count, imported byte count, and chunk state summary.

This recorder is approximate benchmark telemetry. It is not a replacement for Unity Profiler, Frame Debugger, Memory Profiler, or Windows EXE profiling.

For the high-detail scene continuation, `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity` is the P7-D profiling target. Current P7-C evidence is a scene shell and import checklist unless actual PLATEAU SDK import has populated renderable assets.

## Unity Profiler Workflow

P7-D should use Unity Profiler thinking even before optimizing:

- CPU main thread: identify scripting, culling, rendering setup, physics, and loading costs.
- Render thread: identify batching, material changes, and draw submission costs.
- GPU: identify fragment, vertex, shadow, overdraw, and post-processing costs where GPU timing is available.
- Memory: identify texture, mesh, material, scene, and managed allocations.
- Loading: identify import activation, scene load, and chunk enable-disable spikes.

No final optimization success should be claimed without measured evidence.

## Memory Profiler Workflow

P7-D should capture memory snapshots or profiler memory data after the high-detail scene is loaded. The review should classify:

- texture count and runtime texture memory
- material count and material variants
- mesh count, vertex/index memory, and submesh count
- static batching memory impact if used
- scene object count and renderer count

P7-C only records source file counts and byte sizes.

## Frame Debugger Workflow

P7-D should use Frame Debugger or RenderDoc-style inspection to confirm:

- draw-call count
- batch breaks
- material/shader variants
- shadow caster cost
- transparent or special render queues
- whether SRP Batcher is helping or being defeated by material state

## Draw Call / Batching Risk

Current imported evidence is raw `.gml` plus `.jpg`, so draw calls cannot be measured yet.

The new high-detail scene shell has metadata roots only until PLATEAU assets are imported. It must not be used to claim draw-call success.

Expected risks after conversion:

- one material per texture or surface can produce excessive draw calls
- many small meshes can defeat batching
- high material count can reduce SRP Batcher benefit
- transparent or special-case materials can fragment render state

Conceptual policy:

- SRP Batcher compatibility should be checked after material generation.
- Static batching may help stable city geometry but can increase memory and reduce chunk toggle granularity.
- GPU instancing is useful only for repeated identical meshes with shared material state.
- Dynamic batching is not a primary strategy for high-detail city geometry because mesh complexity, material diversity, and modern SRP behavior limit its benefit.

## Texture And Material Memory Risk

`53393690` currently contains 5837 texture files totaling 219669003 bytes.

Risks:

- large texture count can increase load time and memory pressure
- unique texture/material pairs can increase draw calls
- uncontrolled import settings can create oversized runtime texture memory
- texture atlasing or consolidation may be needed after conversion, but is not implemented in P7-C

P7-D should use Memory Profiler or Unity Profiler memory views to inspect texture memory, material count, and residency.

## Mesh Memory Risk

`53393690` currently contains 6 `.gml` files totaling 415113240 bytes.

No converted mesh memory is available yet. Future conversion must measure:

- mesh count
- vertex count
- index count
- submesh count
- material slots per mesh
- static batching memory impact

## Chunk Loading Strategy

P7-C adds a benchmark registry and controller:

- chunks are grouped by logical UDX category
- chunk roots can be enabled or disabled
- the registry records candidate metadata even when no renderable mesh exists
- the scene avoids a full-scene always-on production assumption

The same strategy informs `P7_HighDetail_Chuo.unity`: imported layers should remain grouped by category or spatial chunk so P7-D can test enable-disable cost and avoid full-scene always-on assumptions.

This is not async streaming, Addressables, or production city paging. It is a safe foundation for later profiling.

## LOD Policy

P7-C policy:

- near LOD: highest available converted detail for active benchmark inspection
- mid LOD: simplified mesh or lower-detail converted output when available
- far LOD: coarse context representation

Current state:

- LOD3 candidate is benchmark-only.
- LOD4 is not available based on current evidence.
- LODGroup strategy is documented but not claimed as implemented because raw CityGML is not renderable mesh yet.
- Average LOD3 is not claimed for the new high-detail scene until actual renderable scene evidence exists.

## Culling Strategy

Frustum Culling is assumed to apply only to renderers that exist in a scene.

Current P7-C placeholders can be frustum culled like ordinary Unity renderers, but this does not prove converted CityGML behavior.

Occlusion Culling may be feasible later if converted chunks become mostly static city geometry. It should be tested separately because occlusion data can increase bake time, scene data size, and workflow complexity.

The `53393690` logical groups can support later visibility grouping, but current evidence is metadata-only.

## Collision Strategy

Raw high-detail city geometry must not become complex MeshCollider geometry by default.

P7-C placeholder volumes have no gameplay collision purpose. Future production integration should use simplified colliders only where gameplay needs them, such as walkable areas, shelter entrances, and selected blocking volumes.

## Editor Vs Windows EXE Handoff To P7-D

Editor measurements can be distorted by editor overhead, asset refresh, scene view, domain reload, and development-only systems.

P7-D must profile a Windows EXE build and record:

- average FPS
- 1 percent low FPS
- CPU main/thread timing
- render thread timing
- GPU timing where available
- memory usage
- load and activation timing
- draw calls, batches, triangles, vertices, textures, materials, and mesh counts

## What Is Measured Now Vs Deferred To P7-D

Measured now:

- file count and byte size for imported candidate `53393690`
- extension summary
- logical group summary
- active placeholder chunk count
- approximate runtime FPS telemetry in P7Benchmark tests/runs
- high-detail scene shell readiness and expected layer roots

Deferred to P7-D:

- authoritative CPU/GPU profiling
- Memory Profiler evidence
- Frame Debugger draw-call evidence
- Windows EXE performance
- converted mesh/material visual quality
- occlusion, LODGroup, batching, and collider performance validation
- final confirmation that `P7_HighDetail_Chuo.unity` contains actual high-detail city assets and can become the P8/P9/P10 baseline
