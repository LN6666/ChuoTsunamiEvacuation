from __future__ import annotations

import csv
import json
import math
from datetime import datetime, timedelta, timezone
from pathlib import Path
from typing import Any

import networkx as nx
import osmnx as ox
from pyproj import Transformer


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
QUALIFICATION_ROOT = PIPELINE_ROOT / "qualification"
PROCESSED_QUALIFICATION_ROOT = PIPELINE_ROOT / "processed" / "qualification"
PROCESSED_ROUTES_ROOT = PIPELINE_ROOT / "processed" / "routes"

ORIGINS_PATH = QUALIFICATION_ROOT / "real_chuo_route_origin_points.json"
QUALIFICATION_PATH = PROCESSED_QUALIFICATION_ROOT / "real_chuo_building_qualification.json"
SHELTERS_PATH = PROCESSED_QUALIFICATION_ROOT / "real_chuo_official_shelters_normalized.json"

OUTPUT_JSON = PROCESSED_ROUTES_ROOT / "real_chuo_osm_routes_sample.json"
OUTPUT_CSV = PROCESSED_ROUTES_ROOT / "real_chuo_osm_routes_sample.csv"
OUTPUT_GEOJSON = PROCESSED_ROUTES_ROOT / "real_chuo_osm_routes_sample.geojson"

OSMNX_CACHE_ROOT = PIPELINE_ROOT / "cache" / "osmnx"
WALKING_SPEED_METERS_PER_SECOND = 1.2
BOUNDING_BOX_BUFFER_DEGREES = 0.015
OSM_NETWORK_TYPE = "walk"
METRIC_CRS = "EPSG:6677"

ROUTE_FIELDS = [
    "routeId",
    "originId",
    "originName",
    "shelterId",
    "shelterName",
    "plateauBuildingId",
    "qualificationStatus",
    "routeAvailability",
    "routeDistanceMeters",
    "estimatedTravelTimeSeconds",
    "walkingSpeedMetersPerSecond",
    "routeSource",
    "routeType",
    "isOfficialEvacuationRoute",
    "geometry",
    "snapDistanceOriginMeters",
    "snapDistanceTargetMeters",
    "routeFailureReason",
    "warnings",
    "notes",
]

TARGET_STATUSES = {
    "official_confirmed",
    "official_confirmed_with_review",
    "strong_candidate",
}


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
        writer = csv.DictWriter(handle, fieldnames=ROUTE_FIELDS)
        writer.writeheader()
        for record in records:
            row: dict[str, Any] = {}
            for field in ROUTE_FIELDS:
                value = record.get(field)
                if isinstance(value, (list, dict)):
                    row[field] = json.dumps(value, ensure_ascii=False, separators=(",", ":"))
                elif value is None:
                    row[field] = ""
                else:
                    row[field] = value
            writer.writerow(row)


def configure_osmnx() -> None:
    OSMNX_CACHE_ROOT.mkdir(parents=True, exist_ok=True)
    ox.settings.use_cache = True
    ox.settings.cache_folder = str(OSMNX_CACHE_ROOT)
    ox.settings.log_console = False
    ox.settings.requests_timeout = 180
    ox.settings.overpass_rate_limit = True


def build_targets() -> list[dict[str, Any]]:
    qualification = load_json(QUALIFICATION_PATH)
    shelters = load_json(SHELTERS_PATH)
    shelters_by_id = {record["shelterId"]: record for record in shelters["records"]}

    targets: list[dict[str, Any]] = []
    for record in qualification["records"]:
        shelter_id = record.get("shelterId")
        shelter = shelters_by_id.get(shelter_id)
        if shelter is None:
            continue
        if record.get("qualificationStatus") not in TARGET_STATUSES:
            continue
        if not record.get("plateauBuildingId"):
            continue
        lat = shelter.get("latitude")
        lon = shelter.get("longitude")
        if not isinstance(lat, (int, float)) or not isinstance(lon, (int, float)):
            continue
        targets.append(
            {
                "shelterId": shelter_id,
                "shelterName": record.get("shelterName") or shelter.get("shelterName"),
                "plateauBuildingId": record.get("plateauBuildingId"),
                "qualificationStatus": record.get("qualificationStatus"),
                "latitude": float(lat),
                "longitude": float(lon),
                "matchMethod": record.get("matchMethod"),
                "confidence": record.get("confidence"),
            }
        )
    return targets


