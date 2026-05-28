#!/usr/bin/env python3
import argparse
import json
import math
import sys
import time
import urllib.parse
import urllib.request
from datetime import datetime, timezone
from pathlib import Path


PROJECT_ROOT = Path(__file__).resolve().parents[2]
DATA_DIR = PROJECT_ROOT / "Assets" / "Data" / "P10"
CONFIG_PATH = DATA_DIR / "newmap_name_enrichment_config.json"
INPUT_POINTS_PATH = DATA_DIR / "newmap_name_enrichment_input_points.json"
CACHE_PATH = DATA_DIR / "newmap_name_cache.json"
REPORT_PATH = DATA_DIR / "newmap_name_enrichment_report.json"
CANDIDATE_RESOURCE_PATH = PROJECT_ROOT / "Assets" / "Resources" / "NewMap" / "newmap_runtime_non_official_candidates.json"


def default_config():
    return {
        "enabled": True,
        "runtimeNetworkRequestsAllowed": False,
        "preferSourceMetadata": True,
        "allowOnlineLookup": True,
        "providerPriority": ["source_metadata", "project_data", "osm_nominatim", "gsi"],
        "cacheResults": True,
        "rateLimitSeconds": 1.1,
        "maxQueriesPerRun": 200,
        "onlyQueryMissingNames": True,
        "queryBuildings": True,
        "queryRoads": True,
        "minConfidence": 0.6,
        "writeAttribution": True,
        "userAgent": "ChuoTsunamiEvacuationNewMapNameEnrichment/1.0",
        "nominatimEndpoint": "https://nominatim.openstreetmap.org/reverse",
        "acceptLanguage": "ja,en",
        "saveRawResponses": False
    }


def load_json(path, fallback):
    if not path.exists():
        return fallback
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path, data):
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(data, handle, ensure_ascii=False, indent=2)
        handle.write("\n")


def merged_config():
    config = default_config()
    if CONFIG_PATH.exists():
        loaded = load_json(CONFIG_PATH, {})
        if isinstance(loaded, dict):
            config.update(loaded)
    config["runtimeNetworkRequestsAllowed"] = False
    config["rateLimitSeconds"] = max(1.1, float(config.get("rateLimitSeconds", 1.1)))
    config["maxQueriesPerRun"] = max(0, int(config.get("maxQueriesPerRun", 0)))
    config["minConfidence"] = min(1.0, max(0.0, float(config.get("minConfidence", 0.6))))
    return config


def as_float(value):
    try:
        result = float(value)
    except (TypeError, ValueError):
        return None
    if math.isnan(result) or math.isinf(result):
        return None
    return result


def normalize_input_point(raw, source):
    if not isinstance(raw, dict):
        return None
    lat = as_float(raw.get("lat", raw.get("latitude")))
    lon = as_float(raw.get("lon", raw.get("longitude")))
    unity = raw.get("unityPosition") or raw.get("position") or {}
    point = {
        "id": str(raw.get("id", "")).strip(),
        "objectType": str(raw.get("objectType", raw.get("type", ""))).strip().lower(),
        "sourceName": str(raw.get("sourceName", raw.get("name", ""))).strip(),
        "displayName": str(raw.get("displayName", "")).strip(),
        "lat": lat,
        "lon": lon,
        "unityPosition": {
            "x": as_float(unity.get("x")) or as_float(raw.get("unityX")) or 0.0,
            "y": as_float(unity.get("y")) or as_float(raw.get("unityY")) or 0.0,
            "z": as_float(unity.get("z")) or as_float(raw.get("unityZ")) or 0.0,
        },
        "source": source,
    }
    if not point["id"]:
        return None
    return point


def collect_points():
    points = []
    if INPUT_POINTS_PATH.exists():
        loaded = load_json(INPUT_POINTS_PATH, {})
        raw_points = loaded.get("points", loaded if isinstance(loaded, list) else [])
        for raw in raw_points:
            point = normalize_input_point(raw, "enrichment_input_points")
            if point:
                points.append(point)

    if CANDIDATE_RESOURCE_PATH.exists():
        loaded = load_json(CANDIDATE_RESOURCE_PATH, {})
        for raw in loaded.get("records", []):
            point = normalize_input_point(
                {
                    "id": raw.get("id"),
                    "objectType": "candidate",
                    "sourceName": raw.get("displayName"),
                    "unityX": raw.get("unityX"),
                    "unityY": raw.get("unityY"),
                    "unityZ": raw.get("unityZ"),
                },
                "project_data_non_official_candidate_resource",
            )
            if point:
                points.append(point)

    return points


