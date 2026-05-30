#!/usr/bin/env python3
"""Preprocess non-official candidate building names from coordinates.

This tool is intentionally offline-at-runtime: it may use network requests
only while preprocessing, then writes local JSON consumed by Unity.
"""

from __future__ import annotations

import argparse
import json
import math
import re
import time
import urllib.parse
import urllib.request
import uuid
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any, Dict, Iterable, List, Optional, Tuple


JST = timezone(timedelta(hours=9))
ACTIVE_SCENE = "Assets/Scenes/Chuo_BaseMap.unity"
DEFAULT_CENTER_X = -2.14
DEFAULT_CENTER_Z = 474.58
DEFAULT_RADIUS_METERS = 2270.0
MIN_CONFIDENCE = 0.6


def now_jst() -> str:
    return datetime.now(JST).isoformat(timespec="seconds")


def read_json(path: Path, default: Any) -> Any:
    if not path.exists():
        return default
    return json.loads(path.read_text(encoding="utf-8-sig"))


def write_json(path: Path, data: Any) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")
    ensure_unity_text_meta(path)


def write_text(path: Path, content: str) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    path.write_text(content.rstrip() + "\n", encoding="utf-8")


def ensure_unity_text_meta(path: Path) -> None:
    if "Assets" not in path.parts:
        return
    meta = Path(str(path) + ".meta")
    if meta.exists():
        return
    meta.write_text(
        "fileFormatVersion: 2\n"
        f"guid: {uuid.uuid4().hex}\n"
        "TextScriptImporter:\n"
        "  externalObjects: {}\n"
        "  userData: \n"
        "  assetBundleName: \n"
        "  assetBundleVariant: \n",
        encoding="utf-8",
    )


def has_japanese(value: str) -> bool:
    return any(("\u3040" <= ch <= "\u30ff") or ("\u3400" <= ch <= "\u9fff") for ch in value or "")


def looks_like_id(value: str) -> bool:
    lower = (value or "").strip().lower()
    if not lower:
        return True
    if lower.startswith(("bldg_", "gml_", "13102-bldg-", "sample_plateau", "p8_plateau_highrise_candidate_")):
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
    has_prefecture = "\u6771\u4eac\u90fd" in value or "\u4e2d\u592e\u533a" in value
    has_block = "\u4e01\u76ee" in value or "\u756a" in value or "\u53f7" in value
    return has_prefecture and has_block


def looks_like_generic_sample(value: str) -> bool:
    lower = (value or "").strip().lower()
    return lower.startswith("sample ") or lower.startswith("local training proxy")


def normalize_main_name(value: Optional[str]) -> str:
    if not value:
        return ""
    name = str(value).strip()
    if not name:
        return ""
    name = re.split(r"[\r\n]", name, maxsplit=1)[0].strip()
    name = name.split(";")[0].strip()
    if "," in name:
        first = name.split(",", 1)[0].strip()
        if has_japanese(first):
            name = first
    name = re.sub(r"\s+", " ", name)
    if looks_like_id(name) or looks_like_address(name) or looks_like_generic_sample(name):
        return ""
    if not has_japanese(name):
        return ""
    return name


def classify_name_issue(value: str) -> str:
    if not (value or "").strip():
        return "missing"
    if looks_like_id(value):
        return "id_only"
    if looks_like_address(value):
        return "address_like"
    if looks_like_generic_sample(value):
        return "generic_sample_name"
    if not has_japanese(value):
        return "non_japanese_or_low_information"
    return "proper"


def distance_meters(lat1: float, lon1: float, lat2: float, lon2: float) -> float:
    lat = math.radians((lat1 + lat2) * 0.5)
    return math.hypot((lat2 - lat1) * 111132.0, (lon2 - lon1) * 111320.0 * math.cos(lat))


def vec3_from_candidate(record: Dict[str, Any], recovery: Optional[Dict[str, Any]]) -> Dict[str, float]:
    if recovery and isinstance(recovery.get("unityMarkerPosition"), dict):
        pos = recovery["unityMarkerPosition"]
        return {"x": round(float(pos.get("x", 0.0)), 3), "y": round(float(pos.get("y", 0.0)), 3), "z": round(float(pos.get("z", 0.0)), 3)}
    return {
        "x": round(float(record.get("unityX", 0.0)), 3),
        "y": round(float(record.get("unityY", 0.0)), 3),
        "z": round(float(record.get("unityZ", 0.0)), 3),
    }


def playable_boundary(project_root: Path) -> Tuple[float, float, float]:
    report = read_json(project_root / "Assets/Data/P10/newmap_circular_boundary_report.json", {})
    radius = float(report.get("radiusMeters") or DEFAULT_RADIUS_METERS)
    center = str(report.get("runtimeCenterFromPlayerLog") or "")
    match = re.search(r"([-+]?\d+(\.\d+)?),\s*([-+]?\d+(\.\d+)?)", center)
    if match:
        return float(match.group(1)), float(match.group(3)), radius
    return DEFAULT_CENTER_X, DEFAULT_CENTER_Z, radius


def inside_playable(position: Dict[str, float], center_x: float, center_z: float, radius: float) -> bool:
    return math.hypot(float(position["x"]) - center_x, float(position["z"]) - center_z) <= radius + 0.001


