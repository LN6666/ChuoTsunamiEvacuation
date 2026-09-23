using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;

public class P9BProxyBoundaryEditModeTests
{
    [Test]
    public void EntranceSafeFloorProxyStatesLoadWithoutFinalOutcomeMutation()
    {
        P9EntranceSafeFloorProxyCollection collection = P9BDataLoader.LoadEntranceMarkerAssignments().data;

        Assert.AreEqual(4, collection.proxies.Length);
        Assert.IsTrue(collection.proxies.Where(proxy => proxy.humanitarianCandidateFlag)
            .All(proxy => !proxy.isOfficialShelter && proxy.nonOfficialWarningRequired));
        Assert.IsTrue(collection.proxies.Any(proxy => proxy.verticalEvacuationStatus == "blocked_proxy"));
        Assert.IsFalse(P9EntranceSafeFloorMarkerRuntime.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P9EntranceSafeFloorMarkerRuntime.ImplementsFinalFailureGameplay);
        Assert.IsFalse(P9EntranceSafeFloorMarkerRuntime.ImplementsIndoorSceneGameplay);
    }

    [Test]
    public void CollapseDebrisRiskZonesAreMarkerOnly()
    {
        P9BCollapseDebrisRiskZoneCollection collection = P9BDataLoader.LoadCollapseDebrisRiskZones().data;
        P9BCollapseDebrisRiskZoneRecord record = collection.zones[0];
        var gameObject = new GameObject("P9B_EditMode_CollapseZone");

        try
        {
            P9CollapseDebrisRiskZone zone = gameObject.AddComponent<P9CollapseDebrisRiskZone>();
            zone.ApplyRecord(record);
            P9CollapseDebrisExposureResult exposure = zone.CalculateExposure(record.position.ToVector3(), 1234);

            Assert.IsTrue(exposure.inZone);
            Assert.IsFalse(exposure.canKillPlayer);
            Assert.IsFalse(exposure.playerOutcomeMutationApplied);
            Assert.IsFalse(exposure.affectsGameplaySuccessFailure);
            Assert.IsFalse(P9CollapseDebrisRiskZone.CanCausePlayerFailureInP9B);
        }
        finally
        {
            Object.DestroyImmediate(gameObject);
        }
    }

    [Test]
    public void P5RouteCandidateGeometryRemainsEstimatedAndNotOfficial()
    {
        P9BRouteCandidateGeometryHandoff handoff = P9BDataLoader.LoadP8RouteCandidateGeometryHandoff().data;

        Assert.IsFalse(handoff.routesAreOfficial);
        Assert.IsTrue(handoff.routesAreEstimatedPrototypeGuidance);
        Assert.IsFalse(handoff.routeRoadGeometryValidated);
        Assert.IsFalse(handoff.routeCoordinateTransformGeometryValidated);
        Assert.IsTrue(handoff.candidatesProxyOrDataBound);
        Assert.IsFalse(P9RouteGuidanceProxyMarker.RoutesAreOfficial);
    }

    [Test]
    public void SemanticBindingIsProxyUsableButNotFullSceneCoverage()
    {
        P9BSemanticBindingCollection bindings = P9BDataLoader.LoadP8SemanticBindings().data;

        Assert.IsFalse(bindings.completeSceneSemanticBinding);
        Assert.IsFalse(bindings.sceneMutationPerformed);
        Assert.GreaterOrEqual(bindings.bindings.Length, 6);
        Assert.IsTrue(bindings.bindings.All(binding => binding.isProxy));
        Assert.IsTrue(bindings.bindings.Any(binding => binding.category == "entrance" && binding.p9Usable));
    }

    [Test]
    public void P9BSourceDoesNotReferenceOutcomeControllersOrIndoorTemplates()
    {
        Assert.IsFalse(P9RuntimePolicy.AffectsGameplaySuccessFailure);
        Assert.IsFalse(P9RuntimePolicy.CanCausePlayerFailureInP9B);
        Assert.IsFalse(P9RuntimePolicy.ImplementsFinalFailureGameplay);
        Assert.IsFalse(P9RuntimePolicy.ImplementsIndoorSceneGameplay);

        string sourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "Assets", "Scripts", "P9");
        string combinedSource = string.Join("\n", Directory.GetFiles(sourceDirectory, "*.cs").Select(File.ReadAllText));

        Assert.IsFalse(combinedSource.Contains("EvacuationGameManager"));
        Assert.IsFalse(combinedSource.Contains("ResultPanelController"));
        Assert.IsFalse(combinedSource.Contains("ShelterEntranceTrigger"));
        Assert.IsFalse(combinedSource.Contains("BuildingShelter"));
        Assert.IsFalse(combinedSource.ToLowerInvariant().Contains("interior template"));
        Assert.IsFalse(combinedSource.ToLowerInvariant().Contains("bim indoor"));
    }
}
