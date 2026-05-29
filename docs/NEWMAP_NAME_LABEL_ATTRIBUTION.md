# NewMap Name Label Attribution

Runtime labels are loaded from `Assets/Data/P10/newmap_name_cache.json`.

Sources used:
- Project official shelter anchor report
- Project non-official candidate resource
- Project P8 humanitarian high-rise candidate audit
- Local OpenStreetMap cache where road or landmark names are present

OpenStreetMap-derived labels require OSM attribution. They are used only as local cached preprocessing outputs and are not queried at runtime.

The cache stores provider, source, confidence, raw type/class, timestamp, and disabled/id-only flags for each label.
