# P7 Asset Inventory Protocol

Date: 2026-05-22

Status: Protocol only. P7-0 performs no asset import.

## Purpose

P7-A must inventory local Chuo PLATEAU and Unity-generated asset candidates before any full import or optimization work. The inventory should be command-line-friendly, repeatable, and safe to run without opening Unity.

## Inputs

Candidate input roots:

- Existing Unity project asset folders, excluding protected paths unless a future P7-A plan allows read-only inspection.
- Local PLATEAU source data under `D:\PLATEAU_DATA\Chuo_2025_CityGML`.
- Existing import logs and documentation.

P7-0 does not scan or import large data. P7-A may scan local paths only after the stage prompt approves the exact command.

## Inventory Rules

- Do not import assets during inventory.
- Do not modify raw PLATEAU data.
- Do not write into `Assets/Scenes/`, `Assets/PLATEAU/`, `Assets/Scripts/`, or `Assets/Data/`.
- Do not download data.
- Do not add dependencies.
- Do not parse CityGML deeply unless a later stage approves the parser and output path.
- Prefer file-system metadata first: path, extension, size, modified time, and likely category.

## Required Fields

Markdown and CSV inventory records should include:

| Field | Description |
|---|---|
| inventoryRunId | Timestamp or stable run identifier. |
| sourceRoot | Root scanned. |
| relativePath | Relative file or directory path. |
| extension | File extension. |
| fileSizeBytes | File size for files. |
| likelyPlateauCategory | Inferred category such as bldg, tran, brid, urf, fld, veg, luse, dem, or unknown. |
| candidateLodLevel | Inferred LOD such as LOD1, LOD2, LOD3, LOD4, or unknown. |
| isLargeFile | Whether the file exceeds the chosen P7-A threshold. |
| importCandidate | yes/no/manual_review. |
| notes | Short risk or follow-up note. |

## Counts To Record

Each inventory run should summarize:

- Total file count.
- Total bytes by root.
- File count by extension.
- File count and bytes by likely PLATEAU category.
- File count and bytes by candidate LOD.
- Top large files.
- Empty or missing expected category folders.
- Paths that should remain local-only and untracked.

## Output Paths

Preferred P7-A output paths, subject to the P7-A stage prompt:

- `docs/p7_inventory/P7_A_ASSET_INVENTORY_<date>.md`
- `docs/p7_inventory/P7_A_ASSET_INVENTORY_<date>.csv`

If CSV output is not approved in a future prompt, record Markdown only.

## Category Heuristics

Initial path heuristics may classify common PLATEAU folder tokens:

| Token | Likely Category |
|---|---|
| `bldg` | Buildings |
| `tran` | Roads / transportation |
| `brid` | Bridges |
| `tun` | Tunnels |
| `urf` | Urban facilities |
| `fld` | Disaster risk / flood |
| `veg` | Vegetation |
| `luse` | Land use |
| `dem` | Terrain / elevation |

Heuristics must be labeled as inferred. They are not a substitute for schema-aware validation.

## P7-A Use

P7-A should use the inventory to choose:

- Benchmark areas.
- Candidate LOD targets.
- Asset categories to include or exclude.
- Expected import size.
- Collision policy.
- Material/texture risk.
- Whether a full Chuo import attempt is too risky.
