using System.Linq;
using NUnit.Framework;

public class P9DCoordinateAnchoringEditModeTests
{
    [Test]
    public void CoordinateAnchoringConfigLoadsWithProxyPolicy()
    {
        P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
        P9DAnchoringReport sample = P9DDataLoader.LoadAnchoringReportSample().data;

        Assert.IsTrue(config.IsProxyPolicySafe());
        Assert.IsTrue(config.coordinateBasedProxyRequired);
        Assert.IsTrue(config.nearestMatchAllowed);
        Assert.IsFalse(config.claimsOfficialRouteStatus);
        Assert.IsFalse(config.claimsExactPlateauObjectBinding);
        Assert.IsFalse(config.exactPlateauObjectIdentityProofAvailable);
        Assert.AreEqual(110, sample.humanitarianCandidateTotal);
        Assert.IsFalse(sample.exactPlateauObjectIdentityClaimed);
    }

    [Test]
    public void ValidLatLonCoordinateCreatesCoordinateProxyAnchor()
    {
        P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
        P9DCoordinateAnchorRecord official = config.anchors.First(anchor => anchor.anchorType == "official_shelter_marker");

        P9DCoordinateAnchoringResult result = P9DNearestAnchorMatcher.AnchorCoordinate(official, config);

        Assert.IsTrue(result.success, result.summary);
        Assert.IsTrue(result.coordinateValid);
        Assert.AreEqual(P9DAnchoringTokens.AnchoredToCoordinate, result.anchoringStatus);
        Assert.AreEqual("high", result.confidence);
        Assert.IsTrue(result.isOfficialShelter);
        Assert.IsFalse(result.isHumanitarianCandidate);
        Assert.IsFalse(result.exactPlateauObjectIdentityProven);
    }

    [Test]
    public void InvalidCoordinateIsRejected()
    {
        P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
        var record = new P9DCoordinateAnchorRecord
        {
            anchorId = "p9d_invalid_coordinate",
            hasCoordinate = true,
            latitude = float.NaN,
            longitude = 139.77f
        };

        P9DCoordinateAnchoringResult result = P9DNearestAnchorMatcher.AnchorCoordinate(record, config);

        Assert.IsFalse(result.success);
        Assert.AreEqual(P9DAnchoringTokens.RejectedInvalidCoordinate, result.anchoringStatus);
        Assert.AreEqual("rejected", result.confidence);
    }

    [Test]
    public void NearestMatchConfidenceIsDeterministic()
    {
        P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
        var source = new P9DCoordinateAnchorRecord
        {
            anchorId = "p9d_source_for_nearest",
            anchorType = "humanitarian_candidate_marker",
            hasCoordinate = true,
            latitude = 35.66662f,
            longitude = 139.77842f,
            isHumanitarianCandidate = true,
            nonOfficialWarningRequired = true
        };
        var candidates = new[]
        {
            new P9DCoordinateAnchorRecord
            {
                anchorId = "p9d_building_near",
                hasCoordinate = true,
                latitude = 35.66664f,
                longitude = 139.77843f
            },
            new P9DCoordinateAnchorRecord
            {
                anchorId = "p9d_building_farther",
                hasCoordinate = true,
                latitude = 35.6674f,
                longitude = 139.7792f
            }
        };

        P9DCoordinateAnchoringResult first = P9DNearestAnchorMatcher.MatchNearest(
            source,
            candidates,
            config,
            P9DAnchoringTokens.AnchoredToNearestBuildingProxy);
        P9DCoordinateAnchoringResult second = P9DNearestAnchorMatcher.MatchNearest(
            source,
            candidates,
            config,
            P9DAnchoringTokens.AnchoredToNearestBuildingProxy);

        Assert.IsTrue(first.success, first.summary);
        Assert.AreEqual(first.matchedAnchorId, second.matchedAnchorId);
        Assert.AreEqual(first.matchedDistanceMeters, second.matchedDistanceMeters, 0.001f);
        Assert.AreEqual("p9d_building_near", first.matchedAnchorId);
        Assert.AreEqual("high", first.confidence);
        Assert.IsTrue(first.nearestMatchUsed);
        Assert.IsFalse(first.exactPlateauObjectIdentityProven);
    }

