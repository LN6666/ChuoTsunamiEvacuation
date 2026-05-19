from __future__ import annotations

import csv
import json
from pathlib import Path
from typing import Any


REPO_ROOT = Path(__file__).resolve().parents[2]
PIPELINE_ROOT = REPO_ROOT / "data_pipeline"
QUALIFICATION_ROOT = PIPELINE_ROOT / "qualification"
CONFIG_PATH = QUALIFICATION_ROOT / "controlled_qualification_pipeline_config.json"

OFFICIAL_EVIDENCE_FAMILIES = {
    "official_shelter_facility",
    "official_evacuation_building",
    "official_hazard_map",
    "official_disaster_prevention_plan",
}

OUTPUT_FIELDNAMES = [
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
    "notes",
]


def resolve_repo_path(path_value: str | Path) -> Path:
    path = Path(path_value)
    if path.is_absolute():
        return path
    return REPO_ROOT / path


def load_json(path: Path) -> Any:
    with path.open("r", encoding="utf-8") as handle:
        return json.load(handle)


def write_json(path: Path, payload: dict[str, Any]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="\n") as handle:
        json.dump(payload, handle, indent=2, ensure_ascii=False)
        handle.write("\n")


def evidence_has_official_family(evidence_sources: list[dict[str, Any]]) -> bool:
    return any(source.get("sourceFamily") in OFFICIAL_EVIDENCE_FAMILIES for source in evidence_sources)


def merge_evidence_sources(
    shelter_sources: list[dict[str, Any]],
    building_sources: list[dict[str, Any]],
) -> list[dict[str, Any]]:
    merged: list[dict[str, Any]] = []
    seen: set[tuple[str, str]] = set()
    for source in shelter_sources + building_sources:
        key = (str(source.get("sourceId", "")), str(source.get("sourceFamily", "")))
        if key in seen:
            continue
        seen.add(key)
        merged.append(source)
    return merged


def first_source_updated_at(evidence_sources: list[dict[str, Any]]) -> str | None:
    for source in evidence_sources:
        updated_at = source.get("sourceUpdatedAt")
        if updated_at:
            return str(updated_at)
    return None


def route_lookup_key(route: dict[str, Any]) -> tuple[str | None, str | None]:
    return route.get("shelterId"), route.get("plateauBuildingId")


def find_route(
    routes_by_key: dict[tuple[str | None, str | None], dict[str, Any]],
    shelter_id: str | None,
    plateau_building_id: str | None,
) -> dict[str, Any] | None:
    exact = routes_by_key.get((shelter_id, plateau_building_id))
    if exact is not None:
        return exact
    return routes_by_key.get((shelter_id, None))


def nearest_confidence(match_distance: float | int | None, thresholds: dict[str, Any]) -> str:
    if match_distance is None:
        return "low"
    if match_distance <= thresholds["nearestHighConfidenceDistanceMeters"]:
        return "high"
    if match_distance <= thresholds["nearestMediumConfidenceDistanceMeters"]:
        return "medium"
    return "low"


def classify_record(
    shelter: dict[str, Any],
    evidence_sources: list[dict[str, Any]],
    match_method: str,
    match_distance: float | None,
    thresholds: dict[str, Any],
) -> tuple[str, str | None, str, str]:
    has_official_evidence = evidence_has_official_family(evidence_sources)
    has_candidate_evidence = bool(evidence_sources)
    safe_floor = shelter.get("safeFloor")
    capacity = shelter.get("capacity")
    can_enter = shelter.get("canEnter")
    source_type = str(shelter.get("sourceType", ""))

    if "conflict" in source_type or can_enter is False:
        return (
            "not_qualified",
            "conflicting",
            "low",
            "Controlled conflict or canEnter=false indicates the building should not be treated as an evacuation building candidate.",
        )

    if match_method == "unmatched":
        official_status = "official_designated_with_review" if has_official_evidence else "unknown"
        return (
            "unknown",
            official_status,
            "low",
            "Evidence exists, but no controlled PLATEAU building match is available; qualification remains unknown until manual review.",
        )

    if has_official_evidence and match_method == "contains":
        return (
            "official_confirmed",
            "official_designated",
            str(thresholds["containsMatchConfidence"]),
            "Official evidence is present and the controlled source point is contained by one synthetic PLATEAU-like building.",
        )

    if has_official_evidence and match_method == "nearest":
        return (
            "official_confirmed_with_review",
            "official_designated_with_review",
            nearest_confidence(match_distance, thresholds),
            "Official evidence is present, but the controlled point uses a nearest-building match and requires manual review.",
        )

    if has_candidate_evidence:
        safe_floor_ok = isinstance(safe_floor, int) and safe_floor >= thresholds["minSafeFloorCandidate"]
        capacity_ok = isinstance(capacity, int) and capacity >= thresholds["minCapacityCandidate"]
        if safe_floor_ok and capacity_ok and match_method in {"contains", "nearest"}:
            return (
                "strong_candidate",
                "not_official",
                "medium",
                "Controlled non-official evidence meets safe-floor and capacity thresholds, but it cannot confirm official designation.",
            )
        return (
            "weak_candidate",
            "not_official",
            "low",
            "Controlled candidate evidence exists, but safe-floor or capacity evidence is missing or below threshold.",
        )

    return (
        "unknown",
        "unknown",
        "unknown",
        "Insufficient evidence exists to qualify or reject this controlled building record.",
    )


