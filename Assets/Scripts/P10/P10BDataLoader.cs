public static class P10BDataLoader
{
    public const string GreenGroundFrameConfigFileName = "p10b_green_ground_frame_config.json";
    public const string GreenGroundFrameTargetsSampleFileName = "p10b_green_ground_frame_targets_sample.json";
    public const string PerformanceMetricsConfigFileName = "p10b_performance_metrics_config.json";
    public const string StressScenariosFileName = "p10b_stress_scenarios.json";
    public const string QualityPresetsFileName = "p10b_quality_presets.json";
    public const string ProfilingReportSampleFileName = "p10b_profiling_report_sample.json";
    public const string ManualPlaytestChecklistFileName = "p10b_manual_playtest_checklist.json";

    public static P9BLoadResult<P10BGreenGroundFrameConfig> LoadGreenGroundFrameConfig()
    {
        return P9BDataLoader.LoadFromPath<P10BGreenGroundFrameConfig>(
            P9BDataPath(GreenGroundFrameConfigFileName),
            "P10-B green ground frame config");
    }

    public static P9BLoadResult<P10BGreenGroundFrameTargetCollection> LoadGreenGroundFrameTargetsSample()
    {
        return P9BDataLoader.LoadFromPath<P10BGreenGroundFrameTargetCollection>(
            P9BDataPath(GreenGroundFrameTargetsSampleFileName),
            "P10-B green ground frame targets sample");
    }

    public static P9BLoadResult<P10BPerformanceMetricsConfig> LoadPerformanceMetricsConfig()
    {
        return P9BDataLoader.LoadFromPath<P10BPerformanceMetricsConfig>(
            P9BDataPath(PerformanceMetricsConfigFileName),
            "P10-B performance metrics config");
    }

    public static P9BLoadResult<P10BStressScenarioCollection> LoadStressScenarios()
    {
        return P9BDataLoader.LoadFromPath<P10BStressScenarioCollection>(
            P9BDataPath(StressScenariosFileName),
            "P10-B stress scenarios");
    }

    public static P9BLoadResult<P10BQualityPresetCollection> LoadQualityPresets()
    {
        return P9BDataLoader.LoadFromPath<P10BQualityPresetCollection>(
            P9BDataPath(QualityPresetsFileName),
            "P10-B quality presets");
    }

    public static P9BLoadResult<P10BProfilingReport> LoadProfilingReportSample()
    {
        return P9BDataLoader.LoadFromPath<P10BProfilingReport>(
            P9BDataPath(ProfilingReportSampleFileName),
            "P10-B profiling report sample");
    }

    public static P9BLoadResult<P10BManualPlaytestChecklist> LoadManualPlaytestChecklist()
    {
        return P9BDataLoader.LoadFromPath<P10BManualPlaytestChecklist>(
            P9BDataPath(ManualPlaytestChecklistFileName),
            "P10-B manual playtest checklist");
    }

    private static string P9BDataPath(string fileName)
    {
        return System.IO.Path.Combine(UnityEngine.Application.dataPath, "Data", "P10", fileName);
    }
}