    [Test]
    public void DistanceThresholdRejectsFarNearestMatch()
    {
        P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
        var source = new P9DCoordinateAnchorRecord
        {
            anchorId = "p9d_source_far_match",
            hasCoordinate = true,
            latitude = 35.65f,
            longitude = 139.75f
        };
        var candidates = new[]
        {
            new P9DCoordinateAnchorRecord
            {
                anchorId = "p9d_too_far_match",
                hasCoordinate = true,
                latitude = 35.70f,
                longitude = 139.81f
            }
        };

        P9DCoordinateAnchoringResult result = P9DNearestAnchorMatcher.MatchNearest(
            source,
            candidates,
            config,
            P9DAnchoringTokens.AnchoredToNearestBuildingProxy);

        Assert.IsFalse(result.success);
        Assert.AreEqual(P9DAnchoringTokens.RejectedDistanceThresholdExceeded, result.anchoringStatus);
    }

    [Test]
    public void AllP8HumanitarianCandidatesCanAnchorAndPreserveWarning()
    {
        P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
        P9BHumanitarianCandidateMarkerCollection candidates = P9BDataLoader.LoadP8HumanitarianCandidateMarkers().data;

        P9DAnchoringReport report = P9DNearestAnchorMatcher.BuildFinalAnchoringReport(config, candidates);

        Assert.AreEqual(110, report.humanitarianCandidateTotal);
        Assert.AreEqual(110, report.humanitarianCandidateAnchored);
        Assert.AreEqual(110, report.humanitarianCandidateWarningRequiredCount);
        Assert.AreEqual(28, report.namedHumanitarianCandidateCount);
        Assert.AreEqual(82, report.idOnlyHumanitarianCandidateCount);
        Assert.IsTrue(report.allHumanitarianCandidatesRemainNonOfficial);
        Assert.IsTrue(report.allHumanitarianCandidatesRequireWarning);
        Assert.IsFalse(report.exactPlateauObjectIdentityClaimed);
    }

    [Test]
    public void OfficialShelterAndRouteProxySemanticsRemainSeparated()
    {
        P9DCoordinateAnchoringConfig config = P9DDataLoader.LoadCoordinateAnchoringConfig().data;
        P9DCoordinateAnchorRecord official = config.anchors.First(anchor => anchor.anchorType == "official_shelter_marker");
        P9DCoordinateAnchorRecord route = config.anchors.First(anchor => anchor.anchorType == "route_guidance_proxy");
        P9DCoordinateAnchorRecord humanitarianEntrance = config.anchors.First(anchor => anchor.anchorType == "entrance_proxy");

        P9DCoordinateAnchoringResult officialResult = P9DNearestAnchorMatcher.AnchorCoordinate(official, config);
        P9DCoordinateAnchoringResult routeResult = P9DNearestAnchorMatcher.AnchorCoordinate(route, config);
        P9DCoordinateAnchoringResult humanitarianResult = P9DNearestAnchorMatcher.AnchorCoordinate(humanitarianEntrance, config);

        Assert.IsTrue(officialResult.isOfficialShelter);
        Assert.IsFalse(officialResult.nonOfficialWarningRequired);
        Assert.IsFalse(routeResult.routeIsOfficial);
        Assert.IsTrue(routeResult.routeEstimatedPrototypeGuidance);
        Assert.AreEqual(P9DAnchoringTokens.AnchoredToNearestRoadProxy, routeResult.anchoringStatus);
        Assert.IsFalse(humanitarianResult.isOfficialShelter);
        Assert.IsTrue(humanitarianResult.isHumanitarianCandidate);
        Assert.IsTrue(humanitarianResult.nonOfficialWarningRequired);
        Assert.IsTrue(humanitarianEntrance.IsHumanitarianWarningSafe());
    }
}
