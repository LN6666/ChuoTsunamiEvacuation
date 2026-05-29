# NewMap Name Label Attribution

Generated: 2026-05-29T00:00:00+09:00

Current runtime cache status: no online-derived building or road labels have been added.

Current preprocessing status:
- `tools/map/enrich_newmap_names_from_coordinates.ps1` has been run without online lookup.
- The cache currently contains project/source candidate labels only.
- Japanese/Kanji main names are kept when present in project data.
- ID-only strings, full-address-like strings, and low-confidence entries are not normal-mode labels.
- Disabled/out-of-map targets have no runtime labels.

If `tools/map/enrich_newmap_names_from_coordinates.ps1 -AllowOnline` is used:
- Query results are cached in `Assets/Data/P10/newmap_name_cache.json`.
- The enrichment report records provider, query count, and accepted/rejected names.
- OSM/Nominatim-derived labels require OpenStreetMap attribution and ODbL/data-source consideration.
- Labels are informational only and are not official shelter, safety, or evacuation route certification.

Runtime remains offline-only.
