from __future__ import annotations

import csv
import json
import re
from datetime import datetime, timezone, timedelta
from pathlib import Path
from typing import Any


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
DOWNLOAD_ROOT = PIPELINE_ROOT / "downloads"
SOURCES_ROOT = PIPELINE_ROOT / "sources"
PROCESSED_ROOT = PIPELINE_ROOT / "processed" / "qualification"

SOURCE_MANIFEST_PATH = SOURCES_ROOT / "real_chuo_official_source_manifest.json"
OUTPUT_JSON = PROCESSED_ROOT / "real_chuo_official_shelters_normalized.json"
OUTPUT_CSV = PROCESSED_ROOT / "real_chuo_official_shelters_normalized.csv"

DISASTER_FIELD_MAP = {
    "災害種別_洪水": "flood",
    "災害種別_崖崩れ、土石流及び地滑り": "landslide",
    "災害種別_高潮": "storm_surge",
    "災害種別_地震": "earthquake",
    "災害種別_津波": "tsunami",
    "災害種別_大規模な火事": "large_fire",
    "災害種別_内水氾濫": "inland_flooding",
    "災害種別_火山現象": "volcanic",
}

TOKYO_DISASTER_FIELD_MAP = {
    "洪水": "flood",
    "崖崩れ、土石流及び地滑り": "landslide",
    "高潮": "storm_surge",
    "地震": "earthquake",
    "津波": "tsunami",
    "大規模な火事": "large_fire",
    "内水氾濫": "inland_flooding",
    "火山現象": "volcanic",
}

