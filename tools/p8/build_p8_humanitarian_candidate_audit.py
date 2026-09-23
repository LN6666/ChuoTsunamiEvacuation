import json
import math
import re
import xml.etree.ElementTree as ET
from datetime import datetime, timedelta, timezone
from pathlib import Path


REPO_ROOT = Path(__file__).resolve().parents[2]
RAW_PLATEAU_BLDG_DIR = Path(r"D:\PLATEAU_DATA\Chuo_2025_CityGML\udx\bldg")

P8_DATA_PATH = REPO_ROOT / "Assets" / "Data" / "P8" / "humanitarian_highrise_candidate_audit_v1.json"
HAZARD_LAYER_PATH = REPO_ROOT / "Assets" / "Data" / "P8" / "tsunami_hazard_layer_v1_chuo.json"
NAME_LIST_PATH = REPO_ROOT / "docs" / "P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md"
AUDIT_DOC_PATH = REPO_ROOT / "docs" / "P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_EXPANSION_AUDIT.md"
P8C_HANDOFF_PATH = REPO_ROOT / "docs" / "P8C_HUMANITARIAN_CANDIDATE_HANDOFF.md"
P8D_SCOPE_PATH = REPO_ROOT / "docs" / "P8D_HUMANITARIAN_CANDIDATE_DAMAGE_STATUS_SCOPE.md"
P8E_SCOPE_PATH = REPO_ROOT / "docs" / "P8E_HUMANITARIAN_CANDIDATE_PERSISTENT_VISIBILITY_SCOPE.md"

SAMPLE_PATHS = [
    REPO_ROOT / "Assets" / "Data" / "p5g_highrise_humanitarian_candidates_sample.json",
    REPO_ROOT / "data_pipeline" / "qualification" / "highrise_humanitarian_candidates_sample.json",
]

OFFICIAL_QUALIFICATION_PATHS = [
    REPO_ROOT / "Assets" / "Data" / "real_chuo_building_qualification.json",
    REPO_ROOT / "data_pipeline" / "processed" / "qualification" / "real_chuo_building_qualification.json",
]

SEARCH_PATHS = [
    "Assets/Data",
    "docs",
    "data_pipeline",
    "Assets/P7Benchmark",
    "Assets/P7HighDetail (not present as directory in this worktree)",
    "Assets/P7Benchmark/Imported/53393690",
    "data_pipeline/processed",
    "processed (root path, not present)",
    str(RAW_PLATEAU_BLDG_DIR),
]

SEARCH_TERMS = [
    "humanitarian high-rise candidates",
    "high-rise candidates",
    "highrise",
    "tower",
    "office building",
    "skyscraper",
    "vertical evacuation",
    "life-first",
    "candidateLayer",
    "humanitarian_candidate",
    "publicAccessStatus",
    "manualReviewNeeded",
    "nonOfficial",
    "PLATEAU measuredHeight",
    "PLATEAU storeysAboveGround",
]

USAGE_LABELS = {
    "401": "business_or_office_facility",
    "402": "commercial_facility",
    "403": "accommodation_or_hotel_facility",
    "404": "commercial_mixed_use_facility",
    "411": "residential",
    "412": "apartment_or_collective_housing",
    "413": "store_residential_mixed_use",
    "414": "store_apartment_mixed_use",
    "421": "government_public_facility",
    "422": "education_or_welfare_facility",
    "431": "transport_or_warehouse_facility",
    "441": "factory",
    "452": "supply_or_utility_facility",
    "454": "other",
    "461": "unknown_use",
}

OFFICE_COMMERCIAL_CODES = {"401", "402", "403", "404", "413", "414"}
TOP_PLATEAU_HEIGHT_FLOOR_LIMIT = 80
ADDITIONAL_PLATEAU_REVIEW_LIMIT = 25


def local_name(tag):
    return tag.rsplit("}", 1)[-1]


def read_json(path):
    return json.loads(path.read_text(encoding="utf-8"))


def repo_display_path(path):
    path = Path(path)
    try:
        return str(path.relative_to(REPO_ROOT)).replace("\\", "/")
    except ValueError:
        return str(path).replace("\\", "/")


def clean_optional_text(value):
    if value is None:
        return None
    text = str(value).strip()
    if not text or text.lower() in {"n/a", "null", "none", "unknown"}:
        return None
    return text


def parse_float(text):
    try:
        value = float(str(text).strip())
    except (TypeError, ValueError):
        return None

    if value <= 0 or value >= 1000 or math.isclose(value, 9999.0):
        return None
    return value


def parse_int(text):
    try:
        value = int(float(str(text).strip()))
    except (TypeError, ValueError):
        return None

    if value <= 0 or value >= 200 or value == 9999:
        return None
    return value


def centroid_from_pos_list(text):
    if not text:
        return None, None

    values = []
    for raw in str(text).split()[:1200]:
        try:
            values.append(float(raw))
        except ValueError:
            continue

    points = []
    for index in range(0, len(values) - 2, 3):
        first = values[index]
        second = values[index + 1]
        if 20 <= first <= 50 and 120 <= second <= 160:
            points.append((first, second))
        elif 20 <= second <= 50 and 120 <= first <= 160:
            points.append((second, first))

    if not points:
        return None, None

    latitude = sum(point[0] for point in points) / len(points)
    longitude = sum(point[1] for point in points) / len(points)
    return round(latitude, 8), round(longitude, 8)


