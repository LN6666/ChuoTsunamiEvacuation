using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

[CreateAssetMenu(fileName = "P7BenchmarkChunkRegistry", menuName = "P7 Benchmark/Chunk Registry")]
public sealed class P7BenchmarkChunkRegistry : ScriptableObject
{
    [SerializeField] private string candidateId = P7BenchmarkChunkInfo.DefaultCandidateId;
    [SerializeField] private string importRootRelativePath = "Assets/P7Benchmark/Imported/53393690";
    [SerializeField] private List<P7BenchmarkChunkInfo> chunks = new List<P7BenchmarkChunkInfo>();

    public string CandidateId => string.IsNullOrWhiteSpace(candidateId) ? P7BenchmarkChunkInfo.DefaultCandidateId : candidateId;
    public string ImportRootRelativePath => importRootRelativePath ?? string.Empty;
    public int ChunkCount => chunks != null ? chunks.Count : 0;
    public IReadOnlyList<P7BenchmarkChunkInfo> Chunks => chunks != null ? chunks.AsReadOnly() : Array.Empty<P7BenchmarkChunkInfo>();
    public int TotalSourceFileCount => SumInt(chunk => chunk.SourceFileCount);
    public long TotalSourceBytes => SumLong(chunk => chunk.SourceBytes);
    public int TotalCityGmlFileCount => SumInt(chunk => chunk.CityGmlFileCount);
    public int TotalTextureFileCount => SumInt(chunk => chunk.TextureFileCount);
    public int TotalRenderableAssetCount => SumInt(chunk => chunk.RenderableAssetCount);
    public int RenderableChunkCount => CountMatching(chunk => chunk.IsRenderableUnityAssetDetected);
    public int PlaceholderOnlyChunkCount => CountMatching(chunk => chunk.PlaceholderOnly);

    public void Configure(string candidateId, string importRootRelativePath, IEnumerable<P7BenchmarkChunkInfo> chunkInfos)
    {
        this.candidateId = string.IsNullOrWhiteSpace(candidateId) ? P7BenchmarkChunkInfo.DefaultCandidateId : candidateId.Trim();
        this.importRootRelativePath = NormalizePath(importRootRelativePath);

        if (chunks == null)
        {
            chunks = new List<P7BenchmarkChunkInfo>();
        }

        chunks.Clear();

        if (chunkInfos == null)
        {
            return;
        }

        foreach (P7BenchmarkChunkInfo chunkInfo in chunkInfos)
        {
            if (chunkInfo != null)
            {
                chunks.Add(chunkInfo);
            }
        }
    }

    public void ClearRegistry()
    {
        Configure(candidateId, importRootRelativePath, null);
    }

    public P7BenchmarkChunkInfo FindChunkById(string chunkId)
    {
        string normalizedId = P7BenchmarkChunkInfo.NormalizeChunkId(chunkId);
        foreach (P7BenchmarkChunkInfo chunk in Chunks)
        {
            if (string.Equals(chunk.ChunkId, normalizedId, StringComparison.OrdinalIgnoreCase))
            {
                return chunk;
            }
        }

        return null;
    }

    public IEnumerable<P7BenchmarkChunkInfo> FindChunksByCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
        {
            yield break;
        }

        foreach (P7BenchmarkChunkInfo chunk in Chunks)
        {
            if (string.Equals(chunk.Category, category.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                yield return chunk;
            }
        }
    }

    public string GetSummaryText()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "P7BenchmarkChunkRegistry candidate={0}; importRoot={1}; chunks={2}; files={3}; bytes={4}; cityGml={5}; textures={6}; renderableAssets={7}; renderableChunks={8}; placeholderOnlyChunks={9}",
            CandidateId,
            ImportRootRelativePath,
            ChunkCount,
            TotalSourceFileCount,
            TotalSourceBytes,
            TotalCityGmlFileCount,
            TotalTextureFileCount,
            TotalRenderableAssetCount,
            RenderableChunkCount,
            PlaceholderOnlyChunkCount);
    }

    public string GetDetailedSummaryText()
    {
        StringBuilder builder = new StringBuilder();
        builder.AppendLine(GetSummaryText());

        foreach (P7BenchmarkChunkInfo chunk in Chunks)
        {
            builder.AppendLine(chunk.GetSafeSummaryText());
        }

        return builder.ToString().TrimEnd();
    }

    private int SumInt(Func<P7BenchmarkChunkInfo, int> selector)
    {
        int total = 0;
        foreach (P7BenchmarkChunkInfo chunk in Chunks)
        {
            total += selector(chunk);
        }

        return total;
    }

    private long SumLong(Func<P7BenchmarkChunkInfo, long> selector)
    {
        long total = 0L;
        foreach (P7BenchmarkChunkInfo chunk in Chunks)
        {
            total += selector(chunk);
        }

        return total;
    }

    private int CountMatching(Func<P7BenchmarkChunkInfo, bool> predicate)
    {
        int total = 0;
        foreach (P7BenchmarkChunkInfo chunk in Chunks)
        {
            if (predicate(chunk))
            {
                total++;
            }
        }

        return total;
    }

    private static string NormalizePath(string path)
    {
        return string.IsNullOrWhiteSpace(path) ? string.Empty : path.Trim().Replace("\\", "/");
    }
}