def build_warnings(
    shelter: dict[str, Any],
    qualification_status: str,
    match_method: str,
    match_distance: float | None,
    route: dict[str, Any] | None,
    thresholds: dict[str, Any],
    warning_rules: dict[str, str],
) -> list[str]:
    warnings: list[str] = []
    safe_floor = shelter.get("safeFloor")
    capacity = shelter.get("capacity")

    if match_method == "nearest":
        warnings.append(warning_rules["nearestMatch"])
        if match_distance is None or match_distance > thresholds["nearestHighConfidenceDistanceMeters"]:
            warnings.append(warning_rules["nearestDistanceTooLarge"])
    elif match_method == "unmatched":
        warnings.append(warning_rules["unmatched"])
    elif match_method == "manual_override":
        warnings.append("manual override match requires review")

    if qualification_status in {"strong_candidate", "weak_candidate"}:
        warnings.append(warning_rules["candidateNonOfficial"])
        warnings.append(warning_rules["literatureCannotConfirmOfficial"])

    if safe_floor is None or capacity is None:
        if qualification_status in {"weak_candidate", "unknown", "not_qualified"}:
            warnings.append(warning_rules["missingSafeFloorOrCapacity"])

    if qualification_status == "unknown":
        if not shelter.get("evidenceSources"):
            warnings.append(warning_rules["insufficientEvidence"])
        if match_method == "unmatched":
            warnings.append("official evidence unresolved because building match is unavailable")

    if qualification_status == "not_qualified":
        warnings.append(warning_rules["sourceConflict"])
        warnings.append(warning_rules["notQualified"])

    if route is not None:
        route_availability = route.get("routeAvailability")
        if route_availability == "unavailable":
            warnings.append(warning_rules["routeUnavailable"])
        elif route_availability == "failed":
            warnings.append(warning_rules["routeFailed"])
        for warning in route.get("warnings", []):
            warnings.append(str(warning))

    return list(dict.fromkeys(warnings))


def manual_review_needed(status: str, match_method: str, confidence: str, warnings: list[str]) -> bool:
    if status == "official_confirmed" and match_method == "contains" and confidence == "high" and not warnings:
        return False
    return True