def usage_label(code):
    if not code:
        return None
    return f"{code}:{USAGE_LABELS.get(code, 'unmapped_building_usage_code')}"


def load_known_official_plateau_ids():
    ids = set()
    files_used = []
    for path in OFFICIAL_QUALIFICATION_PATHS:
        if not path.exists():
            continue
        data = read_json(path)
        files_used.append(repo_display_path(path))
        for record in data.get("records", []):
            status = str(record.get("officialDesignationStatus", ""))
            building_id = clean_optional_text(record.get("plateauBuildingId"))
            if building_id and status.startswith("official"):
                ids.add(building_id)
    return ids, files_used


def extract_string_attribute_value(element):
    attribute_name = ""
    for key, value in element.attrib.items():
        if key.endswith("name"):
            attribute_name = str(value)
            break

    if not attribute_name:
        return None

    lower = attribute_name.lower()
    name_tokens = ("name", "buildingname", "building_name", "名称", "建物名", "建物名称", "施設名")
    if not any(token in lower or token in attribute_name for token in name_tokens):
        return None

    for child in element:
        if local_name(child.tag) == "value" and child.text:
            return child.text.strip()
    return None


def load_hazard_bounds():
    if not HAZARD_LAYER_PATH.exists():
        return []

    data = read_json(HAZARD_LAYER_PATH)
    bounds = []
    for feature in data.get("features", []):
        points = feature.get("inundationBoundary") or []
        xs = [parse_float(point.get("x")) for point in points if isinstance(point, dict)]
        ys = [parse_float(point.get("y")) for point in points if isinstance(point, dict)]
        xs = [value for value in xs if value is not None]
        ys = [value for value in ys if value is not None]
        if xs and ys:
            bounds.append(
                {
                    "featureId": feature.get("featureId"),
                    "minLongitude": min(xs),
                    "maxLongitude": max(xs),
                    "minLatitude": min(ys),
                    "maxLatitude": max(ys),
                }
            )
    return bounds


def hazard_context_for_point(latitude, longitude, hazard_bounds):
    if latitude is None or longitude is None:
        return {
            "p8HazardContextStatus": "unknown_no_coordinates",
            "withinP8ExtractedBoundaryBbox": False,
            "nearestHazardBoundaryFeatureId": None,
        }

    for bounds in hazard_bounds:
        if (
            bounds["minLatitude"] <= latitude <= bounds["maxLatitude"]
            and bounds["minLongitude"] <= longitude <= bounds["maxLongitude"]
        ):
            return {
                "p8HazardContextStatus": "inside_derived_p8_tsunami_boundary_bbox",
                "withinP8ExtractedBoundaryBbox": True,
                "nearestHazardBoundaryFeatureId": bounds["featureId"],
            }

    return {
        "p8HazardContextStatus": "outside_derived_p8_tsunami_boundary_bbox_or_not_screened",
        "withinP8ExtractedBoundaryBbox": False,
        "nearestHazardBoundaryFeatureId": None,
    }


def is_plateau_screening_candidate(record):
    height = record.get("heightMeters") or 0
    floors = record.get("floorCount") or 0
    usage_code = record.get("usageCode")
    return (
        height >= 45
        or floors >= 10
        or (usage_code in OFFICE_COMMERCIAL_CODES and (height >= 31 or floors >= 8))
    )


def extract_plateau_records(known_official_ids, hazard_bounds):
    records = []
    raw_files = []
    if not RAW_PLATEAU_BLDG_DIR.exists():
        return records, raw_files

    for gml_path in sorted(RAW_PLATEAU_BLDG_DIR.glob("*_bldg_*.gml")):
        raw_files.append(repo_display_path(gml_path))
        try:
            context = ET.iterparse(gml_path, events=("end",))
            for _, element in context:
                if local_name(element.tag) != "Building":
                    continue

                record = {
                    "sourceFile": repo_display_path(gml_path),
                    "sourceMeshCode": gml_path.name.split("_", 1)[0],
                    "plateauGmlId": element.attrib.get("{http://www.opengis.net/gml}id"),
                    "fallbackBuildingId": None,
                    "buildingName": None,
                    "heightMeters": None,
                    "floorCount": None,
                    "usageCode": None,
                    "latitude": None,
                    "longitude": None,
                }
                first_pos_list = None

                for child in element.iter():
                    name = local_name(child.tag)
                    if name == "buildingID" and child.text and record["fallbackBuildingId"] is None:
                        record["fallbackBuildingId"] = child.text.strip()
                    elif name == "measuredHeight" and child.text and record["heightMeters"] is None:
                        record["heightMeters"] = parse_float(child.text)
                    elif name == "storeysAboveGround" and child.text and record["floorCount"] is None:
                        record["floorCount"] = parse_int(child.text)
                    elif name == "usage" and child.text and record["usageCode"] is None:
                        record["usageCode"] = child.text.strip()
                    elif name == "name" and child.text and record["buildingName"] is None:
                        text = child.text.strip()
                        if text and not re.fullmatch(r"\d+", text):
                            record["buildingName"] = text
                    elif name == "stringAttribute" and record["buildingName"] is None:
                        record["buildingName"] = extract_string_attribute_value(child)
                    elif name == "posList" and child.text and first_pos_list is None:
                        first_pos_list = child.text

                building_id = record["fallbackBuildingId"]
                if not building_id or not building_id.startswith("13102-"):
                    element.clear()
                    continue
                if building_id in known_official_ids:
                    element.clear()
                    continue

                latitude, longitude = centroid_from_pos_list(first_pos_list)
                record["latitude"] = latitude
                record["longitude"] = longitude
                record["useType"] = usage_label(record["usageCode"])
                record.update(hazard_context_for_point(latitude, longitude, hazard_bounds))

                if is_plateau_screening_candidate(record):
                    records.append(record)

                element.clear()
        except ET.ParseError as exc:
            print(f"[warn] failed to parse {gml_path}: {exc}")

    return records, raw_files


