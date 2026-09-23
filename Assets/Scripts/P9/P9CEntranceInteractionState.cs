using System;

public enum P9CEntranceStatus
{
    Open,
    Crowded,
    Blocked,
    HazardAffected,
    Closed,
    Unknown
}

[Serializable]
public class P9CEntranceInteractionState
{
    public string entranceProxyId = string.Empty;
    public string entranceStatus = "open";
    public int queueLength;
    public float crowdDensity01;
    public float routeCongestionScore01;
    public float baseInteractionDelaySeconds;
    public bool hazardAffected;
    public string notes = string.Empty;

    public P9CEntranceStatus ParsedStatus => ParseStatus(entranceStatus);

    public static P9CEntranceStatus ParseStatus(string status)
    {
        if (string.Equals(status, "open", StringComparison.OrdinalIgnoreCase))
        {
            return P9CEntranceStatus.Open;
        }

        if (string.Equals(status, "crowded", StringComparison.OrdinalIgnoreCase))
        {
            return P9CEntranceStatus.Crowded;
        }

        if (string.Equals(status, "blocked", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(status, "blocked_proxy", StringComparison.OrdinalIgnoreCase))
        {
            return P9CEntranceStatus.Blocked;
        }

        if (string.Equals(status, "hazard_affected", StringComparison.OrdinalIgnoreCase))
        {
            return P9CEntranceStatus.HazardAffected;
        }

        if (string.Equals(status, "closed", StringComparison.OrdinalIgnoreCase))
        {
            return P9CEntranceStatus.Closed;
        }

        return P9CEntranceStatus.Unknown;
    }

    public static string ToToken(P9CEntranceStatus status)
    {
        switch (status)
        {
            case P9CEntranceStatus.Open:
                return "open";
            case P9CEntranceStatus.Crowded:
                return "crowded";
            case P9CEntranceStatus.Blocked:
                return "blocked";
            case P9CEntranceStatus.HazardAffected:
                return "hazard_affected";
            case P9CEntranceStatus.Closed:
                return "closed";
            default:
                return "unknown";
        }
    }
}