def bounding_box(origins: list[dict[str, Any]], targets: list[dict[str, Any]]) -> tuple[float, float, float, float]:
    longitudes = [float(item["longitude"]) for item in origins + targets]
    latitudes = [float(item["latitude"]) for item in origins + targets]
    west = min(longitudes) - BOUNDING_BOX_BUFFER_DEGREES
    south = min(latitudes) - BOUNDING_BOX_BUFFER_DEGREES
    east = max(longitudes) + BOUNDING_BOX_BUFFER_DEGREES
    north = max(latitudes) + BOUNDING_BOX_BUFFER_DEGREES
    return west, south, east, north


def edge_geometry_coordinates(graph: nx.MultiDiGraph, route_nodes: list[int]) -> list[list[float]]:
    coordinates: list[list[float]] = []
    for u, v in zip(route_nodes, route_nodes[1:]):
        edge_data = graph.get_edge_data(u, v)
        if not edge_data:
            continue
        edge = min(edge_data.values(), key=lambda data: float(data.get("length", math.inf)))
        geometry = edge.get("geometry")
        if geometry is not None:
            edge_coords = [[float(x), float(y)] for x, y in geometry.coords]
        else:
            edge_coords = [
                [float(graph.nodes[u]["x"]), float(graph.nodes[u]["y"])],
                [float(graph.nodes[v]["x"]), float(graph.nodes[v]["y"])],
            ]

        if coordinates and edge_coords:
            prev = coordinates[-1]
            first_distance = math.hypot(edge_coords[0][0] - prev[0], edge_coords[0][1] - prev[1])
            last_distance = math.hypot(edge_coords[-1][0] - prev[0], edge_coords[-1][1] - prev[1])
            if last_distance < first_distance:
                edge_coords = list(reversed(edge_coords))
            coordinates.extend(edge_coords[1:])
        else:
            coordinates.extend(edge_coords)
    return coordinates


def route_length_meters(graph: nx.MultiDiGraph, route_nodes: list[int]) -> float:
    total = 0.0
    for u, v in zip(route_nodes, route_nodes[1:]):
        edge_data = graph.get_edge_data(u, v)
        if not edge_data:
            continue
        edge = min(edge_data.values(), key=lambda data: float(data.get("length", math.inf)))
        total += float(edge.get("length", 0.0))
    return total


def nearest_node_projected(
    graph_projected: nx.MultiDiGraph,
    transformer: Transformer,
    longitude: float,
    latitude: float,
) -> tuple[int, float | None]:
    x, y = transformer.transform(longitude, latitude)
    try:
        node, distance = ox.distance.nearest_nodes(graph_projected, x, y, return_dist=True)
        return int(node), float(distance) if distance is not None else None
    except ImportError:
        # Avoid adding optional nearest-neighbor dependencies in B5. The Chuo
        # sample graph is small enough for a deterministic linear fallback.
        best_node: int | None = None
        best_distance = math.inf
        for node_id, data in graph_projected.nodes(data=True):
            distance = math.hypot(float(data["x"]) - x, float(data["y"]) - y)
            if distance < best_distance:
                best_node = int(node_id)
                best_distance = distance
        if best_node is None:
            raise RuntimeError("Projected OSM graph has no nodes available for snapping.")
        return best_node, best_distance


