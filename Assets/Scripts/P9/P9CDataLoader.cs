public static class P9CDataLoader
{
    public const string OutcomeRulesConfigFileName = "p9c_outcome_rules_config.json";
    public const string VerticalEvacuationTargetRulesFileName = "p9c_vertical_evacuation_target_rules.json";
    public const string EntranceCongestionRulesFileName = "p9c_entrance_congestion_rules.json";
    public const string SafeFloorProxyRulesFileName = "p9c_safe_floor_proxy_rules.json";
    public const string CollapseDebrisFatalityConfigFileName = "p9c_collapse_debris_fatality_config.json";
    public const string ScenarioFailurePresetsFileName = "p9c_scenario_failure_presets.json";
    public const string ReasonCodeCatalogFileName = "p9c_reason_code_catalog.json";

    public static P9BLoadResult<P9COutcomeRulesConfig> LoadOutcomeRulesConfig()
    {
        return P9BDataLoader.LoadFromPath<P9COutcomeRulesConfig>(
            P9BDataLoader.GetP9DataPath(OutcomeRulesConfigFileName),
            "P9-C outcome rules config");
    }

    public static P9BLoadResult<P9CVerticalEvacuationTargetRules> LoadVerticalEvacuationTargetRules()
    {
        return P9BDataLoader.LoadFromPath<P9CVerticalEvacuationTargetRules>(
            P9BDataLoader.GetP9DataPath(VerticalEvacuationTargetRulesFileName),
            "P9-C vertical evacuation target rules");
    }

    public static P9BLoadResult<P9CEntranceCongestionRules> LoadEntranceCongestionRules()
    {
        return P9BDataLoader.LoadFromPath<P9CEntranceCongestionRules>(
            P9BDataLoader.GetP9DataPath(EntranceCongestionRulesFileName),
            "P9-C entrance congestion rules");
    }

    public static P9BLoadResult<P9CSafeFloorProxyRules> LoadSafeFloorProxyRules()
    {
        return P9BDataLoader.LoadFromPath<P9CSafeFloorProxyRules>(
            P9BDataLoader.GetP9DataPath(SafeFloorProxyRulesFileName),
            "P9-C safe-floor proxy rules");
    }

    public static P9BLoadResult<P9CCollapseDebrisFatalityConfig> LoadCollapseDebrisFatalityConfig()
    {
        return P9BDataLoader.LoadFromPath<P9CCollapseDebrisFatalityConfig>(
            P9BDataLoader.GetP9DataPath(CollapseDebrisFatalityConfigFileName),
            "P9-C collapse/debris fatality config");
    }

    public static P9BLoadResult<P9CScenarioFailurePresetCollection> LoadScenarioFailurePresets()
    {
        return P9BDataLoader.LoadFromPath<P9CScenarioFailurePresetCollection>(
            P9BDataLoader.GetP9DataPath(ScenarioFailurePresetsFileName),
            "P9-C scenario failure presets");
    }

    public static P9BLoadResult<P9CReasonCodeCatalog> LoadReasonCodeCatalog()
    {
        return P9BDataLoader.LoadFromPath<P9CReasonCodeCatalog>(
            P9BDataLoader.GetP9DataPath(ReasonCodeCatalogFileName),
            "P9-C reason code catalog");
    }
}