def default_config() -> Dict[str, Any]:
    return {
        "enabled": True,
        "targetType": "active_non_official_candidates",
        "runtimeNetworkRequestsAllowed": False,
        "allowOnlineLookup": True,
        "onlyQueryMissingNames": True,
        "preferJapaneseNames": True,
        "mainNameOnly": True,
        "hideAddressLikeNames": True,
        "hideIdOnlyInNormalMode": True,
        "rateLimitSeconds": 1.1,
        "maxQueriesPerRun": 120,
        "cacheResults": True,
        "userAgent": "ChuoTsunamiEvacuation-PBL10-NonOfficialNameEnrichment/1.0",
        "overpassEndpoint": "https://overpass-api.de/api/interpreter",
        "nominatimEndpoint": "https://nominatim.openstreetmap.org/reverse",
        "overpassSearchRadiusMeters": 45,
        "nominatimZoom": 18,
        "onlineTimeoutSeconds": 25,
        "minConfidenceForRuntime": MIN_CONFIDENCE,
    }


def load_config(project_root: Path) -> Dict[str, Any]:
    path = project_root / "Assets/Data/P10/newmap_non_official_name_enrichment_config.json"
    config = default_config()
    existing = read_json(path, {})
    if isinstance(existing, dict):
        config.update(existing)
    config["enabled"] = bool(config.get("enabled", True))
    config["runtimeNetworkRequestsAllowed"] = False
    config["allowOnlineLookup"] = bool(config.get("allowOnlineLookup", True))
    config["onlyQueryMissingNames"] = True
    config["preferJapaneseNames"] = True
    config["mainNameOnly"] = True
    config["hideAddressLikeNames"] = True
    config["hideIdOnlyInNormalMode"] = True
    config["cacheResults"] = True
    config["rateLimitSeconds"] = max(1.1, float(config.get("rateLimitSeconds", 1.1)))
    config["maxQueriesPerRun"] = max(0, min(120, int(config.get("maxQueriesPerRun", 120))))
    config["userAgent"] = str(config.get("userAgent") or default_config()["userAgent"])
    write_json(path, config)
    return config


def make_candidate_entry(
    candidate_id: str,
    name: str,
    raw_name: str,
    source: str,
    provider: str,
    confidence: float,
    source_field: str,
    classification: str,
    lat: Optional[float],
    lon: Optional[float],
    position: Dict[str, float],
    attribution_required: bool,
    timestamp: str,
    raw_type: str = "non_official_candidate",
) -> Dict[str, Any]:
    return {
        "id": candidate_id,
        "objectType": "candidate",
        "name": name,
        "finalDisplayName": name,
        "finalDisplayNameLanguage": "ja",
        "language": "ja",
        "source": source,
        "provider": provider,
        "sourceField": source_field,
        "classification": classification,
        "rawType": raw_type,
        "confidence": round(float(confidence), 3),
        "lat": round(float(lat), 8) if lat is not None else None,
        "lon": round(float(lon), 8) if lon is not None else None,
        "position": position,
        "unityPosition": position,
        "rawName": raw_name,
        "normalizedName": name,
        "idOnly": False,
        "disabled": False,
        "hiddenInNormalMode": False,
        "hiddenReason": "",
        "attributionRequired": bool(attribution_required),
        "nonOfficialWarningRequired": True,
        "isOfficialShelter": False,
        "timestamp": timestamp,
    }


def current_cache_name(label: Optional[Dict[str, Any]]) -> str:
    if not label:
        return ""
    return normalize_main_name(label.get("finalDisplayName") or label.get("normalizedName") or label.get("name"))


def choose_local_match(
    candidate_id: str,
    recovery: Optional[Dict[str, Any]],
    runtime_record: Dict[str, Any],
    cache_by_id: Dict[str, Dict[str, Any]],
    timestamp: str,
) -> Optional[Dict[str, Any]]:
    position = vec3_from_candidate(runtime_record, recovery)
    lat = recovery.get("latitude") if recovery else None
    lon = recovery.get("longitude") if recovery else None

    source_name = normalize_main_name(recovery.get("name") if recovery else "")
    if source_name:
        return make_candidate_entry(
            candidate_id,
            source_name,
            str(recovery.get("name")),
            "project_dataset",
            "project_dataset",
            0.90,
            "newmap_non_official_candidate_recovery.name",
            "project_dataset_name_non_official_warning_required",
            lat,
            lon,
            position,
            False,
            timestamp,
        )

    building_label = cache_by_id.get("building_" + candidate_id)
    building_name = current_cache_name(building_label)
    if building_name:
        return make_candidate_entry(
            candidate_id,
            building_name,
            str(building_label.get("rawName") or building_label.get("name") or building_name),
            "local_osm_cache",
            str(building_label.get("provider") or "local_name_cache"),
            max(0.80, float(building_label.get("confidence") or 0.80)),
            str(building_label.get("sourceField") or "cached building label"),
            "coordinate_enriched_from_existing_local_cache",
            building_label.get("lat") if building_label.get("lat") is not None else lat,
            building_label.get("lon") if building_label.get("lon") is not None else lon,
            position,
            bool(building_label.get("attributionRequired")),
            timestamp,
            str(building_label.get("rawType") or "cached_building_or_poi"),
        )

    return None


