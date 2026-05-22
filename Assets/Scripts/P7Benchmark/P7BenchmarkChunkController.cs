using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

[DisallowMultipleComponent]
public sealed class P7BenchmarkChunkController : MonoBehaviour
{
    [SerializeField] private P7BenchmarkChunkRegistry registry;
    [SerializeField] private List<P7BenchmarkChunkBinding> chunkBindings = new List<P7BenchmarkChunkBinding>();
    [SerializeField] private bool applyInitialStateOnStart = true;

    public static bool AffectsGameplaySuccessFailure => false;

    public P7BenchmarkChunkRegistry Registry => registry;
    public int BindingCount => chunkBindings != null ? chunkBindings.Count : 0;
    public int ActiveChunkCount => CountBindings(binding => binding.IsRootActive);
    public int MissingRootCount => CountBindings(binding => binding.ChunkRoot == null);
    public IReadOnlyList<P7BenchmarkChunkBinding> Bindings => chunkBindings != null ? chunkBindings.AsReadOnly() : Array.Empty<P7BenchmarkChunkBinding>();

    public void SetRegistry(P7BenchmarkChunkRegistry registry)
    {
        this.registry = registry;
    }

    public void ClearBindings()
    {
        if (chunkBindings == null)
        {
            chunkBindings = new List<P7BenchmarkChunkBinding>();
        }

        chunkBindings.Clear();
    }

    public P7BenchmarkChunkBinding RegisterChunkRoot(string chunkId, GameObject chunkRoot, bool enabledByDefault = true)
    {
        string normalizedId = P7BenchmarkChunkInfo.NormalizeChunkId(chunkId);
        P7BenchmarkChunkBinding existing = FindBinding(normalizedId);
        if (existing != null)
        {
            existing.Configure(normalizedId, chunkRoot, enabledByDefault);
            return existing;
        }

        if (chunkBindings == null)
        {
            chunkBindings = new List<P7BenchmarkChunkBinding>();
        }

        P7BenchmarkChunkBinding binding = new P7BenchmarkChunkBinding(normalizedId, chunkRoot, enabledByDefault);
        chunkBindings.Add(binding);
        return binding;
    }

    public bool SetChunkActive(string chunkId, bool active)
    {
        P7BenchmarkChunkBinding binding = FindBinding(chunkId);
        if (binding == null)
        {
            return false;
        }

        binding.SetActive(active);
        return true;
    }

    public int SetGroupActiveByCategory(string category, bool active)
    {
        if (registry == null || string.IsNullOrWhiteSpace(category))
        {
            return 0;
        }

        int changed = 0;
        foreach (P7BenchmarkChunkInfo chunk in registry.FindChunksByCategory(category))
        {
            if (SetChunkActive(chunk.ChunkId, active))
            {
                changed++;
            }
        }

        return changed;
    }

    public void SetAllChunksActive(bool active)
    {
        foreach (P7BenchmarkChunkBinding binding in Bindings)
        {
            binding.SetActive(active);
        }
    }

    public void ApplyInitialChunkState()
    {
        foreach (P7BenchmarkChunkBinding binding in Bindings)
        {
            bool initialState = binding.EnabledByDefault;
            P7BenchmarkChunkInfo chunk = registry != null ? registry.FindChunkById(binding.ChunkId) : null;
            if (chunk != null)
            {
                initialState = chunk.InitiallyEnabled;
            }

            binding.SetActive(initialState);
        }
    }

    public bool TryGetChunkActive(string chunkId, out bool active)
    {
        P7BenchmarkChunkBinding binding = FindBinding(chunkId);
        if (binding == null || binding.ChunkRoot == null)
        {
            active = false;
            return false;
        }

        active = binding.IsRootActive;
        return true;
    }

    public string GetSummaryText()
    {
        string registrySummary = registry != null ? registry.GetSummaryText() : "registry=none";
        return string.Format(
            CultureInfo.InvariantCulture,
            "P7BenchmarkChunkController bindings={0}; activeChunks={1}; missingRoots={2}; affectsGameplaySuccessFailure={3}; {4}; states={5}",
            BindingCount,
            ActiveChunkCount,
            MissingRootCount,
            AffectsGameplaySuccessFailure,
            registrySummary,
            GetChunkStateSummaryText());
    }

    public string GetChunkStateSummaryText()
    {
        if (BindingCount == 0)
        {
            return "none";
        }

        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < Bindings.Count; i++)
        {
            if (i > 0)
            {
                builder.Append(",");
            }

            builder.Append(Bindings[i].GetStateText());
        }

        return builder.ToString();
    }

    private void Start()
    {
        if (applyInitialStateOnStart)
        {
            ApplyInitialChunkState();
        }
    }

    private P7BenchmarkChunkBinding FindBinding(string chunkId)
    {
        string normalizedId = P7BenchmarkChunkInfo.NormalizeChunkId(chunkId);
        foreach (P7BenchmarkChunkBinding binding in Bindings)
        {
            if (binding != null && string.Equals(binding.ChunkId, normalizedId, StringComparison.OrdinalIgnoreCase))
            {
                return binding;
            }
        }

        return null;
    }

    private int CountBindings(Func<P7BenchmarkChunkBinding, bool> predicate)
    {
        int total = 0;
        foreach (P7BenchmarkChunkBinding binding in Bindings)
        {
            if (binding != null && predicate(binding))
            {
                total++;
            }
        }

        return total;
    }
}

[Serializable]
public sealed class P7BenchmarkChunkBinding
{
    [SerializeField] private string chunkId = string.Empty;
    [SerializeField] private GameObject chunkRoot;
    [SerializeField] private bool enabledByDefault = true;

    public P7BenchmarkChunkBinding()
    {
    }

    public P7BenchmarkChunkBinding(string chunkId, GameObject chunkRoot, bool enabledByDefault)
    {
        Configure(chunkId, chunkRoot, enabledByDefault);
    }

    public string ChunkId => P7BenchmarkChunkInfo.NormalizeChunkId(chunkId);
    public GameObject ChunkRoot => chunkRoot;
    public bool EnabledByDefault => enabledByDefault;
    public bool IsRootActive => chunkRoot != null && chunkRoot.activeSelf;

    public void Configure(string chunkId, GameObject chunkRoot, bool enabledByDefault)
    {
        this.chunkId = P7BenchmarkChunkInfo.NormalizeChunkId(chunkId);
        this.chunkRoot = chunkRoot;
        this.enabledByDefault = enabledByDefault;
    }

    public void SetActive(bool active)
    {
        if (chunkRoot != null)
        {
            chunkRoot.SetActive(active);
        }
    }

    public string GetStateText()
    {
        string state = chunkRoot == null ? "missing" : (chunkRoot.activeSelf ? "enabled" : "disabled");
        return string.Format(CultureInfo.InvariantCulture, "{0}:{1}", ChunkId, state);
    }
}
