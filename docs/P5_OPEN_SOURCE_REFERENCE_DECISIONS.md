# P5 Open-Source Reference Decisions

## Purpose

This document records preliminary P5-A1 decisions for the open-source reference candidates listed in `docs/P5_OPEN_SOURCE_REFERENCE_CANDIDATES.md`.

P5-A1 does not install dependencies, modify `requirements.txt`, import sample code, modify Unity `Packages`, modify `ProjectSettings`, or adopt any tool as a dependency. These decisions are planning inputs for P5-A2 and P5-B.

## Decision Summary

| Candidate | useMode | Decision status | Phase |
|---|---|---|---|
| PLATEAU SDK for Unity GIS Sample | `reference_only` | `preliminary_accept_reference_only` | P5-C |
| PLATEAU QGIS Plugin | `optional_tool` | `preliminary_accept_manual_qa_candidate` | P5-B |
| OSMnx | `dependency_candidate` | `evaluate_before_p5b_routing` | P5-B |
| GeoPandas | `likely_dependency` | `evaluate_before_p5b_pipeline` | P5-B |
| Shapely | `likely_dependency` | `evaluate_before_p5b_pipeline` | P5-B |
| pyproj | `likely_dependency` | `evaluate_before_p5b_pipeline` | P5-B |
| NetworkX | `dependency_candidate` | `evaluate_before_p5b_routing` | P5-B |
| GDAL / Fiona / pyogrio stack | `dependency_candidate` | `evaluate_before_p5b_io` | P5-B |

## PLATEAU SDK for Unity GIS Sample

- useMode: `reference_only`
- decisionStatus: `preliminary_accept_reference_only`
- phase: P5-C
- reason: useful for Unity GIS/attribute visualization patterns and PLATEAU-related UI/display ideas.
- risks: importing sample assets or project settings could pollute `ProjectSettings`, `Packages`, scenes, or asset organization.
- boundary: do not import the sample project directly; avoid `ProjectSettings` / `Packages` pollution; reuse only documented patterns after review.
- nextAction: review concepts and compatibility later, then decide whether any pattern should be reimplemented locally in a small Unity integration milestone.

## PLATEAU QGIS Plugin

- useMode: `optional_tool`
- decisionStatus: `preliminary_accept_manual_qa_candidate`
- phase: P5-B
- reason: useful for QGIS inspection of PLATEAU/CityGML layers and manual spatial QA.
- risks: beta/experimental behavior, version-specific QGIS behavior, and weak reproducibility if pipeline results depend on manual plugin state.
- boundary: not a reproducible pipeline dependency unless later approved; not a Unity runtime dependency.
- nextAction: evaluate as a manual QA tool during P5-A2/P5-B planning, especially for visual inspection of PLATEAU building matching assumptions.

## OSMnx

- useMode: `dependency_candidate`
- decisionStatus: `evaluate_before_p5b_routing`
- phase: P5-B routing
- reason: useful for OSM pedestrian network modeling and prototype route generation.
- risks: OSM attribution/license requirements, network completeness, changing OSM data, pedestrian access tagging gaps, and possible mismatch with official evacuation guidance.
- boundary: OSM-derived routes must be labeled as estimated prototype routes, not official evacuation routes.
- nextAction: review license/attribution, reproducibility, cache/offline strategy, route labeling, and whether route graph generation should be part of P5-B.

## GeoPandas

- useMode: `likely_dependency`
- decisionStatus: `evaluate_before_p5b_pipeline`
- phase: P5-B
- reason: core vector geospatial processing, CRS-aware data frames, spatial joins, and GeoJSON/CSV/GPKG-friendly output workflows.
- risks: Windows dependency setup, IO backend compatibility, CRS mistakes, and data-frame schema drift.
- boundary: use with explicit CRS metadata, focused tests, and pinned dependency review; do not add to `requirements.txt` in P5-A1.
- nextAction: evaluate installation path, version compatibility, and expected P5-B output formats before adoption.

## Shapely

- useMode: `likely_dependency`
- decisionStatus: `evaluate_before_p5b_pipeline`
- phase: P5-B
- reason: geometry contains, nearest, distance, intersection, and buffer operations needed for building matching and spatial QA.
- risks: metric distance errors if used in geographic CRS, invalid geometries, and ambiguous boundary behavior.
- boundary: use projected CRS for metric distance and validate geometries before match decisions.
- nextAction: define geometry validation and known-point distance tests in P5-A2/P5-B planning.

## pyproj

- useMode: `likely_dependency`
- decisionStatus: `evaluate_before_p5b_pipeline`
- phase: P5-B
- reason: CRS conversion, coordinate validation, and explicit transform handling between source data, PLATEAU-derived geometry, and Unity-ready outputs.
- risks: wrong EPSG selection, axis-order mistakes, and hidden CRS assumptions.
- boundary: record CRS metadata explicitly; use always-xy style conventions where appropriate and test known reference points.
- nextAction: define the expected CRS policy and transform tests before P5-B implementation.

## NetworkX

- useMode: `dependency_candidate`
- decisionStatus: `evaluate_before_p5b_routing`
- phase: P5-B routing
- reason: graph routing support, either directly or through OSMnx.
- risks: duplicating OSMnx functionality, large graph performance, and route-cost assumptions that may not reflect evacuation behavior.
- boundary: avoid direct use if OSMnx already covers the needed graph operations; keep routing outputs labeled as prototype estimates.
- nextAction: decide whether NetworkX is explicitly needed after the OSMnx route graph plan is reviewed.

## GDAL / Fiona / pyogrio Stack

- useMode: `dependency_candidate`
- decisionStatus: `evaluate_before_p5b_io`
- phase: P5-B
- reason: geospatial file IO for GeoJSON, GeoPackage, shapefile, and related vector data formats.
- risks: Windows binary compatibility, transitive version conflicts, heavy install footprint, and format-specific behavior differences.
- boundary: choose the simplest stable Windows-compatible path later; do not add IO dependencies in P5-A1.
- nextAction: identify actual official source formats and choose the minimum IO backend required for reproducible P5-B outputs.
