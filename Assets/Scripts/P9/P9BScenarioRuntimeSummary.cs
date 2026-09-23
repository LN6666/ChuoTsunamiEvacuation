using System;

public static class P9BScenarioRuntimeSummary
{
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;

    public static P9BScenarioRuntimeSummaryResult Create(
        P9BWeightedSpawnGenerationResult spawnGeneration,
        P9BEntranceMarkerGenerationResult entranceGeneration,
        P9BHumanitarianMarkerGenerationResult humanitarianGeneration,
        P9BCollapseDebrisRiskZoneGenerationResult collapseGeneration,
        P9BSemanticBindingCollection semanticBindings,
        P9BRouteCandidateGeometryHandoff routeHandoff,
        P9BP8HandoffAvailability handoffAvailability)
    {
        int semanticCount = semanticBindings == null || semanticBindings.bindings == null ? 0 : semanticBindings.bindings.Length;
        bool routesOfficial = routeHandoff != null && routeHandoff.routesAreOfficial;
        bool routesEstimated = routeHandoff != null && routeHandoff.routesAreEstimatedPrototypeGuidance;

        return new P9BScenarioRuntimeSummaryResult
        {
            spawnMarkerCount = spawnGeneration == null ? 0 : spawnGeneration.generatedMarkerCount,
            entranceMarkerCount = entranceGeneration == null ? 0 : entranceGeneration.generatedMarkerCount,
            humanitarianMarkerCount = humanitarianGeneration == null ? 0 : humanitarianGeneration.generatedMarkerCount,
            collapseDebrisMarkerCount = collapseGeneration == null ? 0 : collapseGeneration.generatedZoneCount,
            semanticBindingCount = semanticCount,
            p8HandoffExpectedFilesPresent = handoffAvailability != null && handoffAvailability.allExpectedFilesPresent,
            humanitarianCandidatesRemainNonOfficial = humanitarianGeneration == null || humanitarianGeneration.allCandidatesNonOfficial,
            humanitarianWarningsRequired = humanitarianGeneration == null || humanitarianGeneration.allCandidatesRequireNonOfficialWarning,
            routesAreOfficial = routesOfficial,
            routesAreEstimatedPrototypeGuidance = routesEstimated,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            implementsFinalFailureGameplay = P9RuntimePolicy.ImplementsFinalFailureGameplay,
            summary = "P9-B runtime summary: spawnMarkers=" + (spawnGeneration == null ? 0 : spawnGeneration.generatedMarkerCount) +
                      ", entranceMarkers=" + (entranceGeneration == null ? 0 : entranceGeneration.generatedMarkerCount) +
                      ", humanitarianMarkers=" + (humanitarianGeneration == null ? 0 : humanitarianGeneration.generatedMarkerCount) +
                      ", collapseMarkers=" + (collapseGeneration == null ? 0 : collapseGeneration.generatedZoneCount) +
                      ", semanticBindings=" + semanticCount +
                      ". Runtime prototype only; no final failure gameplay."
        };
    }
}

[Serializable]
public class P9BScenarioRuntimeSummaryResult
{
    public int spawnMarkerCount;
    public int entranceMarkerCount;
    public int humanitarianMarkerCount;
    public int collapseDebrisMarkerCount;
    public int semanticBindingCount;
    public bool p8HandoffExpectedFilesPresent;
    public bool humanitarianCandidatesRemainNonOfficial;
    public bool humanitarianWarningsRequired;
    public bool routesAreOfficial;
    public bool routesAreEstimatedPrototypeGuidance;
    public bool affectsGameplaySuccessFailure;
    public bool implementsFinalFailureGameplay;
    public string summary = string.Empty;
}
