#!/usr/bin/env python3
"""Extract P8-B Chuo tsunami hazard layer from official Tokyo/MLIT sources.

The extractor uses only Python stdlib modules. It downloads official CSV/GeoJSON
resources in memory, clips Tokyo ward-area 10m tsunami mesh points to the MLIT
N03 Chuo City administrative polygon, and writes a project JSON layer.
"""

from __future__ import annotations

import argparse
import csv
import io
import json
import sys
import urllib.request
import zipfile
from pathlib import Path


TOKYO_OPEN_DATASET_URL = "https://catalog.data.metro.tokyo.lg.jp/dataset/t000003d2000000392"
TOKYO_REPORT_URL = "https://www.bousai.metro.tokyo.lg.jp/taisaku/torikumi/1000902/1021571.html"
CHUO_REFERENCE_URL = "https://www.city.chuo.lg.jp/a0034/bousaianzen/bousai/saigaitaiou/saigaijoho/tunamiekijouka.html"
MLIT_N03_URL = "https://nlftp.mlit.go.jp/ksj/gml/data/N03/N03-2024/N03-20240101_13_GML.zip"
SUPPLEMENTARY_PDF_URL = "https://www.ktr.mlit.go.jp/ktr_content/content/000083997.pdf"
FLOOD_PROXY_URL = "https://www.city.chuo.lg.jp/bousaianzen/bousai/kouzuihazardmap/index.html"


SCENARIOS = [
    {
        "featureId": "p8b_chuo_taisho_kanto_tokyo_open_data_spatial",
        "scenarioName": "Taisho Kanto earthquake, Tokyo 10m tsunami mesh clipped to Chuo City",
        "depthUrl": "https://www.opendata.metro.tokyo.lg.jp/soumu/2_kubu_taisyoukanntoujisinn_sinnsuisinn.csv",
        "heightUrl": "https://www.opendata.metro.tokyo.lg.jp/soumu/1_kubu_taisyoukanntoujisinn_tunamidaka.csv",
        "arrivalUrl": "https://www.opendata.metro.tokyo.lg.jp/soumu/0_kubu_taisyoukanntoujisinn_toutatujikann.csv",
        "collapseRandomSeed": 8301,
    },
    {
        "featureId": "p8b_chuo_nankai_case1_tokyo_open_data_spatial",
        "scenarioName": "Nankai Trough megathrust earthquake case 1, Tokyo 10m tsunami mesh clipped to Chuo City",
        "depthUrl": "https://www.opendata.metro.tokyo.lg.jp/soumu/2_kubu_nannkaitorafucase1_sinnsuisinn.csv",
        "heightUrl": "https://www.opendata.metro.tokyo.lg.jp/soumu/1_kubu_nannkaitorafucase1_tunamidaka.csv",
        "arrivalUrl": "https://www.opendata.metro.tokyo.lg.jp/soumu/0_kubu_nannkaitorafucase1_toutatujikann.csv",
        "collapseRandomSeed": 8302,
    },
]


def download(url: str) -> bytes:
    request = urllib.request.Request(url, headers={"User-Agent": "P8B-Chuo-Tsunami-Extractor/1.0"})
    with urllib.request.urlopen(request, timeout=120) as response:
        return response.read()


def polygons_from_geometry(geometry: dict) -> list:
    if geometry.get("type") == "Polygon":
        return [geometry["coordinates"]]
    if geometry.get("type") == "MultiPolygon":
        return geometry["coordinates"]
    return []


def point_in_ring(lon: float, lat: float, ring: list) -> bool:
    inside = False
    j = len(ring) - 1
    for i, point in enumerate(ring):
        xi, yi = point[0], point[1]
        xj, yj = ring[j][0], ring[j][1]
        if (yi > lat) != (yj > lat):
            denominator = yj - yi
            if abs(denominator) < 1e-30:
                denominator = 1e-30
            intersect_x = ((xj - xi) * (lat - yi) / denominator) + xi
            if lon < intersect_x:
                inside = not inside
        j = i
    return inside


def point_in_polygon(lon: float, lat: float, polygon: list) -> bool:
    if not polygon or not point_in_ring(lon, lat, polygon[0]):
        return False
    return not any(point_in_ring(lon, lat, hole) for hole in polygon[1:])


def load_chuo_polygons() -> tuple[list, list[float]]:
    archive = download(MLIT_N03_URL)
    with zipfile.ZipFile(io.BytesIO(archive)) as zip_file:
        geojson_name = next(name for name in zip_file.namelist() if name.endswith(".geojson"))
        geojson = json.loads(zip_file.read(geojson_name).decode("utf-8"))

    polygons = []
    for feature in geojson.get("features", []):
        properties = feature.get("properties", {})
        if properties.get("N03_007") == "13102":
            polygons.extend(polygons_from_geometry(feature.get("geometry", {})))

    if not polygons:
        raise RuntimeError("Could not find Chuo City polygon in MLIT N03 Tokyo GeoJSON.")

    all_points = [point for polygon in polygons for ring in polygon for point in ring]
    bounds = [
        min(point[0] for point in all_points),
        min(point[1] for point in all_points),
        max(point[0] for point in all_points),
        max(point[1] for point in all_points),
    ]
    return polygons, bounds


