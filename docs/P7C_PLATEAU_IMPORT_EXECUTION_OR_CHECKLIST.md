# P7-C PLATEAU Import Execution Or Checklist

## Automation Result

Codex did not automate a full PLATEAU SDK import in P7-C. The SDK package reference is present in `Packages/manifest.json`, but P7-C does not modify `Packages` or `ProjectSettings` and does not touch `Assets/PLATEAU`.

P7-C creates a safe scene shell and read-only inspection tooling. Actual high-detail import remains a Unity Editor / PLATEAU SDK workflow unless a safe command-line SDK path is later confirmed.

If the SDK requires importing into `Assets/PLATEAU` or another protected path, stop and request approval before proceeding.

## Target Settings

| Category | Target setting |
|---|---|
| Buildings | Import, target LOD3 |
| Roads | Import, target LOD3 |
| Urban planning decision information | Import, target LOD1 |
| Land use | Import |
| Underground city / streets / spaces | Import, target LOD3 if available |
| City furniture / urban equipment | Import, target LOD2 or LOD3 depending availability and performance |
| Water body | Import, target LOD1 |
| Vegetation | Import, target LOD3 if available |
| Bridges | Import, target LOD3 |
| Disaster risk | Import |
| Relief / terrain | Import |

User-provided screenshots/settings are target settings, not completion proof.

## Preferred Import Destination

Use one of these P7-specific destinations if the PLATEAU SDK allows it:

- `Assets/P7HighDetail/Imported/`
- `Assets/P7HighDetail/PLATEAU/`

Do not modify `Assets/PLATEAU`, `Assets/Data`, `ProjectSettings`, or `Packages` without separate approval.

## Manual Checklist

1. Open Unity on branch `p7-high-detail-city-foundation`.
2. Open `Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity`.
3. In the PLATEAU SDK import UI, select the Chuo CityGML source from `D:\PLATEAU_DATA\Chuo_2025_CityGML`.
4. Select the target categories and LOD settings from the table above.
5. Direct output to a P7-specific path if the SDK supports it.
6. Verify imported objects appear under the scene layer roots or a clearly mapped import root.
7. Save only the P7 high-detail scene and P7-specific assets.
8. Run `tools/p7/inspect_p7c_plateau_import_readiness.ps1`.
9. Run `tools/p7/inspect_p7c_high_detail_scene.ps1`.
10. Run P7-C preflight and Unity GUI tests before commit.

## Pending Work

The current P7-C state does not prove renderable high-detail Unity meshes for the full Chuo scene. Actual import, visual verification, and profiler measurements remain required before final P7-D profiling.