def is_queryable(point, config):
    object_type = point["objectType"]
    if object_type == "building" and not config.get("queryBuildings", True):
        return False
    if object_type == "road" and not config.get("queryRoads", True):
        return False
    if object_type not in {"building", "road"}:
        return False
    if config.get("onlyQueryMissingNames", True) and (point["sourceName"] or point["displayName"]):
        return False
    return point["lat"] is not None and point["lon"] is not None


def nominatim_reverse(point, config):
    params = {
        "format": "jsonv2",
        "lat": f"{point['lat']:.8f}",
        "lon": f"{point['lon']:.8f}",
        "zoom": "18" if point["objectType"] == "building" else "17",
        "addressdetails": "1",
        "namedetails": "1",
        "extratags": "1",
        "accept-language": config.get("acceptLanguage", "ja,en"),
    }
    url = config.get("nominatimEndpoint", default_config()["nominatimEndpoint"]) + "?" + urllib.parse.urlencode(params)
    request = urllib.request.Request(
        url,
        headers={
            "User-Agent": config.get("userAgent", default_config()["userAgent"]),
            "Accept": "application/json",
        },
    )
    with urllib.request.urlopen(request, timeout=20) as response:
        return json.loads(response.read().decode("utf-8"))


def classify_online_result(point, response, min_confidence):
    address = response.get("address") or {}
    namedetails = response.get("namedetails") or {}
    name = response.get("name") or namedetails.get("name:ja") or namedetails.get("name:en") or namedetails.get("name")
    raw_category = str(response.get("category", "")).lower()
    raw_type = str(response.get("type", "")).lower()
    object_type = point["objectType"]

    if object_type == "road":
        road_name = name or address.get("road") or address.get("pedestrian") or address.get("footway")
        if road_name and (raw_category == "highway" or raw_type in {"road", "street", "pedestrian", "footway", "path"} or address.get("road")):
            return road_name, "online_exact_or_near_match", max(min_confidence, 0.72), raw_category + "/" + raw_type
        if address:
            return "", "online_address_only", 0.4, raw_category + "/" + raw_type
        return "", "no_name_found", 0.0, raw_category + "/" + raw_type

    if object_type == "building":
        building_like = raw_category in {"building", "amenity", "tourism", "shop", "office", "leisure", "historic"} or raw_type in {"building", "yes", "apartments", "commercial", "school", "hospital"}
        if name and building_like:
            return name, "online_exact_or_near_match", max(min_confidence, 0.72), raw_category + "/" + raw_type
        if address:
            return "", "online_address_only", 0.4, raw_category + "/" + raw_type
        return "", "no_name_found", 0.0, raw_category + "/" + raw_type

    return "", "no_name_found", 0.0, raw_category + "/" + raw_type


def label_from_source(point):
    name = point["sourceName"] or point["displayName"]
    if not name:
        return None
    object_type = point["objectType"]
    classification = "project_dataset_name" if point["source"].startswith("project_data") else "source_metadata_name"
    return {
        "id": point["id"],
        "objectType": object_type,
        "name": name,
        "language": "source",
        "provider": point["source"],
        "source": point["source"],
        "classification": classification,
        "rawType": object_type,
        "confidence": 1.0,
        "idOnly": False,
        "disabled": False,
        "position": point["unityPosition"],
    }


def label_from_online(point, name, classification, confidence, raw_type):
    return {
        "id": point["id"],
        "objectType": point["objectType"],
        "name": name,
        "language": "source",
        "provider": "osm_nominatim",
        "source": "coordinate_reverse_lookup_cache",
        "classification": classification,
        "rawType": raw_type,
        "confidence": confidence,
        "idOnly": False,
        "disabled": not name or confidence < 0.6 or classification in {"online_address_only", "online_low_confidence", "no_name_found"},
        "position": point["unityPosition"],
    }


