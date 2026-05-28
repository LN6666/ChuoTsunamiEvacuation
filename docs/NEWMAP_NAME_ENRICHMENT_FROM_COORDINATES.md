# NewMap Name Enrichment From Coordinates

Generated: 2026-05-29T00:00:00+09:00

Purpose: optionally enrich missing road/building labels from coordinate-based open map sources during preprocessing only.

Runtime rule:
- Unity player must never perform online geocoding or name lookup.
- Runtime reads `Assets/Data/P10/newmap_name_cache.json`.

Tooling:
- PowerShell wrapper: `tools/map/enrich_newmap_names_from_coordinates.ps1`
- Python tool: `tools/map/enrich_newmap_names_from_coordinates.py`
- Config: `Assets/Data/P10/newmap_name_enrichment_config.json`
- Cache: `Assets/Data/P10/newmap_name_cache.json`
- Report: `Assets/Data/P10/newmap_name_enrichment_report.json`

Online policy:
- Public Nominatim use requires a valid identifying user agent, caching, attribution, and no heavy use.
- The tool is single-process and rate-limited to at least 1.1 seconds per request.
- The tool requires the `-AllowOnline` flag before it performs online queries.
- Bulk public Nominatim use is discouraged; offline extracts or a dedicated provider should be used for larger runs.

Label acceptance:
- Existing source/project names are preferred.
- Address-only reverse-geocode results are not treated as building names.
- Low-confidence results are hidden in normal mode.
- No names are fabricated.

References:
- Nominatim usage policy: https://operations.osmfoundation.org/policies/nominatim/
- Nominatim reverse API: https://nominatim.org/release-docs/latest/api/Reverse/
