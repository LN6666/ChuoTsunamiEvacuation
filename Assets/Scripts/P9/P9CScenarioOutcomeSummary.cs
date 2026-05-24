using System;

public static class P9CScenarioOutcomeSummary
{
    public static P9CScenarioOutcomeSummaryRecord Create(P9COutcomeResult[] results)
    {
        var summary = new P9CScenarioOutcomeSummaryRecord
        {
            deterministic = true,
            routeClaimsOfficial = P9RuntimePolicy.ClaimsOfficialRoutes,
            importsExternalCrowdPackage = P9RuntimePolicy.P9CImportsExternalCrowdPackage
        };

        if (results == null)
        {
            return summary;
        }

        summary.totalRuns = results.Length;
        for (int i = 0; i < results.Length; i++)
        {
            P9COutcomeResult result = results[i];
            if (result == null)
            {
                continue;
            }

            if (result.success)
            {
                summary.successCount++;
            }
            else
            {
                summary.failureCount++;
            }

            if (result.nonOfficialWarningRequired)
            {
                summary.nonOfficialWarningCount++;
            }

            if (result.collapseDebrisFatality)
            {
                summary.collapseDebrisFatalityCount++;
            }
        }

        summary.summary = "P9-C summary: runs=" + summary.totalRuns +
                          ", success=" + summary.successCount +
                          ", failure=" + summary.failureCount + ".";
        return summary;
    }
}

[Serializable]
public class P9CScenarioOutcomeSummaryRecord
{
    public int totalRuns;
    public int successCount;
    public int failureCount;
    public int nonOfficialWarningCount;
    public int collapseDebrisFatalityCount;
    public bool deterministic;
    public bool routeClaimsOfficial;
    public bool importsExternalCrowdPackage;
    public string summary = string.Empty;
}
