using System;
using System.Collections.Generic;
using UnityEngine;

public static class P9DNearestAnchorMatcher
{
    public static P9DCoordinateAnchoringResult AnchorCoordinate(
        P9DCoordinateAnchorRecord record,
        P9DCoordinateAnchoringConfig config)
    {
        config = config ?? new P9DCoordinateAnchoringConfig();
        var result = CreateBaseResult(record, config);
        if (record == null)
        {
            result.anchoringStatus = P9DAnchoringTokens.RejectedInvalidCoordinate;
            result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.Rejected);
            result.fallbackReason = "missing_anchor_record";
            result.summary = "P9-D anchor rejected: missing record.";
            return result;
        }

        if (!record.hasCoordinate)
        {
            result.success = true;
            result.coordinateValid = false;
            result.coordinateBasedProxy = false;
            result.unityPosition = record.fallbackProxyPosition == null ? Vector3.zero : record.fallbackProxyPosition.ToVector3();
            result.anchoringStatus = P9DAnchoringTokens.FallbackMarkerOnly;
            result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.Fallback);
            result.fallbackReason = "missing_coordinate_proxy_marker_only";
            result.summary = "P9-D anchor uses fallback marker only because coordinates are missing.";
            return result;
        }

        if (!IsFinite(record.latitude) || !IsFinite(record.longitude))
        {
            result.anchoringStatus = P9DAnchoringTokens.RejectedInvalidCoordinate;
            result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.Rejected);
            result.fallbackReason = "invalid_lat_lon";
            result.summary = "P9-D anchor rejected: invalid coordinate.";
            return result;
        }

        if (!IsWithinBounds(record.latitude, record.longitude, config))
        {
            result.anchoringStatus = P9DAnchoringTokens.RejectedOutOfBounds;
            result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.Rejected);
            result.fallbackReason = "coordinate_out_of_bounds";
            result.summary = "P9-D anchor rejected: coordinate out of configured Chuo bounds.";
            return result;
        }

        result.success = true;
        result.coordinateValid = true;
        result.coordinateBasedProxy = true;
        result.unityPosition = ProjectToUnity(record.latitude, record.longitude, config);
        result.anchoringStatus = StatusForAnchorType(record.anchorType);
        result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.High);
        result.summary = "P9-D coordinate proxy anchor created. Exact PLATEAU object identity is not claimed.";
        return result;
    }

    public static P9DCoordinateAnchoringResult MatchNearest(
        P9DCoordinateAnchorRecord source,
        P9DCoordinateAnchorRecord[] candidates,
        P9DCoordinateAnchoringConfig config,
        string matchedStatus)
    {
        config = config ?? new P9DCoordinateAnchoringConfig();
        P9DCoordinateAnchoringResult result = AnchorCoordinate(source, config);
        if (!result.success || !result.coordinateValid)
        {
            return result;
        }

        if (candidates == null || candidates.Length == 0)
        {
            result.anchoringStatus = P9DAnchoringTokens.FallbackMarkerOnly;
            result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.Fallback);
            result.fallbackReason = "no_nearest_match_candidates";
            return result;
        }

        P9DCoordinateAnchorRecord nearest = null;
        float nearestDistance = float.MaxValue;
        for (int i = 0; i < candidates.Length; i++)
        {
            P9DCoordinateAnchorRecord candidate = candidates[i];
            if (candidate == null || !candidate.hasCoordinate || !IsWithinBounds(candidate.latitude, candidate.longitude, config))
            {
                continue;
            }

            float distance = CalculateDistanceMeters(
                source.latitude,
                source.longitude,
                candidate.latitude,
                candidate.longitude,
                config);
            if (distance < nearestDistance ||
                (Mathf.Approximately(distance, nearestDistance) && string.CompareOrdinal(candidate.anchorId, nearest == null ? string.Empty : nearest.anchorId) < 0))
            {
                nearest = candidate;
                nearestDistance = distance;
            }
        }

        if (nearest == null)
        {
            result.anchoringStatus = P9DAnchoringTokens.FallbackMarkerOnly;
            result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.Fallback);
            result.fallbackReason = "no_valid_nearest_match_candidates";
            return result;
        }

        result.nearestMatchUsed = true;
        result.matchedAnchorId = nearest.anchorId ?? string.Empty;
        result.matchedDistanceMeters = nearestDistance;
        if (nearestDistance > Mathf.Max(0f, config.maxNearestMatchDistanceMeters))
        {
            result.success = false;
            result.anchoringStatus = P9DAnchoringTokens.RejectedDistanceThresholdExceeded;
            result.confidence = P9DAnchoringTokens.ToToken(P9DAnchoringConfidence.Rejected);
            result.fallbackReason = "nearest_match_distance_threshold_exceeded";
            result.summary = "P9-D nearest-match rejected by distance threshold.";
            return result;
        }

        result.anchoringStatus = string.IsNullOrWhiteSpace(matchedStatus) ? P9DAnchoringTokens.AnchoredToNearestBuildingProxy : matchedStatus;
        result.confidence = P9DAnchoringTokens.ToToken(ConfidenceFromDistance(nearestDistance, config));
        result.exactPlateauObjectIdentityProven = false;
        result.summary = "P9-D nearest proxy match created. Exact PLATEAU object identity is not claimed.";
        return result;
    }

    public static P9DAnchoringReport BuildFinalAnchoringReport(
        P9DCoordinateAnchoringConfig config,
        P9BHumanitarianCandidateMarkerCollection humanitarianCandidates)
    {
        config = config ?? new P9DCoordinateAnchoringConfig();
        var results = new List<P9DCoordinateAnchoringResult>();
        bool allHumanitarianNonOfficial = true;
        bool allHumanitarianWarning = true;

        if (humanitarianCandidates != null && humanitarianCandidates.records != null)
        {
            for (int i = 0; i < humanitarianCandidates.records.Length; i++)
            {
                P9BHumanitarianCandidateMarkerRecord candidate = humanitarianCandidates.records[i];
                if (candidate == null)
                {
                    continue;
                }

                var record = new P9DCoordinateAnchorRecord
                {
                    anchorId = candidate.candidateId,
                    sourceId = candidate.fallbackBuildingId,
                    anchorType = "humanitarian_candidate_marker",
                    displayName = string.IsNullOrWhiteSpace(candidate.buildingName) ? candidate.candidateId : candidate.buildingName,
                    coordinateSource = "P8-E humanitarian candidate persistent marker handoff",
                    hasCoordinate = candidate.hasCoordinates,
                    latitude = candidate.latitude,
                    longitude = candidate.longitude,
                    isOfficialShelter = false,
                    isHumanitarianCandidate = true,
                    nonOfficialWarningRequired = true,
                    safeApprovedByDefault = false,
                    routeIsOfficial = false,
                    routeEstimatedPrototypeGuidance = true,
                    hazardStatus = candidate.hazardStatusEligible ? "hazard_status_attachable" : "hazard_status_unknown",
                    damageStatus = candidate.p8dDamageStatusEligible ? "damage_status_attachable" : "damage_status_unknown",
                    blockageStatus = "proxy_unknown",
                    lowFloorWarningStatus = "proxy_warning_attachable",
                    notes = "Coordinate-based candidate marker anchor. Non-official warning must remain visible."
                };
                P9DCoordinateAnchoringResult anchored = AnchorCoordinate(record, config);
                results.Add(anchored);
                allHumanitarianNonOfficial &= !candidate.isOfficialShelter && !anchored.isOfficialShelter;
                allHumanitarianWarning &= candidate.nonOfficialWarningRequired && anchored.nonOfficialWarningRequired;
            }
        }

        if (config.anchors != null)
        {
            for (int i = 0; i < config.anchors.Length; i++)
            {
                results.Add(AnchorCoordinate(config.anchors[i], config));
            }
        }

        return BuildReportFromResults(results, config, humanitarianCandidates, allHumanitarianNonOfficial, allHumanitarianWarning);
    }

    public static float CalculateDistanceMeters(
        float latitudeA,
        float longitudeA,
        float latitudeB,
        float longitudeB,
        P9DCoordinateAnchoringConfig config)
    {
        config = config ?? new P9DCoordinateAnchoringConfig();
        float dx = (longitudeB - longitudeA) * Mathf.Max(1f, config.metersPerDegreeLongitude);
        float dz = (latitudeB - latitudeA) * Mathf.Max(1f, config.metersPerDegreeLatitude);
        return Mathf.Sqrt(dx * dx + dz * dz);
    }

    public static Vector3 ProjectToUnity(float latitude, float longitude, P9DCoordinateAnchoringConfig config)
    {
        config = config ?? new P9DCoordinateAnchoringConfig();
        float x = (longitude - config.originLongitude) * Mathf.Max(1f, config.metersPerDegreeLongitude);
        float z = (latitude - config.originLatitude) * Mathf.Max(1f, config.metersPerDegreeLatitude);
        return new Vector3(x, 0f, z);
    }

    public static bool IsWithinBounds(float latitude, float longitude, P9DCoordinateAnchoringConfig config)
    {
        config = config ?? new P9DCoordinateAnchoringConfig();
        return latitude >= config.minLatitude &&
            latitude <= config.maxLatitude &&
            longitude >= config.minLongitude &&
            longitude <= config.maxLongitude;
    }

    private static P9DCoordinateAnchoringResult CreateBaseResult(
        P9DCoordinateAnchorRecord record,
        P9DCoordinateAnchoringConfig config)
    {
        record = record ?? new P9DCoordinateAnchorRecord();
        return new P9DCoordinateAnchoringResult
        {
            anchorId = record.anchorId ?? string.Empty,
            sourceId = record.sourceId ?? string.Empty,
            anchorType = record.anchorType ?? string.Empty,
            coordinateSource = record.coordinateSource ?? string.Empty,
            latitude = record.latitude,
            longitude = record.longitude,
            coordinateBasedProxy = config == null || config.coordinateBasedProxyRequired,
            exactPlateauObjectIdentityProven = false,
            routeIsOfficial = false,
            routeEstimatedPrototypeGuidance = record.routeEstimatedPrototypeGuidance && !record.routeIsOfficial,
            isOfficialShelter = record.isOfficialShelter && !record.isHumanitarianCandidate,
            isHumanitarianCandidate = record.isHumanitarianCandidate,
            nonOfficialWarningRequired = record.isHumanitarianCandidate || record.nonOfficialWarningRequired,
            safeApprovedByDefault = false,
            hazardStatus = record.hazardStatus ?? "unknown",
            damageStatus = record.damageStatus ?? "unknown",
            blockageStatus = record.blockageStatus ?? "unknown",
            lowFloorWarningStatus = record.lowFloorWarningStatus ?? "unknown",
            warningText = record.isHumanitarianCandidate
                ? P9CVerticalEvacuationTargetDecision.NonOfficialWarningText
                : string.Empty
        };
    }

    private static P9DAnchoringReport BuildReportFromResults(
        List<P9DCoordinateAnchoringResult> results,
        P9DCoordinateAnchoringConfig config,
        P9BHumanitarianCandidateMarkerCollection humanitarianCandidates,
        bool allHumanitarianNonOfficial,
        bool allHumanitarianWarning)
    {
        var report = new P9DAnchoringReport
        {
            coordinatePolicy = config.coordinatePolicy ?? "coordinate_based_proxy_nearest_match",
            humanitarianCandidateTotal = humanitarianCandidates == null ? 0 : humanitarianCandidates.totalCandidates,
            namedHumanitarianCandidateCount = humanitarianCandidates == null ? 0 : humanitarianCandidates.namedMarkerCount,
            idOnlyHumanitarianCandidateCount = humanitarianCandidates == null ? 0 : humanitarianCandidates.idOnlyMarkerCount,
            allHumanitarianCandidatesRemainNonOfficial = allHumanitarianNonOfficial,
            allHumanitarianCandidatesRequireWarning = allHumanitarianWarning,
            exactPlateauObjectIdentityClaimed = false,
            p8HandoffConsumed = config.p8HandoffConsumed,
            results = results.ToArray()
        };

        report.totalAnchors = report.results.Length;
        for (int i = 0; i < report.results.Length; i++)
        {
            P9DCoordinateAnchoringResult result = report.results[i];
            if (result.success)
            {
                report.successfulAnchors++;
            }
            else
            {
                report.rejectedCount++;
            }

            if (result.anchoringStatus == P9DAnchoringTokens.AnchoredToCoordinate ||
                result.anchoringStatus == P9DAnchoringTokens.AnchoredToHazardGrid)
            {
                report.coordinateAnchoredCount++;
            }
            if (result.nearestMatchUsed)
            {
                report.nearestMatchCount++;
            }
            if (result.anchoringStatus == P9DAnchoringTokens.FallbackMarkerOnly)
            {
                report.fallbackMarkerOnlyCount++;
            }
            if (result.anchorType == "humanitarian_candidate_marker" && result.isHumanitarianCandidate && result.success)
            {
                report.humanitarianCandidateAnchored++;
            }
            if (result.anchorType == "humanitarian_candidate_marker" && result.isHumanitarianCandidate && result.nonOfficialWarningRequired)
            {
                report.humanitarianCandidateWarningRequiredCount++;
            }
            if (result.isOfficialShelter)
            {
                report.officialShelterAnchorCount++;
            }
            if (result.anchorType == "entrance_proxy" || result.anchorType == "safe_floor_proxy_target")
            {
                report.entranceProxyAnchorCount++;
            }
            if (result.anchorType == "route_guidance_proxy")
            {
                report.routeProxyAnchorCount++;
                report.routesRemainEstimatedPrototypeGuidance &= !result.routeIsOfficial && result.routeEstimatedPrototypeGuidance;
            }
            if (result.anchorType == "hazard_grid_lookup")
            {
                report.hazardLookupAnchorCount++;
            }
        }

        report.summary = "P9-D coordinate-based anchoring report: total=" + report.totalAnchors +
                         ", successful=" + report.successfulAnchors +
                         ", humanitarian=" + report.humanitarianCandidateAnchored +
                         ", rejected=" + report.rejectedCount +
                         ". Proxy/nearest-match only; exact PLATEAU object identity is not claimed.";
        return report;
    }

    private static string StatusForAnchorType(string anchorType)
    {
        if (anchorType == "route_guidance_proxy")
        {
            return P9DAnchoringTokens.AnchoredToNearestRoadProxy;
        }

        if (anchorType == "entrance_proxy" || anchorType == "safe_floor_proxy_target")
        {
            return P9DAnchoringTokens.AnchoredToEntranceProxy;
        }

        if (anchorType == "hazard_grid_lookup")
        {
            return P9DAnchoringTokens.AnchoredToHazardGrid;
        }

        return P9DAnchoringTokens.AnchoredToCoordinate;
    }

    private static P9DAnchoringConfidence ConfidenceFromDistance(float distanceMeters, P9DCoordinateAnchoringConfig config)
    {
        if (config.exactPlateauObjectIdentityProofAvailable && distanceMeters <= config.exactDistanceMeters)
        {
            return P9DAnchoringConfidence.Exact;
        }

        if (distanceMeters <= config.highConfidenceDistanceMeters)
        {
            return P9DAnchoringConfidence.High;
        }

        if (distanceMeters <= config.mediumConfidenceDistanceMeters)
        {
            return P9DAnchoringConfidence.Medium;
        }

        if (distanceMeters <= config.lowConfidenceDistanceMeters)
        {
            return P9DAnchoringConfidence.Low;
        }

        return P9DAnchoringConfidence.Fallback;
    }

    private static bool IsFinite(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
