from __future__ import annotations

import csv
import json
import math
import xml.etree.ElementTree as ET
from collections import Counter
from datetime import datetime, timezone, timedelta
from pathlib import Path
from typing import Any, Iterable

from pyproj import Transformer
from shapely.geometry import LineString, Point, Polygon, mapping
from shapely.ops import transform


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
QUALIFICATION_ROOT = PIPELINE_ROOT / "qualification"
PROCESSED_QUALIFICATION_ROOT = PIPELINE_ROOT / "processed" / "qualification"
QGIS_QA_ROOT = PIPELINE_ROOT / "processed" / "qgis_qa"

NORMALIZED_SHELTERS_JSON = PROCESSED_QUALIFICATION_ROOT / "real_chuo_official_shelters_normalized.json"
RULEBOOK_PATH = QUALIFICATION_ROOT / "evacuation_building_qualification_rulebook.json"
INPUT_MANIFEST_PATH = QUALIFICATION_ROOT / "real_chuo_building_matching_input_manifest.json"

CITYGML_BLDG_ROOT = Path(r"D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\bldg")
SOURCE_CRS = "EPSG:4326"
METRIC_CRS = "EPSG:6677"

OUTPUT_QUALIFICATION_JSON = PROCESSED_QUALIFICATION_ROOT / "real_chuo_building_qualification.json"
OUTPUT_QUALIFICATION_CSV = PROCESSED_QUALIFICATION_ROOT / "real_chuo_building_qualification.csv"
OUTPUT_MATCHES_JSON = PROCESSED_QUALIFICATION_ROOT / "real_chuo_shelter_building_matches.json"
OUTPUT_MATCHES_CSV = PROCESSED_QUALIFICATION_ROOT / "real_chuo_shelter_building_matches.csv"

OUTPUT_SHELTER_POINTS = QGIS_QA_ROOT / "real_chuo_shelter_points.geojson"
OUTPUT_BUILDING_FOOTPRINTS = QGIS_QA_ROOT / "real_chuo_building_footprints.geojson"
OUTPUT_MATCH_LINES = QGIS_QA_ROOT / "real_chuo_match_lines.geojson"
OUTPUT_LOW_CONFIDENCE = QGIS_QA_ROOT / "real_chuo_low_confidence_or_unmatched.geojson"

EXTRACTION_BUFFER_METERS = 140.0
NEAREST_HIGH_CONFIDENCE_METERS = 10.0
NEAREST_MEDIUM_CONFIDENCE_METERS = 30.0
UNMATCHED_DISTANCE_METERS = 80.0

QUALIFICATION_FIELDS = [
    "qualificationId",
    "plateauBuildingId",
    "shelterId",
    "shelterName",
    "officialDesignationStatus",
    "qualificationStatus",
    "qualificationReason",
    "evidenceSources",
    "matchMethod",
    "matchDistanceMeters",
    "confidence",
    "manualReviewNeeded",
    "warnings",
    "disasterTypes",
    "safeFloor",
    "capacity",
    "sourceUpdatedAt",
    "routeAvailability",
    "nearestRouteDistanceMeters",
    "estimatedTravelTimeSeconds",
    "notes",
]

MATCH_FIELDS = [
    "shelterId",
    "shelterName",
    "plateauBuildingId",
    "plateauGmlId",
    "matchMethod",
    "matchDistanceMeters",
    "confidence",
    "manualReviewNeeded",
    "warnings",
    "sourceMeshCode",
]


def now_jst() -> str:
    return datetime.now(timezone(timedelta(hours=9))).replace(microsecond=0).isoformat()