def is_inside_chuo(lon: float, lat: float, polygons: list, bounds: list[float]) -> bool:
    if lon < bounds[0] or lon > bounds[2] or lat < bounds[1] or lat > bounds[3]:
        return False
    return any(point_in_polygon(lon, lat, polygon) for polygon in polygons)


def load_clipped_csv(url: str, value_count: int, polygons: list, bounds: list[float]) -> dict:
    text = download(url).decode("cp932")
    reader = csv.reader(io.StringIO(text))
    header = next(reader)
    rows = {}
    for row in reader:
        if len(row) < 4 + value_count:
            continue
        lon = float(row[2])
        lat = float(row[3])
        if not is_inside_chuo(lon, lat, polygons, bounds):
            continue
        key = f"{row[0]},{row[1]}"
        rows[key] = {
            "xMeters": float(row[0]),
            "yMeters": float(row[1]),
            "longitude": lon,
            "latitude": lat,
            "values": [float(row[4 + index]) for index in range(value_count)],
        }
    return rows


def positive(values: list[float]) -> list[float]:
    return [value for value in values if value >= 0.0]


def safe_min(values: list[float], fallback: float = 0.0) -> float:
    filtered = positive(values)
    return min(filtered) if filtered else fallback


def safe_max(values: list[float], fallback: float = 0.0) -> float:
    filtered = positive(values)
    return max(filtered) if filtered else fallback


def round_float(value: float, digits: int = 4) -> float:
    return round(float(value), digits)


def make_boundary(samples: list[dict]) -> list[dict]:
    min_lon = min(sample["longitude"] for sample in samples)
    max_lon = max(sample["longitude"] for sample in samples)
    min_lat = min(sample["latitude"] for sample in samples)
    max_lat = max(sample["latitude"] for sample in samples)
    return [
        {"x": round_float(min_lon, 8), "y": round_float(min_lat, 8)},
        {"x": round_float(max_lon, 8), "y": round_float(min_lat, 8)},
        {"x": round_float(max_lon, 8), "y": round_float(max_lat, 8)},
        {"x": round_float(min_lon, 8), "y": round_float(max_lat, 8)},
    ]


