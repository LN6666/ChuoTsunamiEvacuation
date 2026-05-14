using System;
using System.Collections;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class EvacuationSmokePlayModeTests
{
    private GameObject gameManagerObject;

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        if (gameManagerObject != null)
        {
            UnityEngine.Object.Destroy(gameManagerObject);
            gameManagerObject = null;
        }

        yield return null;
    }

    [UnityTest]
    public IEnumerator GameManagerStartsInPreEventWithoutPlateauScene()
    {
        EvacuationGameManager gameManager = CreateGameManager();

        yield return null;

        Assert.AreEqual(EvacuationGameManager.GameState.PreEvent, gameManager.CurrentState);
    }

    [UnityTest]
    public IEnumerator ManualStartMovesGameManagerToPlaying()
    {
        EvacuationGameManager gameManager = CreateGameManager();

        yield return null;

        gameManager.StartEvacuationEvent();

        Assert.AreEqual(EvacuationGameManager.GameState.Playing, gameManager.CurrentState);
    }

    [UnityTest]
    public IEnumerator MissingTsunamiConfigFallbackDoesNotCrashInPlayMode()
    {
        string missingPath = Path.Combine(Path.GetTempPath(), $"missing_playmode_tsunami_config_{Guid.NewGuid():N}.json");

        LogAssert.Expect(LogType.Warning, new Regex("Missing tsunami event config.*Safe defaults"));
        GameConfigLoader.TsunamiEventConfig config = GameConfigLoader.LoadTsunamiEventConfigFromPath(missingPath);

        Assert.NotNull(config);
        Assert.IsTrue(config.manualStartEnabled);
        Assert.IsFalse(config.randomStartEnabled);
        Assert.Greater(config.evacuationCountdownSeconds, 0f);
        Assert.Greater(config.wallMoveDurationSeconds, 0f);

        yield return null;
    }

    private EvacuationGameManager CreateGameManager()
    {
        gameManagerObject = new GameObject("EvacuationSmokeTest_GameManager");
        return gameManagerObject.AddComponent<EvacuationGameManager>();
    }
}
