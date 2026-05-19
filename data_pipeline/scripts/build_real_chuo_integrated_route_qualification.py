from __future__ import annotations

import csv
import json
from collections import Counter
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
PROCESSED_QUALIFICATION_ROOT = PIPELINE_ROOT / "processed" / "qualification"
PROCESSED_ROUTES_ROOT = PIPELINE_ROOT / "processed" / "routes"
QGIS_QA_ROOT = PIPELINE_ROOT / "processed" / "qgis_qa"

B4_QUALIFICATION_JSON = PROCESSED_QUALIFICATION_ROOT / "real_chuo_building_qualification.json"
ROUTES_JSON = PROCESSED_ROUTES_ROOT / "real_chuo_osm_routes_sample.json"

OUTPUT_JSON = PROCESSED_QUALIFICATION_ROOT / "real_chuo_integrated_route_qualification.json"
OUTPUT_CSV = PROCESSED_QUALIFICATION_ROOT / "real_chuo_integrated_route_qualification.csv"

OUTPUT_ROUTE_LINES = QGIS_QA_ROOT / "real_chuo_route_lines.geojson"
OUTPUT_ROUTE_ORIGINS = QGIS_QA_ROOT / "real_chuo_route_origins.geojson"
OUTPUT_ROUTE_FAILURES = QGIS_QA_ROOT / "real_chuo_route_failures.geojson"

INTEGRATED_FIELDS = [
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
    "routeSource",
    "routeType",
    "isOfficialEvacuationRoute",
    "nearestRouteId",
    "nearestRouteOriginId",
    "nearestRouteOriginName",
    "availableRouteCount",
    "failedRouteCount",
    "notes",
]


def now_jst() -> str:
    return datetime.now(timezone(timedelta(hours=9))).replace(microsecond=0).isoformat()