def load_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: Path, payload: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(payload, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def write_csv(path: Path, records: list[dict[str, Any]], fieldnames: list[str]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=fieldnames)
        writer.writeheader()
        for record in records:
            row: dict[str, Any] = {}
            for field in fieldnames:
                value = record.get(field)
                if isinstance(value, (list, dict)):
                    row[field] = json.dumps(value, ensure_ascii=False, separators=(",", ":"))
                elif value is None:
                    row[field] = ""
                else:
                    row[field] = value
            writer.writerow(row)


def local_name(tag: str) -> str:
    return tag.rsplit("}", 1)[-1]


def get_gml_id(element: ET.Element) -> str | None:
    return element.attrib.get("{http://www.opengis.net/gml}id") or element.attrib.get("gml:id")


def first_text_by_local_name(element: ET.Element, names: set[str]) -> str | None:
    for child in element.iter():
        if local_name(child.tag) in names and child.text and child.text.strip():
            return child.text.strip()
    return None


def first_poslist_from(element: ET.Element, container_names: tuple[str, ...]) -> str | None:
    for child in element.iter():
        if local_name(child.tag) not in container_names:
            continue
        for sub in child.iter():
            if local_name(sub.tag) == "posList" and sub.text and sub.text.strip():
                return sub.text.strip()
    return None


def parse_optional_float(value: str | None) -> float | None:
    if value is None:
        return None
    try:
        parsed = float(value)
    except ValueError:
        return None
    if parsed in {-9999.0, 9999.0}:
        return None
    return parsed


def parse_optional_int(value: str | None) -> int | None:
    if value is None:
        return None
    try:
        parsed = int(float(value))
    except ValueError:
        return None
    if parsed in {-9999, 9999}:
        return None
    return parsed


def coords_from_poslist(poslist: str) -> list[tuple[float, float]]:
    values = [float(value) for value in poslist.split()]
    if len(values) < 4:
        return []
    step = 3 if len(values) % 3 == 0 else 2
    coords: list[tuple[float, float]] = []
    for index in range(0, len(values) - step + 1, step):
        first = values[index]
        second = values[index + 1]
        if 20.0 <= first <= 50.0 and 120.0 <= second <= 150.0:
            lat, lon = first, second
        else:
            lon, lat = first, second
        coords.append((lon, lat))
    if coords and coords[0] != coords[-1]:
        coords.append(coords[0])
    return coords


def polygon_from_building(building: ET.Element) -> Polygon | None:
    poslist = (
        first_poslist_from(building, ("lod0FootPrint",))
        or first_poslist_from(building, ("lod0RoofEdge",))
        or first_poslist_from(building, ("GroundSurface",))
    )
    if not poslist:
        return None
    coords = coords_from_poslist(poslist)
    if len(coords) < 4:
        return None
    polygon = Polygon(coords)
    if polygon.is_empty:
        return None
    if not polygon.is_valid:
        polygon = polygon.buffer(0)
    if polygon.is_empty or polygon.geom_type != "Polygon":
        return None
    return polygon


def mesh_code_for(lat: float, lon: float) -> str:
    first_lat = math.floor(lat * 1.5)
    first_lon = math.floor(lon) - 100
    lat_remainder = lat * 1.5 - first_lat
    lon_remainder = lon - math.floor(lon)
    second_lat = math.floor(lat_remainder * 8)
    second_lon = math.floor(lon_remainder * 8)
    third_lat = math.floor((lat_remainder * 8 - second_lat) * 10)
    third_lon = math.floor((lon_remainder * 8 - second_lon) * 10)
    return f"{first_lat:02d}{first_lon:02d}{second_lat}{second_lon}{third_lat}{third_lon}"


def is_likely_non_building_area(name: str | None) -> bool:
    if not name:
        return False
    return any(token in name for token in ("公園一帯", "地区", "リバーシティ"))


def project_geometry(transformer: Transformer, geometry: Any) -> Any:
    return transform(transformer.transform, geometry)


def source_updated_at(evidence_sources: list[dict[str, Any]]) -> str | None:
    for source in evidence_sources:
        if source.get("sourceUpdatedAt"):
            return str(source["sourceUpdatedAt"])
    return None


def plateau_evidence_source(mesh_code: str) -> dict[str, Any]:
    return {
        "sourceId": f"local_plateau_citygml_bldg_mesh_{mesh_code}",
        "sourceFamily": "plateau_building_geometry",
        "sourceName": "Local PLATEAU 2025 Chuo CityGML building mesh subset",
        "officialStatus": "derived",
        "sourceUrl": None,
        "sourceUpdatedAt": None,
        "evidenceNote": (
            "Building footprint was derived from an existing local PLATEAU CityGML building mesh. "
            "Raw CityGML is not committed."
        ),
    }


def shelter_points(shelters: list[dict[str, Any]], transformer: Transformer) -> list[dict[str, Any]]:
    points: list[dict[str, Any]] = []
    for shelter in shelters:
        lat = shelter.get("latitude")
        lon = shelter.get("longitude")
        if not isinstance(lat, (int, float)) or not isinstance(lon, (int, float)):
            continue
        point_wgs84 = Point(float(lon), float(lat))
        point_projected = project_geometry(transformer, point_wgs84)
        points.append(
            {
                "shelter": shelter,
                "pointWgs84": point_wgs84,
                "pointProjected": point_projected,
                "meshCode": mesh_code_for(float(lat), float(lon)),
            }
        )
    return points


def parse_candidate_buildings(
    mesh_files: dict[str, Path],
    points: list[dict[str, Any]],
    transformer: Transformer,
) -> tuple[list[dict[str, Any]], list[str]]:
    candidates: list[dict[str, Any]] = []
    warnings: list[str] = []

    for mesh_code, path in sorted(mesh_files.items()):
        if not path.exists():
            warnings.append(f"Missing local PLATEAU building mesh file for {mesh_code}: {path}")
            continue

        for _event, building in ET.iterparse(path, events=("end",)):
            if local_name(building.tag) != "Building":
                continue

            polygon_wgs84 = polygon_from_building(building)
            if polygon_wgs84 is None:
                building.clear()
                continue

            polygon_projected = project_geometry(transformer, polygon_wgs84)
            min_distance = min(polygon_projected.distance(point["pointProjected"]) for point in points)
            if min_distance <= EXTRACTION_BUFFER_METERS:
                gml_id = get_gml_id(building)
                plateau_id = first_text_by_local_name(building, {"buildingID"}) or gml_id
                measured_height = parse_optional_float(first_text_by_local_name(building, {"measuredHeight"}))
                floors = parse_optional_int(first_text_by_local_name(building, {"storeysAboveGround"}))
                usage = first_text_by_local_name(building, {"usage", "class"})
                building_name = first_text_by_local_name(building, {"name"})
                candidates.append(
                    {
                        "plateauBuildingId": plateau_id,
                        "plateauGmlId": gml_id,
                        "buildingName": building_name,
                        "heightMeters": measured_height,
                        "floors": floors,
                        "usage": usage,
                        "sourceMeshCode": mesh_code,
                        "geometryWgs84": polygon_wgs84,
                        "geometryProjected": polygon_projected,
                        "centroidWgs84": polygon_wgs84.centroid,
                        "areaSquareMeters": polygon_projected.area,
                    }
                )
            building.clear()

    return candidates, warnings


def classify_match(
    shelter: dict[str, Any],
    containing: list[dict[str, Any]],
    nearest: dict[str, Any] | None,
    nearest_distance: float | None,
) -> tuple[dict[str, Any] | None, str, float | None, str, bool, list[str]]:
    warnings: list[str] = []

    if is_likely_non_building_area(shelter.get("shelterName")):
        return (
            None,
            "unmatched",
            None,
            "low",
            True,
            [
                "official source appears to describe a broad evacuation area rather than a single building",
                "manual QGIS review required before assigning any PLATEAU building",
            ],
        )

    if containing:
        selected = min(containing, key=lambda candidate: candidate["areaSquareMeters"])
        if len(containing) > 1:
            warnings.append("multiple PLATEAU building footprints contain the shelter point")
            return selected, "contains", 0.0, "medium", True, warnings
        return selected, "contains", 0.0, "high", False, warnings

    if nearest is not None and nearest_distance is not None and nearest_distance <= UNMATCHED_DISTANCE_METERS:
        warnings.append("shelter point is outside PLATEAU footprint; nearest-building match requires review")
        if nearest_distance <= NEAREST_HIGH_CONFIDENCE_METERS:
            confidence = "high"
        elif nearest_distance <= NEAREST_MEDIUM_CONFIDENCE_METERS:
            confidence = "medium"
        else:
            confidence = "low"
            warnings.append("nearest PLATEAU building distance exceeds medium-confidence threshold")
        return nearest, "nearest", round(nearest_distance, 3), confidence, True, warnings

    warnings.append("no PLATEAU building footprint found within the B4 unmatched distance threshold")
    if nearest_distance is not None:
        warnings.append(f"nearest candidate distance was {nearest_distance:.3f} meters")
    return None, "unmatched", round(nearest_distance, 3) if nearest_distance is not None else None, "low", True, warnings


def qualify_record(
    index: int,
    shelter: dict[str, Any],
    selected_building: dict[str, Any] | None,
    match_method: str,
    match_distance: float | None,
    confidence: str,
    manual_review: bool,
    warnings: list[str],
) -> dict[str, Any]:
    evidence_sources = list(shelter.get("evidenceSources", []))
    if selected_building is not None:
        evidence_sources.append(plateau_evidence_source(selected_building["sourceMeshCode"]))

    if match_method == "contains" and confidence == "high" and not manual_review:
        qualification_status = "official_confirmed"
        official_status = "official_designated"
        reason = "Official Chuo evidence is present and the shelter point is contained by one local PLATEAU building footprint."
    elif match_method in {"contains", "nearest"} and selected_building is not None:
        qualification_status = "official_confirmed_with_review"
        official_status = "official_designated_with_review"
        reason = "Official Chuo evidence is present, but PLATEAU building matching requires manual review."
    else:
        qualification_status = "unknown"
        official_status = "official_designated_with_review"
        reason = "Official evacuation-place evidence exists, but no unambiguous PLATEAU building match is available in B4."

    if shelter.get("safeFloor") is None:
        warnings = list(dict.fromkeys(warnings + ["official source does not provide safeFloor; preserve null for review"]))
    if shelter.get("capacity") is None:
        warnings = list(dict.fromkeys(warnings + ["official source does not provide capacity or capacity could not be parsed"]))

    return {
        "qualificationId": f"real_chuo_building_qualification_{index:03d}",
        "plateauBuildingId": selected_building["plateauBuildingId"] if selected_building else None,
        "shelterId": shelter.get("shelterId"),
        "shelterName": shelter.get("shelterName"),
        "officialDesignationStatus": official_status,
        "qualificationStatus": qualification_status,
        "qualificationReason": reason,
        "evidenceSources": evidence_sources,
        "matchMethod": match_method,
        "matchDistanceMeters": match_distance,
        "confidence": confidence,
        "manualReviewNeeded": manual_review,
        "warnings": warnings,
        "disasterTypes": shelter.get("disasterTypes"),
        "safeFloor": shelter.get("safeFloor"),
        "capacity": shelter.get("capacity"),
        "sourceUpdatedAt": source_updated_at(evidence_sources),
        "routeAvailability": "not_evaluated",
        "nearestRouteDistanceMeters": None,
        "estimatedTravelTimeSeconds": None,
        "notes": (
            f"{shelter.get('notes', '')} B4 performs PLATEAU building matching only; "
            "route fields remain not_evaluated for P5-B5."
        ).strip(),
    }


def write_feature_collection(path: Path, features: list[dict[str, Any]]) -> None:
    write_json(
        path,
        {
            "type": "FeatureCollection",
            "name": path.stem,
            "crs": {
                "type": "name",
                "properties": {
                    "name": SOURCE_CRS,
                },
            },
            "features": features,
        },
    )


def build_outputs() -> dict[str, Any]:
    if not NORMALIZED_SHELTERS_JSON.exists():
        raise FileNotFoundError(
            f"Normalized shelter input is missing: {NORMALIZED_SHELTERS_JSON}. Run ingest_official_chuo_shelters.py first."
        )
    if not CITYGML_BLDG_ROOT.exists():
        raise FileNotFoundError(f"Local PLATEAU building root is missing: {CITYGML_BLDG_ROOT}")

    normalized = load_json(NORMALIZED_SHELTERS_JSON)
    rulebook = load_json(RULEBOOK_PATH)
    shelters = normalized["records"]

    transformer = Transformer.from_crs(SOURCE_CRS, METRIC_CRS, always_xy=True)
    points = shelter_points(shelters, transformer)
    mesh_codes = sorted({point["meshCode"] for point in points})
    mesh_files = {
        mesh_code: CITYGML_BLDG_ROOT / f"{mesh_code}_bldg_6697_op.gml"
        for mesh_code in mesh_codes
    }

    buildings, extraction_warnings = parse_candidate_buildings(mesh_files, points, transformer)
    if not buildings:
        raise RuntimeError(
            "No candidate PLATEAU building footprints were extracted from the selected local mesh files."
        )

    records: list[dict[str, Any]] = []
    match_records: list[dict[str, Any]] = []
    shelter_features: list[dict[str, Any]] = []
    footprint_features_by_id: dict[str, dict[str, Any]] = {}
    line_features: list[dict[str, Any]] = []
    low_confidence_features: list[dict[str, Any]] = []

    for index, point_payload in enumerate(points, start=1):
        shelter = point_payload["shelter"]
        point_projected = point_payload["pointProjected"]
        containing = [
            building
            for building in buildings
            if building["geometryProjected"].covers(point_projected)
        ]
        nearest = min(
            buildings,
            key=lambda building: building["geometryProjected"].distance(point_projected),
            default=None,
        )
        nearest_distance = (
            nearest["geometryProjected"].distance(point_projected)
            if nearest is not None
            else None
        )
        selected, method, distance, confidence, manual_review, warnings = classify_match(
            shelter,
            containing,
            nearest,
            nearest_distance,
        )
        warnings = list(dict.fromkeys(extraction_warnings + warnings))

        record = qualify_record(
            index,
            shelter,
            selected,
            method,
            distance,
            confidence,
            manual_review,
            warnings,
        )
        records.append(record)

        match_record = {
            "shelterId": shelter.get("shelterId"),
            "shelterName": shelter.get("shelterName"),
            "plateauBuildingId": selected["plateauBuildingId"] if selected else None,
            "plateauGmlId": selected["plateauGmlId"] if selected else None,
            "matchMethod": method,
            "matchDistanceMeters": distance,
            "confidence": confidence,
            "manualReviewNeeded": manual_review,
            "warnings": warnings,
            "sourceMeshCode": selected["sourceMeshCode"] if selected else point_payload["meshCode"],
        }
        match_records.append(match_record)

        shelter_feature = {
            "type": "Feature",
            "geometry": mapping(point_payload["pointWgs84"]),
            "properties": {
                "shelterId": shelter.get("shelterId"),
                "shelterName": shelter.get("shelterName"),
                "matchMethod": method,
                "qualificationStatus": record["qualificationStatus"],
                "confidence": confidence,
                "manualReviewNeeded": manual_review,
            },
        }
        shelter_features.append(shelter_feature)

        if selected is not None:
            footprint_features_by_id[selected["plateauBuildingId"]] = {
                "type": "Feature",
                "geometry": mapping(selected["geometryWgs84"]),
                "properties": {
                    "plateauBuildingId": selected["plateauBuildingId"],
                    "plateauGmlId": selected["plateauGmlId"],
                    "sourceMeshCode": selected["sourceMeshCode"],
                    "heightMeters": selected["heightMeters"],
                    "floors": selected["floors"],
                    "usage": selected["usage"],
                },
            }
            line_features.append(
                {
                    "type": "Feature",
                    "geometry": mapping(LineString([point_payload["pointWgs84"], selected["centroidWgs84"]])),
                    "properties": {
                        "shelterId": shelter.get("shelterId"),
                        "plateauBuildingId": selected["plateauBuildingId"],
                        "matchMethod": method,
                        "matchDistanceMeters": distance,
                        "confidence": confidence,
                    },
                }
            )

        if manual_review or confidence in {"low", "unknown"} or method == "unmatched":
            low_confidence_features.append(shelter_feature)

    qualification_dataset = {
        "datasetId": "p5_b4_real_chuo_building_qualification",
        "generatedAt": now_jst(),
        "coordinateReferenceSystem": SOURCE_CRS,
        "rulebookVersion": rulebook["rulebookVersion"],
        "notes": (
            "Real official Chuo/Tokyo/GSI shelter evidence matched to a limited local PLATEAU CityGML building mesh subset. "
            "Route fields remain not_evaluated for P5-B5. Raw official downloads and raw CityGML are not committed."
        ),
        "records": records,
    }
    matches_dataset = {
        "datasetId": "p5_b4_real_chuo_shelter_building_matches",
        "generatedAt": qualification_dataset["generatedAt"],
        "coordinateReferenceSystem": SOURCE_CRS,
        "metricCoordinateReferenceSystem": METRIC_CRS,
        "records": match_records,
    }

    write_json(OUTPUT_QUALIFICATION_JSON, qualification_dataset)
    write_csv(OUTPUT_QUALIFICATION_CSV, records, QUALIFICATION_FIELDS)
    write_json(OUTPUT_MATCHES_JSON, matches_dataset)
    write_csv(OUTPUT_MATCHES_CSV, match_records, MATCH_FIELDS)
    write_feature_collection(OUTPUT_SHELTER_POINTS, shelter_features)
    write_feature_collection(OUTPUT_BUILDING_FOOTPRINTS, list(footprint_features_by_id.values()))
    write_feature_collection(OUTPUT_MATCH_LINES, line_features)
    write_feature_collection(OUTPUT_LOW_CONFIDENCE, low_confidence_features)

    status_counts = Counter(record["qualificationStatus"] for record in records)
    method_counts = Counter(record["matchMethod"] for record in records)
    manual_review_count = sum(1 for record in records if record["manualReviewNeeded"])

    manifest = {
        "manifestVersion": "p5-b4-building-matching-input-manifest-v1",
        "generatedAt": qualification_dataset["generatedAt"],
        "shelterInputPath": str(NORMALIZED_SHELTERS_JSON.relative_to(REPO_ROOT)).replace("\\", "/"),
        "buildingInputPath": str(CITYGML_BLDG_ROOT),
        "buildingInputOrigin": "existing local PLATEAU CityGML building files, limited to shelter-point mesh codes",
        "buildingInputIsProcessed": False,
        "processedBuildingFootprintOutputPath": str(OUTPUT_BUILDING_FOOTPRINTS.relative_to(REPO_ROOT)).replace("\\", "/"),
        "coordinateReferenceSystem": SOURCE_CRS,
        "metricCoordinateReferenceSystem": METRIC_CRS,
        "selectedMeshCodes": mesh_codes,
        "selectedMeshFileCount": len(mesh_files),
        "candidateBuildingCount": len(buildings),
        "matchedBuildingCount": len(footprint_features_by_id),
        "shelterRecordCount": len(records),
        "matchMethodCounts": dict(method_counts),
        "qualificationStatusCounts": dict(status_counts),
        "manualReviewNeededCount": manual_review_count,
        "rawDataPolicy": "Raw official downloads and raw local PLATEAU CityGML files are local inputs only and are not committed.",
        "limitations": [
            "B4 extracts only building footprints within a fixed buffer around official shelter points.",
            "Broad evacuation areas are not assigned to a PLATEAU building without manual QGIS review.",
            "Route fields remain not_evaluated until P5-B5.",
            "Building attributes are limited to fields available in the parsed local CityGML building elements."
        ],
        "manualReviewNotes": [
            "Review nearest matches and all broad evacuation-area records in QGIS.",
            "Check for coordinate shifts between official shelter points and PLATEAU footprints.",
            "Confirm whether open evacuation places should remain unmatched for building qualification."
        ]
    }
    write_json(INPUT_MANIFEST_PATH, manifest)

    return {
        "records": len(records),
        "buildings": len(buildings),
        "matchedBuildings": len(footprint_features_by_id),
        "methodCounts": dict(method_counts),
        "statusCounts": dict(status_counts),
        "manualReviewNeeded": manual_review_count,
    }


def main() -> int:
    summary = build_outputs()
    print(f"[build-real] wrote {OUTPUT_QUALIFICATION_JSON}")
    print(f"[build-real] wrote {OUTPUT_QUALIFICATION_CSV}")
    print(f"[build-real] wrote {OUTPUT_MATCHES_JSON}")
    print(f"[build-real] wrote {OUTPUT_MATCHES_CSV}")
    print(f"[build-real] wrote QGIS QA layers under {QGIS_QA_ROOT}")
    print(f"[build-real] shelters: {summary['records']}")
    print(f"[build-real] candidate buildings: {summary['buildings']}")
    print(f"[build-real] matched buildings: {summary['matchedBuildings']}")
    print(f"[build-real] match methods: {summary['methodCounts']}")
    print(f"[build-real] qualification statuses: {summary['statusCounts']}")
    print(f"[build-real] manualReviewNeeded: {summary['manualReviewNeeded']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
