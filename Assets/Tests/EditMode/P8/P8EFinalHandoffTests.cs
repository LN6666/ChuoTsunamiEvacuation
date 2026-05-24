using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;

public class P8EFinalHandoffTests
{
    [Test]
    public void P8EHumanitarianPersistentVisibilityHandoffJsonLoads()
    {
        P8EHumanitarianPersistentVisibilityHandoff handoff = LoadHandoff();

        Assert.AreEqual("p8e_humanitarian_candidate_persistent_visibility_handoff_v1", handoff.datasetId);
        Assert.IsTrue(handoff.dataOnly);
        Assert.IsFalse(handoff.persistentSceneObjectsCreatedInP8E);
        Assert.IsFalse(handoff.isOfficialShelterDataset);
        Assert.IsTrue(handoff.candidatesRemainNonOfficial);
        Assert.IsFalse(handoff.isOfficialShelter);
        Assert.IsTrue(handoff.nonOfficialWarningRequired);
        Assert.IsFalse(handoff.selectableGameplayEnabledInP8E);
        Assert.IsFalse(handoff.implementsP9Gameplay);
        Assert.IsFalse(handoff.affectsGameplaySuccessFailure);
        Assert.AreEqual(110, handoff.candidateTotals.totalCandidates);
        Assert.AreEqual(28, handoff.candidateTotals.namedCandidates);
        Assert.AreEqual(82, handoff.candidateTotals.idOnlyOrUnknownNameCandidates);
        StringAssert.Contains("Not an official evacuation shelter", handoff.requiredVisibleLabel);
    }

    [Test]
    public void HumanitarianCandidateAuditRemainsNonOfficialForP8EHandoff()
    {
        string path = Path.Combine(Application.dataPath, "Data", "P8", "humanitarian_highrise_candidate_audit_v1.json");
        P8EHumanitarianCandidateAudit audit =
            JsonUtility.FromJson<P8EHumanitarianCandidateAudit>(File.ReadAllText(path));

        Assert.IsFalse(audit.isOfficialShelterDataset);
        Assert.IsTrue(audit.nonOfficialWarningRequired);
        Assert.IsFalse(audit.affectsGameplaySuccessFailure);
        Assert.IsFalse(audit.implementsP9SelectableGameplay);
        Assert.AreEqual(110, audit.records.Length);
        Assert.IsFalse(audit.records.Any(record => record.isOfficialShelter));
        Assert.IsTrue(audit.records.All(record => record.nonOfficialWarningRequired));
        Assert.IsTrue(audit.records.All(record => record.manualReviewNeeded || !string.IsNullOrWhiteSpace(record.evidenceStatus)));
    }

    [Test]
    public void P8StagePlanAllowsOnlyAThroughE()
    {
        string path = Path.Combine(Application.dataPath, "..", "docs", "P8_STAGE_PLAN.md");
        string text = File.ReadAllText(path);
        MatchCollection matches = Regex.Matches(text, @"^##\s+(P8-[A-Z0-9]+)\s*$", RegexOptions.Multiline);
        string[] stages = matches.Cast<Match>().Select(match => match.Groups[1].Value).ToArray();

        CollectionAssert.AreEqual(new[] { "P8-A", "P8-B", "P8-C", "P8-D", "P8-E" }, stages);
        Assert.IsFalse(stages.Contains("P8-F"));
        Assert.IsFalse(stages.Contains("P8-G"));
        Assert.IsFalse(stages.Contains("P8-0"));
    }

    [Test]
    public void P9HandoffDocsReferenceRequiredP8Artifacts()
    {
        string packagePath = Path.Combine(Application.dataPath, "..", "docs", "P8E_P9_HANDOFF_PACKAGE.md");
        string requiredInputsPath = Path.Combine(Application.dataPath, "..", "docs", "P8E_P9_REQUIRED_INPUTS.md");
        string packageText = File.ReadAllText(packagePath);
        string inputsText = File.ReadAllText(requiredInputsPath);

        foreach (string fragment in new[]
        {
            "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity",
            "Assets/Data/P8/tsunami_hazard_layer_v1_chuo.json",
            "Assets/Data/P8/infrastructure_hazard_interaction_config.json",
            "Assets/Data/P8/infrastructure_damage_proxy_config.json",
            "Assets/Data/P8/humanitarian_highrise_candidate_audit_v1.json",
            "Assets/Data/P8/humanitarian_candidate_persistent_visibility_handoff.json"
        })
        {
            StringAssert.Contains(fragment, packageText + inputsText);
        }

        StringAssert.Contains("P9 should not redo", packageText);
        StringAssert.Contains("entrance / safe-floor / evacuation-complete", inputsText);
    }

    [Test]
    public void P8EHandoffDoesNotEnableP9GameplayOrSuccessFailureRules()
    {
        P8EHumanitarianPersistentVisibilityHandoff handoff = LoadHandoff();

        Assert.IsFalse(handoff.implementsP9Gameplay);
        Assert.IsFalse(handoff.selectableGameplayEnabledInP8E);
        Assert.IsFalse(handoff.affectsGameplaySuccessFailure);
        Assert.IsFalse(handoff.realIndoorSceneGameplayAvailable);
        Assert.IsTrue(handoff.p9DecisionRequired);
        Assert.IsTrue(handoff.p9MustKeepCandidatesNonOfficial);
    }

    private static P8EHumanitarianPersistentVisibilityHandoff LoadHandoff()
    {
        string path = Path.Combine(Application.dataPath, "Data", "P8", "humanitarian_candidate_persistent_visibility_handoff.json");
        Assert.IsTrue(File.Exists(path), path);
        return JsonUtility.FromJson<P8EHumanitarianPersistentVisibilityHandoff>(File.ReadAllText(path));
    }

    [Serializable]
    private class P8EHumanitarianPersistentVisibilityHandoff
    {
        public string datasetId;
        public P8EHumanitarianCandidateTotals candidateTotals;
        public bool dataOnly;
        public bool persistentSceneObjectsCreatedInP8E;
        public bool isOfficialShelterDataset;
        public bool candidatesRemainNonOfficial;
        public bool isOfficialShelter;
        public bool nonOfficialWarningRequired;
        public bool selectableGameplayEnabledInP8E;
        public bool implementsP9Gameplay;
        public bool affectsGameplaySuccessFailure;
        public bool realIndoorSceneGameplayAvailable;
        public bool p9DecisionRequired;
        public bool p9MustKeepCandidatesNonOfficial;
        public string requiredVisibleLabel;
    }

    [Serializable]
    private class P8EHumanitarianCandidateTotals
    {
        public int totalCandidates;
        public int namedCandidates;
        public int idOnlyOrUnknownNameCandidates;
    }

    [Serializable]
    private class P8EHumanitarianCandidateAudit
    {
        public bool isOfficialShelterDataset;
        public bool nonOfficialWarningRequired;
        public bool affectsGameplaySuccessFailure;
        public bool implementsP9SelectableGameplay;
        public P8EHumanitarianCandidateRecord[] records;
    }

    [Serializable]
    private class P8EHumanitarianCandidateRecord
    {
        public string evidenceStatus;
        public bool isOfficialShelter;
        public bool nonOfficialWarningRequired;
        public bool manualReviewNeeded;
    }
}