def load_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: Path, payload: dict[str, Any], compact: bool = False) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        if compact:
            json.dump(payload, handle, ensure_ascii=False, separators=(",", ":"))
        else:
            json.dump(payload, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def write_csv(path: Path, records: list[dict[str, Any]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=INTEGRATED_FIELDS)
        writer.writeheader()
        for record in records:
            row: dict[str, Any] = {}
            for field in INTEGRATED_FIELDS:
                value = record.get(field)
                if isinstance(value, (list, dict)):
                    row[field] = json.dumps(value, ensure_ascii=False, separators=(",", ":"))
                elif value is None:
                    row[field] = ""
                else:
                    row[field] = value
            writer.writerow(row)


def route_key(record: dict[str, Any]) -> tuple[str | None, str | None]:
    return record.get("shelterId"), record.get("plateauBuildingId")


def group_routes(routes: list[dict[str, Any]]) -> dict[tuple[str | None, str | None], list[dict[str, Any]]]:
    grouped: dict[tuple[str | None, str | None], list[dict[str, Any]]] = {}
    for route in routes:
        grouped.setdefault(route_key(route), []).append(route)
    return grouped


def integrate_record(record: dict[str, Any], routes: list[dict[str, Any]]) -> dict[str, Any]:
    available_routes = [
        route
        for route in routes
        if route.get("routeAvailability") == "available"
        and isinstance(route.get("routeDistanceMeters"), (int, float))
    ]
    failed_routes = [route for route in routes if route.get("routeAvailability") == "failed"]
    integrated = dict(record)
    warnings = list(record.get("warnings", []))

    if available_routes:
        nearest = min(available_routes, key=lambda route: float(route["routeDistanceMeters"]))
        route_warnings = []
        for route in available_routes:
            route_warnings.extend(route.get("warnings", []))
        integrated.update(
            {
                "routeAvailability": "available",
                "nearestRouteDistanceMeters": nearest["routeDistanceMeters"],
                "estimatedTravelTimeSeconds": nearest["estimatedTravelTimeSeconds"],
                "routeSource": "OSM",
                "routeType": "estimated_pedestrian_route",
                "isOfficialEvacuationRoute": False,
                "nearestRouteId": nearest["routeId"],
                "nearestRouteOriginId": nearest["originId"],
                "nearestRouteOriginName": nearest["originName"],
                "availableRouteCount": len(available_routes),
                "failedRouteCount": len(failed_routes),
            }
        )
        warnings.extend(route_warnings)
    elif failed_routes:
        integrated.update(
            {
                "routeAvailability": "failed",
                "nearestRouteDistanceMeters": None,
                "estimatedTravelTimeSeconds": None,
                "routeSource": "OSM",
                "routeType": "estimated_pedestrian_route",
                "isOfficialEvacuationRoute": False,
                "nearestRouteId": None,
                "nearestRouteOriginId": None,
                "nearestRouteOriginName": None,
                "availableRouteCount": 0,
                "failedRouteCount": len(failed_routes),
            }
        )
        warnings.append("all B5 OSM route attempts failed for this shelter/building")
    else:
        integrated.update(
            {
                "routeAvailability": "not_evaluated",
                "nearestRouteDistanceMeters": None,
                "estimatedTravelTimeSeconds": None,
                "routeSource": None,
                "routeType": None,
                "isOfficialEvacuationRoute": False,
                "nearestRouteId": None,
                "nearestRouteOriginId": None,
                "nearestRouteOriginName": None,
                "availableRouteCount": 0,
                "failedRouteCount": 0,
            }
        )
        if record.get("qualificationStatus") == "unknown" or not record.get("plateauBuildingId"):
            warnings.append("B5 routing skipped because B4 did not provide a qualified building target")

    warnings.append("OSM route fields are prototype estimates and are not official evacuation routes")
    integrated["warnings"] = list(dict.fromkeys(warnings))
    integrated["notes"] = (
        f"{record.get('notes', '')} B5 merged OSM route estimates where available; "
        "no disaster road closure, flood simulation, or Unity gameplay integration is applied."
    ).strip()
    return integrated


def feature_collection(name: str, features: list[dict[str, Any]]) -> dict[str, Any]:
    return {
        "type": "FeatureCollection",
        "name": name,
        "crs": {
            "type": "name",
            "properties": {
                "name": "EPSG:4326",
            },
        },
        "features": features,
    }


def build_route_origin_features(routes: list[dict[str, Any]]) -> list[dict[str, Any]]:
    seen: dict[str, dict[str, Any]] = {}
    for route in routes:
        geometry = route.get("geometry")
        if not geometry or not geometry.get("coordinates"):
            continue
        first = geometry["coordinates"][0]
        seen[route["originId"]] = {
            "type": "Feature",
            "geometry": {
                "type": "Point",
                "coordinates": first,
            },
            "properties": {
                "originId": route["originId"],
                "originName": route["originName"],
                "routeSource": route["routeSource"],
                "routeType": route["routeType"],
                "isOfficialEvacuationRoute": route["isOfficialEvacuationRoute"],
            },
        }
    return list(seen.values())


def build_route_line_features(routes: list[dict[str, Any]]) -> list[dict[str, Any]]:
    features: list[dict[str, Any]] = []
    for route in routes:
        geometry = route.get("geometry")
        if route.get("routeAvailability") != "available" or geometry is None:
            continue
        features.append(
            {
                "type": "Feature",
                "geometry": geometry,
                "properties": {
                    "routeId": route["routeId"],
                    "originId": route["originId"],
                    "originName": route["originName"],
                    "shelterId": route["shelterId"],
                    "shelterName": route["shelterName"],
                    "plateauBuildingId": route["plateauBuildingId"],
                    "routeDistanceMeters": route["routeDistanceMeters"],
                    "estimatedTravelTimeSeconds": route["estimatedTravelTimeSeconds"],
                    "routeSource": route["routeSource"],
                    "routeType": route["routeType"],
                    "isOfficialEvacuationRoute": route["isOfficialEvacuationRoute"],
                },
            }
        )
    return features


def build_failure_features(routes: list[dict[str, Any]]) -> list[dict[str, Any]]:
    features: list[dict[str, Any]] = []
    for route in routes:
        if route.get("routeAvailability") != "failed":
            continue
        features.append(
            {
                "type": "Feature",
                "geometry": None,
                "properties": {
                    "routeId": route["routeId"],
                    "originId": route["originId"],
                    "originName": route["originName"],
                    "shelterId": route["shelterId"],
                    "shelterName": route["shelterName"],
                    "plateauBuildingId": route["plateauBuildingId"],
                    "routeFailureReason": route.get("routeFailureReason"),
                    "routeSource": route["routeSource"],
                    "routeType": route["routeType"],
                    "isOfficialEvacuationRoute": route["isOfficialEvacuationRoute"],
                },
            }
        )
    return features


def build_integrated_outputs() -> dict[str, Any]:
    b4_dataset = load_json(B4_QUALIFICATION_JSON)
    route_dataset = load_json(ROUTES_JSON)
    grouped = group_routes(route_dataset["records"])

    records = [
        integrate_record(record, grouped.get(route_key(record), []))
        for record in b4_dataset["records"]
    ]

    dataset = {
        "datasetId": "p5_b5_real_chuo_integrated_route_qualification",
        "generatedAt": now_jst(),
        "coordinateReferenceSystem": b4_dataset["coordinateReferenceSystem"],
        "rulebookVersion": b4_dataset["rulebookVersion"],
        "qualificationInputPath": str(B4_QUALIFICATION_JSON.relative_to(REPO_ROOT)).replace("\\", "/"),
        "routeInputPath": str(ROUTES_JSON.relative_to(REPO_ROOT)).replace("\\", "/"),
        "osmAttribution": route_dataset.get("osmAttribution"),
        "notes": (
            "Integrated P5-B5 output preserves B4 evidence/qualification fields and adds OSM prototype walking-route estimates. "
            "Routes are not official evacuation routes."
        ),
        "records": records,
    }

    write_json(OUTPUT_JSON, dataset)
    write_csv(OUTPUT_CSV, records)
    write_json(
        OUTPUT_ROUTE_LINES,
        feature_collection("real_chuo_route_lines", build_route_line_features(route_dataset["records"])),
        compact=True,
    )
    write_json(
        OUTPUT_ROUTE_ORIGINS,
        feature_collection("real_chuo_route_origins", build_route_origin_features(route_dataset["records"])),
        compact=True,
    )
    write_json(
        OUTPUT_ROUTE_FAILURES,
        feature_collection("real_chuo_route_failures", build_failure_features(route_dataset["records"])),
        compact=True,
    )

    return {
        "recordCount": len(records),
        "routeAvailabilityCounts": dict(Counter(record["routeAvailability"] for record in records)),
    }


def main() -> int:
    summary = build_integrated_outputs()
    print(f"[integrate] wrote {OUTPUT_JSON}")
    print(f"[integrate] wrote {OUTPUT_CSV}")
    print(f"[integrate] wrote {OUTPUT_ROUTE_LINES}")
    print(f"[integrate] wrote {OUTPUT_ROUTE_ORIGINS}")
    print(f"[integrate] wrote {OUTPUT_ROUTE_FAILURES}")
    print(f"[integrate] records: {summary['recordCount']}")
    print(f"[integrate] route availability: {summary['routeAvailabilityCounts']}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
