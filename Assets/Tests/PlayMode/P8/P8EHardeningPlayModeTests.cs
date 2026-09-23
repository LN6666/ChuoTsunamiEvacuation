using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class P8EHardeningPlayModeTests
{
    [UnityTest]
    public IEnumerator RiskFrontProgressionModelRunsWithoutSceneDependency()
    {
        yield return null;

        var input = new P8RiskFrontProgressionInput
        {
            simulationTimeSeconds = 30f,
            timeOriginSeconds = 0f,
            arrivalTimeSeconds = 60f,
            hasArrivalTime = true,
            inundationDepthMeters = 0.6f,
            hazardIntensity = 0.5f,
            visualHeightMeters = 1000f,
            maxTsunamiHeightMeters = 3f
        };

        P8RiskFrontProgressionResult result = P8RiskFrontProgressionModel.Evaluate(
            input,
            P8RiskFrontProgressionConfig.Default());

        Assert.IsTrue(result.success, result.summary);
        Assert.IsTrue(result.usedArrivalTime);
        Assert.IsFalse(result.affectsGameplaySuccessFailure);
        Assert.IsFalse(result.implementsP9Gameplay);
    }

    [UnityTest]
    public IEnumerator MarkerReadinessLoadsWithoutChuoBaseMap()
    {
        yield return null;

        string path = Path.Combine(Application.dataPath, "Data", "P8", "humanitarian_candidate_persistent_marker_v1.json");
        P8HumanitarianCandidateMarkerDataSet data = P8HumanitarianCandidateMarkerDataSet.LoadFromFile(path);

        Assert.AreEqual(110, data.records.Length);
        Assert.IsFalse(data.persistentSceneObjectsCreated);
        Assert.IsFalse(data.selectableGameplayEnabled);
        Assert.IsFalse(P8HumanitarianCandidateMarkerStatus.RequiresChuoBaseMap);
        Assert.IsFalse(P8HumanitarianCandidateMarkerStatus.ImplementsP9SelectableGameplay);
        Assert.IsFalse(P8HumanitarianCandidateMarkerStatus.AffectsGameplaySuccessFailure);
    }
}