def build_feature(scenario: dict, polygons: list, bounds: list[float]) -> dict:
    depth_rows = load_clipped_csv(scenario["depthUrl"], 1, polygons, bounds)
    height_rows = load_clipped_csv(scenario["heightUrl"], 1, polygons, bounds)
    arrival_rows = load_clipped_csv(scenario["arrivalUrl"], 4, polygons, bounds)
    keys = sorted(set(depth_rows) & set(height_rows) & set(arrival_rows))
    if not keys:
        raise RuntimeError("No joined clipped rows for " + scenario["featureId"])

    samples = []
    for key in keys:
        depth = depth_rows[key]
        height = height_rows[key]
        arrival = arrival_rows[key]
        samples.append(
            {
                "xMeters": round_float(depth["xMeters"], 3),
                "yMeters": round_float(depth["yMeters"], 3),
                "longitude": round_float(depth["longitude"], 8),
                "latitude": round_float(depth["latitude"], 8),
                "inundationDepthMeters": round_float(depth["values"][0], 4),
                "tsunamiHeightMeters": round_float(height["values"][0], 4),
                "arrivalTime1cmSeconds": round_float(arrival["values"][0], 1),
                "arrivalTime30cmSeconds": round_float(arrival["values"][1], 1),
                "arrivalTime1mSeconds": round_float(arrival["values"][2], 1),
                "arrivalTimeMaxWaterLevelSeconds": round_float(arrival["values"][3], 1),
            }
        )

    depths = [sample["inundationDepthMeters"] for sample in samples]
    heights = [sample["tsunamiHeightMeters"] for sample in samples]
    arrival_30cm = [sample["arrivalTime30cmSeconds"] for sample in samples]
    arrival_1cm = [sample["arrivalTime1cmSeconds"] for sample in samples]
    arrival_max = [sample["arrivalTimeMaxWaterLevelSeconds"] for sample in samples]
    max_depth = max(depths)
    max_height = max(heights)
    earliest_30cm = safe_min(arrival_30cm, safe_min(arrival_1cm, 0.0))
    hazard_intensity = min(1.0, max_depth / 2.5)

    return {
        "featureId": scenario["featureId"],
        "scenarioName": scenario["scenarioName"],
        "sourceMode": "official_tsunami_metropolitan",
        "sourceCategory": "official_tsunami_metropolitan",
        "geometryType": "grid",
        "extractionStatus": "extracted",
        "extractionMethod": "Tokyo Open Data ward-area 10m tsunami mesh clipped to MLIT N03 Chuo City polygon.",
        "arrivalTimeSeconds": round_float(earliest_30cm, 1),
        "arrivalTimeStatus": "earliest_valid_30cm_seconds_from_clipped_grid",
        "inundationDepthMeters": round_float(max_depth, 4),
        "maxInundationDepthMeters": round_float(max_depth, 4),
        "averageInundationDepthMeters": round_float(sum(depths) / len(depths), 4),
        "waterLevelMeters": 0.0,
        "waterLevelStatus": "not_separate_in_source",
        "tsunamiHeightMeters": round_float(max_height, 4),
        "maxTsunamiHeightMeters": round_float(max_height, 4),
        "inundationDepthStatus": "extracted_spatial_max_depth_from_tokyo_10m_mesh",
        "boundaryStatus": "extracted_grid_extent_bbox_not_official_inundation_contour",
        "spatialExtractionStatus": "extracted",
        "spatialSampleCount": len(samples),
        "minArrivalTime1cmSeconds": round_float(safe_min(arrival_1cm), 1),
        "minArrivalTime30cmSeconds": round_float(safe_min(arrival_30cm), 1),
        "maxArrivalTimeMaxWaterLevelSeconds": round_float(safe_max(arrival_max), 1),
        "inundationBoundary": make_boundary(samples),
        "spatialSamples": samples,
        "hazardIntensity": round_float(hazard_intensity, 4),
        "confidence": 0.9,
        "evidenceSourceId": "tokyo_damage_estimation_map_tsunami",
        "notes": "Official Tokyo 10m mesh tsunami data clipped to official MLIT N03 Chuo City polygon. inundationDepthMeters is an extracted spatial maximum from clipped grid points. maxTsunamiHeightMeters is a tsunami-height field and is not used as the inundation-depth grid. inundationBoundary is a derived grid extent bbox, not an official inundation contour.",
        "visualHeightMeters": 120.0,
        "visualHeightIsCinematicOnly": True,
        "boundaryIsEvidenceBasedOrPrototype": "evidence_based",
        "affectedInfrastructureTypes": {
            "roads": True,
            "buildings": True,
            "bridges": True,
            "underground": True,
            "entrances": True,
            "waterfront": True,
            "open_space": True,
        },
        "buildingDamageState": "none",
        "collapseProxyState": "data_only",
        "collapseProbability": 0.0,
        "collapseRandomSeed": scenario["collapseRandomSeed"],
        "hazardDrivenCollapse": False,
    }