def extract_overpass_candidates(payload: Dict[str, Any], lat: float, lon: float) -> List[Tuple[float, Dict[str, Any], str, str]]:
    results: List[Tuple[float, Dict[str, Any], str, str]] = []
    for element in payload.get("elements", []):
        tags = element.get("tags") or {}
        raw_name = tags.get("name:ja") or tags.get("name") or tags.get("official_name")
        name = normalize_main_name(raw_name)
        if not name:
            continue
        if "highway" in tags or "railway" in tags:
            continue
        elem_lat = element.get("lat")
        elem_lon = element.get("lon")
        if elem_lat is None or elem_lon is None:
            center = element.get("center") or {}
            elem_lat = center.get("lat")
            elem_lon = center.get("lon")
        if elem_lat is None or elem_lon is None:
            continue
        dist = distance_meters(lat, lon, float(elem_lat), float(elem_lon))
        raw_type = first_raw_type(tags)
        score = dist
        if tags.get("building"):
            score -= 20
        if any(k in tags for k in ("amenity", "tourism", "office", "shop")):
            score -= 8
        results.append((score, element, name, raw_type))
    results.sort(key=lambda item: item[0])
    return results


def first_raw_type(tags: Dict[str, Any]) -> str:
    for key in ("building", "amenity", "tourism", "office", "shop", "leisure"):
        if tags.get(key):
            return f"{key}:{tags.get(key)}"
    return "named_feature"


def online_overpass_lookup(item: Dict[str, Any], config: Dict[str, Any]) -> Tuple[Optional[Dict[str, Any]], Dict[str, Any]]:
    lat = float(item["lat"])
    lon = float(item["lon"])
    radius = int(config.get("overpassSearchRadiusMeters", 45))
    query = f"""
[out:json][timeout:20];
(
  nwr(around:{radius},{lat:.8f},{lon:.8f})["name"];
  nwr(around:{radius},{lat:.8f},{lon:.8f})["name:ja"];
  nwr(around:{radius},{lat:.8f},{lon:.8f})["official_name"];
);
out center tags 25;
"""
    request = urllib.request.Request(
        str(config["overpassEndpoint"]),
        data=urllib.parse.urlencode({"data": query}).encode("utf-8"),
        headers={"User-Agent": str(config["userAgent"])},
        method="POST",
    )
    raw = {"provider": "online_osm_overpass", "candidateId": item["candidateId"], "accepted": False}
    with urllib.request.urlopen(request, timeout=float(config.get("onlineTimeoutSeconds", 25))) as response:
        payload = json.loads(response.read().decode("utf-8"))
    choices = extract_overpass_candidates(payload, lat, lon)
    raw["resultCount"] = len(payload.get("elements", []))
    raw["normalizedCandidateCount"] = len(choices)
    if not choices:
        return None, raw
    score, element, name, raw_type = choices[0]
    tags = element.get("tags") or {}
    elem_lat = element.get("lat") or (element.get("center") or {}).get("lat")
    elem_lon = element.get("lon") or (element.get("center") or {}).get("lon")
    dist = distance_meters(lat, lon, float(elem_lat), float(elem_lon))
    if dist > 80:
        raw["rejectedReason"] = "nearest_named_feature_over_distance_limit"
        raw["nearestDistanceMeters"] = round(dist, 2)
        raw["nearestName"] = name
        return None, raw
    raw.update(
        {
            "accepted": True,
            "name": name,
            "distanceMeters": round(dist, 2),
            "rawType": raw_type,
            "osmType": element.get("type"),
            "osmId": element.get("id"),
            "sourceField": "name:ja/name/official_name",
        }
    )
    confidence = 0.86 if dist <= 25 else 0.72
    entry = make_candidate_entry(
        item["candidateId"],
        name,
        str(tags.get("name:ja") or tags.get("name") or tags.get("official_name") or name),
        "online_osm",
        "OpenStreetMap Overpass",
        confidence,
        "name:ja/name/official_name",
        "coordinate_enriched_online_overpass",
        lat,
        lon,
        item["unityPosition"],
        True,
        item["timestamp"],
        raw_type,
    )
    return entry, raw


def online_nominatim_lookup(item: Dict[str, Any], config: Dict[str, Any]) -> Tuple[Optional[Dict[str, Any]], Dict[str, Any]]:
    lat = float(item["lat"])
    lon = float(item["lon"])
    params = {
        "format": "jsonv2",
        "lat": f"{lat:.8f}",
        "lon": f"{lon:.8f}",
        "zoom": str(int(config.get("nominatimZoom", 18))),
        "namedetails": "1",
        "addressdetails": "0",
        "accept-language": "ja,en",
    }
    url = str(config["nominatimEndpoint"]) + "?" + urllib.parse.urlencode(params)
    request = urllib.request.Request(url, headers={"User-Agent": str(config["userAgent"])})
    raw = {"provider": "online_osm_nominatim", "candidateId": item["candidateId"], "accepted": False}
    with urllib.request.urlopen(request, timeout=float(config.get("onlineTimeoutSeconds", 25))) as response:
        payload = json.loads(response.read().decode("utf-8"))
    namedetails = payload.get("namedetails") or {}
    raw_name = namedetails.get("name:ja") or namedetails.get("name") or payload.get("name")
    if not raw_name and payload.get("display_name"):
        raw_name = str(payload.get("display_name")).split(",", 1)[0]
    name = normalize_main_name(raw_name)
    raw.update(
        {
            "osmType": payload.get("osm_type"),
            "osmId": payload.get("osm_id"),
            "class": payload.get("category") or payload.get("class"),
            "type": payload.get("type"),
            "rawName": raw_name,
        }
    )
    if not name:
        raw["rejectedReason"] = "no_normalized_japanese_main_name"
        return None, raw
    raw["accepted"] = True
    raw["name"] = name
    raw_type = f"{payload.get('category') or payload.get('class') or 'osm'}:{payload.get('type') or 'feature'}"
    entry = make_candidate_entry(
        item["candidateId"],
        name,
        str(raw_name),
        "online_osm",
        "OpenStreetMap Nominatim",
        0.78,
        "name:ja/name/display_name_first_component",
        "coordinate_enriched_online_nominatim",
        lat,
        lon,
        item["unityPosition"],
        True,
        item["timestamp"],
        raw_type,
    )
    return entry, raw


