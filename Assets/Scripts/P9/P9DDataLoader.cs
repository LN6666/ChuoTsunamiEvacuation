public static class P9DDataLoader
{
    public const string CoordinateAnchoringConfigFileName = "p9d_coordinate_anchoring_config.json";
    public const string AnchoringReportSampleFileName = "p9d_anchoring_report_sample.json";
    public const string FinalGameplayScenarioSampleFileName = "p9d_final_gameplay_scenario_sample.json";
    public const string P10HandoffStatusFileName = "p9d_p10_handoff_status.json";

    public static P9BLoadResult<P9DCoordinateAnchoringConfig> LoadCoordinateAnchoringConfig()
    {
        return P9BDataLoader.LoadFromPath<P9DCoordinateAnchoringConfig>(
            P9BDataLoader.GetP9DataPath(CoordinateAnchoringConfigFileName),
            "P9-D coordinate anchoring config");
    }

    public static P9BLoadResult<P9DAnchoringReport> LoadAnchoringReportSample()
    {
        return P9BDataLoader.LoadFromPath<P9DAnchoringReport>(
            P9BDataLoader.GetP9DataPath(AnchoringReportSampleFileName),
            "P9-D anchoring report sample");
    }

    public static P9BLoadResult<P9DFinalGameplayScenarioCollection> LoadFinalGameplayScenarioSample()
    {
        return P9BDataLoader.LoadFromPath<P9DFinalGameplayScenarioCollection>(
            P9BDataLoader.GetP9DataPath(FinalGameplayScenarioSampleFileName),
            "P9-D final gameplay scenario sample");
    }

    public static P9BLoadResult<P9DP10HandoffStatus> LoadP10HandoffStatus()
    {
        return P9BDataLoader.LoadFromPath<P9DP10HandoffStatus>(
            P9BDataLoader.GetP9DataPath(P10HandoffStatusFileName),
            "P9-D P10 handoff status");
    }
}
