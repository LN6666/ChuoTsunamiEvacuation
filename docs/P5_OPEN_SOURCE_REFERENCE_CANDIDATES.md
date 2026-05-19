# P5 Open-Source Reference Candidates

## Purpose

This registry records open-source projects that may support Phase 5 evidence review, GIS processing, PLATEAU inspection, routing, or Unity integration.

P5-A0 records candidates only. It does not install dependencies, modify `requirements.txt`, import sample code, modify Unity `Packages`, modify `ProjectSettings`, or copy external project assets.

Use modes:

- `reference_only`: inspect concepts, architecture, or examples without adopting code.
- `optional_tool`: may be used manually for QA or inspection, but is not required by the reproducible pipeline.
- `dependency_candidate`: candidate for later controlled dependency adoption after review.
- `dependency`: approved project dependency. No P5-A0 candidate has this status.

## Candidate 1: PLATEAU SDK for Unity GIS Sample

- Name: PLATEAU SDK for Unity GIS Sample
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-C
- Expected role: Unity GIS visualization and PLATEAU-related visualization reference
- useMode: `reference_only` unless later approved
- Decision status: `evaluate_in_P5A1`
- Risk notes: may introduce `ProjectSettings`, `Packages`, sample-scene, or asset dependency pollution; sample project assumptions may not match this Unity project.
- Boundary notes: do not import the sample project directly in P5-A0; do not modify Unity packages or settings.
- Next evaluation step: review repository scope, license, supported Unity/PLATEAU SDK version, and whether any pattern can be documented without code adoption.

## Candidate 2: PLATEAU QGIS Plugin

- Name: PLATEAU QGIS Plugin
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-B
- Expected role: QGIS inspection and PLATEAU CityGML visual QA candidate
- useMode: `optional_tool` / QA tool candidate
- Decision status: `evaluate_in_P5A1`
- Risk notes: may be beta or experimental; not ideal as a reproducible pipeline dependency; output behavior may depend on local QGIS/plugin versions.
- Boundary notes: may be used for manual QA later, but not as a Unity runtime dependency or required automated pipeline dependency unless separately approved.
- Next evaluation step: review plugin maturity, license, supported QGIS versions, supported PLATEAU formats, and manual QA value.

## Candidate 3: OSMnx

- Name: OSMnx
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-B routing pipeline
- Expected role: OSM pedestrian network download, graph modeling, and prototype route generation
- useMode: `dependency_candidate`
- Decision status: `evaluate_in_P5A1`
- Risk notes: OSM attribution/license requirements; network completeness and tag quality vary; routes are not official evacuation routes.
- Boundary notes: any OSM-derived routes must be labeled as estimated prototype routes and kept distinct from official evacuation guidance.
- Next evaluation step: review license/attribution, network extraction reproducibility, offline/cache policy, and whether P5-B should depend on it.

## Candidate 4: GeoPandas

- Name: GeoPandas
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-B
- Expected role: core geospatial vector processing for official records, candidate layers, and processed output generation
- useMode: `dependency_candidate` / likely dependency
- Decision status: `evaluate_in_P5A1`
- Risk notes: Windows dependency setup can be fragile; CRS discipline is required; version compatibility with IO libraries must be controlled.
- Boundary notes: do not add to `requirements.txt` in P5-A0; evaluate with the rest of the geospatial stack before adoption.
- Next evaluation step: review dependency chain, supported Python versions, install strategy, CRS handling, and schema/export needs.

## Candidate 5: Shapely

- Name: Shapely
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-B
- Expected role: geometry operations for building footprints, buffers, intersections, nearest geometry checks, and spatial QA helpers
- useMode: `dependency_candidate`
- Decision status: `evaluate_in_P5A1`
- Risk notes: geometry validity, coordinate units, and CRS assumptions can cause incorrect spatial decisions if unmanaged.
- Boundary notes: no adoption in P5-A0; any later use must be covered by tests for geometry validity and CRS expectations.
- Next evaluation step: review API fit for qualification/matching geometry operations and compatibility with GeoPandas.

## Candidate 6: pyproj

- Name: pyproj
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-B
- Expected role: CRS transforms for source data, PLATEAU-derived geometry, and Unity-ready coordinate preparation
- useMode: `dependency_candidate`
- Decision status: `evaluate_in_P5A1`
- Risk notes: wrong CRS selection or axis-order mistakes can create large spatial errors.
- Boundary notes: no adoption in P5-A0; later pipeline code must make CRS and axis order explicit.
- Next evaluation step: review needed source CRSs, target coordinate policy, and test strategy for known reference points.

## Candidate 7: NetworkX

- Name: NetworkX
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-B routing pipeline
- Expected role: routing graph processing and shortest-path calculations when routing graphs are prepared from OSM or other network data
- useMode: `dependency_candidate`
- Decision status: `evaluate_in_P5A1`
- Risk notes: graph construction assumptions may dominate route quality; performance may be limited for large graphs.
- Boundary notes: no adoption in P5-A0; routes must remain prototype estimates unless officially sourced.
- Next evaluation step: review routing graph size, path cost model, and compatibility with OSMnx or a lighter custom graph export.

## Candidate 8: GDAL / Fiona / pyogrio Stack

- Name: GDAL / Fiona / pyogrio stack
- Source link: TBD in P5-A1 official repository/source review
- Phase relevance: P5-B
- Expected role: geospatial file IO for vector data, GeoPackage/Shapefile/GeoJSON handling, and pipeline interoperability
- useMode: `dependency_candidate`
- Decision status: `evaluate_in_P5A1`
- Risk notes: Windows installation and binary compatibility can be fragile; transitive dependency versions must be controlled.
- Boundary notes: no installation or requirements changes in P5-A0; choose only the minimum IO stack needed for reproducible outputs.
- Next evaluation step: review official source file formats, install options, license implications, and whether pyogrio/Fiona/GDAL are needed directly or only through GeoPandas.
