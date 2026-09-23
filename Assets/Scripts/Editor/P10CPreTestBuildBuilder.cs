#if UNITY_EDITOR
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class P10CPreTestBuildBuilder
{
    private const string OutputPathArgument = "-p10cPreBuildOutputPath";
    private const string SummaryPathArgument = "-p10cPreBuildSummaryPath";
    private const string ScenesArgument = "-p10cPreBuildScenes";

    public static void BuildFromCommandLine()
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string outputPath = GetArgumentValue(
            OutputPathArgument,
            @"D:\UnityProjects\ChuoTsunamiEvacuation-Builds\P10CPre\ChuoTsunamiEvacuation_P10CPre.exe");
        string summaryPath = GetArgumentValue(
            SummaryPathArgument,
            Path.Combine(Path.GetDirectoryName(outputPath) ?? projectRoot, "p10c_pre_build_summary.json"));
        string[] scenes = ResolveBuildScenes(GetArgumentValue(ScenesArgument, string.Empty));

        Directory.CreateDirectory(Path.GetDirectoryName(outputPath) ?? projectRoot);
        Directory.CreateDirectory(Path.GetDirectoryName(summaryPath) ?? projectRoot);

        var summary = new P10CPreBuiltPlayerProfileSummary
        {
            status = "build_started",
            buildAttempted = true,
            buildSucceeded = false,
            buildType = "temporary_windows_x64_development_playable_test_build",
            buildOutputPath = outputPath,
            buildScenes = scenes,
            buildStartedAtLocal = DateTime.Now.ToString("s"),
            summary = "Temporary P10-C-Pre Windows x64 playable test build started. This is not the final release build."
        };

        try
        {
            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = outputPath,
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development | BuildOptions.ConnectWithProfiler
            };

            BuildReport report = BuildPipeline.BuildPlayer(options);
            summary.buildFinishedAtLocal = DateTime.Now.ToString("s");
            summary.buildSucceeded = report != null && report.summary.result == BuildResult.Succeeded;
            summary.status = summary.buildSucceeded ? "build_succeeded" : "build_failed";
            summary.summary = BuildSummaryText(report);
            if (summary.buildSucceeded)
            {
                string copiedDataPath = CopyAssetsDataToPlayerData(outputPath);
                summary.summary += " Copied Assets/Data to temporary player data path: " + copiedDataPath + ".";
            }
            if (!summary.buildSucceeded)
            {
                summary.buildFailureReason = report == null ? "BuildPipeline returned null report." : report.summary.result.ToString();
            }

            WriteSummary(summaryPath, summary);
            EditorApplication.Exit(summary.buildSucceeded ? 0 : 1);
        }
        catch (Exception exception)
        {
            summary.status = "build_failed";
            summary.buildFinishedAtLocal = DateTime.Now.ToString("s");
            summary.buildFailureReason = exception.Message;
            summary.summary = "P10-C-Pre temporary build failed. " + exception.Message;
            WriteSummary(summaryPath, summary);
            Debug.LogError(summary.summary);
            EditorApplication.Exit(1);
        }
    }

    private static string[] ResolveBuildScenes(string sceneArgument)
    {
        if (!string.IsNullOrWhiteSpace(sceneArgument))
        {
            return sceneArgument.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
        }

        var enabledScenes = EditorBuildSettings.scenes;
        if (enabledScenes != null && enabledScenes.Length > 0)
        {
            var paths = new System.Collections.Generic.List<string>();
            for (int i = 0; i < enabledScenes.Length; i++)
            {
                if (enabledScenes[i] != null && enabledScenes[i].enabled && !string.IsNullOrWhiteSpace(enabledScenes[i].path))
                {
                    paths.Add(enabledScenes[i].path);
                }
            }

            if (paths.Count > 0)
            {
                return paths.ToArray();
            }
        }

        return new[] { "Assets/Scenes/P7HighDetail/P7_HighDetail_Chuo.unity" };
    }

    private static string BuildSummaryText(BuildReport report)
    {
        if (report == null)
        {
            return "BuildPipeline returned no report.";
        }

        return "P10-C-Pre temporary build result=" + report.summary.result +
               ", totalSizeBytes=" + report.summary.totalSize +
               ", totalTime=" + report.summary.totalTime +
               ", warnings=" + report.summary.totalWarnings +
               ", errors=" + report.summary.totalErrors +
               ".";
    }

    private static string CopyAssetsDataToPlayerData(string outputPath)
    {
        string outputDirectory = Path.GetDirectoryName(outputPath);
        string playerDataDirectory = Path.Combine(
            outputDirectory ?? string.Empty,
            Path.GetFileNameWithoutExtension(outputPath) + "_Data");
        string sourceDirectory = Path.Combine(Application.dataPath, "Data");
        string destinationDirectory = Path.Combine(playerDataDirectory, "Data");
        if (!Directory.Exists(sourceDirectory))
        {
            return destinationDirectory + " (source Assets/Data missing)";
        }

        CopyDirectory(sourceDirectory, destinationDirectory);
        return destinationDirectory;
    }

    private static void CopyDirectory(string sourceDirectory, string destinationDirectory)
    {
        Directory.CreateDirectory(destinationDirectory);
        foreach (string sourceFile in Directory.GetFiles(sourceDirectory))
        {
            if (sourceFile.EndsWith(".meta", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            string destinationFile = Path.Combine(destinationDirectory, Path.GetFileName(sourceFile));
            File.Copy(sourceFile, destinationFile, true);
        }

        foreach (string sourceChildDirectory in Directory.GetDirectories(sourceDirectory))
        {
            string destinationChildDirectory = Path.Combine(destinationDirectory, Path.GetFileName(sourceChildDirectory));
            CopyDirectory(sourceChildDirectory, destinationChildDirectory);
        }
    }

    private static string GetArgumentValue(string name, string fallback)
    {
        string[] args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length - 1; i++)
        {
            if (string.Equals(args[i], name, StringComparison.OrdinalIgnoreCase))
            {
                return args[i + 1];
            }
        }

        return fallback;
    }

    private static void WriteSummary(string path, P10CPreBuiltPlayerProfileSummary summary)
    {
        File.WriteAllText(path, JsonUtility.ToJson(summary, true), new UTF8Encoding(false));
        Debug.Log("P10-C-Pre build summary written to " + path);
    }
}
#endif
