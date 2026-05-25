using System;
using System.IO;
using UnityEngine;

public static class RuntimeDataPathResolver
{
    public const string AssetsDataPrefix = "Assets/Data/";

    public static string GetDataRoot()
    {
        string playerOrEditorDataRoot = Path.Combine(Application.dataPath, "Data");
        if (Directory.Exists(playerOrEditorDataRoot))
        {
            return Path.GetFullPath(playerOrEditorDataRoot);
        }

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
        string editorAssetDataRoot = Path.Combine(projectRoot, "Assets", "Data");
        if (Directory.Exists(editorAssetDataRoot))
        {
            return Path.GetFullPath(editorAssetDataRoot);
        }

        return Path.GetFullPath(playerOrEditorDataRoot);
    }

    public static string GetDataPath(string fileName)
    {
        return Path.Combine(GetDataRoot(), fileName ?? string.Empty);
    }

    public static string GetDataPath(string folderName, string fileName)
    {
        return Path.Combine(GetDataRoot(), folderName ?? string.Empty, fileName ?? string.Empty);
    }

    public static string ResolveAssetsDataPath(string configuredPath)
    {
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            return string.Empty;
        }

        string normalizedPath = configuredPath.Replace('\\', '/').Trim();
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        if (normalizedPath.StartsWith(AssetsDataPrefix, StringComparison.OrdinalIgnoreCase))
        {
            string relativeToData = normalizedPath.Substring(AssetsDataPrefix.Length);
            return Path.Combine(GetDataRoot(), relativeToData.Replace('/', Path.DirectorySeparatorChar));
        }

        return Path.Combine(GetDataRoot(), configuredPath);
    }

    public static string ResolveAssetRelativePath(string assetRelativePath)
    {
        if (string.IsNullOrWhiteSpace(assetRelativePath))
        {
            return string.Empty;
        }

        string normalizedPath = assetRelativePath.Replace('\\', '/').Trim();
        if (normalizedPath.StartsWith(AssetsDataPrefix, StringComparison.OrdinalIgnoreCase))
        {
            return ResolveAssetsDataPath(normalizedPath);
        }

        string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
        return Path.Combine(projectRoot, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
    }

    public static bool DataFolderExists(string folderName)
    {
        return Directory.Exists(Path.Combine(GetDataRoot(), folderName ?? string.Empty));
    }

    public static bool DataFileExists(string folderName, string fileName)
    {
        return File.Exists(GetDataPath(folderName, fileName));
    }
}
