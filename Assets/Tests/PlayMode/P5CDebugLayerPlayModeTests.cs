using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P5CDebugLayerPlayModeTests
{
    private GameObject gameplayRoot;
    private GameObject testGround;
    private GameObject generatorObject;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        DestroyIfPresent(generatorObject);
        DestroyIfPresent(gameplayRoot);
        DestroyIfPresent(testGround);
        yield return null;
    }

    [UnityTest]
    public IEnumerator P5CQualificationOverlayCreatesColliderFreeLimitedMarkers()
    {
        gameplayRoot = new GameObject("GameplayTestRoot");
        testGround = new GameObject("TestGround");
        testGround.transform.position = new Vector3(0f, 80f, -250f);

        P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();
        Assert.IsTrue(bundle.HasCoreQualificationData);

        generatorObject = new GameObject("P5C_PlayMode_Generator");
        P5CQualificationOverlayRuntimeGenerator generator =
            generatorObject.AddComponent<P5CQualificationOverlayRuntimeGenerator>();

        int generatedCount = generator.Generate(bundle);
        yield return null;

        Assert.Greater(generatedCount, 0);
        Assert.LessOrEqual(generatedCount, 12);

        Transform overlayRoot = gameplayRoot.transform.Find("P5C_QualificationOverlay_Runtime");
        Assert.NotNull(overlayRoot);
        Assert.AreEqual(0, overlayRoot.GetComponentsInChildren<Collider>().Length);
        Assert.IsFalse(P5CQualificationOverlayRuntimeGenerator.AffectsGameplayRules);
        Assert.AreEqual(0, generator.LastGeneratedRouteLineCount);
        Assert.IsTrue(ContainsLabel(overlayRoot.GetComponentsInChildren<TextMesh>(), "official_confirmed"));
        Assert.IsTrue(ContainsLabel(overlayRoot.GetComponentsInChildren<TextMesh>(), "Route:"));

        generator.ToggleDetailedMetadata();
        yield return null;

        Assert.AreEqual(0, generator.LastGeneratedRouteLineCount);
        TextMesh[] detailedLabels = overlayRoot.GetComponentsInChildren<TextMesh>();
        Assert.IsTrue(ContainsLabel(detailedLabels, P5CStaticDataLoader.EstimatedPrototypeRouteLabel));
        Assert.IsTrue(ContainsLabel(detailedLabels, "OSM/ODbL attribution applies."));
        Assert.IsTrue(ContainsLabel(detailedLabels, "Unity line rendering disabled"));
    }

    [UnityTest]
    public IEnumerator ResultMetricsCanDisplayP5CFeedbackWithoutChangingOutcome()
    {
        P5CStaticDataLoader.P5CDataBundle bundle = P5CStaticDataLoader.LoadBundleFromAssetsData();
        string shelterId = bundle.integratedRouteQualifications.records[0].shelterId;

        ResultMetrics metrics = new ResultMetrics
        {
            success = true,
            selectedShelterId = shelterId,
            selectedShelterName = "P5-C Test Shelter",
            selectedShelterSourceType = P5CStaticDataLoader.P5CSourceType,
            shelterRank = "Real",
            isOfficialShelter = true,
            evacuationCountdownSeconds = 60f,
            p5cDecisionFeedback = P5CDecisionFeedbackFormatter.BuildFromBundle(bundle, shelterId, true)
        };

        string detailText = metrics.GetDetailText();

        StringAssert.Contains("- Result: Success", detailText);
        StringAssert.Contains("P5-C evidence", detailText);
        StringAssert.Contains("Evidence qualification:", detailText);
        StringAssert.Contains(P5CStaticDataLoader.EstimatedPrototypeRouteLabel, detailText);
        Assert.IsTrue(metrics.success);

        yield return null;
    }

    private static bool ContainsLabel(TextMesh[] labels, string expectedText)
    {
        foreach (TextMesh label in labels)
        {
            if (label != null && label.text.Contains(expectedText))
            {
                return true;
            }
        }

        return false;
    }

    private static void DestroyIfPresent(GameObject gameObject)
    {
        if (gameObject != null)
        {
            Object.Destroy(gameObject);
        }
    }
}
