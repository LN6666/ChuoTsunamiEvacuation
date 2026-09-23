using System;
using UnityEngine;

public class P9SceneRuntimeBootstrap : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;
    public const bool RequiresChuoBaseMap = P9RuntimePolicy.RequiresChuoBaseMap;
    public const bool RequiresP7HighDetailScene = P9RuntimePolicy.RequiresP7HighDetailScene;

    [SerializeField] private bool generateOnStart;
    [SerializeField] private int requestedSpawnCount = 12;
    [SerializeField] private int humanitarianMarkerLimit;

    private Transform runtimeRoot;
    private P9BScenarioRuntimeSummaryResult lastSummary;

    public Transform RuntimeRoot => runtimeRoot;
    public P9BScenarioRuntimeSummaryResult LastSummary => lastSummary;

    private void Start()
    {
        if (generateOnStart)
        {
            GenerateRuntimePrototype();
        }
    }

    public P9BScenarioRuntimeSummaryResult GenerateRuntimePrototype()
    {
        runtimeRoot = EnsureRuntimeRoot();

        P9BLoadResult<P9BWeightedSpawnConfig> spawnConfig = P9BDataLoader.LoadWeightedSpawnConfig();
        P9BLoadResult<P9BWeightedSpawnZoneCollection> spawnZones = P9BDataLoader.LoadSpawnZones();
        P9BLoadResult<P9EntranceSafeFloorProxyCollection> entranceAssignments = P9BDataLoader.LoadEntranceMarkerAssignments();
        P9BLoadResult<P9BHumanitarianCandidateMarkerCollection> humanitarianCandidates = P9BDataLoader.LoadP8HumanitarianCandidateMarkers();
        P9BLoadResult<P9BCollapseDebrisRiskZoneCollection> collapseZones = P9BDataLoader.LoadCollapseDebrisRiskZones();
        P9BLoadResult<P9BSemanticBindingCollection> semanticBindings = P9BDataLoader.LoadP8SemanticBindings();
        P9BLoadResult<P9BRouteCandidateGeometryHandoff> routeHandoff = P9BDataLoader.LoadP8RouteCandidateGeometryHandoff();

        P9BWeightedSpawnGenerationResult spawnGeneration = null;
        if (spawnConfig.success && spawnZones.success)
        {
            var generator = gameObject.GetComponent<P9SpawnRuntimeGenerator>();
            if (generator == null)
            {
                generator = gameObject.AddComponent<P9SpawnRuntimeGenerator>();
            }

            spawnGeneration = generator.Generate(spawnZones.data, spawnConfig.data, requestedSpawnCount, runtimeRoot);
        }

        P9BEntranceMarkerGenerationResult entranceGeneration = entranceAssignments.success
            ? P9EntranceSafeFloorMarkerRuntime.GenerateMarkers(entranceAssignments.data, runtimeRoot)
            : new P9BEntranceMarkerGenerationResult();

        P9BHumanitarianMarkerGenerationResult humanitarianGeneration = humanitarianCandidates.success
            ? P9HumanitarianCandidateMarkerRuntime.GenerateMarkers(humanitarianCandidates.data, runtimeRoot, humanitarianMarkerLimit)
            : new P9BHumanitarianMarkerGenerationResult();

        P9BCollapseDebrisRiskZoneGenerationResult collapseGeneration = collapseZones.success
            ? P9CollapseDebrisRiskZone.GenerateMarkers(collapseZones.data, runtimeRoot)
            : new P9BCollapseDebrisRiskZoneGenerationResult();

        lastSummary = P9BScenarioRuntimeSummary.Create(
            spawnGeneration,
            entranceGeneration,
            humanitarianGeneration,
            collapseGeneration,
            semanticBindings.success ? semanticBindings.data : null,
            routeHandoff.success ? routeHandoff.data : null,
            P9BDataLoader.VerifyP8HandoffAvailability());

        return lastSummary;
    }

    private Transform EnsureRuntimeRoot()
    {
        if (runtimeRoot != null)
        {
            return runtimeRoot;
        }

        var rootObject = new GameObject("P9B_RuntimeRoot");
        rootObject.transform.SetParent(transform, false);
        runtimeRoot = rootObject.transform;
        return runtimeRoot;
    }
}