def score_plateau_candidate(record):
    score = 0
    height = record.get("heightMeters") or 0
    floors = record.get("floorCount") or 0
    if height:
        score += min(45, height * 0.35)
    if floors:
        score += min(30, floors * 1.5)
    if record.get("usageCode") in OFFICE_COMMERCIAL_CODES:
        score += 10
    if record.get("withinP8ExtractedBoundaryBbox"):
        score += 10
    if record.get("latitude") is not None and record.get("longitude") is not None:
        score += 4
    if record.get("buildingName"):
        score += 6
    return round(min(score, 99), 1)


def plateau_sort_key(record):
    has_name = 1 if record.get("buildingName") else 0
    hazard = 1 if record.get("withinP8ExtractedBoundaryBbox") else 0
    height = record.get("heightMeters") or 0
    floors = record.get("floorCount") or 0
    office = 1 if record.get("usageCode") in OFFICE_COMMERCIAL_CODES else 0
    return has_name, score_plateau_candidate(record), hazard, height, floors, office


def classify_plateau(record, force_additional_review=False):
    categories = []
    height = record.get("heightMeters") or 0
    floors = record.get("floorCount") or 0
    usage_code = record.get("usageCode")

    if record.get("buildingName") and re.search(r"tower|twr|high.?rise|skyscraper", record["buildingName"], re.IGNORECASE):
        categories.append("named_tower_candidate")
    elif record.get("buildingName"):
        categories.append("named_office_candidate")
    if height >= 45:
        categories.append("plateau_height_candidate")
    if floors >= 10:
        categories.append("plateau_floor_candidate")
    if usage_code in OFFICE_COMMERCIAL_CODES:
        categories.append("office_or_commercial_candidate")
    if record.get("withinP8ExtractedBoundaryBbox"):
        categories.append("highrise_near_hazard_candidate")
    if force_additional_review:
        categories.append("manual_review_required")
    if not categories:
        categories.append("insufficient_evidence")
    if "manual_review_required" not in categories:
        categories.append("manual_review_required")
    return categories


def build_plateau_candidate(record, index, force_additional_review=False):
    categories = classify_plateau(record, force_additional_review)
    score = score_plateau_candidate(record)
    height = record.get("heightMeters")
    floors = record.get("floorCount")
    details = []
    if height is not None:
        details.append(f"PLATEAU measuredHeight={height}m")
    if floors is not None:
        details.append(f"storeysAboveGround={floors}")
    if record.get("useType"):
        details.append(f"useType={record['useType']}")
    if record.get("withinP8ExtractedBoundaryBbox"):
        details.append("centroid inside derived P8 tsunami boundary bbox")
    if not details:
        details.append("PLATEAU geometry/attribute record requires manual review")

    return {
        "candidateId": f"p8_plateau_highrise_candidate_{index:03d}",
        "candidateLayer": "humanitarian_candidate",
        "candidateCategories": categories,
        "buildingName": clean_optional_text(record.get("buildingName")),
        "fallbackBuildingId": record.get("fallbackBuildingId"),
        "plateauGmlId": record.get("plateauGmlId"),
        "address": None,
        "locationText": "PLATEAU centroid from local CityGML geometry" if record.get("latitude") and record.get("longitude") else None,
        "latitude": record.get("latitude"),
        "longitude": record.get("longitude"),
        "heightMeters": height,
        "floorCount": floors,
        "useType": record.get("useType"),
        "usageCode": record.get("usageCode"),
        "sourceFile": record.get("sourceFile"),
        "evidenceSourceFile": record.get("sourceFile"),
        "evidenceSourceType": "local_plateau_citygml_building_attribute",
        "candidateReason": "; ".join(categories),
        "candidateReasonDetails": "; ".join(details) + "; candidate is non-official and review-only.",
        "candidateScore": score,
        "confidence": "medium" if score >= 55 else "low",
        "publicAccessStatus": "unknown",
        "manualReviewNeeded": True,
        "isOfficialShelter": False,
        "officialDesignationStatus": "not_official_candidate_layer",
        "nonOfficialWarningRequired": True,
        "hazardStatusEligible": True,
        "p8cProxyEligible": True,
        "p8dDamageStatusEligible": True,
        "p8ePersistentVisibilityReviewNeeded": True,
        "p9LifeFirstSelectableReviewNeeded": True,
        "affectsGameplaySuccessFailure": False,
        "implementsP8D": False,
        "implementsP9SelectableGameplay": False,
        "evidenceStatus": "plateau_geometry_attribute_only",
        "p8HazardContextStatus": record.get("p8HazardContextStatus"),
        "withinP8ExtractedBoundaryBbox": record.get("withinP8ExtractedBoundaryBbox", False),
        "nearestHazardBoundaryFeatureId": record.get("nearestHazardBoundaryFeatureId"),
        "confidenceSortGroup": "1_named_highrise_tower_candidate" if record.get("buildingName") else ("2_plateau_height_floor_candidate" if not force_additional_review else "4_unknown_name_plateau_manual_review"),
        "notes": "Not an official evacuation shelter. Not marked safe or approved. Public access, management agreement, seismic evidence, entrance, and safe-floor proxy require manual review before any future gameplay use.",
    }