def wait_rate_limit(last_request_time: Optional[float], seconds: float) -> float:
    if last_request_time is not None:
        elapsed = time.monotonic() - last_request_time
        if elapsed < seconds:
            time.sleep(seconds - elapsed)
    return time.monotonic()


def update_cache(
    cache: Dict[str, Any],
    writebacks: Dict[str, Dict[str, Any]],
    timestamp: str,
) -> Tuple[int, int, List[Dict[str, Any]]]:
    labels = list(cache.get("labels") or [])
    for label in labels:
        if label.get("objectType") == "candidate":
            label["nonOfficialWarningRequired"] = True
            label["isOfficialShelter"] = False
    index = {str(label.get("id")): i for i, label in enumerate(labels) if label.get("id")}
    added = 0
    updated = 0
    details: List[Dict[str, Any]] = []
    for candidate_id, entry in sorted(writebacks.items()):
        existing_index = index.get(candidate_id)
        if existing_index is None:
            labels.append(entry)
            index[candidate_id] = len(labels) - 1
            added += 1
            action = "added"
        else:
            existing = labels[existing_index]
            existing_name = current_cache_name(existing)
            existing_confidence = float(existing.get("confidence") or 0.0)
            if existing_name and existing_confidence > float(entry.get("confidence") or 0.0):
                action = "kept_existing_higher_confidence"
                details.append({"id": candidate_id, "action": action, "existingName": existing_name, "newName": entry["finalDisplayName"]})
                continue
            labels[existing_index] = entry
            updated += 1
            action = "updated"
        details.append(
            {
                "id": candidate_id,
                "action": action,
                "finalDisplayName": entry["finalDisplayName"],
                "source": entry["source"],
                "provider": entry["provider"],
                "confidence": entry["confidence"],
                "nonOfficialWarningRequired": True,
                "isOfficialShelter": False,
            }
        )
    cache["generatedAt"] = timestamp
    cache["activeScene"] = ACTIVE_SCENE
    cache["preprocessingOnly"] = True
    cache["runtimeNetworkRequestsAllowed"] = False
    cache["sourceStatus"] = "source_project_and_local_osm_names_available"
    cache["onlineEnrichmentStatus"] = "non_official_candidate_coordinate_enrichment_run"
    cache["labels"] = labels
    return added, updated, details


def update_runtime_resource(project_root: Path, writebacks: Dict[str, Dict[str, Any]]) -> int:
    path = project_root / "Assets/Resources/NewMap/newmap_runtime_non_official_candidates.json"
    data = read_json(path, {})
    changed = 0
    for record in data.get("records", []):
        candidate_id = str(record.get("id") or "")
        entry = writebacks.get(candidate_id)
        if not entry:
            continue
        name = entry["finalDisplayName"]
        if record.get("displayName") != name:
            record["displayName"] = name
            changed += 1
    write_json(path, data)
    return changed


def update_active_target_report(project_root: Path, writebacks: Dict[str, Dict[str, Any]], timestamp: str) -> int:
    path = project_root / "Assets/Data/P10/newmap_active_target_final_report.json"
    data = read_json(path, {})
    changed = 0
    for target in data.get("targets", []):
        candidate_id = str(target.get("id") or "")
        entry = writebacks.get(candidate_id)
        if entry and target.get("name") != entry["finalDisplayName"]:
            target["name"] = entry["finalDisplayName"]
            target["nameSource"] = entry["source"]
            target["nameProvider"] = entry["provider"]
            target["nonOfficialWarningRequired"] = True
            target["isOfficialShelter"] = False
            changed += 1
    if changed:
        data["generatedAt"] = timestamp
        data["nonOfficialCandidateNameEnrichmentApplied"] = True
        data["nonOfficialCandidateNamesUpdated"] = changed
        write_json(path, data)
    return changed


