using NUnit.Framework;
using UnityEngine;

public sealed class FullscreenModeControllerTests
{
    [Test]
    public void ToggleShortcutAcceptsF11AndAltEnterOnly()
    {
        Assert.IsTrue(NewMapFullscreenModeController.IsToggleShortcut(true, false, false, false, false));
        Assert.IsTrue(NewMapFullscreenModeController.IsToggleShortcut(false, true, false, true, false));
        Assert.IsTrue(NewMapFullscreenModeController.IsToggleShortcut(false, false, true, false, true));
        Assert.IsFalse(NewMapFullscreenModeController.IsToggleShortcut(false, true, false, false, false));
        Assert.IsFalse(NewMapFullscreenModeController.IsToggleShortcut(false, false, false, true, false));
    }

    [Test]
    public void CommandLineFullscreenOverrideParsesRecoveryArguments()
    {
        Assert.IsTrue(NewMapFullscreenModeController.TryGetCommandLineFullscreenOverride(
            new[] { "game.exe", "-screen-fullscreen", "0" },
            out bool windowed));
        Assert.IsFalse(windowed);

        Assert.IsTrue(NewMapFullscreenModeController.TryGetCommandLineFullscreenOverride(
            new[] { "game.exe", "-fullscreen" },
            out bool fullscreen));
        Assert.IsTrue(fullscreen);

        Assert.IsTrue(NewMapFullscreenModeController.TryGetCommandLineFullscreenOverride(
            new[] { "game.exe", "--windowed" },
            out bool customWindowed));
        Assert.IsFalse(customWindowed);
    }

    [Test]
    public void WindowedFallbackUsesDisplayScaleWithoutTinyResolution()
    {
        Vector2Int size = NewMapFullscreenModeController.ResolveWindowedSize(
            0,
            0,
            1920,
            1080,
            1920,
            1080);

        Assert.GreaterOrEqual(size.x, 1024);
        Assert.GreaterOrEqual(size.y, 576);
        Assert.LessOrEqual(size.x, 1920);
        Assert.LessOrEqual(size.y, 1080);
    }

    [Test]
    public void SavedWindowedSizeIsClampedToCurrentDisplay()
    {
        Vector2Int size = NewMapFullscreenModeController.ResolveWindowedSize(
            3000,
            2000,
            1920,
            1080,
            1920,
            1080);

        Assert.AreEqual(1920, size.x);
        Assert.AreEqual(1080, size.y);
    }
}