def failure_record(
    route_id: str,
    origin: dict[str, Any],
    target: dict[str, Any],
    reason: str,
    snap_origin: float | None = None,
    snap_target: float | None = None,
) -> dict[str, Any]:
    return {
        "routeId": route_id,
        "originId": origin["originId"],
        "originName": origin["originName"],
        "shelterId": target["shelterId"],
        "shelterName": target["shelterName"],
        "plateauBuildingId": target["plateauBuildingId"],
        "qualificationStatus": target["qualificationStatus"],
        "routeAvailability": "failed",
        "routeDistanceMeters": None,
        "estimatedTravelTimeSeconds": None,
        "walkingSpeedMetersPerSecond": WALKING_SPEED_METERS_PER_SECOND,
        "routeSource": "OSM",
        "routeType": "estimated_pedestrian_route",
        "isOfficialEvacuationRoute": False,
        "geometry": None,
        "snapDistanceOriginMeters": snap_origin,
        "snapDistanceTargetMeters": snap_target,
        "routeFailureReason": reason,
        "warnings": [
            "OSM route calculation failed for this origin-target pair",
            "route output is a prototype estimate and is not an official evacuation route",
        ],
        "notes": "Failure is retained for QGIS QA and later routing review.",
    }


def build_routes() -> dict[str, Any]:
    configure_osmnx()
    origins_payload = load_json(ORIGINS_PATH)
    origins = origins_payload["origins"]
    targets = build_targets()
    if not targets:
        raise RuntimeError("No qualified B4 shelter/building targets were available for routing.")

    bbox = bounding_box(origins, targets)
    graph = ox.graph_from_bbox(
        bbox,
        network_type=OSM_NETWORK_TYPE,
        simplify=True,
        retain_all=True,
        truncate_by_edge=True,
    )
    if graph.number_of_nodes() == 0:
        raise RuntimeError("OSMnx returned an empty walking network for the B5 bounding box.")
    graph_projected = ox.project_graph(graph, to_crs=METRIC_CRS)
    to_metric = Transformer.from_crs("EPSG:4326", METRIC_CRS, always_xy=True)

    records: list[dict[str, Any]] = []
    features: list[dict[str, Any]] = []

    for origin_index, origin in enumerate(origins, start=1):
        origin_node, origin_snap = nearest_node_projected(
            graph_projected,
            to_metric,
            float(origin["longitude"]),
            float(origin["latitude"]),
        )
        for target_index, target in enumerate(targets, start=1):
            route_id = f"osm_route_{origin_index:02d}_{target_index:03d}"
            target_node, target_snap = nearest_node_projected(
                graph_projected,
                to_metric,
                target["longitude"],
                target["latitude"],
            )
            try:
                route_nodes = nx.shortest_path(graph, origin_node, target_node, weight="length")
            except (nx.NetworkXNoPath, nx.NodeNotFound) as exc:
                records.append(
                    failure_record(route_id, origin, target, str(exc), origin_snap, target_snap)
                )
                continue

            coords = edge_geometry_coordinates(graph, route_nodes)
            if len(coords) < 2:
                records.append(
                    failure_record(
                        route_id,
                        origin,
                        target,
                        "route geometry has fewer than two coordinates",
                        origin_snap,
                        target_snap,
                    )
                )
                continue

            route_distance = round(route_length_meters(graph, route_nodes), 3)
            travel_time = round(route_distance / WALKING_SPEED_METERS_PER_SECOND, 3)
            geometry = {
                "type": "LineString",
                "coordinates": coords,
            }
            warnings = [
                "OSM route is an estimated prototype pedestrian route, not an official evacuation route"
            ]
            if target["qualificationStatus"] == "official_confirmed_with_review":
                warnings.append("target building qualification requires manual review")
            if target.get("matchMethod") == "nearest":
                warnings.append("target uses nearest PLATEAU building match; review in QGIS")

            record = {
                "routeId": route_id,
                "originId": origin["originId"],
                "originName": origin["originName"],
                "shelterId": target["shelterId"],
                "shelterName": target["shelterName"],
                "plateauBuildingId": target["plateauBuildingId"],
                "qualificationStatus": target["qualificationStatus"],
                "routeAvailability": "available",
                "routeDistanceMeters": route_distance,
                "estimatedTravelTimeSeconds": travel_time,
                "walkingSpeedMetersPerSecond": WALKING_SPEED_METERS_PER_SECOND,
                "routeSource": "OSM",
                "routeType": "estimated_pedestrian_route",
                "isOfficialEvacuationRoute": False,
                "geometry": geometry,
                "snapDistanceOriginMeters": round(origin_snap, 3) if origin_snap is not None else None,
                "snapDistanceTargetMeters": round(target_snap, 3) if target_snap is not None else None,
                "routeFailureReason": None,
                "warnings": warnings,
                "notes": (
                    "Computed with OSMnx/NetworkX shortest path by OSM edge length. "
                    "No flood, road closure, or official evacuation route logic is applied."
                ),
            }
            records.append(record)
            features.append(
                {
                    "type": "Feature",
                    "geometry": geometry,
                    "properties": {
                        key: value
                        for key, value in record.items()
                        if key not in {"geometry", "warnings", "notes"}
                    }
                    | {
                        "warnings": "; ".join(warnings),
                    },
                }
            )

    generated_at = now_jst()
    dataset = {
        "datasetId": "p5_b5_real_chuo_osm_routes_sample",
        "generatedAt": generated_at,
        "coordinateReferenceSystem": "EPSG:4326",
        "metricCoordinateReferenceSystem": METRIC_CRS,
        "originInputPath": str(ORIGINS_PATH.relative_to(REPO_ROOT)).replace("\\", "/"),
        "qualificationInputPath": str(QUALIFICATION_PATH.relative_to(REPO_ROOT)).replace("\\", "/"),
        "shelterInputPath": str(SHELTERS_PATH.relative_to(REPO_ROOT)).replace("\\", "/"),
        "osmNetworkType": OSM_NETWORK_TYPE,
        "osmAttribution": "Route network data from OpenStreetMap contributors; use under the Open Database License.",
        "cachePolicy": "OSMnx cache/download files are local ignored working files and are not committed or required at runtime.",
        "boundingBox": {
            "west": bbox[0],
            "south": bbox[1],
            "east": bbox[2],
            "north": bbox[3],
        },
        "walkingSpeedMetersPerSecond": WALKING_SPEED_METERS_PER_SECOND,
        "originCount": len(origins),
        "targetCount": len(targets),
        "recordCount": len(records),
        "notes": (
            "P5-B5 route outputs are prototype OSM walking estimates only. "
            "They are not official evacuation routes and do not affect Unity gameplay."
        ),
        "records": records,
    }

    geojson = {
        "type": "FeatureCollection",
        "name": "real_chuo_osm_routes_sample",
        "crs": {
            "type": "name",
            "properties": {
                "name": "EPSG:4326",
            },
        },
        "features": features,
    }

    write_json(OUTPUT_JSON, dataset, compact=True)
    write_csv(OUTPUT_CSV, records)
    write_json(OUTPUT_GEOJSON, geojson, compact=True)
    return dataset


def main() -> int:
    dataset = build_routes()
    available = sum(1 for record in dataset["records"] if record["routeAvailability"] == "available")
    failed = sum(1 for record in dataset["records"] if record["routeAvailability"] == "failed")
    print(f"[routes] wrote {OUTPUT_JSON}")
    print(f"[routes] wrote {OUTPUT_CSV}")
    print(f"[routes] wrote {OUTPUT_GEOJSON}")
    print(f"[routes] origins: {dataset['originCount']}")
    print(f"[routes] targets: {dataset['targetCount']}")
    print(f"[routes] available: {available}")
    print(f"[routes] failed: {failed}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
