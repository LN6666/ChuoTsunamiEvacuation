# P8-B Evidence-Backed Front Model

Date: 2026-05-24.

## Model Position

P8-B now answers "what drives the risk front" with a hazard-layer v1 model:

1. Load `tsunami_hazard_sample_chuo.json`.
2. Validate source mode, evidence IDs, science/visual separation, boundary kind, geometry type, and collapse-disabled state.
3. Select the active hazard feature by `arrivalTimeSeconds`.
4. Convert `inundationBoundary` into a local data boundary when present.
5. Fall back to a procedural prototype boundary only when the selected feature boundary is incomplete.
6. Use `inundationDepthMeters` and `hazardIntensity` to compute a visual warning intensity.
7. Render only the cinematic visual curtain; gameplay success/failure rules are unchanged.

## Evidence Boundaries

Current v1 records are manual-sample records, not official values.

Allowed claim:

- "The P8-B risk front is driven by hazard-layer v1 fields and manual-sample evidence-shaped records."

Disallowed claim:

- "The P8-B front is an official Chuo inundation map."
- "The P8-B front is an academic tsunami simulation."
- "The cinematic curtain height is real tsunami height."
- "The current depth/intensity values are final hazard values."
- "`visualHeightMeters` is a scientific inundation-depth, water-level, or tsunami-height field."

## Driver Fields

| Field | Runtime Use | Claim Safety |
|---|---|---|
| `arrivalTimeSeconds` | Chooses upcoming/latest feature and computes arrival progress. | Manual-sample unless reviewed source attached. |
| `inundationBoundary` | Supplies data/prototype geometry for the visual boundary. | Prototype unless `boundaryIsEvidenceBasedOrPrototype=evidence_based` and reviewed evidence exists. |
| `inundationDepthMeters` | Raises visual warning intensity. | Not final official depth in current sample. |
| `hazardIntensity` | Raises visual warning intensity. | Prototype normalized signal in current sample. |
| `confidence` | Reported in status for transparency. | Current value documents low confidence. |
| `evidenceSourceId` | Links each feature to evidence metadata. | Required for manual/evidence-planned records. |
| `sourceMode` | Prevents official-looking runtime modes. | `manual_sample` in v1. |

## P8-C Relationship

P8-C should use the same feature boundaries, arrival times, affected infrastructure type flags, and provenance fields when adding infrastructure interactions. P8-C must not infer official road/building/bridge/underground hazard behavior from the current manual sample alone.
