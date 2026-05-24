using System;

public static class P9CCollapseDebrisFatalityEvaluator
{
    public const bool ExposureEventLevelProbability = true;
    public const bool FrameLevelRandomDeath = false;

    public static P9CCollapseDebrisFatalityResult Evaluate(
        P9CCollapseDebrisFatalityConfig config,
        P9CCollapseDebrisExposureEvent exposureEvent,
        int fatalEventsAlreadyRecorded)
    {
        config = config ?? new P9CCollapseDebrisFatalityConfig();
        var result = new P9CCollapseDebrisFatalityResult
        {
            exposureEventLevelProbability = ExposureEventLevelProbability,
            frameLevelRandomDeath = FrameLevelRandomDeath,
            configuredFatalityProbability = Clamp01(config.collapseDebrisExposureFatalityProbability),
            maxFatalEventsPerRun = Math.Max(0, config.maxCollapseDebrisFatalEventsPerRun),
            deterministicSeed = config.deterministicSeed,
            reasonCode = P9COutcomeReasonCode.SurvivedCollapseDebrisExposure
        };

        if (!config.enableCollapseDebrisFatalityProxy)
        {
            if (exposureEvent != null && exposureEvent.exposureTriggered)
            {
                result.exposureTriggered = true;
                result.eventId = exposureEvent.eventId ?? string.Empty;
                result.zoneId = exposureEvent.zoneId ?? string.Empty;
            }

            result.reasonCode = P9COutcomeReasonCode.CollapseDebrisProxyDisabled;
            result.summary = "P9-C collapse/debris fatality proxy disabled.";
            return result;
        }

        if (exposureEvent == null || !exposureEvent.exposureTriggered)
        {
            result.reasonCode = P9COutcomeReasonCode.SurvivedCollapseDebrisExposure;
            result.summary = "No collapse/debris exposure event was triggered.";
            return result;
        }

        result.exposureTriggered = true;
        result.eventId = exposureEvent.eventId ?? string.Empty;
        result.zoneId = exposureEvent.zoneId ?? string.Empty;
        if (config.requireDamagedBuildingState && !exposureEvent.damagedBuildingState)
        {
            result.reasonCode = P9COutcomeReasonCode.SurvivedCollapseDebrisExposure;
            result.summary = "Exposure event survived because damaged-building state was not present.";
            return result;
        }

        if (fatalEventsAlreadyRecorded >= result.maxFatalEventsPerRun)
        {
            result.reasonCode = P9COutcomeReasonCode.SurvivedCollapseDebrisExposure;
            result.summary = "Exposure event survived because fatal event cap was already reached.";
            return result;
        }

        result.deterministicRoll = StableUnitRandom(config.deterministicSeed, exposureEvent.GetStableEventHash());
        result.fatality = result.deterministicRoll < result.configuredFatalityProbability;
        result.playerOutcomeMutationApplied = result.fatality;
        result.reasonCode = result.fatality
            ? P9COutcomeReasonCode.KilledByBuildingCollapseProxy
            : P9COutcomeReasonCode.SurvivedCollapseDebrisExposure;
        result.secondaryReasonCode = result.fatality
            ? P9COutcomeReasonCode.CollapseDebrisFatalityProxy
            : P9COutcomeReasonCode.CollapseDebrisExposureEvent;
        result.summary = "P9-C collapse/debris exposure event evaluated once: roll=" +
                         result.deterministicRoll.ToString("0.000") +
                         ", probability=" + result.configuredFatalityProbability.ToString("0.00") +
                         ", fatality=" + result.fatality + ".";
        return result;
    }

    private static float StableUnitRandom(int seed, int eventHash)
    {
        unchecked
        {
            int hash = 17;
            hash = hash * 31 + seed;
            hash = hash * 31 + eventHash;
            var random = new System.Random(hash & int.MaxValue);
            return (float)random.NextDouble();
        }
    }

    private static float Clamp01(float value)
    {
        if (value < 0f)
        {
            return 0f;
        }

        return value > 1f ? 1f : value;
    }
}

[Serializable]
public class P9CCollapseDebrisFatalityConfig
{
    public string schemaVersion = string.Empty;
    public bool enableCollapseDebrisFatalityProxy = true;
    public float collapseDebrisExposureFatalityProbability = 0.35f;
    public int maxCollapseDebrisFatalEventsPerRun = 1;
    public int deterministicSeed = 9735;
    public string riskZoneCategory = "collapse_debris_proxy";
    public float exposureDistanceMeters = 6f;
    public bool requireDamagedBuildingState;
    public bool frameLevelRandomDeath;
    public bool scenarioConfigurable = true;
    public string resultPanelDisclosure = "Gameplay-level collapse/debris proxy, not real structural simulation.";
}

[Serializable]
public class P9CCollapseDebrisFatalityResult
{
    public bool exposureTriggered;
    public bool fatality;
    public bool playerOutcomeMutationApplied;
    public bool exposureEventLevelProbability;
    public bool frameLevelRandomDeath;
    public string eventId = string.Empty;
    public string zoneId = string.Empty;
    public float configuredFatalityProbability;
    public float deterministicRoll;
    public int maxFatalEventsPerRun;
    public int deterministicSeed;
    public string reasonCode = string.Empty;
    public string secondaryReasonCode = string.Empty;
    public string summary = string.Empty;
}
