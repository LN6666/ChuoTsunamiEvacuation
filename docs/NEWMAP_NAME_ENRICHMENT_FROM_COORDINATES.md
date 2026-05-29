# NewMap Name Enrichment From Coordinates

Tool: `tools/map/enrich_newmap_names_from_coordinates.py`

The tool is preprocessing-only. It writes `Assets/Data/P10/newmap_name_cache.json`; Unity runtime only reads this cache and never calls the network.

Source priority:
- active official shelter names from project reports
- active non-official candidate names from runtime resource data
- PLATEAU/project building names from the P8 candidate audit
- local OSM cache for road and Tokyo Station labels when available
- online reverse lookup only for missing selected names, rate-limited and capped

Current run produced a local cache with official shelter, non-official candidate, building, road, and Tokyo Station labels. Online lookup was not needed because source/project/local OSM data supplied the selected names.

Cache result:
- total labels: 94
- active official shelters: 15
- active non-official candidates: 19
- building labels: 19
- road labels: 40
- Tokyo Station labels: 1
- runtime network requests allowed: false