def build_record(
    index: int,
    shelter: dict[str, Any],
    buildings_by_id: dict[str, dict[str, Any]],
    routes_by_key: dict[tuple[str | None, str | None], dict[str, Any]],
    thresholds: dict[str, Any],
    warning_rules: dict[str, str],
) -> dict[str, Any]:
    controlled_match = shelter.get("controlledMatch", {})
    plateau_building_id = controlled_match.get("plateauBuildingId")
    match_method = controlled_match.get("matchMethod", "not_evaluated")
    match_distance = controlled_match.get("matchDistanceMeters")

    building = buildings_by_id.get(plateau_building_id) if plateau_building_id else None
    evidence_sources = merge_evidence_sources(
        list(shelter.get("evidenceSources", [])),
        list(building.get("evidenceSources", [])) if building else [],
    )
    route = find_route(routes_by_key, shelter.get("shelterId"), plateau_building_id)

    status, official_status, confidence, reason = classify_record(
        shelter,
        evidence_sources,
        match_method,
        match_distance,
        thresholds,
    )
    warnings = build_warnings(shelter, status, match_method, match_distance, route, thresholds, warning_rules)

    route_availability = route.get("routeAvailability") if route else "not_evaluated"
    route_distance = route.get("nearestRouteDistanceMeters") if route else None
    route_time = route.get("estimatedTravelTimeSeconds") if route else None
    route_note = route.get("notes") if route else "No controlled prototype route was evaluated."

    notes = (
        f"{shelter.get('notes', '')} "
        f"Route fields are controlled prototype estimates only and are not official evacuation routes. {route_note}"
    ).strip()

    return {
        "qualificationId": f"controlled_qualification_{index:03d}",
        "plateauBuildingId": plateau_building_id,
        "shelterId": shelter.get("shelterId"),
        "shelterName": shelter.get("shelterName"),
        "officialDesignationStatus": official_status,
        "qualificationStatus": status,
        "qualificationReason": reason,
        "evidenceSources": evidence_sources,
        "matchMethod": match_method,
        "matchDistanceMeters": match_distance,
        "confidence": confidence,
        "manualReviewNeeded": manual_review_needed(status, match_method, confidence, warnings),
        "warnings": warnings,
        "disasterTypes": shelter.get("disasterTypes"),
        "safeFloor": shelter.get("safeFloor"),
        "capacity": shelter.get("capacity"),
        "sourceUpdatedAt": first_source_updated_at(evidence_sources),
        "routeAvailability": route_availability,
        "nearestRouteDistanceMeters": route_distance,
        "estimatedTravelTimeSeconds": route_time,
        "notes": notes,
    }


def write_csv(path: Path, records: list[dict[str, Any]]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("w", encoding="utf-8", newline="") as handle:
        writer = csv.DictWriter(handle, fieldnames=OUTPUT_FIELDNAMES)
        writer.writeheader()
        for record in records:
            row: dict[str, Any] = {}
            for field in OUTPUT_FIELDNAMES:
                value = record.get(field)
                if isinstance(value, (list, dict)):
                    row[field] = json.dumps(value, ensure_ascii=False, separators=(",", ":"))
                elif value is None:
                    row[field] = ""
                else:
                    row[field] = value
            writer.writerow(row)


def build_dataset() -> dict[str, Any]:
    config = load_json(CONFIG_PATH)
    rulebook = load_json(resolve_repo_path(config["rulebookPath"]))
    load_json(resolve_repo_path(config["schemaPath"]))

    shelters_payload = load_json(resolve_repo_path(config["inputFixturePaths"]["shelters"]))
    buildings_payload = load_json(resolve_repo_path(config["inputFixturePaths"]["buildings"]))
    routes_payload = load_json(resolve_repo_path(config["inputFixturePaths"]["routes"]))

    buildings_by_id = {
        building["plateauBuildingId"]: building
        for building in buildings_payload["buildings"]
    }
    routes_by_key = {
        route_lookup_key(route): route
        for route in routes_payload["routes"]
    }

    records = [
        build_record(
            index,
            shelter,
            buildings_by_id,
            routes_by_key,
            config["defaultThresholds"],
            config["warningRules"],
        )
        for index, shelter in enumerate(shelters_payload["shelters"], start=1)
    ]

    return {
        "datasetId": "p5_b1_controlled_building_qualification_output",
        "generatedAt": config["generatedAt"],
        "coordinateReferenceSystem": shelters_payload["coordinateReferenceSystem"],
        "rulebookVersion": rulebook["rulebookVersion"],
        "notes": (
            "Controlled P5-B1 output generated from synthetic fixtures only. "
            "Matching and route fields are placeholders for P5-B2 and are not real PLATEAU matches or official evacuation routes."
        ),
        "records": records,
    }


def main() -> int:
    config = load_json(CONFIG_PATH)
    dataset = build_dataset()
    json_path = resolve_repo_path(config["outputPaths"]["json"])
    csv_path = resolve_repo_path(config["outputPaths"]["csv"])
    write_json(json_path, dataset)
    write_csv(csv_path, dataset["records"])
    print(f"[build] wrote {json_path}")
    print(f"[build] wrote {csv_path}")
    print(f"[build] records: {len(dataset['records'])}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
