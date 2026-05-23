using System.Text;
using UnityEngine;

public class P8InfrastructureHazardDebugSummary : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;
    public const bool RequiresChuoBaseMap = false;
    public const bool RequiresP9Systems = false;

    [SerializeField] private P8InfrastructureHazardTarget[] targets = new P8InfrastructureHazardTarget[0];
    [SerializeField] private TextMesh label;

    public string LastSummary { get; private set; } = string.Empty;

    public string RefreshSummary(P8HazardLayerData hazardLayer, float simulationTimeSeconds)
    {
        var builder = new StringBuilder();
        builder.Append("P8-C infrastructure hazard summary");

        if (targets != null)
        {
            for (int i = 0; i < targets.Length; i++)
            {
                P8InfrastructureHazardTarget target = targets[i];
                if (target == null)
                {
                    continue;
                }

                P8InfrastructureHazardEvaluation evaluation = target.Evaluate(hazardLayer, simulationTimeSeconds);
                builder.Append("\n");
                builder.Append(target.TargetId);
                builder.Append(": ");
                builder.Append(evaluation.state);
                builder.Append(" ");
                builder.Append(evaluation.contactPhase);
            }
        }

        LastSummary = builder.ToString();
        if (label != null)
        {
            label.text = LastSummary;
        }

        return LastSummary;
    }

    public void ConfigureTargetsForTests(P8InfrastructureHazardTarget[] hazardTargets)
    {
        targets = hazardTargets ?? new P8InfrastructureHazardTarget[0];
    }
}