def build_layer() -> dict:
    polygons, bounds = load_chuo_polygons()
    features = [build_feature(scenario, polygons, bounds) for scenario in SCENARIOS]
    return {
        "scenarioId": "p8b_chuo_tokyo_tsunami_official_spatial_v1",
        "sourceMode": "official_tsunami_metropolitan",
        "sourceCategory": "official_tsunami_metropolitan",
        "hazardLayerVersion": "p8b.1.2",
        "evidenceRegistryFile": "tsunami_hazard_evidence_registry.json",
        "p8cGateDecision": "PASS",
        "spatialExtractionStatus": "extracted",
        "extractionStatus": "extracted",
        "extractionMethod": "Official Tokyo Open Data tsunami 10m mesh CSV resources clipped to official MLIT N03 Chuo City polygon.",
        "officialMetropolitanEvidenceIdentified": True,
        "completeOfficialSpatialLayerExtracted": True,
        "timeOriginSeconds": 0,
        "scienceLayerFields": [
            "arrivalTimeSeconds",
            "inundationDepthMeters",
            "waterLevelMeters",
            "tsunamiHeightMeters",
            "inundationBoundary",
            "hazardIntensity",
            "confidence",
            "evidenceSourceId",
            "geometryType",
            "sourceMode",
            "boundaryIsEvidenceBasedOrPrototype",
        ],
        "visualLayerFields": [
            "visualHeightMeters",
            "visualHeightIsCinematicOnly",
        ],
        "features": features,
        "evidenceSources": [
            {
                "evidenceSourceId": "tokyo_damage_estimation_map_tsunami",
                "sourceMode": "official_tsunami_metropolitan",
                "sourceCategory": "official_tsunami_metropolitan",
                "title": "Tokyo Open Data tsunami distribution 10m mesh CSV resources",
                "url": TOKYO_OPEN_DATASET_URL,
                "reviewedStatus": "reviewed_for_values",
                "accessMethod": "direct_https_csv_download_no_token",
                "requiresTokenOrLogin": False,
                "expectedFileType": "CSV",
                "notes": "Primary official metropolitan tsunami spatial source. Used for maximum inundation depth, maximum tsunami height, and arrival-time grid values."
            },
            {
                "evidenceSourceId": "mlit_n03_chuo_admin_boundary",
                "sourceMode": "official_tsunami_metropolitan",
                "sourceCategory": "official_admin_boundary",
                "title": "MLIT National Land Numerical Information N03 Tokyo administrative boundary, 2024",
                "url": MLIT_N03_URL,
                "reviewedStatus": "reviewed_for_values",
                "accessMethod": "direct_https_zip_download_no_token",
                "requiresTokenOrLogin": False,
                "expectedFileType": "ZIP with GeoJSON/Shapefile",
                "notes": "Used only to clip Tokyo ward-area tsunami mesh points to Chuo City polygon."
            },
            {
                "evidenceSourceId": "tokyo_damage_estimation_report_tsunami",
                "sourceMode": "official_tsunami_metropolitan",
                "sourceCategory": "official_tsunami_report_reference",
                "title": "Tokyo Metropolitan Government 2022 damage estimation report tsunami sections",
                "url": TOKYO_REPORT_URL,
                "reviewedStatus": "reviewed_for_planning",
                "accessMethod": "public_web_page_no_token",
                "requiresTokenOrLogin": False,
                "expectedFileType": "HTML/PDF references",
                "notes": "Report provenance for Tokyo tsunami simulation context. The spatial grid comes from Tokyo Open Data CSV resources."
            },
            {
                "evidenceSourceId": "chuo_city_tsunami_liquefaction_page",
                "sourceMode": "evidence_planned",
                "sourceCategory": "official_tsunami_report_reference",
                "title": "Chuo City page on tsunami and liquefaction",
                "url": CHUO_REFERENCE_URL,
                "reviewedStatus": "reviewed_for_planning",
                "accessMethod": "public_web_page_no_token",
                "requiresTokenOrLogin": False,
                "expectedFileType": "HTML",
                "notes": "Chuo City references Tokyo Metropolitan Government tsunami numerical simulation results."
            },
            {
                "evidenceSourceId": "supplementary_pdf_tokyo_bay_tsunami_height_chuo",
                "sourceMode": "evidence_planned",
                "sourceCategory": "manual_extraction_required",
                "title": "Supplementary public PDF reference for Tokyo Bay tsunami height by municipality",
                "url": SUPPLEMENTARY_PDF_URL,
                "reviewedStatus": "attached_unreviewed",
                "accessMethod": "public_pdf_no_token",
                "requiresTokenOrLogin": False,
                "expectedFileType": "PDF",
                "notes": "Supplementary maximum-height reference only. Not used for the extracted spatial inundation-depth grid."
            },
            {
                "evidenceSourceId": "flood_proxy_chuo_hazard_map",
                "sourceMode": "evidence_planned",
                "sourceCategory": "official_flood_proxy",
                "title": "Chuo flood hazard map materials, non-tsunami proxy fallback only",
                "url": FLOOD_PROXY_URL,
                "reviewedStatus": "not_attached",
                "accessMethod": "public_web_page_no_token",
                "requiresTokenOrLogin": False,
                "expectedFileType": "HTML/PDF",
                "notes": "Non-tsunami flood proxy. Not used by P8-B now that official Tokyo tsunami spatial CSV data is extracted."
            },
        ],
        "notes": "P8-B official spatial hazard layer v1. Tokyo Open Data tsunami 10m mesh CSV resources were clipped to the official MLIT N03 Chuo City polygon. Maximum inundation depth is spatially extracted. Maximum tsunami height remains a separate height field. The derived boundary is a grid extent bbox, not an official inundation contour. No P8-C infrastructure behavior is implemented by this layer."
    }


def main() -> int:
    parser = argparse.ArgumentParser(description="Extract P8-B Chuo tsunami layer from official Tokyo/MLIT sources.")
    parser.add_argument(
        "--output",
        default="Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
        help="Output JSON path relative to repo root.",
    )
    args = parser.parse_args()

    repo_root = Path(__file__).resolve().parents[2]
    output_path = (repo_root / args.output).resolve()
    if repo_root not in output_path.parents:
        raise RuntimeError("Output path must stay inside the repository.")

    layer = build_layer()
    output_path.parent.mkdir(parents=True, exist_ok=True)
    output_path.write_text(json.dumps(layer, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")

    print("P8-B Chuo tsunami spatial layer extracted.")
    print("Output:", output_path)
    for feature in layer["features"]:
        print(
            feature["featureId"],
            "samples=", feature["spatialSampleCount"],
            "maxDepth=", feature["maxInundationDepthMeters"],
            "maxTsunamiHeight=", feature["maxTsunamiHeightMeters"],
        )
    return 0


if __name__ == "__main__":
    sys.exit(main())
