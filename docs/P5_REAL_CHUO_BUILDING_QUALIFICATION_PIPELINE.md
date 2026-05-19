# P5 Real Chuo Building Qualification Pipeline

## P5-B4 Status

P5-B4 is blocked by missing approved real inputs.

The project-local P5 Python environment was created successfully under `data_pipeline/.venv`, and required B4 packages import from that environment:

- `jsonschema`
- `pytest`
- `geopandas`
- `shapely`
- `pyproj`
- `networkx`

`osmnx` is also installed, but B4 does not use OSM routing.

## Input Search Result

Local repository/project paths inspected:

- `data_pipeline/raw/`
- `data_pipeline/sources/`
- `data_pipeline/processed/`
- `data_pipeline/processed/release/`
- `data_pipeline/qualification/`
- `data_pipeline/manual/`
- `data_pipeline/input/`
- `data_pipeline/external/`

Known local PLATEAU data root was checked only at directory level:

- `D:\PLATEAU_DATA\Chuo_2025_CityGML`
- `D:\PLATEAU_DATA\Chuo_2025_3DTiles_MVT`
- `D:\PLATEAU_DATA\Chuo_2025_Related`
- `D:\PLATEAU_DATA\Original_Zip`

No real PLATEAU building footprint/attribute input suitable for B4 matching was found in the repository or approved processed paths.

## Blockers

The available shelter-like inputs are P3 synthetic samples:

- `data_pipeline/processed/real_chuo_shelters_sample.json`
- `data_pipeline/processed/release/real_chuo_shelters_sample.json`

Both explicitly state that records are synthetic placeholders and not official.

The available source registry files identify candidate official sources but do not contain downloaded, manually reviewed, or approved official records:

- `data_pipeline/sources/source_candidates.json`
- `data_pipeline/sources/source_manifest.json`

The available PLATEAU data is raw/local source data, not a processed building footprint fixture:

- raw CityGML exists under `D:\PLATEAU_DATA\Chuo_2025_CityGML`
- no approved small processed building footprint GeoJSON/CSV/GPKG was found
- parsing raw CityGML or full PLATEAU data is outside this B4 prompt

Because of these gaps, B4 cannot honestly produce full real Chuo building qualification and PLATEAU matching outputs.

## Not Created

The following B4 success outputs were not created because doing so would require fabricating data or using synthetic fixtures as if they were real:

- `data_pipeline/processed/qualification/real_chuo_building_qualification.json`
- `data_pipeline/processed/qualification/real_chuo_building_qualification.csv`
- `data_pipeline/processed/qualification/real_chuo_shelter_building_matches.json`
- `data_pipeline/processed/qualification/real_chuo_shelter_building_matches.csv`
- `data_pipeline/processed/qgis_qa/real_chuo_shelter_points.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_building_footprints.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_match_lines.geojson`
- `data_pipeline/processed/qgis_qa/real_chuo_low_confidence_or_unmatched.geojson`

## Required To Unblock

P5-B4 needs both of the following before implementation:

1. A small, approved real Chuo official shelter/evacuation facility input with source provenance and license/terms review.
2. A small, approved processed PLATEAU building footprint/attribute input, such as GeoJSON or CSV plus geometry, prepared without parsing raw CityGML inside this milestone.

The processed PLATEAU input should include:

- `plateauBuildingId`
- geometry or footprint representation
- CRS metadata
- building name if available
- height/floor/use attributes if available
- source/provenance notes

## Validation Result

No B4 output validation or matching tests were run because the required real input contract is not satisfiable with the current repository inputs.

## Next Step

P5-B4-unblock should prepare the missing inputs explicitly:

- manually review and add a small real official source fixture if license/terms allow it
- create an approved small processed PLATEAU building footprint/attribute fixture outside Unity and outside raw CityGML parsing
- then rerun the B4 qualification and matching implementation prompt