def main(argv):
    parser = argparse.ArgumentParser(description="Enrich NewMap building/road labels from coordinate-based preprocessing.")
    parser.add_argument("--allow-online", action="store_true", help="Permit online lookup when config also allows it.")
    parser.add_argument("--dry-run", action="store_true", help="Build the report without writing cache/report files.")
    args = parser.parse_args(argv)

    config = merged_config()
    points = collect_points()
    labels = []
    report = {
        "generatedAt": datetime.now(timezone.utc).isoformat(),
        "runtimeNetworkRequestsAllowed": False,
        "configPath": str(CONFIG_PATH.relative_to(PROJECT_ROOT)),
        "cachePath": str(CACHE_PATH.relative_to(PROJECT_ROOT)),
        "inputPointCount": len(points),
        "sourceOrProjectNamesUsed": 0,
        "queryableMissingNamePoints": 0,
        "onlineQueriesAttempted": 0,
        "onlineNamesAccepted": 0,
        "onlineAddressOnlyRejected": 0,
        "onlineNoNameFound": 0,
        "skippedNoCoordinates": 0,
        "skippedOnlineDisabled": 0,
        "providerAttribution": [],
        "status": "not_run",
        "errors": [],
    }

    if not config.get("enabled", True):
        report["status"] = "disabled"
    else:
        for point in points:
            source_label = label_from_source(point)
            if source_label and point["objectType"] in {"building", "road", "landmark", "candidate", "shelter"}:
                labels.append(source_label)
                report["sourceOrProjectNamesUsed"] += 1
                continue

            if not is_queryable(point, config):
                if point["lat"] is None or point["lon"] is None:
                    report["skippedNoCoordinates"] += 1
                continue

            report["queryableMissingNamePoints"] += 1
            online_allowed = args.allow_online and bool(config.get("allowOnlineLookup", False))
            if not online_allowed:
                report["skippedOnlineDisabled"] += 1
                continue

            if report["onlineQueriesAttempted"] >= int(config["maxQueriesPerRun"]):
                break

            try:
                if report["onlineQueriesAttempted"] > 0:
                    time.sleep(float(config["rateLimitSeconds"]))
                response = nominatim_reverse(point, config)
                report["onlineQueriesAttempted"] += 1
                name, classification, confidence, raw_type = classify_online_result(point, response, float(config["minConfidence"]))
                labels.append(label_from_online(point, name, classification, confidence, raw_type))
                if name and confidence >= float(config["minConfidence"]) and classification == "online_exact_or_near_match":
                    report["onlineNamesAccepted"] += 1
                elif classification == "online_address_only":
                    report["onlineAddressOnlyRejected"] += 1
                else:
                    report["onlineNoNameFound"] += 1
            except Exception as exc:  # noqa: BLE001
                report["errors"].append({"id": point["id"], "error": str(exc)})

        report["status"] = "completed"

    cache = {
        "generatedAt": report["generatedAt"],
        "runtimeNetworkRequestsAllowed": False,
        "sourceStatus": "source_or_cached_names_available" if any(label["objectType"] in {"building", "road"} and not label["disabled"] for label in labels) else "no_source_name_available",
        "labels": labels,
    }
    if config.get("writeAttribution", True) and report["onlineQueriesAttempted"] > 0:
        report["providerAttribution"].append(
            {
                "provider": "OpenStreetMap/Nominatim",
                "usagePolicy": "https://operations.osmfoundation.org/policies/nominatim/",
                "dataLicense": "ODbL",
                "note": "Results are cached locally; Unity player runtime performs no web requests.",
            }
        )

    report["cachedLabelCount"] = len(labels)
    report["normalRuntimeBuildingRoadLabels"] = sum(
        1 for label in labels if label["objectType"] in {"building", "road"} and not label["disabled"] and label["confidence"] >= float(config["minConfidence"])
    )

    if not args.dry_run:
        write_json(CACHE_PATH, cache)
        write_json(REPORT_PATH, report)
    print(json.dumps(report, ensure_ascii=False, indent=2))
    return 0 if not report["errors"] else 2


if __name__ == "__main__":
    raise SystemExit(main(sys.argv[1:]))