OUTPUT_FIELDS = [
    "shelterId",
    "shelterName",
    "officialSourceName",
    "sourceOwner",
    "sourceUrl",
    "sourceUpdatedAt",
    "downloadedAt",
    "licenseOrTerms",
    "address",
    "latitude",
    "longitude",
    "coordinateReferenceSystem",
    "disasterTypes",
    "officialDesignationStatus",
    "safeFloor",
    "capacity",
    "notes",
    "manualReviewNeeded",
    "evidenceSources",
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


def read_csv_auto(path: Path) -> list[dict[str, str]]:
    if not path.exists():
        raise FileNotFoundError(f"Required official raw input is missing: {path}")

    raw = path.read_bytes()
    last_error: UnicodeDecodeError | None = None
    for encoding in ("utf-8-sig", "cp932", "utf-8"):
        try:
            text = raw.decode(encoding)
            break
        except UnicodeDecodeError as exc:
            last_error = exc
    else:
        raise UnicodeDecodeError(
            last_error.encoding if last_error else "unknown",
            last_error.object if last_error else raw,
            last_error.start if last_error else 0,
            last_error.end if last_error else 0,
            last_error.reason if last_error else "unsupported CSV encoding",
        )

    rows = [row for row in csv.reader(text.splitlines()) if any(cell.strip() for cell in row)]
    if not rows:
        return []
    header = rows[0]
    return [dict(zip(header, row)) for row in rows[1:]]


def source_by_id(manifest: dict[str, Any], source_id: str) -> dict[str, Any]:
    for source in manifest["sources"]:
        if source["sourceId"] == source_id:
            return source
    raise KeyError(f"Source not found in manifest: {source_id}")


def normalize_name(value: str | None) -> str:
    if not value:
        return ""
    return re.sub(r"[\s　（）()]", "", value)


def parse_float(value: str | None) -> float | None:
    if value is None:
        return None
    value = value.strip()
    if not value:
        return None
    return float(value)


def parse_capacity(value: str | None) -> int | None:
    if not value:
        return None
    match = re.search(r"([0-9,]+)\s*人", value)
    if not match:
        return None
    return int(match.group(1).replace(",", ""))


def disaster_types_from(row: dict[str, str], field_map: dict[str, str]) -> list[str]:
    disaster_types: list[str] = []
    for field, disaster_type in field_map.items():
        if row.get(field, "").strip() == "1":
            disaster_types.append(disaster_type)
    return disaster_types


def index_by_name(rows: list[dict[str, str]], name_fields: tuple[str, ...], municipality_field: str | None = None) -> dict[str, list[dict[str, str]]]:
    index: dict[str, list[dict[str, str]]] = {}
    for row in rows:
        if municipality_field and row.get(municipality_field) != "中央区":
            continue
        for field in name_fields:
            key = normalize_name(row.get(field))
            if key:
                index.setdefault(key, []).append(row)
                break
    return index


def evidence_source(source: dict[str, Any], note: str) -> dict[str, Any]:
    return {
        "sourceId": source["sourceId"],
        "sourceFamily": "official_shelter_facility",
        "sourceName": source["sourceName"],
        "officialStatus": source["reliabilityLevel"],
        "sourceUrl": source["sourceUrl"],
        "sourceUpdatedAt": source.get("sourceUpdatedAt"),
        "evidenceNote": note,
    }


def is_likely_non_building_area(name: str) -> bool:
    return any(token in name for token in ("公園一帯", "地区", "リバーシティ"))


def build_record(
    index: int,
    row: dict[str, str],
    manifest: dict[str, Any],
    supporting_indexes: dict[str, dict[str, list[dict[str, str]]]],
) -> dict[str, Any]:
    chuo_source = source_by_id(manifest, "chuo_open_data_designated_emergency_shelters")
    name = row["名称"].strip()
    name_key = normalize_name(name)
    latitude = parse_float(row.get("緯度"))
    longitude = parse_float(row.get("経度"))
    disaster_types = disaster_types_from(row, DISASTER_FIELD_MAP)

    evidence_sources = [
        evidence_source(
            chuo_source,
            "Primary Chuo City open-data record for a designated emergency evacuation shelter/place.",
        )
    ]

    supporting_notes: list[str] = []
    for source_id, index_by_name_map in supporting_indexes.items():
        matches = index_by_name_map.get(name_key, [])
        if not matches:
            continue
        source = source_by_id(manifest, source_id)
        evidence_sources.append(
            evidence_source(source, f"Supporting official source has {len(matches)} record(s) with matching normalized facility name.")
        )
        supporting_notes.append(f"{source['sourceName']} match count: {len(matches)}")
        if not disaster_types:
            disaster_types = disaster_types_from(matches[0], TOKYO_DISASTER_FIELD_MAP)

    capacity = parse_capacity(row.get("想定収容人数"))
    manual_review = (
        latitude is None
        or longitude is None
        or is_likely_non_building_area(name)
        or len(evidence_sources) == 1
    )

    notes_parts = [
        "Normalized from official Chuo City open data.",
        f"Original municipality code: {row.get('全国地方公共団体コード', '')}.",
    ]
    if row.get("指定避難所との重複") == "1":
        notes_parts.append("Source marks overlap with designated shelter address.")
    if is_likely_non_building_area(name):
        notes_parts.append("Name appears to describe a broad evacuation area rather than a single building.")
    if row.get("備考"):
        notes_parts.append(f"Source remarks: {row['備考']}")
    notes_parts.extend(supporting_notes)

    return {
        "shelterId": f"chuo_official_emergency_{index:03d}",
        "shelterName": name,
        "officialSourceName": chuo_source["sourceName"],
        "sourceOwner": chuo_source["sourceOwner"],
        "sourceUrl": chuo_source["sourceUrl"],
        "sourceUpdatedAt": chuo_source.get("sourceUpdatedAt"),
        "downloadedAt": chuo_source.get("downloadedAt"),
        "licenseOrTerms": chuo_source["licenseOrTerms"],
        "address": row.get("所在地_連結表記") or row.get("所在地住所") or "",
        "latitude": latitude,
        "longitude": longitude,
        "coordinateReferenceSystem": "EPSG:4326",
        "disasterTypes": disaster_types,
        "officialDesignationStatus": "official_designated",
        "safeFloor": None,
        "capacity": capacity,
        "notes": " ".join(part for part in notes_parts if part),
        "manualReviewNeeded": manual_review,
        "evidenceSources": evidence_sources,
    }


def build_dataset() -> dict[str, Any]:
    manifest = load_json(SOURCE_MANIFEST_PATH)

    chuo_source = source_by_id(manifest, "chuo_open_data_designated_emergency_shelters")
    tokyo_center_source = source_by_id(manifest, "tokyo_bosai_map_evacuation_center")
    tokyo_area_source = source_by_id(manifest, "tokyo_bosai_map_evacuation_area")
    gsi_emergency_source = source_by_id(manifest, "gsi_chuo_designated_emergency_evacuation_places")
    gsi_shelter_source = source_by_id(manifest, "gsi_chuo_designated_shelters")

    chuo_rows = read_csv_auto(REPO_ROOT / chuo_source["localRawPath"])
    tokyo_center_rows = read_csv_auto(REPO_ROOT / tokyo_center_source["localRawPath"])
    tokyo_area_rows = read_csv_auto(REPO_ROOT / tokyo_area_source["localRawPath"])
    gsi_emergency_rows = read_csv_auto(REPO_ROOT / gsi_emergency_source["localRawPath"])
    gsi_shelter_rows = read_csv_auto(REPO_ROOT / gsi_shelter_source["localRawPath"])

    supporting_indexes = {
        "tokyo_bosai_map_evacuation_center": index_by_name(tokyo_center_rows, ("避難所_施設名称",), "指定市区町村名"),
        "tokyo_bosai_map_evacuation_area": index_by_name(tokyo_area_rows, ("施設名",), "区市町村"),
        "gsi_chuo_designated_emergency_evacuation_places": index_by_name(gsi_emergency_rows, ("施設・場所名",)),
        "gsi_chuo_designated_shelters": index_by_name(gsi_shelter_rows, ("施設・場所名",)),
    }

    records = [
        build_record(index, row, manifest, supporting_indexes)
        for index, row in enumerate(chuo_rows, start=1)
    ]

    return {
        "datasetId": "p5_b4_real_chuo_official_shelters_normalized",
        "generatedAt": now_jst(),
        "coordinateReferenceSystem": "EPSG:4326",
        "sourceManifestPath": str(SOURCE_MANIFEST_PATH.relative_to(REPO_ROOT)).replace("\\", "/"),
        "recordCount": len(records),
        "sourceRowCounts": {
            "chuoOpenData": len(chuo_rows),
            "tokyoEvacuationCenterChuoMatches": sum(
                1 for row in tokyo_center_rows if row.get("指定市区町村名") == "中央区"
            ),
            "tokyoEvacuationAreaChuoMatches": sum(
                1 for row in tokyo_area_rows if row.get("区市町村") == "中央区"
            ),
            "gsiEmergencyEvacuationPlaces": len(gsi_emergency_rows),
            "gsiDesignatedShelters": len(gsi_shelter_rows),
        },
        "notes": (
            "Official Chuo City open data is the primary source. Tokyo Metropolitan Government and GSI records are "
            "used as official reference cross-checks where facility names match. No raw downloaded files are required "
            "at Unity runtime."
        ),
        "records": records,
    }


def write_csv(path: Path, records: list[dict[str, Any]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=OUTPUT_FIELDS)
        writer.writeheader()
        for record in records:
            row: dict[str, Any] = {}
            for field in OUTPUT_FIELDS:
                value = record.get(field)
                if isinstance(value, (list, dict)):
                    row[field] = json.dumps(value, ensure_ascii=False, separators=(",", ":"))
                elif value is None:
                    row[field] = ""
                else:
                    row[field] = value
            writer.writerow(row)


def main() -> int:
    dataset = build_dataset()
    write_json(OUTPUT_JSON, dataset)
    write_csv(OUTPUT_CSV, dataset["records"])
    print(f"[ingest] wrote {OUTPUT_JSON}")
    print(f"[ingest] wrote {OUTPUT_CSV}")
    print(f"[ingest] records: {len(dataset['records'])}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
