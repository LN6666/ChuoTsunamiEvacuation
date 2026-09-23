# P7 LOD Asset Strategy

Date: 2026-05-22

Status: Strategy foundation only. No asset import or Unity scene change is approved by this document.

## Objective

P7 should raise Chuo visual/detail quality while keeping the project usable in Unity Editor and Windows x64 EXE builds. The strategy is to avoid all-or-nothing full-detail loading and instead choose evidence-backed LOD, area, material, texture, collision, and streaming policies.

## LOD Policy

| LOD | P7 Policy | Use Case |
|---|---|---|
| LOD4 | Selected key areas only after benchmark approval. | Landmark/key demonstration blocks, complex bridge/underground candidates, close-view presentation areas. |
| LOD3 | Average target where available and performance allows. | Main city visual quality target for selected Chuo areas. |
| LOD2 | Minimum baseline for broad city coverage. | Broader Chuo context, medium-distance urban shape, fallback if LOD3 is too heavy. |
| LOD1 | Fallback only. | Emergency performance fallback, distant context, or missing higher LOD coverage. |

P7 must not assume that every asset category has every LOD. P7-A inventory must record actual candidate LOD levels before import decisions.

## Key-Area Prioritization

Prioritize areas that support the P7 purpose:

1. Waterfront / low-elevation evacuation context near Harumi, Kachidoki, Tsukishima, and Tsukuda.
2. Dense building blocks where LOD/material/draw-call pressure is representative.
3. Bridge and road structures that affect visual continuity and navigation context.
4. Underground or station-adjacent assets if local PLATEAU data contains relevant categories.
5. Current gameplay demonstration areas only after protecting existing P6 gameplay behavior.

Each key area must have a written reason, approximate bounds, target LOD, included asset categories, and rollback plan.

## Quality And Performance Tradeoff

P7 should prefer a stable 60 FPS target when feasible and treat 30 FPS as the minimum acceptable lower bound for prototype builds. Visual quality must be reduced before gameplay stability is compromised.

Tradeoff order:

1. Reduce imported area size.
2. Reduce highest LOD coverage.
3. Reduce texture resolution or compress textures.
4. Reduce material count and shader variation.
5. Disable nonessential categories.
6. Simplify collision.
7. Use chunk loading or streaming if static loading is too heavy.

## Collision Simplification

P7 high-detail visual assets should not imply high-detail collision.

Collision policy:

- Prefer no colliders for decorative/background city meshes.
- Use simple primitive or low-poly proxy colliders only where interaction requires it.
- Do not generate per-triangle MeshColliders for broad Chuo city geometry without benchmark approval.
- Keep shelter/gameplay collision separate from PLATEAU visual geometry unless a later stage explicitly approves integration.

## Material Count Strategy

High material count can increase draw calls, memory, and shader variant pressure.

P7 should record:

- Unique material count by imported area and category.
- Shared-material opportunities.
- Transparent material count.
- Texture set count.
- Shader count and render pipeline assumptions.

Preferred strategy:

- Reuse shared materials where visual difference is not important.
- Avoid per-building material uniqueness unless required for key areas.
- Keep material reductions reversible and documented.
- Use Frame Debugger evidence before material consolidation work.

## Texture Compression And Atlas Strategy

P7 should not import broad high-resolution textures blindly.

Texture policy:

- Record texture count, dimensions, and estimated memory.
- Prefer platform-appropriate compression for Windows x64 after visual checks.
- Consider atlasing only for static, repeated, compatible materials.
- Avoid atlasing that destroys per-object culling, LOD control, or material semantics without a benchmark.
- Keep source textures and generated textures out of Git unless a later plan approves small tracked samples.

## LOD Popping Risk

LOD popping is expected when high-detail models switch too close to the camera or when LOD categories differ semantically.

Mitigation candidates:

- Tune LODGroup screen-relative thresholds per category.
- Use cross-fade only after measuring cost and visual quality.
- Avoid mixing visually incompatible LODs in the same key camera path.
- Record screenshots/video notes for benchmark viewpoints.

## Small-Area Benchmark Requirement

No full Chuo high-detail import is approved until a small-area benchmark is complete.

The benchmark must compare at least:

- LOD2 baseline.
- LOD3 average-target candidate.
- LOD4 selected-key-area candidate if available.
- Collision disabled vs simplified collision where relevant.
- Material/texture baseline vs reduced variant if feasible.

The benchmark must produce a Markdown record using `docs/P7_PERFORMANCE_METRICS_TEMPLATE.md`.
