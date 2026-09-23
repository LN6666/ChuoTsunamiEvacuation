public class P8InfrastructureDamageProxy : P8InfrastructureHazardProxy
{
    public const string ProxyPurpose = "P8-D lightweight infrastructure damage/blockage visual-status proxy";
    public const bool AffectsGameplaySuccessFailureInP8D = false;
    public const bool ImplementsTrueStructuralCollapse = false;
    public const bool RequiresP9SystemsInP8D = false;

    public P8InfrastructureDamageEvaluation CurrentDamageEvaluation { get; private set; }
    public P8InfrastructureDamageState CurrentDamageState { get; private set; } = P8InfrastructureDamageState.NoDamage;

    public P8InfrastructureDamageEvaluation EvaluateDamage(
        P8HazardLayerData hazardLayer,
        P8InfrastructureDamageConfig config,
        float timeSeconds,
        P8InfrastructureDamageEvaluationInput damageInput)
    {
        P8InfrastructureHazardEvaluation hazardEvaluation = Evaluate(hazardLayer, timeSeconds);
        CurrentDamageEvaluation = P8InfrastructureDamageEvaluator.Evaluate(hazardEvaluation, config, damageInput);
        CurrentDamageState = CurrentDamageEvaluation != null ? CurrentDamageEvaluation.state : P8InfrastructureDamageState.NoDamage;
        return CurrentDamageEvaluation;
    }
}
