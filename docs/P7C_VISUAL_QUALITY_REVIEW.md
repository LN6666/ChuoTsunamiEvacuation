# P7-C Visual Quality Review

## Scope

This review is limited to the P7Benchmark sandbox and imported candidate `53393690`.

P7-C does not verify full production visual quality. It records visual-quality risks and prepares controlled checks for P7-D.

## Current Evidence

The import inspection found:

- 5843 non-meta files.
- 6 `.gml` CityGML source files.
- 5837 `.jpg` texture files.
- 0 renderable Unity/model/prefab assets by extension scan.
- 0 LOD4 evidence in current P7 records.

Conclusion: `53393690` remains raw CityGML plus texture source files in the P7Benchmark sandbox. Geometry/material conversion remains pending.

## Material And Texture Risk

The texture count is high for a single benchmark candidate. Risks:

- material explosion after conversion
- many unique texture bindings
- high texture memory pressure
- fragmented draw calls if each surface becomes a unique material
- inconsistent material import settings if future conversion is uncontrolled

P7-C does not change texture import settings, ProjectSettings, render pipeline settings, or packages.

## Renderability Risk

The imported `.gml` files are source data, not verified Unity meshes.

P7-C therefore uses metadata-driven placeholder chunks. This avoids claiming visual fidelity before a CityGML-to-Unity mesh conversion workflow exists.

## Lighting, Shadow, And AA Variables

For P7-D, visual checks should treat these as controlled variables:

- anti-aliasing mode and quality
- shadow distance and shadow resolution
- directional light angle and intensity
- material consistency after conversion
- texture compression and mipmap policy
- camera distance for near/mid/far comparisons

P7-C records these as future benchmark variables only. It does not modify `ProjectSettings`.

## LOD Visual Policy

P7-C defines the visual policy but does not prove it with converted meshes:

- near: highest available converted detail for the active benchmark chunk
- mid: simplified visual representation or lower-detail converted mesh
- far: coarse building/context representation

LOD3 remains benchmark-only. LOD4 is not assumed available.

## Visual Claim Limit

The only P7-C visual claim is that placeholder chunk groups can be enabled and disabled in the P7Benchmark scene.

No production visual quality, geometry correctness, material correctness, or final frame-rate quality is claimed.