def load_sample_candidates(start_index):
    records = []
    files_used = []
    source_path = next((path for path in SAMPLE_PATHS if path.exists()), None)
    if not source_path:
        return records, files_used

    data = read_json(source_path)
    files_used.append(repo_display_path(source_path))
    index = start_index
    for record in data.get("records", []):
        if record.get("candidateLayer") != "humanitarian_candidate":
            continue

        notes = ["Existing P5-F/P5-GH controlled sample; not a real expanded Chuo building record."]
        if record.get("humanitarianCandidateStatus") in {"unknown", "not_recommended"}:
            notes.append("Included for continuity and audit visibility only; insufficient or negative sample evidence.")

        categories = ["existing_sample_candidate", "manual_review_required"]
        if record.get("humanitarianCandidateStatus") in {"unknown", "not_recommended"}:
            categories.append("insufficient_evidence")

        records.append(
            {
                "candidateId": record.get("candidateId") or f"p8_existing_sample_candidate_{index:03d}",
                "candidateLayer": "humanitarian_candidate",
                "candidateCategories": categories,
                "buildingName": clean_optional_text(record.get("buildingName")),
                "fallbackBuildingId": record.get("plateauBuildingId") or record.get("buildingId"),
                "plateauGmlId": None,
                "address": clean_optional_text(record.get("address")),
                "locationText": clean_optional_text(record.get("address")),
                "latitude": record.get("latitude"),
                "longitude": record.get("longitude"),
                "heightMeters": record.get("heightMeters"),
                "floorCount": record.get("floorsAboveGround"),
                "useType": record.get("usageType") or record.get("buildingUse"),
                "usageCode": None,
                "sourceFile": repo_display_path(source_path),
                "evidenceSourceFile": repo_display_path(source_path),
                "evidenceSourceType": "existing_p5_sample_fixture",
                "candidateReason": "; ".join(categories),
                "candidateReasonDetails": record.get("classificationReason") or "Existing controlled sample humanitarian candidate.",
                "candidateScore": record.get("physicalSuitabilityScore") or 0,
                "confidence": record.get("confidence") or "low",
                "publicAccessStatus": record.get("publicAccessStatus", "unknown"),
                "manualReviewNeeded": True,
                "isOfficialShelter": False,
                "officialDesignationStatus": "not_official_candidate_layer",
                "nonOfficialWarningRequired": True,
                "hazardStatusEligible": True,
                "p8cProxyEligible": True,
                "p8dDamageStatusEligible": True,
                "p8ePersistentVisibilityReviewNeeded": True,
                "p9LifeFirstSelectableReviewNeeded": True,
                "affectsGameplaySuccessFailure": False,
                "implementsP8D": False,
                "implementsP9SelectableGameplay": False,
                "evidenceStatus": "existing_controlled_sample_only",
                "p8HazardContextStatus": "sample_estimated_context_not_real_plateau",
                "withinP8ExtractedBoundaryBbox": None,
                "nearestHazardBoundaryFeatureId": None,
                "confidenceSortGroup": "3_existing_p5_sample_candidate",
                "notes": " ".join(notes),
            }
        )
        index += 1
    return records, files_used


