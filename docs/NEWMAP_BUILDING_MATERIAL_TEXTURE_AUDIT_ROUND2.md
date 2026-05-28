# NewMap Building Material Texture Audit Round 2

Generated: 2026-05-29T00:00:00+09:00

Status: `audited_lighting_readability_improved_material_texture_limitation_documented`

The current project decision history documents the active base-map import as Buildings/LOD1, so this task does not claim LOD2 material quality. The manual “photo pasted on geometry” issue is treated as a material/appearance audit item, not hidden by replacing all building materials with flat fallback colors.

Round 2 keeps imported PLATEAU materials/textures intact and improves readability through lighting:

- clear day brightness is preserved
- night sky is darkened separately from building lighting
- night building readability uses ambient/fill lighting instead of blackening materials
- generated gameplay materials keep shader fallback coverage
- no blanket dark gray fallback is applied to imported buildings

Classification: `material_limitation_documented`, with `material_valid_plateau_appearance` possible where imported textures are valid but inherently photo/projected.

JSON: `Assets/Data/P10/newmap_building_material_texture_audit_round2.json`
