#!/usr/bin/env python3
"""Build NewMap runtime label cache from project data and preprocessing lookups.

This script is intentionally preprocessing-only. Unity runtime reads the cache
that this tool writes and must not perform web requests.
"""

from __future__ import annotations

import argparse
import json
import math
import re
import sys
import time
import urllib.parse
import urllib.request
from datetime import datetime, timezone, timedelta
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Tuple


JST = timezone(timedelta(hours=9))
ACTIVE_SCENE = "Assets/Scenes/Chuo_BaseMap.unity"


def now_jst() -> str:
    return datetime.now(JST).isoformat(timespec="seconds")


def read_json(path: Path, default: Any) -> Any:
    if not path.exists():
        return default
    return json.loads(path.read_text(encoding="utf-8"))


def write_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def has_japanese(value: str) -> bool:
    return any(
        ("\u3040" <= ch <= "\u30ff") or
        ("\u3400" <= ch <= "\u9fff")
        for ch in value
    )


def looks_like_id(value: str) -> bool:
    lower = value.strip().lower()
    if not lower:
        return True
    if lower.startswith(("bldg_", "gml_", "sample_plateau")):
        return True
    if lower.startswith("13102-bldg-"):
        return True
    if "_unknown_" in lower or lower in {"unknown", "unnamed", "none", "null"}:
        return True
    if re.fullmatch(r"[-+]?\d+(\.\d+)?\s*,\s*[-+]?\d+(\.\d+)?", lower):
        return True
    return False


def looks_like_address(value: str) -> bool:
    if not value:
        return False
    lower = value.lower()
    if "postal" in lower or "address" in lower or "\u3012" in value:
        return True
    return (
        "\u6771\u4eac\u90fd" in value and
        "\u4e2d\u592e\u533a" in value and
        ("\u4e01\u76ee" in value or "\u756a" in value or "\u53f7" in value)
    )


def looks_like_mojibake(value: str) -> bool:
    if "\ufffd" in value:
        return True
    halfwidth = sum(1 for ch in value if "\uff61" <= ch <= "\uff9f")
    if halfwidth >= 2:
        return True
    return any(token in value for token in (
        "\u7e3a", "\u7e5d", "\u90e2", "\u8b5a", "\u83a0",
        "\u9b27", "\u9a5b", "\u86f9", "\u8373", "\u87c6"
    ))


def normalize_main_name(value: Optional[str]) -> str:
    if not value:
        return ""
    name = str(value).strip()
    if not name:
        return ""
    name = re.split(r"[\r\n]", name, maxsplit=1)[0].strip()
    name = name.split(";")[0].strip()
    name = re.sub(r"\s+", " ", name)
    if looks_like_id(name) or looks_like_address(name) or looks_like_mojibake(name):
        return ""
    if not has_japanese(name):
        return ""
    return name


def vec3(x: float, y: float, z: float) -> Dict[str, float]:
    return {"x": round(float(x), 3), "y": round(float(y), 3), "z": round(float(z), 3)}


def add_label(
    labels: List[Dict[str, Any]],
    seen: set,
    label_id: str,
    object_type: str,
    name: Optional[str],
    position: Dict[str, float],
    provider: str,
    source: str,
    classification: str,
    raw_type: str,
    confidence: float,
    disabled: bool = False,
) -> bool:
    normalized = normalize_main_name(name)
    if not normalized:
        return False
    if label_id in seen:
        return False
    seen.add(label_id)
    labels.append({
        "id": label_id,
        "objectType": object_type,
        "name": normalized,
        "language": "ja",
        "provider": provider,
        "source": source,
        "classification": classification,
        "rawType": raw_type,
        "confidence": round(float(confidence), 3),
        "idOnly": False,
        "disabled": bool(disabled),
        "position": position,
    })
    return True


def load_transform(project_root: Path) -> Optional[Dict[str, Any]]:
    path = project_root / "Assets/Data/P10/newmap_coordinate_transform_anchor_fit.json"
    data = read_json(path, {})
    fit = data.get("fit") if isinstance(data, dict) else None
    if not isinstance(fit, dict) or fit.get("status") != "transform_validated_from_official_anchors":
        return None
    return fit


