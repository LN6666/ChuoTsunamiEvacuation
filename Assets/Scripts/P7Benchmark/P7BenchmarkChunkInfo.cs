using System;
using System.Globalization;

[Serializable]
public sealed class P7BenchmarkChunkInfo
{
    public const string DefaultCandidateId = "53393690";

    [UnityEngine.SerializeField] private string chunkId = string.Empty;
    [UnityEngine.SerializeField] private string candidateId = DefaultCandidateId;
    [UnityEngine.SerializeField] private string displayName = string.Empty;
    [UnityEngine.SerializeField] private string category = string.Empty;
    [UnityEngine.SerializeField] private string sourceRelativePath = string.Empty;
    [UnityEngine.SerializeField] private int sourceFileCount;
    [UnityEngine.SerializeField] private long sourceBytes;
    [UnityEngine.SerializeField] private int cityGmlFileCount;
    [UnityEngine.SerializeField] private int textureFileCount;
    [UnityEngine.SerializeField] private int renderableAssetCount;
    [UnityEngine.SerializeField] private bool placeholderOnly = true;
    [UnityEngine.SerializeField] private bool initiallyEnabled = true;
    [UnityEngine.SerializeField] private string notes = string.Empty;

    public P7BenchmarkChunkInfo()
    {
    }

    public P7BenchmarkChunkInfo(
        string chunkId,
        string candidateId,
        string displayName,
        string category,
        string sourceRelativePath,
        int sourceFileCount,
        long sourceBytes,
        int cityGmlFileCount,
        int textureFileCount,
        int renderableAssetCount,
        bool placeholderOnly,
        bool initiallyEnabled,
        string notes)
    {
        this.chunkId = NormalizeText(chunkId);
        this.candidateId = string.IsNullOrWhiteSpace(candidateId) ? DefaultCandidateId : candidateId.Trim();
        this.displayName = NormalizeText(displayName);
        this.category = NormalizeText(category);
        this.sourceRelativePath = NormalizePath(sourceRelativePath);
        this.sourceFileCount = Math.Max(0, sourceFileCount);
        this.sourceBytes = Math.Max(0L, sourceBytes);
        this.cityGmlFileCount = Math.Max(0, cityGmlFileCount);
        this.textureFileCount = Math.Max(0, textureFileCount);
        this.renderableAssetCount = Math.Max(0, renderableAssetCount);
        this.placeholderOnly = placeholderOnly || renderableAssetCount <= 0;
        this.initiallyEnabled = initiallyEnabled;
        this.notes = NormalizeText(notes);
    }

    public string ChunkId => chunkId ?? string.Empty;
    public string CandidateId => string.IsNullOrWhiteSpace(candidateId) ? DefaultCandidateId : candidateId;
    public string DisplayName => string.IsNullOrWhiteSpace(displayName) ? ChunkId : displayName;
    public string Category => category ?? string.Empty;
    public string SourceRelativePath => sourceRelativePath ?? string.Empty;
    public int SourceFileCount => Math.Max(0, sourceFileCount);
    public long SourceBytes => Math.Max(0L, sourceBytes);
    public int CityGmlFileCount => Math.Max(0, cityGmlFileCount);
    public int TextureFileCount => Math.Max(0, textureFileCount);
    public int RenderableAssetCount => Math.Max(0, renderableAssetCount);
    public bool IsRenderableUnityAssetDetected => RenderableAssetCount > 0 && !placeholderOnly;
    public bool PlaceholderOnly => placeholderOnly || RenderableAssetCount <= 0;
    public bool InitiallyEnabled => initiallyEnabled;
    public string Notes => notes ?? string.Empty;

    public string GetSafeSummaryText()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "chunk={0}; candidate={1}; category={2}; files={3}; bytes={4}; cityGml={5}; textures={6}; renderableAssets={7}; placeholderOnly={8}; initiallyEnabled={9}; source={10}; notes={11}",
            ChunkId,
            CandidateId,
            Category,
            SourceFileCount,
            SourceBytes,
            CityGmlFileCount,
            TextureFileCount,
            RenderableAssetCount,
            PlaceholderOnly,
            InitiallyEnabled,
            SourceRelativePath,
            Notes);
    }

    public static string NormalizeChunkId(string chunkId)
    {
        string normalized = NormalizeText(chunkId);
        return string.IsNullOrWhiteSpace(normalized) ? "unassigned_chunk" : normalized;
    }

    private static string NormalizeText(string value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }

    private static string NormalizePath(string value)
    {
        return NormalizeText(value).Replace("\\", "/");
    }
}
