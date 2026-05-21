using UnityEngine;
using UnityEngine.UI;

public class P6DPlayableBehaviorScenarioConfig
{
    public string scenarioName = "P6D_GeneratedPlayableBehaviorValidation";
    public Vector3 playerStartPosition = Vector3.zero;
    public Vector3 shelterPosition = new Vector3(0f, 0f, 8f);
    public int npcCount = 4;
    public Vector3 npcSpawnOrigin = Vector3.zero;
    public Vector2 npcSpawnAreaSize = Vector2.zero;
    public float npcMoveSpeed = 8f;
    public float npcArrivalDistance = 0.1f;
    public float walkingSpeedMetersPerSecond = NavigationGuidanceCalculator.DefaultWalkingSpeedMetersPerSecond;
}

public class P6DPlayableBehaviorScenarioContext
{
    public GameObject Root { get; private set; }
    public Transform PlayerTransform { get; private set; }
    public BuildingShelter Shelter { get; private set; }
    public NavigationGuidanceController GuidanceController { get; private set; }
    public NavigationGuidanceDisplay GuidanceDisplay { get; private set; }
    public NpcEvacuationSpawner NpcSpawner { get; private set; }
    public Text TargetText { get; private set; }
    public Text DistanceText { get; private set; }
    public Text EstimatedTimeText { get; private set; }
    public Text StatusText { get; private set; }

    public NpcEvacuationAgent[] NpcAgents
    {
        get { return NpcSpawner != null ? NpcSpawner.SpawnedAgents : new NpcEvacuationAgent[0]; }
    }

    internal P6DPlayableBehaviorScenarioContext(
        GameObject root,
        Transform playerTransform,
        BuildingShelter shelter,
        NavigationGuidanceController guidanceController,
        NavigationGuidanceDisplay guidanceDisplay,
        NpcEvacuationSpawner npcSpawner,
        Text targetText,
        Text distanceText,
        Text estimatedTimeText,
        Text statusText)
    {
        Root = root;
        PlayerTransform = playerTransform;
        Shelter = shelter;
        GuidanceController = guidanceController;
        GuidanceDisplay = guidanceDisplay;
        NpcSpawner = npcSpawner;
        TargetText = targetText;
        DistanceText = distanceText;
        EstimatedTimeText = estimatedTimeText;
        StatusText = statusText;
    }

    public NavigationGuidanceResult RefreshGuidance()
    {
        return GuidanceController != null ? GuidanceController.RefreshGuidance() : null;
    }

    public void TickNpcs(float deltaTime)
    {
        NpcEvacuationAgent[] agents = NpcAgents;
        for (int i = 0; i < agents.Length; i++)
        {
            if (agents[i] != null)
            {
                agents[i].Tick(deltaTime);
            }
        }
    }

    public P6DBehaviorValidationResult Evaluate(
        NavigationGuidanceResult initialGuidance,
        NavigationGuidanceResult finalGuidance,
        bool successFailureStateUntouched)
    {
        return P6DBehaviorValidationRunner.Evaluate(
            initialGuidance,
            finalGuidance,
            NpcAgents,
            successFailureStateUntouched);
    }

    public void DestroyGeneratedObjects()
    {
        if (Root == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            Object.Destroy(Root);
        }
        else
        {
            Object.DestroyImmediate(Root);
        }

        Root = null;
    }
}

public static class P6DPlayableBehaviorScenarioBuilder
{
    public static P6DPlayableBehaviorScenarioContext CreateDefaultScenario()
    {
        return Create(new P6DPlayableBehaviorScenarioConfig());
    }