def wgs84_to_unity(lat: float, lon: float, fit: Dict[str, Any], fallback_y: float) -> Optional[Dict[str, float]]:
    origin = fit.get("origin") or {}
    affine = fit.get("affine2d") or {}
    x_fit = affine.get("unityX") or {}
    z_fit = affine.get("unityZ") or {}
    try:
        origin_lat = float(origin["lat"])
        origin_lon = float(origin["lon"])
        lat_rad = math.radians(origin_lat)
        meters_per_degree_lat = (
            111132.92 -
            559.82 * math.cos(2 * lat_rad) +
            1.175 * math.cos(4 * lat_rad)
        )
        meters_per_degree_lon = (
            111412.84 * math.cos(lat_rad) -
            93.5 * math.cos(3 * lat_rad)
        )
        east = (lon - origin_lon) * meters_per_degree_lon
        north = (lat - origin_lat) * meters_per_degree_lat
        x = float(x_fit["eastCoefficient"]) * east + float(x_fit["northCoefficient"]) * north + float(x_fit["offset"])
        z = float(z_fit["eastCoefficient"]) * east + float(z_fit["northCoefficient"]) * north + float(z_fit["offset"])
        return vec3(x, fallback_y, z)
    except (KeyError, TypeError, ValueError):
        return None


def bounds_contains(position: Dict[str, float], bounds: Dict[str, Any]) -> bool:
    if not bounds:
        return True
    try:
        mn = bounds["min"]
        mx = bounds["max"]
        return (
            float(mn["x"]) <= position["x"] <= float(mx["x"]) and
            float(mn["z"]) <= position["z"] <= float(mx["z"])
        )
    except (KeyError, TypeError, ValueError):
        return True


def extract_official_labels(project_root: Path, labels: List[Dict[str, Any]], seen: set) -> int:
    path = project_root / "Assets/Data/P10/newmap_official_shelter_anchor_report.json"
    data = read_json(path, {})
    count = 0
    for record in data.get("records", []):
        if not record.get("activeInGame", False):
            continue
        pos = record.get("unityMarkerPosition") or {}
        if not {"x", "y", "z"} <= set(pos):
            continue
        position = vec3(pos["x"], pos["y"], pos["z"])
        if add_label(
            labels,
            seen,
            str(record.get("id", "")),
            "official_shelter",
            record.get("name"),
            position,
            "project",
            "Assets/Data/P10/newmap_official_shelter_anchor_report.json",
            "project_dataset_name",
            "official_shelter",
            1.0,
        ):
            count += 1
    return count


def extract_candidate_labels(project_root: Path, labels: List[Dict[str, Any]], seen: set) -> Tuple[int, Dict[str, Dict[str, Any]]]:
    path = project_root / "Assets/Resources/NewMap/newmap_runtime_non_official_candidates.json"
    data = read_json(path, {})
    active_candidates: Dict[str, Dict[str, Any]] = {}
    count = 0
    for record in data.get("records", []):
        if not record.get("activeInGame", False):
            continue
        if record.get("isOfficialShelter", False) or not record.get("nonOfficialWarningRequired", False):
            continue
        candidate_id = str(record.get("id", ""))
        position = vec3(record.get("unityX", 0.0), record.get("unityY", 0.0), record.get("unityZ", 0.0))
        active_candidates[candidate_id] = record
        if add_label(
            labels,
            seen,
            candidate_id,
            "candidate",
            record.get("displayName"),
            position,
            "project",
            "Assets/Resources/NewMap/newmap_runtime_non_official_candidates.json",
            "project_dataset_name_non_official_warning_required",
            "non_official_candidate",
            0.95,
        ):
            count += 1
    return count, active_candidates


def extract_building_labels(
    project_root: Path,
    labels: List[Dict[str, Any]],
    seen: set,
    active_candidates: Dict[str, Dict[str, Any]],
    max_building_labels: int,
) -> int:
    path = project_root / "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
    data = read_json(path, {})
    count = 0
    for record in data.get("records", []):
        candidate_id = str(record.get("candidateId", ""))
        runtime = active_candidates.get(candidate_id)
        if runtime is None:
            continue
        position = vec3(runtime.get("unityX", 0.0), runtime.get("unityY", 0.0), runtime.get("unityZ", 0.0))
        if add_label(
            labels,
            seen,
            "building_" + candidate_id,
            "building",
            record.get("buildingName") or runtime.get("displayName"),
            position,
            "project",
            "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
            "project_dataset_name",
            "plateau_building_attribute",
            0.86,
        ):
            count += 1
            if count >= max_building_labels:
                break
    return count


