# P8-D Collapse Proxy Visual Rules

Date: 2026-05-24.

`collapsed_proxy_visual` is a marker/status visualization only. It is not real collapse, not an engineering prediction, not official damage assessment, and not gameplay failure.

## Deterministic Rule

Implementation:

- `Assets/Scripts/P8/P8CollapseProxyRule.cs`
- `Assets/Scripts/P8/P8InfrastructureDamageEvaluator.cs`

Config:

- `enableCollapseProxyVisual=true`
- `collapseProxyProbability=0.03`
- `collapseProxyRandomSeed=8302`
- `maxCollapseProxySampleCount=8`

The evaluator hashes stable target id plus seed, converts the hash to a stable probability roll, and selects only targets within the configured max sample count. The same id and seed produce the same decision across runs.

## Eligible Categories

- building
- humanitarian_candidate_proxy
- highrise_candidate_marker

## Explicit Limits

- no physics collapse;
- no debris;
- no rigidbody destruction;
- no structural safety statement;
- no official damage prediction;
- no gameplay success/failure mutation.