    public static P6DPlayableBehaviorScenarioContext Create(P6DPlayableBehaviorScenarioConfig config)
    {
        P6DPlayableBehaviorScenarioConfig safeConfig = config ?? new P6DPlayableBehaviorScenarioConfig();
        GameObject root = new GameObject(safeConfig.scenarioName);

        GameObject playerObject = CreateChild(root.transform, "P6D_Player");
        playerObject.transform.position = safeConfig.playerStartPosition;

        BuildingShelter shelter = CreateShelter(root.transform, safeConfig.shelterPosition);
        NavigationGuidanceDisplay display = CreateDisplay(
            root.transform,
            out Text targetText,
            out Text distanceText,
            out Text estimatedTimeText,
            out Text statusText);

        NavigationGuidanceController guidanceController =
            CreateChild(root.transform, "P6D_NavigationGuidanceController").AddComponent<NavigationGuidanceController>();
        guidanceController.SetPlayerTransform(playerObject.transform);
        guidanceController.SetTargetShelter(shelter);
        guidanceController.SetDisplay(display);
        guidanceController.SetWalkingSpeedMetersPerSecond(safeConfig.walkingSpeedMetersPerSecond);

        NpcEvacuationSpawner npcSpawner =
            CreateChild(root.transform, "P6D_NpcSpawner").AddComponent<NpcEvacuationSpawner>();
        npcSpawner.ConfigureForTests(
            safeConfig.npcCount,
            safeConfig.npcSpawnOrigin,
            safeConfig.npcSpawnAreaSize);
        npcSpawner.ConfigureMovementForTests(safeConfig.npcMoveSpeed, safeConfig.npcArrivalDistance);
        npcSpawner.Spawn(new[]
        {
            NpcEvacuationTargetInfo.FromBuildingShelter(shelter)
        });

        return new P6DPlayableBehaviorScenarioContext(
            root,
            playerObject.transform,
            shelter,
            guidanceController,
            display,
            npcSpawner,
            targetText,
            distanceText,
            estimatedTimeText,
            statusText);
    }

    private static BuildingShelter CreateShelter(Transform parent, Vector3 position)
    {
        GameObject shelterObject = CreateChild(parent, "P6D_TestShelter");
        shelterObject.transform.position = position;

        BuildingShelter shelter = shelterObject.AddComponent<BuildingShelter>();
        shelter.ApplyShelterData(new ShelterDataLoader.ShelterData
        {
            shelterId = "p6d_generated_test_shelter",
            shelterName = "P6-D Generated Test Shelter",
            shelterRank = "A",
            isOfficialShelter = true,
            canEnter = true,
            entryDelaySeconds = 0f,
            climbTimeSeconds = 8f,
            crowdingDelaySeconds = 0f,
            sourceType = "test"
        });

        return shelter;
    }

    private static NavigationGuidanceDisplay CreateDisplay(
        Transform parent,
        out Text targetText,
        out Text distanceText,
        out Text estimatedTimeText,
        out Text statusText)
    {
        GameObject displayObject = CreateChild(parent, "P6D_NavigationGuidanceDisplay");
        NavigationGuidanceDisplay display = displayObject.AddComponent<NavigationGuidanceDisplay>();

        RectTransform arrow = CreateChild(displayObject.transform, "P6D_GuidanceArrow", typeof(RectTransform))
            .GetComponent<RectTransform>();
        targetText = CreateText(displayObject.transform, "P6D_TargetText");
        distanceText = CreateText(displayObject.transform, "P6D_DistanceText");
        estimatedTimeText = CreateText(displayObject.transform, "P6D_EstimatedTimeText");
        statusText = CreateText(displayObject.transform, "P6D_StatusText");

        display.BindForTests(arrow, targetText, distanceText, estimatedTimeText, statusText);
        return display;
    }

    private static Text CreateText(Transform parent, string name)
    {
        return CreateChild(parent, name, typeof(RectTransform)).AddComponent<Text>();
    }

    private static GameObject CreateChild(Transform parent, string name, params System.Type[] components)
    {
        GameObject gameObject = components == null || components.Length == 0
            ? new GameObject(name)
            : new GameObject(name, components);
        gameObject.transform.SetParent(parent, false);
        return gameObject;
    }
}
