using System.IO;
using NUnit.Framework;

public class P10CPrePlayableStartupHotfixEditModeTests
{
    [Test]
    public void PlayableStartupConfigDefaultsToPlayableAndNoAutoQuit()
    {
        P10CPrePlayableStartupConfig config = P10CPreDataLoader.LoadPlayableStartupConfig().data;

        Assert.IsNotNull(config);
        Assert.IsTrue(config.IsDefaultPlayable());
        Assert.IsTrue(config.playableStartupMode);
        Assert.IsFalse(config.enableProfilingExporterByDefault);
        Assert.IsFalse(config.enableProfilingAutoQuit);
        Assert.AreEqual(P10CPreHighDetailSceneLocator.TargetScenePath, config.targetHighDetailScenePath);
    }

    [Test]
    public void HighDetailSourceAndTargetScenePathsAreKnown()
    {
        string sourceScenePath;
        string sourceMetaPath;
        long sourceBytes;
        if (!P10CPreHighDetailSceneLocator.TryGetActualHighDetailSource(out sourceScenePath, out sourceMetaPath, out sourceBytes))
        {
            Assert.Inconclusive(
                "External P7 high-detail source scene is not available on this machine. " +
                "Set " + P10CPreHighDetailSceneLocator.SourceSceneEnvironmentVariable +
                " and " + P10CPreHighDetailSceneLocator.SourceSceneMetaEnvironmentVariable +
                " to run this source-worktree check.");
        }

        Assert.IsTrue(File.Exists(P10CPreHighDetailSceneLocator.TargetScenePath), P10CPreHighDetailSceneLocator.TargetScenePath);
        Assert.IsTrue(File.Exists(P10CPreHighDetailSceneLocator.TargetSceneMetaPath), P10CPreHighDetailSceneLocator.TargetSceneMetaPath);
        Assert.IsTrue(File.Exists(sourceScenePath), sourceScenePath);
        Assert.IsTrue(File.Exists(sourceMetaPath), sourceMetaPath);
        Assert.Greater(sourceBytes, P10CPreHighDetailSceneLocator.MinimumActualHighDetailSceneBytes);
    }

    [Test]
    public void RuntimeDataResolverFindsPlayerCopiedDataEquivalent()
    {
        string dataRoot = RuntimeDataPathResolver.GetDataRoot();

        Assert.IsTrue(Directory.Exists(dataRoot));
        Assert.IsTrue(RuntimeDataPathResolver.DataFolderExists("P8"));
        Assert.IsTrue(RuntimeDataPathResolver.DataFolderExists("P9"));
        Assert.IsTrue(RuntimeDataPathResolver.DataFolderExists("P10"));
        Assert.IsTrue(File.Exists(RuntimeDataPathResolver.ResolveAssetRelativePath(P9BDataLoader.P8PersistentMarkerPath)));
        Assert.IsTrue(File.Exists(RuntimeDataPathResolver.GetDataPath("P10", P10BPlusDataLoader.LocalizationEnglishFileName)));
    }

    [Test]
    public void DefaultBuildScenePointsAtPlayableHighDetailTarget()
    {
        P10CPrePerformanceGateConfig performanceConfig = P10CPreDataLoader.LoadPerformanceGateConfig().data;
        P10CPrePlayableStartupConfig startupConfig = P10CPreDataLoader.LoadPlayableStartupConfig().data;

        Assert.IsNotNull(performanceConfig);
        Assert.IsNotNull(startupConfig);
        Assert.AreEqual(startupConfig.targetHighDetailScenePath, performanceConfig.defaultBuildScene);
        Assert.IsTrue(startupConfig.generateRuntimeGameplayBootstrap);
    }
}