def way_midpoint(element: Dict[str, Any], nodes: Dict[int, Tuple[float, float]]) -> Optional[Tuple[float, float]]:
    node_ids = element.get("nodes") or []
    coords = [nodes.get(int(node_id)) for node_id in node_ids if int(node_id) in nodes]
    coords = [coord for coord in coords if coord is not None]
    if not coords:
        center = element.get("center")
        if isinstance(center, dict) and "lat" in center and "lon" in center:
            return float(center["lat"]), float(center["lon"])
        return None
    return coords[len(coords) // 2]


def extract_local_osm_labels(
    project_root: Path,
    labels: List[Dict[str, Any]],
    seen: set,
    fit: Optional[Dict[str, Any]],
    map_bounds: Dict[str, Any],
    max_road_labels: int,
    fallback_y: float,
) -> Tuple[int, int, bool]:
    if fit is None:
        return 0, 0, False

    cache_dir = project_root / "data_pipeline/cache/osmnx"
    files = list(cache_dir.glob("*.json"))
    if not files:
        return 0, 0, False

    data = read_json(files[0], {})
    elements = data.get("elements", [])
    nodes: Dict[int, Tuple[float, float]] = {}
    for element in elements:
        if element.get("type") == "node" and "lat" in element and "lon" in element:
            nodes[int(element["id"])] = (float(element["lat"]), float(element["lon"]))

    road_names_seen = set()
    road_count = 0
    landmark_count = 0
    source = str(files[0].relative_to(project_root)).replace("\\", "/")

    for element in elements:
        tags = element.get("tags") or {}
        raw_type = tags.get("highway") or tags.get("railway") or tags.get("amenity") or element.get("type", "osm")
        name = normalize_main_name(tags.get("name:ja") or tags.get("name") or tags.get("official_name"))
        if not name:
            continue

        if name == "\u6771\u4eac\u99c5" and landmark_count == 0:
            if element.get("type") == "node":
                lat_lon = (float(element["lat"]), float(element["lon"]))
            else:
                lat_lon = way_midpoint(element, nodes)
            if lat_lon:
                position = wgs84_to_unity(lat_lon[0], lat_lon[1], fit, fallback_y)
                if position and bounds_contains(position, map_bounds):
                    if add_label(
                        labels,
                        seen,
                        "landmark_tokyo_station",
                        "landmark",
                        name,
                        position,
                        "OpenStreetMap",
                        source,
                        "local_osm_cache_name",
                        str(raw_type),
                        0.82,
                    ):
                        landmark_count += 1
            continue

        if element.get("type") != "way" or "highway" not in tags:
            continue
        if name in road_names_seen:
            continue
        if road_count >= max_road_labels:
            continue
        lat_lon = way_midpoint(element, nodes)
        if not lat_lon:
            continue
        position = wgs84_to_unity(lat_lon[0], lat_lon[1], fit, fallback_y)
        if not position or not bounds_contains(position, map_bounds):
            continue
        if add_label(
            labels,
            seen,
            "road_osm_" + str(element.get("id")),
            "road",
            name,
            position,
            "OpenStreetMap",
            source,
            "local_osm_cache_name",
            "highway:" + str(tags.get("highway")),
            0.78,
        ):
            road_names_seen.add(name)
            road_count += 1

    return road_count, landmark_count, True


def reverse_lookup_name(lat: float, lon: float, user_agent: str, timeout_seconds: float) -> Optional[Dict[str, Any]]:
    params = urllib.parse.urlencode({
        "format": "jsonv2",
        "lat": f"{lat:.7f}",
        "lon": f"{lon:.7f}",
        "namedetails": 1,
        "accept-language": "ja",
        "zoom": 18,
    })
    url = "https://nominatim.openstreetmap.org/reverse?" + params
    request = urllib.request.Request(url, headers={"User-Agent": user_agent})
    with urllib.request.urlopen(request, timeout=timeout_seconds) as response:
        return json.loads(response.read().decode("utf-8"))


def run_online_queries(
    labels: List[Dict[str, Any]],
    seen: set,
    fit: Optional[Dict[str, Any]],
    query_points: List[Dict[str, Any]],
    config: Dict[str, Any],
    fallback_y: float,
) -> Tuple[str, int, int, List[str]]:
    if not config.get("allowOnlineLookup", True):
        return "disabled_by_config", 0, 0, []
    if fit is None:
        return "skipped_missing_coordinate_transform", 0, 0, []

    max_queries = int(config.get("maxOnlineQueries", 12))
    if max_queries <= 0 or not query_points:
        return "not_needed_source_and_local_osm_cache_provided_names", 0, 0, []

    user_agent = str(config.get("userAgent", "ChuoTsunamiEvacuationNewMapNamePreprocessor/1.0"))
    rate_seconds = float(config.get("onlineRateLimitSeconds", 1.1))
    timeout_seconds = float(config.get("onlineTimeoutSeconds", 8.0))
    attempted = 0
    added = 0
    errors: List[str] = []

    for point in query_points[:max_queries]:
        attempted += 1
        try:
            result = reverse_lookup_name(float(point["lat"]), float(point["lon"]), user_agent, timeout_seconds)
            namedetails = result.get("namedetails") or {}
            name = (
                namedetails.get("name:ja") or
                namedetails.get("name") or
                result.get("name")
            )
            normalized = normalize_main_name(name)
            if normalized:
                position = wgs84_to_unity(float(point["lat"]), float(point["lon"]), fit, fallback_y)
                if position:
                    if add_label(
                        labels,
                        seen,
                        "online_" + str(point["id"]),
                        str(point.get("objectType", "landmark")),
                        normalized,
                        position,
                        "OpenStreetMap Nominatim",
                        "online_preprocessing_reverse_geocode",
                        "online_exact_or_near_match",
                        str(result.get("category", "")) + ":" + str(result.get("type", "")),
                        0.7,
                    ):
                        added += 1
            time.sleep(max(0.0, rate_seconds))
        except Exception as exc:  # noqa: BLE001 - report graceful online failure
            errors.append(f"{point.get('id')}: {exc}")
            break

    if errors:
        return "failed_or_pending", attempted, added, errors
    return ("completed" if attempted > 0 else "not_needed_source_and_local_osm_cache_provided_names"), attempted, added, errors


def collect_missing_online_points(road_count: int, config: Dict[str, Any]) -> List[Dict[str, Any]]:
    minimum_road_labels = int(config.get("minimumRoadLabelsBeforeOnline", 8))
    if road_count >= minimum_road_labels:
        return []
    # Controlled representative Chuo/Tokyo Station area points, queried only when
    # local/source data did not produce enough reliable road labels.
    return [
        {"id": "tokyo_station_area", "objectType": "landmark", "lat": 35.681236, "lon": 139.767125},
        {"id": "nihonbashi_area", "objectType": "road", "lat": 35.682839, "lon": 139.773542},
        {"id": "ginza_area", "objectType": "road", "lat": 35.671989, "lon": 139.763965},
    ]


def default_config() -> Dict[str, Any]:
    return {
        "enabled": True,
        "preprocessingOnly": True,
        "runtimeNetworkRequestsAllowed": False,
        "allowOnlineLookup": True,
        "maxOnlineQueries": 12,
        "onlineRateLimitSeconds": 1.1,
        "onlineTimeoutSeconds": 8.0,
        "minimumRoadLabelsBeforeOnline": 8,
        "maxRoadLabels": 40,
        "maxBuildingLabels": 60,
        "minConfidenceForRuntime": 0.6,
        "userAgent": "ChuoTsunamiEvacuationNewMapNamePreprocessor/1.0",
        "sourcePriority": [
            "project official shelter names",
            "project non-official candidate names",
            "local PLATEAU/GameObject metadata",
            "local OSM cache",
            "online reverse lookup for missing selected names only"
        ],
    }


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", default=str(Path(__file__).resolve().parents[2]))
    args = parser.parse_args()

    project_root = Path(args.project_root).resolve()
    config_path = project_root / "Assets/Data/P10/newmap_name_enrichment_config.json"
    config = default_config()
    existing = read_json(config_path, {})
    if isinstance(existing, dict):
        config.update(existing)
    config["runtimeNetworkRequestsAllowed"] = False
    config["preprocessingOnly"] = True
    write_json(config_path, config)

    fallback_ground_y = 2.4
    raise_config = read_json(project_root / "Assets/Data/P10/newmap_ground_cover_raise_config.json", {})
    cover_config = read_json(project_root / "Assets/Data/P10/newmap_gameplay_ground_cover_config.json", {})
    if isinstance(raise_config, dict):
        fallback_ground_y = float(cover_config.get("coverY", 0.0)) + float(raise_config.get("fallbackRaiseOffsetMeters", 2.4))

    official_report = read_json(project_root / "Assets/Data/P10/newmap_official_shelter_anchor_report.json", {})
    map_bounds = official_report.get("mapBoundsFromSceneMeshAabbs") if isinstance(official_report, dict) else {}
    fit = load_transform(project_root)
    labels: List[Dict[str, Any]] = []
    seen: set = set()

    official_count = extract_official_labels(project_root, labels, seen)
    candidate_count, active_candidates = extract_candidate_labels(project_root, labels, seen)
    building_count = extract_building_labels(project_root, labels, seen, active_candidates, int(config["maxBuildingLabels"]))
    road_count, landmark_count, local_osm_available = extract_local_osm_labels(
        project_root,
        labels,
        seen,
        fit,
        map_bounds or {},
        int(config["maxRoadLabels"]),
        fallback_ground_y,
    )
    online_points = collect_missing_online_points(road_count, config)
    online_status, online_attempted, online_added, online_errors = run_online_queries(
        labels,
        seen,
        fit,
        online_points,
        config,
        fallback_ground_y,
    )

    labels.sort(key=lambda item: (item["objectType"], item["id"]))
    generated_at = now_jst()
    source_status = "source_project_and_local_osm_names_available" if local_osm_available else "source_project_names_available"
    if online_added > 0:
        source_status += "_online_enriched"
    cache = {
        "generatedAt": generated_at,
        "activeScene": ACTIVE_SCENE,
        "preprocessingOnly": True,
        "runtimeNetworkRequestsAllowed": False,
        "sourceStatus": source_status,
        "onlineEnrichmentStatus": online_status,
        "providerSummary": {
            "projectOfficialShelters": official_count,
            "projectNonOfficialCandidates": candidate_count,
            "projectBuildingNames": building_count,
            "localOsmRoadLabels": road_count,
            "localOsmLandmarkLabels": landmark_count,
            "onlineQueriesAttempted": online_attempted,
            "onlineLabelsAdded": online_added,
        },
        "labels": labels,
    }
    report = {
        "generatedAt": generated_at,
        "activeScene": ACTIVE_SCENE,
        "preprocessingOnly": True,
        "runtimeNetworkRequestsAllowed": False,
        "sourcePriorityApplied": config["sourcePriority"],
        "cachePath": "Assets/Data/P10/newmap_name_cache.json",
        "labelCount": len(labels),
        "officialShelterLabels": official_count,
        "nonOfficialCandidateLabels": candidate_count,
        "buildingLabels": building_count,
        "roadLabels": road_count,
        "landmarkLabels": landmark_count,
        "localOsmCacheAvailable": local_osm_available,
        "onlineEnrichmentStatus": online_status,
        "onlineQueriesAttempted": online_attempted,
        "onlineLabelsAdded": online_added,
        "onlineErrors": online_errors[:8],
        "normalization": {
            "japaneseKanjiMainNameOnly": True,
            "fullAddressesHidden": True,
            "idOnlyHidden": True,
            "machineTranslationUsed": False,
            "fabricatedNamesAllowed": False,
            "lowConfidenceHiddenInNormalMode": True,
        },
        "finalStatus": "completed" if labels else "failed_no_labels",
    }
    write_json(project_root / "Assets/Data/P10/newmap_name_cache.json", cache)
    write_json(project_root / "Assets/Data/P10/newmap_name_enrichment_report.json", report)

    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if labels else 1


if __name__ == "__main__":
    sys.exit(main())