def select_plateau_candidates(records):
    named = [record for record in records if record.get("buildingName")]
    unnamed = [record for record in records if not record.get("buildingName")]

    primary = [
        record
        for record in unnamed
        if (record.get("heightMeters") or 0) >= 45 or (record.get("floorCount") or 0) >= 10
    ]
    primary.sort(key=plateau_sort_key, reverse=True)

    selected = []
    selected_ids = set()
    for record in sorted(named, key=plateau_sort_key, reverse=True):
        if len(selected) >= TOP_PLATEAU_HEIGHT_FLOOR_LIMIT:
            break
        selected.append((record, False))
        selected_ids.add(record["fallbackBuildingId"])

    for record in primary:
        if len([item for item in selected if not item[1]]) >= TOP_PLATEAU_HEIGHT_FLOOR_LIMIT:
            break
        selected.append((record, False))
        selected_ids.add(record["fallbackBuildingId"])

    additional = [
        record
        for record in unnamed
        if record["fallbackBuildingId"] not in selected_ids
        and record.get("usageCode") in OFFICE_COMMERCIAL_CODES
        and ((record.get("heightMeters") or 0) >= 31 or (record.get("floorCount") or 0) >= 8)
    ]
    additional.sort(key=plateau_sort_key, reverse=True)
    for record in additional[:ADDITIONAL_PLATEAU_REVIEW_LIMIT]:
        selected.append((record, True))
        selected_ids.add(record["fallbackBuildingId"])

    return selected


def sort_records(records):
    order = {
        "1_named_highrise_tower_candidate": 1,
        "2_plateau_height_floor_candidate": 2,
        "3_existing_p5_sample_candidate": 3,
        "4_unknown_name_plateau_manual_review": 4,
    }
    return sorted(
        records,
        key=lambda record: (
            order.get(record.get("confidenceSortGroup"), 99),
            -(record.get("candidateScore") or 0),
            -(record.get("heightMeters") or 0),
            -(record.get("floorCount") or 0),
            record.get("candidateId", ""),
        ),
    )


def markdown_value(value):
    if value is None:
        return "unknown"
    if isinstance(value, float):
        return f"{value:.6f}".rstrip("0").rstrip(".")
    if isinstance(value, list):
        return ", ".join(str(item) for item in value)
    return str(value).replace("|", "/").replace("\n", " ")


def table_named(records):
    lines = [
        "| index | candidateId | buildingName | address/locationText | candidateReason | heightMeters | floorCount | sourceFile | publicAccessStatus | manualReviewNeeded | notes |",
        "|---:|---|---|---|---|---:|---:|---|---|---|---|",
    ]
    if not records:
        lines.append("| 0 | n/a | No named real local candidates found | n/a | n/a | n/a | n/a | n/a | n/a | n/a | Local PLATEAU attributes did not expose building names; no names were fabricated. |")
        return "\n".join(lines)

    for index, record in enumerate(records, start=1):
        address = record.get("address") or record.get("locationText")
        lines.append(
            "| "
            + " | ".join(
                [
                    str(index),
                    f"`{markdown_value(record.get('candidateId'))}`",
                    markdown_value(record.get("buildingName")),
                    markdown_value(address),
                    markdown_value(record.get("candidateReason")),
                    markdown_value(record.get("heightMeters")),
                    markdown_value(record.get("floorCount")),
                    f"`{markdown_value(record.get('sourceFile'))}`",
                    markdown_value(record.get("publicAccessStatus")),
                    str(record.get("manualReviewNeeded")).lower(),
                    markdown_value(record.get("notes")),
                ]
            )
            + " |"
        )
    return "\n".join(lines)


def table_sample(records):
    lines = [
        "| index | candidateId | buildingName | address/locationText | candidateReason | heightMeters | floorCount | sourceFile | publicAccessStatus | manualReviewNeeded | notes |",
        "|---:|---|---|---|---|---:|---:|---|---|---|---|",
    ]
    for index, record in enumerate(records, start=1):
        address = record.get("address") or record.get("locationText")
        lines.append(
            "| "
            + " | ".join(
                [
                    str(index),
                    f"`{markdown_value(record.get('candidateId'))}`",
                    markdown_value(record.get("buildingName")),
                    markdown_value(address),
                    markdown_value(record.get("candidateReason")),
                    markdown_value(record.get("heightMeters")),
                    markdown_value(record.get("floorCount")),
                    f"`{markdown_value(record.get('sourceFile'))}`",
                    markdown_value(record.get("publicAccessStatus")),
                    str(record.get("manualReviewNeeded")).lower(),
                    markdown_value(record.get("notes")),
                ]
            )
            + " |"
        )
    return "\n".join(lines)


def table_unknown_plateau(records):
    lines = [
        "| index | candidateId | fallbackBuildingId | approximate location | heightMeters | floorCount | sourceFile | candidateReason | manualReviewNeeded | notes |",
        "|---:|---|---|---|---:|---:|---|---|---|---|",
    ]
    for index, record in enumerate(records, start=1):
        location = "unknown"
        if record.get("latitude") is not None and record.get("longitude") is not None:
            location = f"{record['latitude']} / {record['longitude']}"
        lines.append(
            "| "
            + " | ".join(
                [
                    str(index),
                    f"`{markdown_value(record.get('candidateId'))}`",
                    f"`{markdown_value(record.get('fallbackBuildingId'))}`",
                    markdown_value(location),
                    markdown_value(record.get("heightMeters")),
                    markdown_value(record.get("floorCount")),
                    f"`{markdown_value(record.get('sourceFile'))}`",
                    markdown_value(record.get("candidateReason")),
                    str(record.get("manualReviewNeeded")).lower(),
                    markdown_value(record.get("notes")),
                ]
            )
            + " |"
        )
    return "\n".join(lines)


