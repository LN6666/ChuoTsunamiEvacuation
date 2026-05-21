using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;

public class NpcEvacuationPrototypeTests
{
    [Test]
    public void TargetScoringPrefersCloserSuitableTarget()
    {
        var targets = new List<NpcEvacuationTargetInfo>
        {
            CreateTarget("far", new Vector3(20f, 0f, 0f), true, true),
            CreateTarget("near", new Vector3(4f, 0f, 0f), true, true)
        };

        bool selected = NpcTargetScorer.TrySelectBestTarget(
            Vector3.zero,
            targets,
            out NpcEvacuationTargetInfo target,
            out float score);

        Assert.IsTrue(selected);
        Assert.AreEqual("near", target.targetId);
        Assert.Less(score, 10f);
    }

    [Test]
    public void BlockedTargetIsSkippedWhenAvailableTargetExists()
    {
        var targets = new List<NpcEvacuationTargetInfo>
        {
            CreateTarget("blocked-near", new Vector3(1f, 0f, 0f), false, true),
            CreateTarget("available-far", new Vector3(12f, 0f, 0f), true, true)
        };

        bool selected = NpcTargetScorer.TrySelectBestTarget(
            Vector3.zero,
            targets,
            out NpcEvacuationTargetInfo target,
            out _);

        Assert.IsTrue(selected);
        Assert.AreEqual("available-far", target.targetId);
    }

    [Test]
    public void HumanitarianManualReviewAndWarningPenaltyCanOutweighDistance()
    {
        NpcEvacuationTargetInfo officialTarget = CreateTarget("official", new Vector3(10f, 0f, 0f), true, true);
        NpcEvacuationTargetInfo humanitarianTarget = CreateTarget("humanitarian", new Vector3(1f, 0f, 0f), true, false);
        humanitarianTarget.isHumanitarianCandidate = true;
        humanitarianTarget.manualReviewNeeded = true;
        humanitarianTarget.warningCount = 2;

        var targets = new List<NpcEvacuationTargetInfo>
        {
            humanitarianTarget,
            officialTarget
        };

        bool selected = NpcTargetScorer.TrySelectBestTarget(
            Vector3.zero,
            targets,
            out NpcEvacuationTargetInfo target,
            out _);

        Assert.IsTrue(selected);
        Assert.AreEqual("official", target.targetId);
    }

    [Test]
    public void NoTargetCaseReturnsFailSafeState()
    {
        GameObject npcObject = new GameObject("P6B_EditMode_NoTargetNpc");
        try
        {
            NpcEvacuationAgent agent = npcObject.AddComponent<NpcEvacuationAgent>();
            agent.ConfigureTargets(new List<NpcEvacuationTargetInfo>(), true);

            Assert.AreEqual(NpcEvacuationState.FailedNoTarget, agent.CurrentState);
            Assert.IsNull(agent.CurrentTarget);
        }
        finally
        {
            Object.DestroyImmediate(npcObject);
        }
    }

    [Test]
    public void AgentStateTransitionsAreDeterministicWithManualTicks()
    {
        GameObject npcObject = new GameObject("P6B_EditMode_MovingNpc");
        try
        {
            NpcEvacuationAgent agent = npcObject.AddComponent<NpcEvacuationAgent>();
            agent.ConfigureMovement(1f, 0.05f);
            agent.ConfigureTargets(new[]
            {
                CreateTarget("target", new Vector3(1f, 0f, 0f), true, true)
            }, true);

            Assert.AreEqual(NpcEvacuationState.MovingToTarget, agent.CurrentState);

            agent.Tick(0.5f);
            Assert.AreEqual(NpcEvacuationState.MovingToTarget, agent.CurrentState);
            Assert.AreEqual(0.5f, npcObject.transform.position.x, 0.001f);

            agent.Tick(0.5f);
            Assert.AreEqual(NpcEvacuationState.Arrived, agent.CurrentState);
            Assert.AreEqual(1f, npcObject.transform.position.x, 0.001f);
        }
        finally
        {
            Object.DestroyImmediate(npcObject);
        }
    }

    [Test]
    public void NpcScriptsDoNotReferencePlayerSuccessFailureApis()
    {
        string npcDirectory = Path.Combine(Application.dataPath, "Scripts", "NPC");
        string[] npcScripts = Directory.GetFiles(npcDirectory, "*.cs");
        string combinedSource = string.Empty;

        foreach (string npcScript in npcScripts)
        {
            combinedSource += File.ReadAllText(npcScript);
        }

        Assert.IsFalse(combinedSource.Contains("EvacuationGameManager"));
        Assert.IsFalse(combinedSource.Contains("ResultMetrics"));
        Assert.IsFalse(combinedSource.Contains("ResultPanelController"));
        Assert.IsFalse(combinedSource.Contains("CompleteSuccess"));
        Assert.IsFalse(combinedSource.Contains("CompleteFailure"));
        Assert.IsFalse(NpcEvacuationAgent.AffectsPlayerSuccessFailure);
        Assert.IsFalse(NpcEvacuationSpawner.AffectsPlayerSuccessFailure);
    }

    private static NpcEvacuationTargetInfo CreateTarget(
        string id,
        Vector3 position,
        bool canEnter,
        bool isOfficial)
    {
        return new NpcEvacuationTargetInfo
        {
            targetId = id,
            displayName = id,
            position = position,
            canEnter = canEnter,
            isSelectable = canEnter,
            postEarthquakeStatus = canEnter ? "usable" : "blocked",
            isOfficialShelter = isOfficial,
            entryDelaySeconds = 0f,
            climbTimeSeconds = 0f,
            crowdingDelaySeconds = 0f
        };
    }
}
