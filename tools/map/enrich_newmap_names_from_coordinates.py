#!/usr/bin/env python3
"""Build the NewMap runtime label cache from source data and online preprocessing.

This tool is intentionally preprocessing-only. It may use local project data,
local OSM cache files, and capped online Nominatim reverse lookups. Unity
runtime reads the generated JSON cache and must not perform web requests.
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
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Tuple


JST = timezone(timedelta(hours=9))
ACTIVE_SCENE = "Assets/Scenes/Chuo_BaseMap.unity"
MIN_RUNTIME_CONFIDENCE = 0.6


def now_jst() -> str:
    return datetime.now(JST).isoformat(timespec="seconds")


def read_json(path: Path, default: Any) -> Any:
    if not path.exists():
        return default
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def has_japanese(value: str) -> bool:
    return any(("\u3040" <= ch <= "\u30ff") or ("\u3400" <= ch <= "\u9fff") for ch in value)


def looks_like_id(value: str) -> bool:
    lower = value.strip().lower()
    if not lower:
        return True
    if lower.startswith(("bldg_", "gml_", "sample_plateau", "13102-bldg-")):
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
    address_tokens = ("東京都", "中央区")
    block_tokens = ("丁目", "番", "号")
    return all(token in value for token in address_tokens) and any(token in value for token in block_tokens)


def looks_like_mojibake(value: str) -> bool:
    if not value:
        return False
    if "\ufffd" in value:
        return True
    halfwidth = sum(1 for ch in value if "\uff61" <= ch <= "\uff9f")
    if halfwidth >= 2:
        return True
    suspicious = (
        "縺", "繧", "譚", "譌", "譛", "莠", "荳", "螟", "蛹", "鬧",
        "驛", "陬", "隕", "邵", "郢", "闔", "闕", "髯", "蜿", "逡",
        "蟆", "譬", "鬆", "驥", "蠎", "螻", "繝", "繧", "竊", "",
    )
    return any(token in value for token in suspicious)


def normalize_main_name(value: Optional[str]) -> str:
    if not value:
        return ""
    name = str(value).strip()
    if not name:
        return ""
    name = re.split(r"[\r\n]", name, maxsplit=1)[0].strip()
    name = name.split(";")[0].strip()
    name = re.sub(r"\s+", " ", name)
    name = re.sub(r"^\s*(名称|施設名|建物名|道路名)\s*[:：]\s*", "", name)
    if looks_like_id(name) or looks_like_address(name) or looks_like_mojibake(name):
        return ""
    if not has_japanese(name):
        return ""
    return name


def is_road_main_name(value: str) -> bool:
    if not value:
        return False
    return any(token in value for token in ("通り", "通", "街道", "道路", "線", "橋", "坂"))


def vec3(x: Any, y: Any, z: Any) -> Dict[str, float]:
    return {"x": round(float(x), 3), "y": round(float(y), 3), "z": round(float(z), 3)}


def relative(path: Path, root: Path) -> str:
    try:
        return str(path.relative_to(root)).replace("\\", "/")
    except ValueError:
        return str(path).replace("\\", "/")


def default_config() -> Dict[str, Any]:
    return {
        "enabled": True,
        "preprocessingOnly": True,
        "runtimeNetworkRequestsAllowed": False,
        "allowOnlineLookup": True,
        "onlyQueryMissingNames": True,
        "maxQueriesPerRun": 500,
        "maxOnlineQueries": 500,
        "rateLimitSeconds": 1.1,
        "onlineRateLimitSeconds": 1.1,
        "onlineTimeoutSeconds": 10.0,
        "preferJapaneseNames": True,
        "hideLowConfidenceInNormalMode": True,
        "hideIdOnlyInNormalMode": True,
        "usePublicNominatimCarefully": True,
        "userAgent": "ChuoTsunamiEvacuation-PBL10-NameEnrichment/1.0",
        "maxRoadLabels": 140,
        "maxBuildingLabels": 160,
        "maxLocalOsmBuildingLabels": 40,
        "maxRouteRoadQueryPoints": 80,
        "maxMissingBuildingQueries": 420,
        "queryBuildingsNearGameplayArea": True,
        "queryRoadsNearRoutes": True,
        "mainNameOnly": True,
        "hideAddressLikeNames": True,
        "cacheResults": True,
        "minConfidenceForRuntime": MIN_RUNTIME_CONFIDENCE,
        "sourcePriority": [
            "project official shelter names",
            "project non-official candidate names",
            "local PLATEAU/GameObject metadata",
            "local OSM cache",
            "online Nominatim reverse lookup for missing selected names only",
        ],
    }


def load_config(project_root: Path) -> Dict[str, Any]:
    config_path = project_root / "Assets/Data/P10/newmap_name_enrichment_config.json"
    config = default_config()
    existing = read_json(config_path, {})
    if isinstance(existing, dict):
        config.update(existing)
    if str(config.get("userAgent", "")).startswith("ChuoTsunamiEvacuationNewMap"):
        config["userAgent"] = "ChuoTsunamiEvacuation-PBL10-NameEnrichment/1.0"
    config["sourcePriority"] = default_config()["sourcePriority"]
    config["maxRoadLabels"] = max(140, int(config.get("maxRoadLabels", 140)))
    config["maxBuildingLabels"] = max(160, int(config.get("maxBuildingLabels", 160)))
    config["maxLocalOsmBuildingLabels"] = max(40, int(config.get("maxLocalOsmBuildingLabels", 40)))
    config["maxRouteRoadQueryPoints"] = max(80, int(config.get("maxRouteRoadQueryPoints", 80)))
    config["maxMissingBuildingQueries"] = max(420, int(config.get("maxMissingBuildingQueries", 420)))
    if "maxQueriesPerRun" not in config and "maxOnlineQueries" in config:
        config["maxQueriesPerRun"] = config["maxOnlineQueries"]
    config["maxQueriesPerRun"] = int(config.get("maxQueriesPerRun", 200))
    config["maxOnlineQueries"] = config["maxQueriesPerRun"]
    config["rateLimitSeconds"] = float(config.get("rateLimitSeconds", config.get("onlineRateLimitSeconds", 1.1)))
    config["onlineRateLimitSeconds"] = config["rateLimitSeconds"]
    config["runtimeNetworkRequestsAllowed"] = False
    config["preprocessingOnly"] = True
    config["onlyQueryMissingNames"] = True
    config["preferJapaneseNames"] = True
    config["mainNameOnly"] = True
    config["hideAddressLikeNames"] = True
    config["hideIdOnlyInNormalMode"] = True
    config["cacheResults"] = True
    config["queryBuildingsNearGameplayArea"] = True
    config["queryRoadsNearRoutes"] = True
    write_json(config_path, config)
    return config


def load_transform(project_root: Path) -> Optional[Dict[str, Any]]:
    path = project_root / "Assets/Data/P10/newmap_coordinate_transform_anchor_fit.json"
    data = read_json(path, {})
    fit = data.get("fit") if isinstance(data, dict) else None
    if not isinstance(fit, dict) or fit.get("status") != "transform_validated_from_official_anchors":
        return None
    return fit


def meters_per_degree(origin_lat: float) -> Tuple[float, float]:
    lat_rad = math.radians(origin_lat)
    meters_lat = 111132.92 - 559.82 * math.cos(2 * lat_rad) + 1.175 * math.cos(4 * lat_rad)
    meters_lon = 111412.84 * math.cos(lat_rad) - 93.5 * math.cos(3 * lat_rad)
    return meters_lat, meters_lon


def wgs84_to_unity(lat: float, lon: float, fit: Dict[str, Any], fallback_y: float) -> Optional[Dict[str, float]]:
    origin = fit.get("origin") or {}
    affine = fit.get("affine2d") or {}
    x_fit = affine.get("unityX") or {}
    z_fit = affine.get("unityZ") or {}
    try:
        origin_lat = float(origin["lat"])
        origin_lon = float(origin["lon"])
        meters_lat, meters_lon = meters_per_degree(origin_lat)
        east = (lon - origin_lon) * meters_lon
        north = (lat - origin_lat) * meters_lat
        x = float(x_fit["eastCoefficient"]) * east + float(x_fit["northCoefficient"]) * north + float(x_fit["offset"])
        z = float(z_fit["eastCoefficient"]) * east + float(z_fit["northCoefficient"]) * north + float(z_fit["offset"])
        return vec3(x, fallback_y, z)
    except (KeyError, TypeError, ValueError):
        return None


def unity_to_wgs84(x: float, z: float, fit: Dict[str, Any]) -> Optional[Tuple[float, float]]:
    origin = fit.get("origin") or {}
    affine = fit.get("affine2d") or {}
    x_fit = affine.get("unityX") or {}
    z_fit = affine.get("unityZ") or {}
    try:
        origin_lat = float(origin["lat"])
        origin_lon = float(origin["lon"])
        a = float(x_fit["eastCoefficient"])
        b = float(x_fit["northCoefficient"])
        c = float(z_fit["eastCoefficient"])
        d = float(z_fit["northCoefficient"])
        ox = float(x_fit["offset"])
        oz = float(z_fit["offset"])
        det = a * d - b * c
        if abs(det) < 1e-9:
            return None
        east = (d * (x - ox) - b * (z - oz)) / det
        north = (-c * (x - ox) + a * (z - oz)) / det
        meters_lat, meters_lon = meters_per_degree(origin_lat)
        lat = origin_lat + north / meters_lat
        lon = origin_lon + east / meters_lon
        return lat, lon
    except (KeyError, TypeError, ValueError, ZeroDivisionError):
        return None


def bounds_contains(position: Dict[str, float], bounds: Dict[str, Any]) -> bool:
    if not bounds:
        return True
    try:
        mn = bounds["min"]
        mx = bounds["max"]
        return float(mn["x"]) <= position["x"] <= float(mx["x"]) and float(mn["z"]) <= position["z"] <= float(mx["z"])
    except (KeyError, TypeError, ValueError):
        return True


def make_label(
    label_id: str,
    object_type: str,
    raw_name: Optional[str],
    position: Dict[str, float],
    provider: str,
    source: str,
    classification: str,
    raw_type: str,
    confidence: float,
    source_field: str,
    timestamp: str,
    lat: Optional[float] = None,
    lon: Optional[float] = None,
    attribution_required: bool = False,
) -> Optional[Dict[str, Any]]:
    normalized = normalize_main_name(raw_name)
    if not normalized:
        return None
    confidence = round(float(confidence), 3)
    hidden = confidence < MIN_RUNTIME_CONFIDENCE
    return {
        "id": label_id,
        "objectType": object_type,
        "name": normalized,
        "finalDisplayName": normalized,
        "finalDisplayNameLanguage": "ja",
        "language": "ja",
        "source": source,
        "provider": provider,
        "sourceField": source_field,
        "classification": classification,
        "rawType": raw_type,
        "confidence": confidence,
        "lat": round(float(lat), 8) if lat is not None else None,
        "lon": round(float(lon), 8) if lon is not None else None,
        "position": position,
        "unityPosition": position,
        "rawName": str(raw_name or ""),
        "normalizedName": normalized,
        "idOnly": False,
        "disabled": hidden,
        "hiddenInNormalMode": hidden,
        "hiddenReason": "low_confidence" if hidden else "",
        "attributionRequired": bool(attribution_required),
        "timestamp": timestamp,
    }


def add_label(labels: List[Dict[str, Any]], seen: set, label: Optional[Dict[str, Any]]) -> bool:
    if label is None:
        return False
    label_id = label["id"]
    if label_id in seen:
        return False
    seen.add(label_id)
    labels.append(label)
    return True


def label_by_id(labels: Iterable[Dict[str, Any]]) -> Dict[str, Dict[str, Any]]:
    return {str(label.get("id")): label for label in labels if label.get("id")}


def load_candidate_lat_lon(project_root: Path) -> Dict[str, Tuple[float, float, Dict[str, Any]]]:
    path = project_root / "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json"
    data = read_json(path, {})
    result: Dict[str, Tuple[float, float, Dict[str, Any]]] = {}
    for record in data.get("records", []):
        candidate_id = str(record.get("candidateId", ""))
        try:
            lat = float(record["latitude"])
            lon = float(record["longitude"])
        except (KeyError, TypeError, ValueError):
            continue
        result[candidate_id] = (lat, lon, record)
    return result


def extract_official_labels(project_root: Path, labels: List[Dict[str, Any]], seen: set, timestamp: str) -> int:
    path = project_root / "Assets/Data/P10/newmap_official_shelter_anchor_report.json"
    data = read_json(path, {})
    count = 0
    for record in data.get("records", []):
        if not record.get("activeInGame", False):
            continue
        pos = record.get("unityMarkerPosition") or {}
        if not {"x", "y", "z"} <= set(pos):
            continue
        label = make_label(
            str(record.get("id", "")),
            "official_shelter",
            record.get("name"),
            vec3(pos["x"], pos["y"], pos["z"]),
            "project_dataset",
            "Assets/Data/P10/newmap_official_shelter_anchor_report.json",
            "project_dataset_name",
            "official_shelter",
            1.0,
            "name",
            timestamp,
            record.get("lat"),
            record.get("lon"),
            False,
        )
        if add_label(labels, seen, label):
            count += 1
    return count


def extract_candidate_labels(
    project_root: Path,
    labels: List[Dict[str, Any]],
    seen: set,
    timestamp: str,
) -> Tuple[int, Dict[str, Dict[str, Any]]]:
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
        active_candidates[candidate_id] = record
        label = make_label(
            candidate_id,
            "candidate",
            record.get("displayName"),
            vec3(record.get("unityX", 0.0), record.get("unityY", 0.0), record.get("unityZ", 0.0)),
            "project_dataset",
            "Assets/Resources/NewMap/newmap_runtime_non_official_candidates.json",
            "project_dataset_name_non_official_warning_required",
            "non_official_candidate",
            0.95,
            "displayName",
            timestamp,
            None,
            None,
            False,
        )
        if add_label(labels, seen, label):
            count += 1
    return count, active_candidates


def extract_project_building_labels(
    project_root: Path,
    labels: List[Dict[str, Any]],
    seen: set,
    active_candidates: Dict[str, Dict[str, Any]],
    candidate_geo: Dict[str, Tuple[float, float, Dict[str, Any]]],
    fit: Optional[Dict[str, Any]],
    fallback_y: float,
    max_building_labels: int,
    timestamp: str,
) -> int:
    count = 0
    for candidate_id, runtime in active_candidates.items():
        lat_lon_record = candidate_geo.get(candidate_id)
        lat = lat_lon_record[0] if lat_lon_record else None
        lon = lat_lon_record[1] if lat_lon_record else None
        audit_record = lat_lon_record[2] if lat_lon_record else {}
        # Runtime candidate names are post-cleanup display names. Prefer them
        # over older PLATEAU audit attributes that may be mojibake or ID-only.
        raw_name = runtime.get("displayName") or audit_record.get("buildingName")
        label = make_label(
            "building_" + candidate_id,
            "building",
            raw_name,
            vec3(runtime.get("unityX", 0.0), runtime.get("unityY", 0.0), runtime.get("unityZ", 0.0)),
            "project_dataset",
            "Assets/Resources/NewMap/newmap_runtime_non_official_candidates.json",
            "project_dataset_name",
            "plateau_building_attribute",
            0.86,
            "displayName",
            timestamp,
            lat,
            lon,
            False,
        )
        if add_label(labels, seen, label):
            count += 1
            if count >= max_building_labels:
                return count

    for candidate_id, lat_lon_record in sorted(candidate_geo.items()):
        if count >= max_building_labels:
            break
        label_id = "building_" + candidate_id
        if label_id in seen:
            continue

        lat, lon, audit_record = lat_lon_record
        raw_name = audit_record.get("buildingName")
        if not normalize_main_name(raw_name):
            continue

        position = wgs84_to_unity(lat, lon, fit, fallback_y) if fit is not None else None
        if position is None:
            continue

        label = make_label(
            label_id,
            "building",
            raw_name,
            position,
            "project_dataset",
            "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
            "project_dataset_name",
            "plateau_building_attribute",
            0.84,
            "buildingName",
            timestamp,
            lat,
            lon,
            False,
        )
        if add_label(labels, seen, label):
            count += 1
    return count


def way_midpoint(element: Dict[str, Any], nodes: Dict[int, Tuple[float, float]]) -> Optional[Tuple[float, float]]:
    node_ids = element.get("nodes") or []
    coords = [nodes.get(int(node_id)) for node_id in node_ids if int(node_id) in nodes]
    coords = [coord for coord in coords if coord is not None]
    if coords:
        return coords[len(coords) // 2]
    center = element.get("center")
    if isinstance(center, dict) and "lat" in center and "lon" in center:
        return float(center["lat"]), float(center["lon"])
    if "lat" in element and "lon" in element:
        return float(element["lat"]), float(element["lon"])
    return None


def is_osm_building_or_landmark(tags: Dict[str, Any]) -> bool:
    if tags.get("building") or tags.get("building:part"):
        return True

    amenity = str(tags.get("amenity", "")).lower()
    tourism = str(tags.get("tourism", "")).lower()
    healthcare = str(tags.get("healthcare", "")).lower()
    if amenity in {"hospital", "school", "university", "college", "library", "theatre", "townhall", "public_building"}:
        return True
    if tourism in {"museum", "hotel"}:
        return True
    if healthcare in {"hospital", "clinic"}:
        return True

    return False


def extract_local_osm_labels(
    project_root: Path,
    labels: List[Dict[str, Any]],
    seen: set,
    fit: Optional[Dict[str, Any]],
    map_bounds: Dict[str, Any],
    max_road_labels: int,
    max_building_labels: int,
    fallback_y: float,
    timestamp: str,
) -> Tuple[int, int, int, bool]:
    if fit is None:
        return 0, 0, 0, False

    cache_dir = project_root / "data_pipeline/cache/osmnx"
    files = list(cache_dir.glob("*.json"))
    if not files:
        return 0, 0, 0, False

    data = read_json(files[0], {})
    elements = data.get("elements", [])
    nodes: Dict[int, Tuple[float, float]] = {}
    for element in elements:
        if element.get("type") == "node" and "lat" in element and "lon" in element:
            nodes[int(element["id"])] = (float(element["lat"]), float(element["lon"]))

    road_names_seen = set()
    building_names_seen = set()
    road_count = 0
    building_count = 0
    landmark_count = 0
    source = relative(files[0], project_root)

    for element in elements:
        tags = element.get("tags") or {}
        raw_type = tags.get("highway") or tags.get("building") or tags.get("amenity") or tags.get("tourism") or tags.get("railway") or element.get("type", "osm")
        raw_name = tags.get("name:ja") or tags.get("name") or tags.get("official_name") or tags.get("short_name")
        name = normalize_main_name(raw_name)
        if not name:
            continue
        lat_lon = way_midpoint(element, nodes)
        if not lat_lon:
            continue
        position = wgs84_to_unity(lat_lon[0], lat_lon[1], fit, fallback_y)
        if not position or not bounds_contains(position, map_bounds):
            continue

        if name == "東京駅" and landmark_count == 0:
            if add_label(labels, seen, make_label(
                "landmark_tokyo_station",
                "landmark",
                name,
                position,
                "OpenStreetMap",
                source,
                "local_osm_cache_name",
                str(raw_type),
                0.86,
                "name/name:ja",
                timestamp,
                lat_lon[0],
                lat_lon[1],
                True,
            )):
                landmark_count += 1
            continue

        if element.get("type") == "way" and "highway" in tags and road_count < max_road_labels:
            if name not in road_names_seen:
                if add_label(labels, seen, make_label(
                    "road_osm_" + str(element.get("id")),
                    "road",
                    name,
                    position,
                    "OpenStreetMap",
                    source,
                    "local_osm_cache_name",
                    "highway:" + str(tags.get("highway")),
                    0.78,
                    "name/name:ja",
                    timestamp,
                    lat_lon[0],
                    lat_lon[1],
                    True,
                )):
                    road_names_seen.add(name)
                    road_count += 1
            continue

        if building_count < max_building_labels and is_osm_building_or_landmark(tags) and name not in building_names_seen:
            if tags.get("railway") and not tags.get("building"):
                continue
            object_type = "building"
            label_id = "building_osm_" + str(element.get("id"))
            if add_label(labels, seen, make_label(
                label_id,
                object_type,
                name,
                position,
                "OpenStreetMap",
                source,
                "local_osm_cache_name",
                str(raw_type),
                0.76,
                "name/name:ja",
                timestamp,
                lat_lon[0],
                lat_lon[1],
                True,
            )):
                building_names_seen.add(name)
                building_count += 1

    return road_count, building_count, landmark_count, True


def collect_route_road_query_points(project_root: Path, fit: Optional[Dict[str, Any]], fallback_y: float, max_points: int) -> List[Dict[str, Any]]:
    if fit is None or max_points <= 0:
        return []
    path = project_root / "data_pipeline/processed/routes/real_chuo_osm_routes_sample.json"
    data = read_json(path, {})
    points: List[Dict[str, Any]] = []
    seen = set()
    for record in data.get("records", []):
        coords = ((record.get("geometry") or {}).get("coordinates") or [])
        if not coords:
            continue
        for index in (0, len(coords) // 2, len(coords) - 1):
            try:
                lon, lat = coords[index]
                key = (round(float(lat), 4), round(float(lon), 4))
            except (TypeError, ValueError, IndexError):
                continue
            if key in seen:
                continue
            seen.add(key)
            position = wgs84_to_unity(float(lat), float(lon), fit, fallback_y)
            points.append({
                "id": f"route_road_{record.get('routeId')}_{index}",
                "objectType": "road",
                "unityPosition": position,
                "lat": float(lat),
                "lon": float(lon),
                "currentName": "",
                "reasonForQuery": "route_or_road_point_missing_cached_specific_road_name",
                "priority": 4,
                "sourceDataPath": "data_pipeline/processed/routes/real_chuo_osm_routes_sample.json",
                "enabledForOnlineLookup": position is not None,
            })
            if len(points) >= max_points:
                return points
    return points


def build_query_list(
    project_root: Path,
    labels: List[Dict[str, Any]],
    active_candidates: Dict[str, Dict[str, Any]],
    candidate_geo: Dict[str, Tuple[float, float, Dict[str, Any]]],
    fit: Optional[Dict[str, Any]],
    fallback_y: float,
    config: Dict[str, Any],
) -> Tuple[List[Dict[str, Any]], Dict[str, Any]]:
    existing = label_by_id(labels)
    query_list: List[Dict[str, Any]] = []
    queued_ids = set()
    missing_building_count = 0
    missing_building_no_coordinate = 0

    for candidate_id, runtime in active_candidates.items():
        building_label_id = "building_" + candidate_id
        if building_label_id in existing:
            continue
        current_name = str(runtime.get("displayName", ""))
        lat_lon_record = candidate_geo.get(candidate_id)
        lat = lat_lon_record[0] if lat_lon_record else None
        lon = lat_lon_record[1] if lat_lon_record else None
        if lat is None or lon is None:
            if fit is not None:
                inverse = unity_to_wgs84(float(runtime.get("unityX", 0.0)), float(runtime.get("unityZ", 0.0)), fit)
                if inverse is not None:
                    lat, lon = inverse
        enabled = lat is not None and lon is not None
        if not enabled:
            missing_building_no_coordinate += 1
        missing_building_count += 1
        position = vec3(runtime.get("unityX", 0.0), runtime.get("unityY", fallback_y), runtime.get("unityZ", 0.0))
        queued_ids.add(building_label_id)
        query_list.append({
            "id": building_label_id,
            "objectType": "building",
            "unityPosition": position,
            "lat": lat,
            "lon": lon,
            "currentName": "" if looks_like_id(current_name) else current_name,
            "reasonForQuery": "active_candidate_building_name_missing_or_id_only",
            "priority": 3,
            "sourceDataPath": "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
            "enabledForOnlineLookup": enabled,
        })

    if config.get("queryBuildingsNearGameplayArea", True):
        for candidate_id, lat_lon_record in sorted(candidate_geo.items()):
            building_label_id = "building_" + candidate_id
            if building_label_id in existing or building_label_id in queued_ids:
                continue
            lat, lon, audit_record = lat_lon_record
            if normalize_main_name(audit_record.get("buildingName")):
                continue
            position = wgs84_to_unity(lat, lon, fit, fallback_y) if fit is not None else None
            missing_building_count += 1
            if position is None:
                missing_building_no_coordinate += 1
            queued_ids.add(building_label_id)
            query_list.append({
                "id": building_label_id,
                "objectType": "building",
                "unityPosition": position or vec3(0.0, fallback_y, 0.0),
                "lat": lat,
                "lon": lon,
                "currentName": "",
                "reasonForQuery": "expanded_visible_or_route_near_p8_building_name_missing",
                "priority": 4,
                "sourceDataPath": "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
                "enabledForOnlineLookup": position is not None,
            })

    road_queries = collect_route_road_query_points(
        project_root,
        fit,
        fallback_y,
        int(config.get("maxRouteRoadQueryPoints", 80)) if config.get("queryRoadsNearRoutes", True) else 0,
    )
    query_list.extend(road_queries)
    query_list.sort(key=lambda item: (int(item.get("priority", 99)), str(item.get("id", ""))))
    max_missing_building = int(config.get("maxMissingBuildingQueries", 80))
    filtered: List[Dict[str, Any]] = []
    building_kept = 0
    for item in query_list:
        if item.get("objectType") == "building":
            if building_kept >= max_missing_building:
                continue
            building_kept += 1
        filtered.append(item)

    summary = {
        "missingBuildingNames": missing_building_count,
        "missingBuildingNamesWithoutCoordinates": missing_building_no_coordinate,
        "routeRoadQueryPointCount": len(road_queries),
        "onlineEnabledQueryCount": len([item for item in filtered if item.get("enabledForOnlineLookup")]),
    }
    return filtered, summary


def reverse_lookup_name(lat: float, lon: float, object_type: str, user_agent: str, timeout_seconds: float) -> Dict[str, Any]:
    zoom = 18 if object_type == "building" else 17
    params = urllib.parse.urlencode({
        "format": "jsonv2",
        "lat": f"{lat:.7f}",
        "lon": f"{lon:.7f}",
        "namedetails": 1,
        "accept-language": "ja",
        "zoom": zoom,
    })
    url = "https://nominatim.openstreetmap.org/reverse?" + params
    request = urllib.request.Request(url, headers={"User-Agent": user_agent})
    with urllib.request.urlopen(request, timeout=timeout_seconds) as response:
        return json.loads(response.read().decode("utf-8"))


def select_online_name(result: Dict[str, Any], object_type: str) -> Tuple[str, str, str]:
    namedetails = result.get("namedetails") or {}
    address = result.get("address") or {}
    raw_type = str(result.get("category", "")) + ":" + str(result.get("type", ""))
    source_field = "namedetails.name/name:ja"
    candidates = [
        namedetails.get("name:ja"),
        namedetails.get("name"),
        namedetails.get("official_name"),
        namedetails.get("short_name"),
        result.get("name"),
    ]
    if object_type == "road":
        candidates.extend([address.get("road"), address.get("pedestrian"), address.get("footway")])
        source_field = "namedetails.name/name:ja_or_address.road"
    elif object_type == "building":
        candidates.extend([address.get("building"), address.get("amenity"), address.get("tourism")])
        source_field = "namedetails.name/name:ja_or_address.building"
    for candidate in candidates:
        normalized = normalize_main_name(candidate)
        if object_type == "road" and normalized and not is_road_main_name(normalized):
            continue
        if normalized:
            return normalized, raw_type, source_field
    return "", raw_type, source_field


def online_result_matches_type(result: Dict[str, Any], object_type: str) -> bool:
    category = str(result.get("category", "")).lower()
    result_type = str(result.get("type", "")).lower()
    if object_type == "road":
        return category in {"highway", "road"} or result_type in {"road", "pedestrian", "footway", "residential", "tertiary", "secondary", "primary"}
    if object_type == "building":
        if category == "building":
            return True
        if category == "amenity" and result_type in {"hospital", "school", "university", "college", "library", "theatre", "townhall", "public_building"}:
            return True
        if category == "tourism" and result_type in {"museum", "hotel", "attraction"}:
            return True
        if category in {"office", "shop"}:
            return True
        if result_type in {"yes", "office", "commercial", "retail", "apartments", "school", "hospital", "university", "hotel", "public_building"}:
            return True
        return False
    return True


def run_online_queries(
    labels: List[Dict[str, Any]],
    seen: set,
    query_points: List[Dict[str, Any]],
    config: Dict[str, Any],
    fallback_y: float,
    timestamp: str,
) -> Dict[str, Any]:
    if not config.get("allowOnlineLookup", True):
        return {
            "status": "disabled_by_config",
            "attempted": 0,
            "httpSucceeded": 0,
            "labelsAdded": 0,
            "failed": 0,
            "rejected": 0,
            "errors": [],
            "rejectedResults": [],
        }

    enabled_points = [point for point in query_points if point.get("enabledForOnlineLookup")]
    max_queries = max(0, int(config.get("maxQueriesPerRun", config.get("maxOnlineQueries", 0))))
    if max_queries <= 0 or not enabled_points:
        return {
            "status": "not_run_no_enabled_missing_name_queries",
            "attempted": 0,
            "httpSucceeded": 0,
            "labelsAdded": 0,
            "failed": 0,
            "rejected": 0,
            "errors": [],
            "rejectedResults": [],
        }

    user_agent = str(config.get("userAgent", "ChuoTsunamiEvacuation-PBL10-NameEnrichment/1.0"))
    rate_seconds = max(1.1, float(config.get("rateLimitSeconds", config.get("onlineRateLimitSeconds", 1.1))))
    timeout_seconds = float(config.get("onlineTimeoutSeconds", 10.0))
    attempted = 0
    http_succeeded = 0
    labels_added = 0
    errors: List[str] = []
    rejected_results: List[Dict[str, Any]] = []

    for point in enabled_points[:max_queries]:
        attempted += 1
        object_type = str(point.get("objectType", "building"))
        try:
            result = reverse_lookup_name(float(point["lat"]), float(point["lon"]), object_type, user_agent, timeout_seconds)
            http_succeeded += 1
            selected_name, raw_type, source_field = select_online_name(result, object_type)
            if not selected_name:
                rejected_results.append({
                    "id": point.get("id"),
                    "objectType": object_type,
                    "reason": "no_clean_japanese_main_name",
                    "rawType": raw_type,
                    "displayName": result.get("display_name", ""),
                })
            elif not online_result_matches_type(result, object_type):
                rejected_results.append({
                    "id": point.get("id"),
                    "objectType": object_type,
                    "reason": "online_result_wrong_object_type",
                    "rawName": selected_name,
                    "rawType": raw_type,
                })
            else:
                confidence = 0.74 if object_type == "road" else 0.72
                label = make_label(
                    str(point["id"]),
                    object_type,
                    selected_name,
                    point.get("unityPosition") or vec3(0, fallback_y, 0),
                    "OpenStreetMap Nominatim",
                    "online_preprocessing_reverse_geocode",
                    "online_exact_or_near_match",
                    raw_type,
                    confidence,
                    source_field,
                    timestamp,
                    point.get("lat"),
                    point.get("lon"),
                    True,
                )
                if add_label(labels, seen, label):
                    labels_added += 1
                else:
                    rejected_results.append({
                        "id": point.get("id"),
                        "objectType": object_type,
                        "reason": "duplicate_or_low_confidence_label",
                        "rawName": selected_name,
                        "rawType": raw_type,
                    })
            time.sleep(rate_seconds)
        except Exception as exc:  # noqa: BLE001 - graceful partial-cache failure
            errors.append(f"{point.get('id')}: {exc}")
            break

    if errors:
        status = "failed_or_pending"
    elif attempted > 0:
        status = "completed"
    else:
        status = "not_run_no_enabled_missing_name_queries"

    return {
        "status": status,
        "attempted": attempted,
        "httpSucceeded": http_succeeded,
        "labelsAdded": labels_added,
        "failed": max(0, attempted - http_succeeded),
        "rejected": len(rejected_results),
        "errors": errors,
        "rejectedResults": rejected_results,
    }


def count_bad_visible(labels: Iterable[Dict[str, Any]]) -> Dict[str, int]:
    id_only = 0
    address_like = 0
    low_confidence = 0
    for label in labels:
        if label.get("disabled") or label.get("hiddenInNormalMode"):
            continue
        name = str(label.get("finalDisplayName") or label.get("name") or "")
        if looks_like_id(name):
            id_only += 1
        if looks_like_address(name):
            address_like += 1
        if float(label.get("confidence", 0.0)) < MIN_RUNTIME_CONFIDENCE:
            low_confidence += 1
    return {"idOnly": id_only, "addressLike": address_like, "lowConfidence": low_confidence}


def build_cache_audit(
    previous_cache: Dict[str, Any],
    labels: List[Dict[str, Any]],
    query_summary: Dict[str, Any],
    online: Dict[str, Any],
    runtime_report: Dict[str, Any],
    enrichment_report_before: Dict[str, Any],
    timestamp: str,
) -> Dict[str, Any]:
    previous_labels = previous_cache.get("labels", []) if isinstance(previous_cache, dict) else []
    visible_bad = count_bad_visible(labels)
    return {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "previousTotalCachedNameRecords": len(previous_labels),
        "totalCachedNameRecords": len(labels),
        "activeOfficialShelterNames": len([label for label in labels if label.get("objectType") == "official_shelter" and not label.get("disabled")]),
        "activeNonOfficialCandidateNames": len([label for label in labels if label.get("objectType") == "candidate" and not label.get("disabled")]),
        "buildingNamesCached": len([label for label in labels if label.get("objectType") == "building" and not label.get("disabled")]),
        "roadNamesCached": len([label for label in labels if label.get("objectType") == "road" and not label.get("disabled")]),
        "missingBuildingNames": query_summary.get("missingBuildingNames", 0),
        "missingRoadQueryPoints": query_summary.get("routeRoadQueryPointCount", 0),
        "idOnlyLabelsVisible": visible_bad["idOnly"],
        "addressLikeLabelsVisible": visible_bad["addressLike"],
        "lowConfidenceLabelsVisible": visible_bad["lowConfidence"],
        "labelsCurrentlyShownRuntime": {
            "availableLabels": runtime_report.get("availableLabels"),
            "activeLabels": runtime_report.get("activeLabels"),
            "buildingLabelsRuntime": runtime_report.get("buildingLabelsRuntime"),
            "roadLabelsRuntime": runtime_report.get("roadLabelsRuntime"),
            "nameCacheLoaded": runtime_report.get("nameCacheLoaded", runtime_report.get("nameCacheExists")),
        },
        "previousOnlineQueriesAttempted": enrichment_report_before.get("onlineQueriesAttempted", 0),
        "onlineQueriesAttemptedThisRun": online["attempted"],
        "onlineQueriesSucceededThisRun": online["httpSucceeded"],
        "onlineLabelsAddedThisRun": online["labelsAdded"],
        "hardRuleStatus": "online_queries_executed" if online["attempted"] > 0 else "many_missing_names_no_online_queries_do_not_claim_complete",
    }


def build_normalization_report(labels: List[Dict[str, Any]], online: Dict[str, Any], timestamp: str) -> Dict[str, Any]:
    visible_bad = count_bad_visible(labels)
    return {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "normalizationRulesApplied": True,
        "japaneseKanjiMainNameOnly": True,
        "fullAddressesHidden": True,
        "postalAddressesHidden": True,
        "gmlAndBuildingIdsHidden": True,
        "coordinatesHiddenAsNames": True,
        "machineTranslationUsed": False,
        "fabricatedNamesAllowed": False,
        "lowConfidenceHiddenInNormalMode": True,
        "visibleIdOnlyLabelCount": visible_bad["idOnly"],
        "visibleAddressLikeLabelCount": visible_bad["addressLike"],
        "visibleLowConfidenceLabelCount": visible_bad["lowConfidence"],
        "onlineResultsRejected": online["rejected"],
        "rejectedReasonsSample": online["rejectedResults"][:12],
        "finalStatus": "passed" if sum(visible_bad.values()) == 0 else "failed_visible_bad_names",
    }


def write_docs(project_root: Path, audit: Dict[str, Any], query_list: List[Dict[str, Any]], report: Dict[str, Any], normalization: Dict[str, Any]) -> None:
    docs = project_root / "docs"
    write_text = lambda name, text: (docs / name).write_text(text, encoding="utf-8")
    write_text(
        "NEWMAP_NAME_CACHE_COVERAGE_AUDIT.md",
        "# NewMap Name Cache Coverage Audit\n\n"
        f"- Generated: `{audit['generatedAt']}`\n"
        f"- Total cache records: `{audit['totalCachedNameRecords']}`\n"
        f"- Official shelter names: `{audit['activeOfficialShelterNames']}`\n"
        f"- Non-official candidate names: `{audit['activeNonOfficialCandidateNames']}`\n"
        f"- Building names cached: `{audit['buildingNamesCached']}`\n"
        f"- Road names cached: `{audit['roadNamesCached']}`\n"
        f"- Missing building names queued before online lookup: `{audit['missingBuildingNames']}`\n"
        f"- Online queries attempted this run: `{audit['onlineQueriesAttemptedThisRun']}`\n"
        f"- Online labels added this run: `{audit['onlineLabelsAddedThisRun']}`\n"
        f"- Visible ID/address/low-confidence labels: `{audit['idOnlyLabelsVisible']}` / `{audit['addressLikeLabelsVisible']}` / `{audit['lowConfidenceLabelsVisible']}`\n"
        f"- Hard rule status: `{audit['hardRuleStatus']}`\n",
    )
    write_text(
        "NEWMAP_NAME_ENRICHMENT_QUERY_LIST.md",
        "# NewMap Name Enrichment Query List\n\n"
        f"- Query items: `{len(query_list)}`\n"
        f"- Enabled online lookup items: `{len([item for item in query_list if item.get('enabledForOnlineLookup')])}`\n"
        "- Query scope: active official/candidate targets, visible candidate buildings, and route/road sample points only.\n"
        "- The tool does not query the whole map blindly.\n",
    )
    write_text(
        "NEWMAP_NAME_ENRICHMENT_FROM_COORDINATES.md",
        "# NewMap Name Enrichment From Coordinates\n\n"
        "- Stage: preprocessing/tooling only.\n"
        "- Unity runtime web requests: `false`.\n"
        f"- Provider: `{report['provider']}`\n"
        f"- Online status: `{report['onlineEnrichmentStatus']}`\n"
        f"- Online queries attempted/succeeded/failed: `{report['onlineQueriesAttempted']}` / `{report['onlineQueriesSucceeded']}` / `{report['onlineQueriesFailed']}`\n"
        f"- Names newly added: `{report['namesNewlyAdded']}`\n"
        f"- Names rejected: `{report['namesRejected']}`\n"
        f"- Cache path: `{report['cachePath']}`\n"
        "- Low-confidence, address-like, ID-only, and non-Japanese names are hidden from normal runtime labels.\n",
    )
    write_text(
        "NEWMAP_NAME_ENRICHMENT_EXPANDED_BUILDINGS_ROADS.md",
        "# NewMap Name Enrichment Expanded Buildings Roads\n\n"
        "- Stage: preprocessing/tooling only.\n"
        "- Runtime web requests: `false`.\n"
        "- Scope: active official shelters, active non-official candidates, P8 gameplay-area building candidates, and route-near road sample points.\n"
        f"- Query candidates built: `{len(query_list)}`\n"
        f"- Online lookup candidates enabled: `{len([item for item in query_list if item.get('enabledForOnlineLookup')])}`\n"
        f"- Max queries per run: `{report.get('maxQueriesPerRun', 'see config')}`\n"
        f"- Building labels after normalization: `{report['buildingLabels']}`\n"
        f"- Road labels after normalization: `{report['roadLabels']}`\n"
        f"- Online queries attempted/succeeded: `{report['onlineQueriesAttempted']}` / `{report['onlineQueriesSucceeded']}`\n"
        "- The tool does not query the entire map blindly and does not fabricate missing names.\n",
    )
    write_text(
        "NEWMAP_NAME_NORMALIZATION_RULES.md",
        "# NewMap Name Normalization Rules\n\n"
        "- Prefer `name:ja`, then `name`, `official_name`, and only then clearly valid `short_name`.\n"
        "- Keep only the main Japanese/Kanji display name.\n"
        "- Hide full postal addresses, coordinate strings, GML IDs, and `bldg_`/`13102-bldg-` IDs in normal mode.\n"
        "- Do not machine translate and do not fabricate names.\n"
        f"- Visible bad-label counts: ID `{normalization['visibleIdOnlyLabelCount']}`, address `{normalization['visibleAddressLikeLabelCount']}`, low-confidence `{normalization['visibleLowConfidenceLabelCount']}`.\n",
    )
    write_text(
        "NEWMAP_NAME_NORMALIZATION_REPORT.md",
        "# NewMap Name Normalization Report\n\n"
        f"- Generated: `{normalization['generatedAt']}`\n"
        f"- Japanese/Kanji main-name only: `{normalization['japaneseKanjiMainNameOnly']}`\n"
        f"- Full addresses hidden: `{normalization['fullAddressesHidden']}`\n"
        f"- GML/building IDs hidden: `{normalization['gmlAndBuildingIdsHidden']}`\n"
        f"- Machine translation used: `{normalization['machineTranslationUsed']}`\n"
        f"- Fabricated names allowed: `{normalization['fabricatedNamesAllowed']}`\n"
        f"- Visible ID/address/low-confidence counts: `{normalization['visibleIdOnlyLabelCount']}` / `{normalization['visibleAddressLikeLabelCount']}` / `{normalization['visibleLowConfidenceLabelCount']}`\n"
        f"- Online results rejected: `{normalization['onlineResultsRejected']}`\n"
        f"- Final status: `{normalization['finalStatus']}`\n",
    )
    write_text(
        "NEWMAP_NAME_LABEL_ATTRIBUTION.md",
        "# NewMap Name Label Attribution\n\n"
        f"- Generated: `{report['generatedAt']}`\n"
        "- Cache file: `Assets/Data/P10/newmap_name_cache.json`\n"
        "- Project sources: official shelter anchors, runtime non-official candidate cache, PLATEAU/P8 candidate audit.\n"
        "- OpenStreetMap local cache and Nominatim preprocessing results are © OpenStreetMap contributors and used under the Open Database License.\n"
        "- Online lookups, when present, are preprocessing-only and are not performed by the Unity player.\n"
        "- Labels are informational prototype labels, not official facility certification, official road guidance, or GIS-grade validation.\n",
    )


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", default=str(Path(__file__).resolve().parents[2]))
    args = parser.parse_args()

    project_root = Path(args.project_root).resolve()
    timestamp = now_jst()
    config = load_config(project_root)
    previous_cache = read_json(project_root / "Assets/Data/P10/newmap_name_cache.json", {})
    previous_enrichment_report = read_json(project_root / "Assets/Data/P10/newmap_name_enrichment_report.json", {})
    runtime_report = read_json(project_root / "Assets/Data/P10/newmap_name_label_runtime_report.json", {})

    fallback_ground_y = 2.4
    raise_config = read_json(project_root / "Assets/Data/P10/newmap_ground_cover_raise_config.json", {})
    cover_config = read_json(project_root / "Assets/Data/P10/newmap_gameplay_ground_cover_config.json", {})
    if isinstance(raise_config, dict):
        fallback_ground_y = float(cover_config.get("coverY", 0.0)) + float(raise_config.get("fallbackRaiseOffsetMeters", 2.4))

    official_report = read_json(project_root / "Assets/Data/P10/newmap_official_shelter_anchor_report.json", {})
    map_bounds = official_report.get("mapBoundsFromSceneMeshAabbs") if isinstance(official_report, dict) else {}
    fit = load_transform(project_root)
    candidate_geo = load_candidate_lat_lon(project_root)
    labels: List[Dict[str, Any]] = []
    seen: set = set()

    official_count = extract_official_labels(project_root, labels, seen, timestamp)
    candidate_count, active_candidates = extract_candidate_labels(project_root, labels, seen, timestamp)
    project_building_count = extract_project_building_labels(
        project_root,
        labels,
        seen,
        active_candidates,
        candidate_geo,
        fit,
        fallback_ground_y,
        int(config["maxBuildingLabels"]),
        timestamp,
    )
    local_road_count, local_osm_building_count, local_landmark_count, local_osm_available = extract_local_osm_labels(
        project_root,
        labels,
        seen,
        fit,
        map_bounds or {},
        int(config["maxRoadLabels"]),
        int(config["maxLocalOsmBuildingLabels"]),
        fallback_ground_y,
        timestamp,
    )
    query_list, query_summary = build_query_list(
        project_root,
        labels,
        active_candidates,
        candidate_geo,
        fit,
        fallback_ground_y,
        config,
    )
    online = run_online_queries(labels, seen, query_list, config, fallback_ground_y, timestamp)

    labels.sort(key=lambda item: (item["objectType"], item["id"]))
    online_added = online["labelsAdded"]
    source_status = "source_project_and_local_osm_names_available" if local_osm_available else "source_project_names_available"
    if online_added > 0:
        source_status += "_online_enriched"

    cache = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "preprocessingOnly": True,
        "runtimeNetworkRequestsAllowed": False,
        "sourceStatus": source_status,
        "onlineEnrichmentStatus": online["status"],
        "providerSummary": {
            "projectOfficialShelters": official_count,
            "projectNonOfficialCandidates": candidate_count,
            "projectBuildingNames": project_building_count,
            "localOsmRoadLabels": local_road_count,
            "localOsmBuildingLabels": local_osm_building_count,
            "localOsmLandmarkLabels": local_landmark_count,
            "onlineQueriesAttempted": online["attempted"],
            "onlineQueriesSucceeded": online["httpSucceeded"],
            "onlineLabelsAdded": online_added,
            "onlineLabelsRejected": online["rejected"],
        },
        "labels": labels,
    }

    report = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "preprocessingOnly": True,
        "runtimeNetworkRequestsAllowed": False,
        "sourcePriorityApplied": config["sourcePriority"],
        "cachePath": "Assets/Data/P10/newmap_name_cache.json",
        "queryListPath": "Assets/Data/P10/newmap_name_enrichment_query_list.json",
        "labelCount": len(labels),
        "officialShelterLabels": official_count,
        "nonOfficialCandidateLabels": candidate_count,
        "buildingLabels": len([label for label in labels if label["objectType"] == "building" and not label["disabled"]]),
        "roadLabels": len([label for label in labels if label["objectType"] == "road" and not label["disabled"]]),
        "landmarkLabels": len([label for label in labels if label["objectType"] == "landmark" and not label["disabled"]]),
        "projectBuildingLabels": project_building_count,
        "localOsmCacheAvailable": local_osm_available,
        "localOsmRoadLabels": local_road_count,
        "localOsmBuildingLabels": local_osm_building_count,
        "provider": "OpenStreetMap Nominatim" if online["attempted"] > 0 else "project/local sources only",
        "onlineEnrichmentStatus": online["status"],
        "onlineQueriesAttempted": online["attempted"],
        "onlineQueriesSucceeded": online["httpSucceeded"],
        "onlineQueriesFailed": online["failed"],
        "onlineLabelsAdded": online_added,
        "namesNewlyAdded": online_added,
        "namesRejected": online["rejected"],
        "maxQueriesPerRun": config["maxQueriesPerRun"],
        "onlineErrors": online["errors"][:8],
        "rateLimitSeconds": config["rateLimitSeconds"],
        "userAgent": config["userAgent"],
        "attributionRequired": local_osm_available or online["attempted"] > 0,
        "attributionStatus": "documented_in_docs_NEWMAP_NAME_LABEL_ATTRIBUTION",
        "normalization": {
            "japaneseKanjiMainNameOnly": True,
            "fullAddressesHidden": True,
            "idOnlyHidden": True,
            "machineTranslationUsed": False,
            "fabricatedNamesAllowed": False,
            "lowConfidenceHiddenInNormalMode": True,
        },
        "finalStatus": "completed" if labels and online["attempted"] > 0 and not online["errors"] else ("completed_with_online_errors" if labels and online["attempted"] > 0 else "partial_no_online_queries"),
    }

    audit = build_cache_audit(previous_cache, labels, query_summary, online, runtime_report, previous_enrichment_report, timestamp)
    normalization = build_normalization_report(labels, online, timestamp)
    query_payload = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "runtimeNetworkRequestsAllowed": False,
        "onlyQueryMissingNames": True,
        "queryCount": len(query_list),
        "enabledForOnlineLookupCount": len([item for item in query_list if item.get("enabledForOnlineLookup")]),
        "maxQueriesPerRun": config["maxQueriesPerRun"],
        "items": query_list,
    }

    write_json(project_root / "Assets/Data/P10/newmap_name_cache.json", cache)
    write_json(project_root / "Assets/Data/P10/newmap_name_enrichment_report.json", report)
    write_json(project_root / "Assets/Data/P10/newmap_name_cache_coverage_audit.json", audit)
    write_json(project_root / "Assets/Data/P10/newmap_name_enrichment_query_list.json", query_payload)
    write_json(project_root / "Assets/Data/P10/newmap_name_normalization_report.json", normalization)
    write_docs(project_root, audit, query_list, report, normalization)

    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if labels else 1


if __name__ == "__main__":
    sys.exit(main())
