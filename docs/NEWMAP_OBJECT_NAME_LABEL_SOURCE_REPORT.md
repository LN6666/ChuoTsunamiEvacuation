# NewMap Object Name Label Source Report

Generated: 2026-05-29T00:00:00+09:00

Source-name rule: do not fabricate building or road names.

Current source status:
- Generic building names from imported model metadata: `no source name available`
- Road names from imported model metadata: `no source name available`
- Official shelter names: available from existing official shelter anchor records
- Non-official candidate names: available from existing project candidate dataset
- Tokyo Station label: not shown unless a source/cache entry exists
- ID-only labels: hidden in normal mode

Runtime behavior:
- Shows official shelter labels only when their verified runtime targets are active.
- Shows non-official candidate labels with explicit non-official wording.
- Shows road/building labels only from source metadata or confidence-gated local cache.
- Does not run web requests.