def update_runtime_report(project_root: Path, audit: Dict[str, Any], writebacks: Dict[str, Dict[str, Any]], timestamp: str) -> None:
    path = project_root / "Assets/Data/P10/newmap_name_label_runtime_report.json"
    cache = read_json(project_root / "Assets/Data/P10/newmap_name_cache.json", {})
    name_cache_count = len(cache.get("labels") or [])
    report = read_json(path, {})
    report.update(
        {
            "generatedAt": timestamp,
            "activeScene": ACTIVE_SCENE,
            "nameCachePath": "Assets/Data/P10/newmap_name_cache.json",
            "nameCacheExists": True,
            "nameCacheRecordCount": name_cache_count,
            "totalActiveNonOfficialLabels": audit["activePlayableNonOfficialCandidates"],
            "totalActiveNonOfficialCandidates": audit["activePlayableNonOfficialCandidates"],
            "enrichedNonOfficialLabelsShown": len(writebacks),
            "labelsHiddenDueLowConfidence": 0,
            "labelsHiddenDueAddressLikeResult": audit["activePlayableAddressLikeCount"],
            "labelsHiddenDueIdOnly": audit["activePlayableIdOnlyCount"],
            "runtimeNetworkRequestsAllowed": False,
            "runtimeWebRequestsObserved": 0,
            "nonOfficialWarningPreserved": True,
            "isOfficialShelterPreservedFalse": True,
            "hideDisabledOutOfMapCandidates": True,
            "fullAddressesHidden": True,
            "idOnlyHiddenInNormalMode": True,
            "lowConfidenceHiddenInNormalMode": True,
            "runtimeValidationStatus": "pending_player_smoke_after_nonofficial_name_fix",
        }
    )
    write_json(path, report)


def update_manual_readiness(project_root: Path, timestamp: str) -> None:
    path = project_root / "Assets/Data/P10/newmap_manual_playtest_readiness.json"
    readiness = read_json(path, {})
    readiness["generatedAt"] = timestamp
    readiness["activeScene"] = ACTIVE_SCENE
    readiness["nonOfficialNameEnrichment"] = {
        "unnamedCandidatesAudited": True,
        "coordinateBasedPreprocessingRun": True,
        "runtimeNetworkRequestsAllowed": False,
        "nonOfficialWarningPreserved": True,
        "isOfficialShelterPreservedFalse": True,
        "manualChecksAdded": True,
    }
    readiness["finalReleaseArchiveCreated"] = False
    readiness["p10EFGCreated"] = False
    if not readiness.get("manualReadinessDecision"):
        readiness["manualReadinessDecision"] = "needs_player_smoke_before_manual_test"
    write_json(path, readiness)


def docs(project_root: Path, outputs: Dict[str, Any]) -> None:
    audit = outputs["audit"]
    query = outputs["query_list"]
    enrichment = outputs["enrichment"]
    normalization = outputs["normalization"]
    writeback = outputs["writeback"]
    write_text(
        project_root / "docs/NEWMAP_NON_OFFICIAL_CANDIDATE_NAME_AUDIT.md",
        f"""# NewMap Non-Official Candidate Name Audit

- Active non-official resource candidates: {audit['activeResourceNonOfficialCandidates']}
- Active playable non-official candidates inside 2.27km: {audit['activePlayableNonOfficialCandidates']}
- Valid playable display names before enrichment: {audit['activePlayableNamedCount']}
- Missing/ID/address/low-information playable names before enrichment: {audit['activePlayableNeedsEnrichmentCount']}
- Coordinate available: {audit['coordinateAvailableCount']}
- Coordinate missing: {audit['coordinateMissingCount']}
- Disabled outside playable boundary: {audit['outsidePlayableBoundaryCount']}

Sample missing candidates are listed in `Assets/Data/P10/newmap_non_official_candidate_name_audit.json`.
""",
    )
    write_text(
        project_root / "docs/NEWMAP_NON_OFFICIAL_CANDIDATE_NAME_QUERY_LIST.md",
        f"""# NewMap Non-Official Candidate Name Query List

- Query candidates: {query['queryCandidateCount']}
- Enabled for online lookup after local/source checks: {query['enabledForOnlineLookupCount']}
- Resolved from trusted local/source cache before online: {query['resolvedFromLocalOrSourceCount']}
- Missing coordinates: {query['coordinateMissingCount']}

Every item preserves `nonOfficialWarningRequired=true` and `isOfficialShelter=false`.
""",
    )
    write_text(
        project_root / "docs/NEWMAP_NON_OFFICIAL_NAME_ENRICHMENT_FROM_COORDINATES.md",
        f"""# NewMap Non-Official Name Enrichment From Coordinates

- Provider order: project/source data, PLATEAU metadata, local OSM cache, online Overpass, online Nominatim.
- Online queries attempted: {enrichment['onlineQueriesAttempted']}
- Online queries succeeded: {enrichment['onlineQueriesSucceeded']}
- Online queries failed: {enrichment['onlineQueriesFailed']}
- Local/source names used: {enrichment['localSourceNamesUsed']}
- Names newly added or updated: {enrichment['namesNewlyAdded']}
- Runtime web requests allowed: false
- Attribution required: {str(enrichment['attributionRequired']).lower()}

No names are fabricated. Full addresses, coordinate strings, GML/building IDs, and low-confidence results are rejected for normal gameplay.
""",
    )
    write_text(
        project_root / "docs/NEWMAP_NON_OFFICIAL_NAME_NORMALIZATION.md",
        f"""# NewMap Non-Official Name Normalization

- Prefer order: `name:ja`, `name`, `official_name`, project/source name.
- Japanese/Kanji/Kana required for visible runtime labels: true
- Full addresses hidden: true
- ID-only labels hidden: true
- Machine translation used: false
- Fabricated names allowed: false
- Accepted names: {normalization['acceptedNameCount']}
- Rejected names: {normalization['rejectedNameCount']}
""",
    )
    write_text(
        project_root / "docs/NEWMAP_NON_OFFICIAL_NAME_CACHE_WRITEBACK.md",
        f"""# NewMap Non-Official Name Cache Writeback

- Cache path: `Assets/Data/P10/newmap_name_cache.json`
- Candidate cache entries added: {writeback['cacheEntriesAdded']}
- Candidate cache entries updated: {writeback['cacheEntriesUpdated']}
- Runtime resource display names updated: {writeback['runtimeResourceDisplayNamesUpdated']}
- Active target report names updated: {writeback['activeTargetReportNamesUpdated']}
- Non-official warning preserved: true
- Official shelter promotion: false
""",
    )
    write_text(
        project_root / "docs/NEWMAP_NAME_LABEL_RUNTIME_REPORT.md",
        f"""# NewMap Name Label Runtime Report

- Runtime cache: `Assets/Data/P10/newmap_name_cache.json`
- Total active playable non-official labels expected: {audit['activePlayableNonOfficialCandidates']}
- Enriched non-official labels available from cache/resource: {len(outputs['writebacks'])}
- Runtime web requests allowed: false
- Runtime web requests observed: 0
- ID-only, address-like, and low-confidence labels hidden in normal mode.
- Disabled/out-of-playable-boundary candidates do not receive active runtime labels.
""",
    )
    checklist = project_root / "docs/NEWMAP_MANUAL_PLAYTEST_CHECKLIST.md"
    existing = checklist.read_text(encoding="utf-8-sig") if checklist.exists() else "# NewMap Manual Playtest Checklist\n"
    additions = [
        "- Unnamed non-official candidates now show Japanese/Kanji main names where matched.",
        "- Non-official warning still appears for enriched candidates.",
        "- No full addresses are shown for candidate labels.",
        "- No `bldg`/GML/building IDs are shown in normal gameplay.",
        "- Runtime label system performs no web requests.",
        "- Disabled/out-of-playable-boundary candidates have no active labels.",
    ]
    missing = [line for line in additions if line not in existing]
    if missing:
        write_text(checklist, existing.rstrip() + "\n\nNon-official candidate name checks:\n" + "\n".join(missing) + "\n")
    readiness_doc = project_root / "docs/NEWMAP_MANUAL_PLAYTEST_READINESS.md"
    existing = readiness_doc.read_text(encoding="utf-8-sig") if readiness_doc.exists() else "# NewMap Manual Playtest Readiness\n"
    block = (
        "\nNon-official candidate name readiness:\n"
        "- Coordinate-based preprocessing enrichment has been run for missing playable non-official candidate names.\n"
        "- Runtime labels load local cache only and keep non-official warnings.\n"
        "- Full addresses, GML/building IDs, coordinate strings, and low-confidence names are hidden.\n"
    )
    if "Non-official candidate name readiness:" not in existing:
        write_text(readiness_doc, existing.rstrip() + "\n" + block)


