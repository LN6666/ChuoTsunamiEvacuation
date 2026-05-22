using UnityEngine;

[DisallowMultipleComponent]
public sealed class P7BenchmarkMarker : MonoBehaviour
{
    public const string BenchmarkStage = "P7-C";
    public const string BenchmarkLabel = "P7 Benchmark Streaming Chunk Loading";
    public const string BenchmarkSceneName = "P7_Benchmark_Skeleton";
    public const string BenchmarkScenePath = "Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity";
    public const string BenchmarkRootName = "P7BenchmarkRoot";
    public const string BenchmarkScope = "p7benchmark_sandbox_chunk_loading_metadata_only";

    [SerializeField] private string stage = BenchmarkStage;
    [SerializeField] private string label = BenchmarkLabel;
    [SerializeField] private string scope = BenchmarkScope;
    [SerializeField] private string notes = "P7-C sandbox chunk/loading benchmark metadata for imported candidate 53393690. Raw CityGML conversion remains pending.";

    public string Stage => string.IsNullOrWhiteSpace(stage) ? BenchmarkStage : stage;
    public string Label => string.IsNullOrWhiteSpace(label) ? BenchmarkLabel : label;
    public string Scope => string.IsNullOrWhiteSpace(scope) ? BenchmarkScope : scope;
    public string Notes => notes ?? string.Empty;

    public void ResetToDefaults()
    {
        stage = BenchmarkStage;
        label = BenchmarkLabel;
        scope = BenchmarkScope;
        notes = "P7-C sandbox chunk/loading benchmark metadata for imported candidate 53393690. Raw CityGML conversion remains pending.";
    }

    private void Reset()
    {
        ResetToDefaults();
    }
}
