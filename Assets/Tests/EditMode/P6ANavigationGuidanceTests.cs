using NUnit.Framework;
using UnityEngine;

public class P6ANavigationGuidanceTests
{
    [Test]
    public void DirectionCalculationUsesHorizontalVector()
    {
        Vector3 direction = NavigationGuidanceCalculator.CalculateHorizontalDirection(
            new Vector3(1f, 5f, 1f),
            new Vector3(4f, 25f, 5f));

        Assert.AreEqual(0f, direction.y);
        Assert.AreEqual(0.6f, direction.x, 0.0001f);
        Assert.AreEqual(0.8f, direction.z, 0.0001f);
    }

    [Test]
    public void DistanceCalculationUsesUnityWorldDistance()
    {
        float distance = NavigationGuidanceCalculator.CalculateWorldDistanceMeters(
            Vector3.zero,
            new Vector3(3f, 4f, 12f));

        Assert.AreEqual(13f, distance, 0.0001f);
    }

    [Test]
    public void EstimatedTimeUsesRouteMetadataWhenAvailable()
    {
        var targetInfo = new NavigationTargetInfo
        {
            hasWorldPosition = true,
            worldPosition = Vector3.forward,
            estimatedRouteTimeSeconds = 75f,
            routeDistanceMeters = 1000f
        };

        float estimate = NavigationGuidanceCalculator.CalculateEstimatedTimeSeconds(
            10f,
            targetInfo,
            2f,
            out bool usedRouteMetadata);

        Assert.IsTrue(usedRouteMetadata);
        Assert.AreEqual(75f, estimate);
    }

    [Test]
    public void EstimatedTimeFallsBackToDistanceAndWalkingSpeed()
    {
        float estimate = NavigationGuidanceCalculator.CalculateEstimatedTimeSeconds(
            120f,
            null,
            1.5f,
            out bool usedRouteMetadata);

        Assert.IsFalse(usedRouteMetadata);
        Assert.AreEqual(80f, estimate, 0.0001f);
    }

    [Test]
    public void WarningTextContainsRequiredPrototypeDisclaimersAndAttribution()
    {
        var targetInfo = new NavigationTargetInfo
        {
            routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel,
            osmAttribution = "Route network data from OpenStreetMap contributors; use under the Open Database License."
        };

        string warningText = NavigationGuidanceCalculator.BuildWarningText(targetInfo, true);

        StringAssert.Contains("Estimated prototype route", warningText);
        StringAssert.Contains("Not official navigation", warningText);
        StringAssert.Contains("Not official evacuation guidance", warningText);
        StringAssert.Contains("Route line rendering disabled until coordinate transform is validated", warningText);
        StringAssert.Contains("OSM/ODbL attribution:", warningText);
        StringAssert.Contains("OpenStreetMap", warningText);
        StringAssert.Contains("Open Database License", warningText);
    }

    [Test]
    public void WarningTextCanOmitRouteLineRenderingWarning()
    {
        var targetInfo = new NavigationTargetInfo
        {
            routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel,
            routeStatusNote = "Custom route rendering warning"
        };

        string warningText = NavigationGuidanceCalculator.BuildWarningText(targetInfo, false);

        StringAssert.Contains("Estimated prototype route", warningText);
        StringAssert.Contains("Not official navigation", warningText);
        StringAssert.Contains("Not official evacuation guidance", warningText);
        StringAssert.Contains("estimated prototype route, not an official evacuation route", warningText);
        StringAssert.DoesNotContain("Custom route rendering warning", warningText);
        StringAssert.DoesNotContain("Route line rendering disabled until coordinate transform is validated", warningText);
    }

    [Test]
    public void WarningTextFlagsHumanitarianCandidatesAsNotOfficialDesignatedShelters()
    {
        var targetInfo = new NavigationTargetInfo
        {
            isHumanitarianCandidate = true
        };

        string warningText = NavigationGuidanceCalculator.BuildWarningText(targetInfo, true);

        StringAssert.Contains("Not official navigation", warningText);
        StringAssert.Contains("Not official evacuation guidance", warningText);
        StringAssert.Contains("Humanitarian candidates are not official designated shelters", warningText);
    }

    [Test]
    public void MissingTargetFailsSafeWithoutThrowing()
    {
        NavigationGuidanceResult result = null;

        Assert.DoesNotThrow(() =>
        {
            result = NavigationGuidanceCalculator.BuildGuidance(
                Vector3.zero,
                null,
                NavigationGuidanceCalculator.DefaultWalkingSpeedMetersPerSecond);
        });

        Assert.NotNull(result);
        Assert.IsFalse(result.hasTarget);
        Assert.AreEqual(NavigationGuidanceCalculator.MissingTargetWarning, result.statusText);
        Assert.AreEqual("Distance unavailable", result.distanceText);
        Assert.AreEqual("Estimated time unavailable", result.estimatedTimeText);
    }

    [Test]
    public void WarningTextDoesNotMakeUnsafeOfficialNavigationClaim()
    {
        string warningText = NavigationGuidanceCalculator.BuildWarningText(new NavigationTargetInfo(), true);

        Assert.IsFalse(NavigationGuidanceCalculator.ContainsUnsafeOfficialNavigationClaim(warningText));
        Assert.IsTrue(NavigationGuidanceCalculator.ContainsUnsafeOfficialNavigationClaim("Use this official navigation route."));
        Assert.IsTrue(NavigationGuidanceCalculator.ContainsUnsafeOfficialNavigationClaim("Official evacuation guidance."));
    }

    [Test]
    public void ShelterTargetInfoReadsExistingMetadataReadOnly()
    {
        GameObject shelterObject = new GameObject("P6A_MetadataTarget_Test");
        try
        {
            BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
            shelter.ApplyShelterData(new ShelterDataLoader.ShelterData
            {
                shelterId = "real_target",
                shelterName = "Real Target",
                shelterRank = "Real",
                isOfficialShelter = true,
                canEnter = true,
                climbTimeSeconds = 10f,
                sourceType = RealQualifiedShelterDataLoader.SourceType
            });

            RealQualifiedShelterMetadata metadata = shelterObject.AddComponent<RealQualifiedShelterMetadata>();
            metadata.ApplyRecord(new RealQualifiedShelterDataLoader.RealQualifiedShelterRecord
            {
                gameplayShelterId = "real_target",
                displayName = "Real Metadata Target",
                routeDistanceMeters = 360f,
                estimatedRouteTimeSeconds = 300f,
                routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel,
                attribution = "Route network data from OpenStreetMap contributors; use under the Open Database License."
            });

            Assert.IsTrue(NavigationTargetInfo.TryFromShelter(shelter, out NavigationTargetInfo targetInfo));
            Assert.AreEqual("real_target", targetInfo.targetId);
            Assert.AreEqual("Real Metadata Target", targetInfo.displayName);
            Assert.IsTrue(targetInfo.isOfficialShelter);
            Assert.AreEqual(360f, targetInfo.routeDistanceMeters);
            Assert.AreEqual(300f, targetInfo.estimatedRouteTimeSeconds);
            Assert.That(targetInfo.osmAttribution, Does.Contain("OpenStreetMap"));
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(shelterObject);
        }
    }
}