def table_rejected(dataset):
    screened = dataset["screeningSummary"]["rawPlateauScreeningCandidateCount"]
    included = dataset["screeningSummary"]["plateauIncludedCount"]
    lines = [
        "| source id | reason rejected |",
        "|---|---|",
        f"| known official shelter-matched PLATEAU IDs | {dataset['screeningSummary']['knownOfficialPlateauBuildingIdsExcluded']} IDs excluded from the non-official humanitarian layer. |",
        f"| local PLATEAU screened records outside top selection | {screened - included} screened records were not selected to avoid converting all buildings into candidates. |",
        "| root processed path | No root `processed` directory was found; project processed outputs are under `data_pipeline/processed`. |",
    ]
    return "\n".join(lines)


def write_docs(dataset):
    records = dataset["records"]
    named_real = [r for r in records if r.get("buildingName") and r.get("confidenceSortGroup") != "3_existing_p5_sample_candidate"]
    plateau_records = [r for r in records if r.get("evidenceStatus") == "plateau_geometry_attribute_only"]
    sample_records = [r for r in records if r.get("confidenceSortGroup") == "3_existing_p5_sample_candidate"]
    unknown_plateau = [r for r in plateau_records if not r.get("buildingName")]
    hazard_relevant = [r for r in plateau_records if r.get("withinP8ExtractedBoundaryBbox") is True]
    named_total = len(named_real) + len([r for r in sample_records if r.get("buildingName")])

    name_list = f"""# P8-C Humanitarian High-Rise Candidate Name List

Date: 2026-05-24.

## Warning

These are not official evacuation shelters.

They are non-official humanitarian / life-first high-rise candidates.

They must not be displayed as official shelters.

They require manual review before persistent visibility or selectable gameplay.

Every audit v1 candidate has `isOfficialShelter=false`, `nonOfficialWarningRequired=true`, and `manualReviewNeeded=true`.

## Summary

- Total candidates found: {len(records)}.
- Total named candidates: {named_total}.
- Total unknown-name / ID-only candidates: {len(unknown_plateau)}.
- Total existing sample candidates: {len(sample_records)}.
- Total PLATEAU-derived candidates: {len(plateau_records)}.
- PLATEAU-derived candidates inside a derived P8 tsunami boundary bbox: {len(hazard_relevant)}.
- Raw PLATEAU records matching height/floor/office-commercial screening before top-N selection: {dataset['screeningSummary']['rawPlateauScreeningCandidateCount']}.
- Known official shelter-matched PLATEAU building IDs excluded: {dataset['screeningSummary']['knownOfficialPlateauBuildingIdsExcluded']}.
- Candidate list expanded beyond original 5 sample candidates: yes.

## Sorting

1. Named real high-rise/tower candidates, if any local source exposes names.
2. PLATEAU height/floor candidates with geometry attributes.
3. Existing P5 sample candidates for continuity.
4. Additional unknown-name PLATEAU office/commercial candidates requiring review.

## Table A: Named High-Rise / Office / Tower Candidates

{table_named(named_real)}

## Table B: Existing P5 Sample Candidates

{table_sample(sample_records)}

## Table C: PLATEAU ID-Only Candidates Requiring Manual Name Review

{table_unknown_plateau(unknown_plateau)}

## Table D: Rejected / Insufficient-Evidence Items

{table_rejected(dataset)}

## Source Files Searched

{chr(10).join('- `' + path + '`' for path in dataset['filesSearched'])}

Search terms:

{chr(10).join('- `' + term + '`' for term in dataset['searchTerms'])}

## Source Files Used

{chr(10).join('- `' + path + '`' for path in dataset['filesUsed'])}

## Source Files Missing / No Candidate Data Found

{chr(10).join('- ' + item for item in dataset['missingOrLimitedData'])}

## Interpretation Boundary

PLATEAU `measuredHeight`, `storeysAboveGround`, usage code, and centroid evidence are geometry/attribute screening signals only. They do not prove public access, management agreement, seismic safety, entrance usability, indoor routes, safe floors, or official shelter designation.

Future persistent visibility requires explicit non-official labeling. P9 may review these as life-first vertical evacuation candidates, but they must still use entrance / safe-floor / evacuation-complete proxy behavior rather than real indoor scenes.
"""

    audit_doc = f"""# P8-C Humanitarian High-Rise Candidate Expansion Audit

Date: 2026-05-24.

## Result

The P8 humanitarian high-rise candidate audit is no longer sample-only.

Audit v1 combines:

- existing P5-F/P5-GH non-official sample candidates;
- local PLATEAU CityGML building attributes for Chuo (`13102-*`) using measured height, above-ground storeys, usage code, and centroid geometry;
- P8-B official tsunami layer bbox context for hazard-status eligibility only;
- a known-official exclusion set from P5 real Chuo building qualification outputs.

Output data:

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`

## Screening Counts

- Raw local PLATEAU building records matching screening criteria before top-N selection: {dataset['screeningSummary']['rawPlateauScreeningCandidateCount']}.
- PLATEAU records included in audit v1: {len(plateau_records)}.
- Existing sample records retained: {len(sample_records)}.
- Total audit records: {len(records)}.
- Real source-name records found in PLATEAU: {len(named_real)}.
- Unknown-name PLATEAU records retained by building ID: {len(unknown_plateau)}.
- PLATEAU records inside a derived P8 tsunami boundary bbox: {len(hazard_relevant)}.

## Selection Rules

PLATEAU candidates were selected only when local evidence had at least one of:

- `measuredHeight >= 45m`;
- `storeysAboveGround >= 10`;
- office/commercial/hotel/mixed-use code with `measuredHeight >= 31m` or `storeysAboveGround >= 8`.

Known official shelter-matched PLATEAU building IDs from P5 qualification outputs were excluded from the non-official candidate layer.

## Candidate Categories

- `existing_sample_candidate`: existing P5-F/P5-GH controlled sample retained for continuity.
- `plateau_height_candidate`: local PLATEAU measured-height screen.
- `plateau_floor_candidate`: local PLATEAU above-ground-storey screen.
- `office_or_commercial_candidate`: local PLATEAU usage code suggests office/commercial/hotel/mixed-use context.
- `highrise_near_hazard_candidate`: centroid is inside a derived P8 tsunami boundary bbox.
- `named_tower_candidate` / `named_office_candidate`: reserved for source-named records; no real local PLATEAU source names were found in this run.
- `manual_review_required`: every audit v1 record requires manual review before persistent gameplay use.
- `insufficient_evidence`: sample/low-confidence records retained only for audit visibility.

## Non-Official Boundary

Every record has:

- `isOfficialShelter=false`;
- `nonOfficialWarningRequired=true`;
- `manualReviewNeeded=true`;
- `hazardStatusEligible=true`;
- `p8cProxyEligible=true`;
- `affectsGameplaySuccessFailure=false`;
- `implementsP8D=false`;
- `implementsP9SelectableGameplay=false`;
- no safe/approved status.

P8-D may use these records only as data-only building warning / entrance blocked / low-floor inundation warning / damage-proxy inputs. P8-E must verify persistent explicit non-official visibility and P9 handoff. P9 decides whether any become life-first selectable vertical evacuation targets using entrance/safe-floor/evacuation-complete proxies, never real indoor scenes.

## Limitations

- No web search was used.
- No candidate names were fabricated.
- Real PLATEAU-derived candidates mostly lack building names; building IDs are used as fallback identifiers.
- Public access, management agreement, seismic evidence, safe-floor assumptions, and entrance location remain unresolved.
- This audit does not claim official shelter status or safety approval.
"""

    p8c_handoff = """# P8-C Humanitarian Candidate Handoff

Date: 2026-05-24.

## Scope

P8-C owns data/proxy representation only.

Expanded audit input:

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`

## P8-C Allocation

- Candidates are represented as `humanitarian_candidate_proxy` / `highrise_candidate_marker` in the hazard interaction model.
- Candidates can receive hazard status at proxy/data level through `P8InfrastructureHazardEvaluator`.
- Candidate name list is for user review before persistent visibility or gameplay use.
- No final selectable gameplay is implemented.
- No gameplay success/failure rule changes are introduced.

## Future Allocation

P8-D may use the audit records for building warning, entrance blocked, low-floor inundation warning, and lightweight damage-proxy status. P8-D must not implement real structural collapse or official shelter claims.

P8-E verifies persistent explicit visibility, non-official warnings/disclaimers, and handoff to P9.

P9 decides whether any candidates become life-first selectable vertical evacuation targets. They remain non-official, use entrance/safe-floor/evacuation-complete proxies, and do not use real indoor scenes.
"""

    p8d_doc = """# P8-D Humanitarian Candidate Damage Status Scope

Date: 2026-05-24.

P8-D allocation note only. Do not implement P8-D behavior in the candidate-audit task.

## Scope

The expanded audit data at `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json` may be used by a future P8-D damage/blockage proxy as non-official building candidates.

Allowed future P8-D proxy states:

- building warning;
- entrance blocked proxy;
- low-floor inundation warning;
- restricted/avoid proxy state from hazard data;
- lightweight damage-status marker.

Not allowed:

- real structural collapse simulation;
- official shelter claim;
- safe/approved candidate claim;
- P9 selectable gameplay;
- indoor stair/fire-route scene.

Every candidate must keep `isOfficialShelter=false` and `nonOfficialWarningRequired=true`.
"""

    p8e_doc = """# P8-E Humanitarian Candidate Persistent Visibility Scope

Date: 2026-05-24.

P8-E is reserved for final P8 closeout and handoff verification before P9.

## Expanded Candidate Audit Input

P8-E should verify the expanded audit list:

- `Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json`
- `docs/P8C_HUMANITARIAN_HIGHRISE_CANDIDATE_NAME_LIST.md`

Audit v1 is expanded beyond the old five sample candidates. It includes PLATEAU-derived Chuo high-rise / office / commercial / hotel / mixed-use screening candidates where local attributes exist.

## Visibility Requirement

Future persistent visibility must:

- make candidates visible only as non-official humanitarian candidates;
- show explicit warnings/disclaimers;
- avoid official shelter styling;
- avoid safe/approved wording;
- preserve manual review status;
- tell P9 how to use entrance/safe-floor/evacuation-complete proxies if P9 makes any candidate selectable.

P8-E must not implement P9 gameplay or real indoor scenes.
"""

    NAME_LIST_PATH.write_text(name_list, encoding="utf-8", newline="\n")
    AUDIT_DOC_PATH.write_text(audit_doc, encoding="utf-8", newline="\n")
    P8C_HANDOFF_PATH.write_text(p8c_handoff, encoding="utf-8", newline="\n")
    P8D_SCOPE_PATH.write_text(p8d_doc, encoding="utf-8", newline="\n")
    P8E_SCOPE_PATH.write_text(p8e_doc, encoding="utf-8", newline="\n")