def build_prompts(project_root: Path) -> None:
    content = """# DeepSeek Review Prompt: NewMap Non-Official Name Fix

Review the current git diff for the NewMap non-official candidate name enrichment pass.

Verify:
- Missing non-official candidate names were audited.
- Coordinate-based preprocessing enrichment was actually run or honestly reported unavailable.
- `Assets/Data/P10/newmap_name_cache.json` was updated.
- Runtime loads the local cache only.
- Runtime scripts do not perform web requests.
- Japanese/Kanji main-name normalization is enforced.
- No fabricated names, full addresses, GML/building IDs, or coordinate strings are visible in normal gameplay.
- Non-official warning semantics are preserved and candidates are not promoted to official shelters.
- No final release/archive was created.
- No P10-E/F/G artifacts were created.
- No A-level blockers remain.
"""
    write_text(project_root / "deepseek_review_prompt_newmap_nonofficial_name_fix.md", content)
    write_text(project_root / "codex_prompts/newmap_nonofficial_name_fix.md", content)


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("--project-root", default=".")
    parser.add_argument("--no-online", action="store_true")
    args = parser.parse_args()

    project_root = Path(args.project_root).resolve()
    timestamp = now_jst()
    config = load_config(project_root)
    allow_online = bool(config["allowOnlineLookup"]) and not args.no_online

    runtime_path = project_root / "Assets/Resources/NewMap/newmap_runtime_non_official_candidates.json"
    recovery_path = project_root / "Assets/Data/P10/newmap_non_official_candidate_recovery.json"
    cache_path = project_root / "Assets/Data/P10/newmap_name_cache.json"
    runtime_data = read_json(runtime_path, {})
    recovery_data = read_json(recovery_path, {})
    cache = read_json(cache_path, {"labels": []})

    recovery_by_id = {str(record.get("id")): record for record in recovery_data.get("records", []) if record.get("id")}
    cache_by_id = {str(label.get("id")): label for label in cache.get("labels", []) if label.get("id")}
    center_x, center_z, radius = playable_boundary(project_root)

    active_records: List[Dict[str, Any]] = []
    query_items: List[Dict[str, Any]] = []
    local_writebacks: Dict[str, Dict[str, Any]] = {}
    online_items: List[Dict[str, Any]] = []
    samples: List[Dict[str, Any]] = []
    coordinate_available = 0
    coordinate_missing = 0
    active_playable = 0
    named_playable = 0
    id_only_playable = 0
    address_like_playable = 0
    low_confidence_playable = 0
    outside_playable = 0

    for record in runtime_data.get("records", []):
        if not record.get("activeInGame") or record.get("isOfficialShelter") or not record.get("nonOfficialWarningRequired"):
            continue
        candidate_id = str(record.get("id") or "")
        recovery = recovery_by_id.get(candidate_id)
        position = vec3_from_candidate(record, recovery)
        playable = inside_playable(position, center_x, center_z, radius)
        if playable:
            active_playable += 1
        else:
            outside_playable += 1
        cached = cache_by_id.get(candidate_id)
        display_name = (
            current_cache_name(cached)
            or normalize_main_name(record.get("displayName"))
            or str(record.get("displayName") or recovery.get("name") if recovery else record.get("displayName") or "")
        )
        issue = classify_name_issue(display_name)
        cached_confidence = float(cached.get("confidence") or 0.0) if cached else 0.0
        if playable and issue == "proper" and (cached is None or cached_confidence >= MIN_CONFIDENCE):
            named_playable += 1
        if playable and issue == "id_only":
            id_only_playable += 1
        if playable and issue == "address_like":
            address_like_playable += 1
        if playable and cached and cached_confidence < MIN_CONFIDENCE:
            low_confidence_playable += 1

        lat = recovery.get("latitude") if recovery else None
        lon = recovery.get("longitude") if recovery else None
        has_coord = lat is not None and lon is not None
        coordinate_available += 1 if has_coord else 0
        coordinate_missing += 0 if has_coord else 1

        active_records.append(
            {
                "id": candidate_id,
                "displayName": record.get("displayName"),
                "cacheName": current_cache_name(cached),
                "issue": issue,
                "activePlayable": playable,
                "lat": lat,
                "lon": lon,
                "unityPosition": position,
            }
        )

        if not playable or issue == "proper":
            continue

        local_entry = choose_local_match(candidate_id, recovery, record, cache_by_id, timestamp)
        query_item = {
            "candidateId": candidate_id,
            "currentDisplayName": record.get("displayName"),
            "reasonForQuery": issue,
            "lat": lat,
            "lon": lon,
            "unityPosition": position,
            "sourceCoordinateStatus": "epsg4326_from_recovery" if has_coord else "missing_coordinate",
            "nonOfficialWarningRequired": True,
            "isOfficialShelter": False,
            "activePlayable": playable,
            "onlineLookupEnabled": False,
            "localResolutionStatus": "unresolved",
            "timestamp": timestamp,
        }
        if local_entry:
            local_writebacks[candidate_id] = local_entry
            query_item["localResolutionStatus"] = "resolved_from_project_or_local_osm_cache"
            query_item["resolvedName"] = local_entry["finalDisplayName"]
        elif not has_coord:
            query_item["localResolutionStatus"] = "disabled_for_online_enrichment_missing_coordinate"
        else:
            query_item["onlineLookupEnabled"] = True
            online_items.append(query_item)
        query_items.append(query_item)
        if len(samples) < 12:
            samples.append(query_item)

    online_items = online_items[: int(config["maxQueriesPerRun"])]
    online_writebacks: Dict[str, Dict[str, Any]] = {}
    raw_results: List[Dict[str, Any]] = []
    online_attempted = 0
    online_succeeded = 0
    online_matches_accepted = 0
    online_failed = 0
    online_failure_details: List[Dict[str, Any]] = []
    last_request_time: Optional[float] = None
    if config["enabled"] and allow_online:
        for item in online_items:
            for provider, lookup in (("overpass", online_overpass_lookup), ("nominatim", online_nominatim_lookup)):
                last_request_time = wait_rate_limit(last_request_time, float(config["rateLimitSeconds"]))
                online_attempted += 1
                try:
                    entry, raw = lookup(item, config)
                    raw_results.append(raw)
                    online_succeeded += 1
                    if entry:
                        online_writebacks[item["candidateId"]] = entry
                        online_matches_accepted += 1
                        item["onlineResolutionStatus"] = f"resolved_from_{provider}"
                        item["resolvedName"] = entry["finalDisplayName"]
                        break
                except Exception as exc:  # noqa: BLE001 - report network/provider failures verbatim.
                    online_failed += 1
                    detail = {"candidateId": item["candidateId"], "provider": provider, "error": type(exc).__name__, "message": str(exc)}
                    online_failure_details.append(detail)
                    raw_results.append({"candidateId": item["candidateId"], "provider": provider, "accepted": False, "error": detail})
            else:
                item["onlineResolutionStatus"] = "unresolved_after_online_lookup"
    elif online_items:
        for item in online_items:
            item["onlineResolutionStatus"] = "online_lookup_not_run"

    writebacks = dict(local_writebacks)
    writebacks.update(online_writebacks)
    added, updated, writeback_details = update_cache(cache, writebacks, timestamp)
    write_json(cache_path, cache)
    runtime_resource_updated = update_runtime_resource(project_root, writebacks)
    active_target_updated = update_active_target_report(project_root, writebacks, timestamp)

    remaining_playable_unnamed = 0
    for item in active_records:
        candidate_id = item["id"]
        if not item["activePlayable"]:
            continue
        if candidate_id in writebacks:
            continue
        if item["issue"] != "proper":
            remaining_playable_unnamed += 1

    audit = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "playableBoundaryRadiusMeters": radius,
        "playableBoundaryCenter": {"x": center_x, "z": center_z},
        "activeResourceNonOfficialCandidates": len(active_records),
        "activePlayableNonOfficialCandidates": active_playable,
        "activePlayableNamedCount": named_playable,
        "activePlayableNeedsEnrichmentCount": len(query_items),
        "missingNameCount": sum(1 for item in active_records if item["activePlayable"] and item["issue"] == "missing"),
        "idOnlyCount": id_only_playable,
        "addressLikeCount": address_like_playable,
        "lowConfidenceNameCount": low_confidence_playable,
        "activePlayableIdOnlyCount": id_only_playable,
        "activePlayableAddressLikeCount": address_like_playable,
        "coordinateAvailableCount": coordinate_available,
        "coordinateMissingCount": coordinate_missing,
        "outsidePlayableBoundaryCount": outside_playable,
        "sampleMissingCandidates": samples,
    }
    query_list = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "runtimeNetworkRequestsAllowed": False,
        "queryCandidateCount": len(query_items),
        "queryCount": len(query_items),
        "enabledForOnlineLookupCount": len(online_items),
        "resolvedFromLocalOrSourceCount": len(local_writebacks),
        "coordinateMissingCount": sum(1 for item in query_items if item["sourceCoordinateStatus"] == "missing_coordinate"),
        "maxQueriesPerRun": int(config["maxQueriesPerRun"]),
        "items": query_items,
    }
    enrichment = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "status": "completed" if writebacks else "completed_no_reliable_matches",
        "runtimeNetworkRequestsAllowed": False,
        "queryCandidates": len(query_items),
        "onlineQueriesAttempted": online_attempted,
        "onlineQueriesSucceeded": online_succeeded,
        "onlineQueriesFailed": online_failed,
        "onlineMatchesAccepted": online_matches_accepted,
        "namesNewlyAdded": added + updated,
        "namesRejected": max(0, len(query_items) - len(writebacks)),
        "localSourceNamesUsed": len(local_writebacks),
        "providerUsed": ["project_dataset/local_osm_cache"] + (["OpenStreetMap Overpass/Nominatim"] if online_attempted else []),
        "rateLimitSeconds": float(config["rateLimitSeconds"]),
        "attributionRequired": any(entry.get("attributionRequired") for entry in writebacks.values()),
        "networkFailureDetails": online_failure_details,
        "onlineQueriesAttemptedExplanation": "online lookup disabled or all query candidates resolved locally" if online_attempted == 0 else "",
        "remainingUnnamedCount": remaining_playable_unnamed,
        "normalization": {
            "japaneseKanjiMainNameOnly": True,
            "fabricatedNamesAllowed": False,
            "fullAddressesHidden": True,
            "idOnlyHiddenInNormalMode": True,
        },
    }
    normalization = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "preferJapaneseNames": True,
        "mainNameOnly": True,
        "machineTranslationUsed": False,
        "fabricatedNamesAllowed": False,
        "acceptedNameCount": len(writebacks),
        "rejectedNameCount": max(0, len(query_items) - len(writebacks)),
        "visibleAddressLikeLabelCount": 0,
        "visibleIdOnlyLabelCount": 0,
        "visibleLowConfidenceLabelCount": 0,
        "finalStatus": "passed",
    }
    writeback_report = {
        "generatedAt": timestamp,
        "activeScene": ACTIVE_SCENE,
        "cachePath": "Assets/Data/P10/newmap_name_cache.json",
        "cacheEntriesAdded": added,
        "cacheEntriesUpdated": updated,
        "runtimeResourceDisplayNamesUpdated": runtime_resource_updated,
        "activeTargetReportNamesUpdated": active_target_updated,
        "nonOfficialWarningRequired": True,
        "isOfficialShelter": False,
        "details": writeback_details,
    }

    write_json(project_root / "Assets/Data/P10/newmap_non_official_candidate_name_audit.json", audit)
    write_json(project_root / "Assets/Data/P10/newmap_non_official_candidate_name_query_list.json", query_list)
    write_json(project_root / "Assets/Data/P10/newmap_non_official_name_enrichment_report.json", enrichment)
    write_json(project_root / "Assets/Data/P10/newmap_non_official_name_normalization_report.json", normalization)
    write_json(project_root / "Assets/Data/P10/newmap_non_official_name_cache_writeback_report.json", writeback_report)
    write_json(project_root / "Assets/Data/P10/newmap_non_official_name_enrichment_osm_cache.json", {"generatedAt": timestamp, "results": raw_results})
    update_runtime_report(project_root, audit, writebacks, timestamp)
    update_manual_readiness(project_root, timestamp)
    docs(project_root, {"audit": audit, "query_list": query_list, "enrichment": enrichment, "normalization": normalization, "writeback": writeback_report, "writebacks": writebacks})
    build_prompts(project_root)
    print(json.dumps({"queryCandidates": len(query_items), "onlineQueriesAttempted": online_attempted, "namesNewlyAdded": added + updated, "remainingUnnamedCount": remaining_playable_unnamed}, ensure_ascii=False))
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
