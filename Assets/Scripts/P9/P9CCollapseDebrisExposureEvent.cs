using System;
using UnityEngine;

[Serializable]
public class P9CCollapseDebrisExposureEvent
{
    public string eventId = string.Empty;
    public string zoneId = string.Empty;
    public string riskZoneCategory = "collapse_debris_proxy";
    public string routeSegmentId = string.Empty;
    public Vector3 exposurePosition;
    public bool exposureTriggered = true;
    public bool damagedBuildingState;
    public float exposureDistanceMeters;

    public int GetStableEventHash()
    {
        int hash = 23;
        hash = Append(hash, eventId);
        hash = Append(hash, zoneId);
        hash = Append(hash, riskZoneCategory);
        hash = Append(hash, routeSegmentId);
        hash = hash * 31 + Mathf.RoundToInt(exposurePosition.x * 10f);
        hash = hash * 31 + Mathf.RoundToInt(exposurePosition.z * 10f);
        return hash & int.MaxValue;
    }

    private static int Append(int hash, string value)
    {
        string safe = value ?? string.Empty;
        for (int i = 0; i < safe.Length; i++)
        {
            hash = hash * 31 + safe[i];
        }

        return hash;
    }
}
