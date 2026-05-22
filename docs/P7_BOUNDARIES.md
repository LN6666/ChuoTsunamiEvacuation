# P7 Boundaries

Date: 2026-05-22

## Allowed P7-0 Scope

P7-0 may create or update only documentation, P7 command-line tools, Codex prompt records, and DeepSeek review prompts that define the P7 high-detail city foundation.

Allowed P7-0 work:

- Reference review and technical selection notes.
- LOD and asset strategy documentation.
- Asset inventory and benchmark protocols.
- PowerShell scope/status/preflight scripts under `tools/p7/`.
- P7 task and review backlog entries.
- DeepSeek review prompt for P7-0.

P7-0 must not change gameplay behavior, Unity assets, scenes, packages, project settings, or data files.

## Protected Paths

The following paths are protected in P7-0 and must remain untouched:

- `ProjectSettings/`
- `Packages/`
- `Assets/Scenes/`
- `Assets/PLATEAU/`
- `Assets/Scripts/`
- `Assets/Data/`

The local generated base map scene remains protected:

- `Assets/Scenes/Chuo_BaseMap.unity`

Local raw PLATEAU data remains outside Git and must not be modified by P7-0:

- `D:\PLATEAU_DATA\Chuo_2025_CityGML`

## Forbidden Scope

P7 must not implement:

- Real tsunami fluid simulation.
- Tsunami height, inundation depth, dynamic light curtain, or P8 hazard systems.
- Indoor shelter evacuation or P9 indoor navigation systems.
- Real crowd simulation.
- NPC failure mechanics.
- Road-blocking gameplay.
- Building-collapse gameplay.
- Real spawn point systems.
- New gameplay success/failure rules.

P7-0 must not create P7-E, P7-F, or P7-G.

## Dependency Approval Rules

P7-0 adopts no new dependency.

Any future dependency candidate must have a Markdown decision record before installation or package modification. The record must include:

- Package or repository name.
- License.
- Required Unity version and render pipeline.
- Whether `Packages/` or `ProjectSettings/` must change.
- Import size and generated asset risk.
- Rollback plan.
- Benchmark reason for adoption.
- Human approval.

No Codex agent may add a package, modify `Packages/`, or modify `ProjectSettings/` without explicit approval in the stage prompt or a confirmed Markdown plan.

## Asset Import Rules

P7-0 imports no assets.

Future P7 asset work must follow this order:

1. Inventory local source folders without importing.
2. Select a small benchmark area.
3. Import or generate only the approved benchmark area.
4. Record size, object count, material count, texture count, draw calls, triangles, RAM, VRAM/texture memory if available, and FPS.
5. Approve or reject the import path before full Chuo work.

Generated large scenes, imported PLATEAU assets, raw CityGML, large GIS files, and benchmark build outputs must not be committed unless a later plan explicitly approves a small tracked artifact.

## Full Chuo Import Safety Rule

Full Chuo import is not allowed directly from P7-0. It is blocked until P7-A inventory and P7-B small-area benchmark demonstrate that the selected LOD, material, texture, collision, and chunking strategy is viable.

The safe sequence is:

1. Inventory the local PLATEAU source and existing Unity import.
2. Select representative benchmark areas.
3. Benchmark LOD2/LOD3/LOD4 combinations on small areas.
4. Define rollback criteria.
5. Only then consider broader Chuo import or chunk loading.

## Windows x64 EXE Optimization Boundary

P7 targets Windows x64 EXE profiling and optimization, but P7-0 does not change player settings, build settings, render pipeline settings, quality settings, or package configuration.

Future P7 Windows optimization must be evidence-driven:

- Measure Editor and Windows x64 separately.
- Prefer reversible scene/asset/data organization changes before global settings changes.
- Record each ProjectSettings or Packages change in a decision log before applying it.

## P7 / P8 / P9 / P10 Separation

P7 owns city asset loading, LOD, streaming, underground/bridge feasibility, and performance measurement.

P8 owns hazard visualization and tsunami data systems.

P9 owns indoor shelter and indoor evacuation systems.

P10 owns final integration/presentation/closeout. P10 may cite P7 metrics, but P7 must not start P10 presentation polish unless explicitly approved.