def main():
    known_official_ids, official_files_used = load_known_official_plateau_ids()
    hazard_bounds = load_hazard_bounds()
    plateau_raw_records, raw_files = extract_plateau_records(known_official_ids, hazard_bounds)
    selected_plateau = select_plateau_candidates(plateau_raw_records)

    records = []
    for index, (record, force_additional_review) in enumerate(selected_plateau, start=1):
        records.append(build_plateau_candidate(record, index, force_additional_review))

    sample_records, sample_files_used = load_sample_candidates(len(records) + 1)
    records.extend(sample_records)
    records = sort_records(records)

    files_used = sorted(
        set(sample_files_used + official_files_used + raw_files + ([repo_display_path(HAZARD_LAYER_PATH)] if HAZARD_LAYER_PATH.exists() else []))
    )
    plateau_included = [record for record in records if record.get("evidenceStatus") == "plateau_geometry_attribute_only"]
    plateau_named_count = len([record for record in plateau_included if record.get("buildingName")])
    plateau_id_only_count = len([record for record in plateau_included if not record.get("buildingName")])
    missing_or_limited_data = [
        "Assets/P7HighDetail is not present as a directory in this worktree; the large high-detail scene is under Assets/Scenes/P7HighDetail and was not modified.",
        "The root processed path requested by the audit is not present; processed project outputs are under data_pipeline/processed.",
        "Public access, management agreement, seismic evidence, safe-floor, entrance, and indoor-route evidence are not available in local PLATEAU geometry attributes.",
        "Known official shelter-matched PLATEAU building IDs were excluded from this non-official candidate layer.",
    ]
    if plateau_named_count > 0:
        missing_or_limited_data = [
            f"Local PLATEAU attributes exposed {plateau_named_count} source building names; remaining PLATEAU records use fallback building IDs and require name review.",
            *missing_or_limited_data,
        ]
    else:
        missing_or_limited_data = [
            "No source-exposed real building names were found in local PLATEAU candidate attributes; PLATEAU records use fallback building IDs.",
            *missing_or_limited_data,
        ]

    dataset = {
        "datasetId": "p8_humanitarian_highrise_candidate_audit_v1",
        "generatedAt": datetime.now(timezone(timedelta(hours=9))).replace(microsecond=0).isoformat(),
        "coordinateReferenceSystem": "EPSG:4326",
        "candidateLayer": "humanitarian_candidate_audit",
        "isOfficialShelterDataset": False,
        "nonOfficialWarningRequired": True,
        "dataOnly": True,
        "affectsGameplaySuccessFailure": False,
        "implementsP8D": False,
        "implementsP9SelectableGameplay": False,
        "notes": "Expanded non-official humanitarian high-rise candidate audit. Data-only; not official shelters; not marked safe or approved; manual review required before future persistent visibility or gameplay use.",
        "screeningSummary": {
            "rawPlateauScreeningCandidateCount": len(plateau_raw_records),
            "plateauIncludedCount": len(plateau_included),
            "existingSampleIncludedCount": len(sample_records),
            "knownOfficialPlateauBuildingIdsExcluded": len(known_official_ids),
            "realPlateauNamedCandidateCount": plateau_named_count,
            "plateauIdOnlyCandidateCount": plateau_id_only_count,
            "plateauInsideDerivedP8TsunamiBoundaryBboxCount": len([record for record in plateau_included if record.get("withinP8ExtractedBoundaryBbox") is True]),
        },
        "filesSearched": SEARCH_PATHS,
        "searchTerms": SEARCH_TERMS,
        "filesUsed": files_used,
        "missingOrLimitedData": missing_or_limited_data,
        "records": records,
    }

    P8_DATA_PATH.write_text(json.dumps(dataset, ensure_ascii=False, indent=2) + "\n", encoding="utf-8", newline="\n")
    write_docs(dataset)

    print(f"Wrote {repo_display_path(P8_DATA_PATH)}")
    print(f"Wrote {repo_display_path(NAME_LIST_PATH)}")
    print(f"Wrote {repo_display_path(AUDIT_DOC_PATH)}")
    print(f"records={len(records)} plateau_raw_screened={len(plateau_raw_records)}")


if __name__ == "__main__":
    main()
