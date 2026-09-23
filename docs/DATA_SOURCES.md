# Data Sources

The final build uses project-local, preprocessed data and Unity scene assets. Runtime gameplay does not perform web requests.

Primary data/source families:

- Project PLATEAU 3D city model data for Tokyo Chuo City, used as the 3D urban model source.
- Chuo/Tokyo official or open datasets represented in the project pipeline for official shelter references and qualification metadata.
- OpenStreetMap-derived route/name/reference data where present in preprocessing outputs.
- GSI or other Japanese open data references where present in preprocessing outputs.
- Project-authored gameplay proxy data for training targets, local hazards, safe-floor behavior, crowd delay, collapse/debris, labels, and tuning reports.

Notes:

- Labels and routes are informational/prototype outputs.
- Route guidance is not official disaster guidance.
- Non-official candidates remain non-official and warning-required.
- Runtime does not query OSM, GSI, Nominatim, Overpass, or other web services.
