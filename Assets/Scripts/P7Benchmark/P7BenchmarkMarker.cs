using UnityEngine;

[DisallowMultipleComponent]
public sealed class P7BenchmarkMarker : MonoBehaviour
{
    public const string BenchmarkStage = "P7-B Wave 2-A";
    public const string BenchmarkLabel = "P7 Benchmark Skeleton";
    public const string BenchmarkSceneName = "P7_Benchmark_Skeleton";
    public const string BenchmarkScenePath = "Assets/Scenes/P7Benchmark/P7_Benchmark_Skeleton.unity";
    public const string BenchmarkRootName = "P7BenchmarkRoot";
    public const string BenchmarkScope = "isolated_skeleton_no_real_assets";

    [SerializeField] private string stage = BenchmarkStage;
    [SerializeField] private string label = BenchmarkLabel;
    [SerializeField] private string scope = BenchmarkScope;
    [SerializeField] private string notes = "Primitive-only P7-B benchmark placeholder. No PLATEAU assets are imported.";

    public string Stage => string.IsNullOrWhiteSpace(stage) ? BenchmarkStage : stage;
    public string Label => string.IsNullOrWhiteSpace(label) ? BenchmarkLabel : label;
    public string Scope => string.IsNullOrWhiteSpace(scope) ? BenchmarkScope : scope;
    public string Notes => notes ?? string.Empty;

    public void ResetToDefaults()
    {
        stage = BenchmarkStage;
        label = BenchmarkLabel;
        scope = BenchmarkScope;
        notes = "Primitive-only P7-B benchmark placeholder. No PLATEAU assets are imported.";
    }

    private void Reset()
    {
        ResetToDefaults();
    }
}
