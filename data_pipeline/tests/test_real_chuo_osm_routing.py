from __future__ import annotations

import json
from collections import Counter
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[2]
ORIGINS = REPO_ROOT / "data_pipeline" / "qualification" / "real_chuo_route_origin_points.json"
ROUTE_SCRIPT = REPO_ROOT / "data_pipeline" / "scripts" / "build_real_chuo_osm_routes.py"
INTEGRATED_SCRIPT = REPO_ROOT / "data_pipeline" / "scripts" / "build_real_chuo_integrated_route_qualification.py"
ROUTES_JSON = REPO_ROOT / "data_pipeline" / "processed" / "routes" / "real_chuo_osm_routes_sample.json"
ROUTES_CSV = REPO_ROOT / "data_pipeline" / "processed" / "routes" / "real_chuo_osm_routes_sample.csv"
ROUTES_GEOJSON = REPO_ROOT / "data_pipeline" / "processed" / "routes" / "real_chuo_osm_routes_sample.geojson"
INTEGRATED_JSON = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_integrated_route_qualification.json"
INTEGRATED_CSV = REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_integrated_route_qualification.csv"
QGIS_LAYERS = [
    REPO_ROOT / "data_pipeline" / "processed" / "qgis_qa" / "real_chuo_route_lines.geojson",
    REPO_ROOT / "data_pipeline" / "processed" / "qgis_qa" / "real_chuo_route_origins.geojson",
    REPO_ROOT / "data_pipeline" / "processed" / "qgis_qa" / "real_chuo_route_failures.geojson",
]


def load_json(path: Path) -> dict:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def test_origin_points_json_valid() -> None:
    payload = load_json(ORIGINS)
    assert payload["datasetId"] == "p5_b5_real_chuo_route_origin_points"
    assert payload["coordinateReferenceSystem"] == "EPSG:4326"
    assert len(payload["origins"]) >= 5
    for origin in payload["origins"]:
        assert origin["sourceType"] == "controlled_route_test_origin"
        assert isinstance(origin["latitude"], (int, float))
        assert isinstance(origin["longitude"], (int, float))
        assert origin["coordinateReferenceSystem"] == "EPSG:4326"


def test_scripts_and_outputs_exist() -> None:
    assert ROUTE_SCRIPT.exists()
    assert INTEGRATED_SCRIPT.exists()
    for path in [ROUTES_JSON, ROUTES_CSV, ROUTES_GEOJSON, INTEGRATED_JSON, INTEGRATED_CSV, *QGIS_LAYERS]:
        assert path.exists(), path


def test_route_records_are_osm_estimates_not_official_routes() -> None:
    payload = load_json(ROUTES_JSON)
    assert payload["osmNetworkType"] == "walk"
    assert payload["osmAttribution"]
    assert payload["originCount"] >= 5
    assert payload["targetCount"] > 0
    assert payload["records"]

    available = [record for record in payload["records"] if record["routeAvailability"] == "available"]
    failed = [record for record in payload["records"] if record["routeAvailability"] == "failed"]
    assert available

    for record in payload["records"]:
        assert record["routeSource"] == "OSM"
        assert record["routeType"] == "estimated_pedestrian_route"
        assert record["isOfficialEvacuationRoute"] is False
        if record["routeAvailability"] == "available":
            assert record["routeDistanceMeters"] > 0
            assert record["estimatedTravelTimeSeconds"] > 0
            assert record["geometry"]["type"] == "LineString"
            assert len(record["geometry"]["coordinates"]) >= 2
        if record["routeAvailability"] == "failed":
            assert record["routeFailureReason"]

    assert len(failed) >= 0


def test_integrated_output_preserves_qualification_and_adds_route_info() -> None:
    payload = load_json(INTEGRATED_JSON)
    assert payload["datasetId"] == "p5_b5_real_chuo_integrated_route_qualification"
    assert payload["records"]
    availability = Counter(record["routeAvailability"] for record in payload["records"])
    assert availability["available"] > 0
    assert availability["not_evaluated"] < len(payload["records"])

    for record in payload["records"]:
        assert record["qualificationStatus"]
        assert record["confidence"] in {"high", "medium", "low", "unknown"}
        assert record["isOfficialEvacuationRoute"] is False
        if record["routeAvailability"] == "available":
            assert record["routeSource"] == "OSM"
            assert record["routeType"] == "estimated_pedestrian_route"
            assert record["nearestRouteDistanceMeters"] > 0
            assert record["estimatedTravelTimeSeconds"] > 0
            assert record["nearestRouteId"]


def test_qgis_route_layers_exist_and_are_geojson() -> None:
    for path in QGIS_LAYERS:
        payload = load_json(path)
        assert payload["type"] == "FeatureCollection"
        assert payload["crs"]["properties"]["name"] == "EPSG:4326"

    route_lines = load_json(QGIS_LAYERS[0])
    route_origins = load_json(QGIS_LAYERS[1])
    assert route_lines["features"]
    assert route_origins["features"]


def test_outputs_do_not_reference_raw_cache_downloads_as_runtime_dependencies() -> None:
    for path in [ROUTES_JSON, ROUTES_GEOJSON, INTEGRATED_JSON, *QGIS_LAYERS]:
        text = path.read_text(encoding="utf-8")
        assert "data_pipeline/raw" not in text
        assert "data_pipeline/cache" not in text
        assert "data_pipeline/downloads" not in text
