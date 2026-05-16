using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static class TsunamiHazardDebugLayout
{
    private const float StartX = -22f;
    private const float StartZ = 20f;
    private const float SpacingX = 16f;
    private const float SpacingZ = 10f;
    private const int Columns = 3;

    public class HazardDebugShape
    {
        public string zoneId = string.Empty;
        public string label = string.Empty;
        public int hazardLevel;
        public Vector3 localPosition;
        public Vector3 localScale;
        public Color color;
        public bool affectsGameplayRules;
    }

    public static HazardDebugShape[] CreateShapes(TsunamiHazardFixtureLoader.HazardZoneRecord[] zones)
    {
        if (zones == null || zones.Length == 0)
        {
            return new HazardDebugShape[0];
        }

        var shapes = new List<HazardDebugShape>();

        for (int i = 0; i < zones.Length; i++)
        {
            TsunamiHazardFixtureLoader.HazardZoneRecord zone = zones[i];
            if (zone == null)
            {
                continue;
            }

            shapes.Add(new HazardDebugShape
            {
                zoneId = zone.zoneId,
                label = BuildLabel(zone),
                hazardLevel = zone.hazardLevel,
                localPosition = CreateDeterministicPosition(i),
                localScale = CreateScale(zone.hazardLevel),
                color = CreateColor(zone.hazardLevel),
                affectsGameplayRules = false
            });
        }

        return shapes.ToArray();
    }

    public static Vector3 CreateDeterministicPosition(int index)
    {
        int safeIndex = Mathf.Max(0, index);
        int column = safeIndex % Columns;
        int row = safeIndex / Columns;

        return new Vector3(
            StartX + column * SpacingX,
            0.08f,
            StartZ + row * SpacingZ);
    }

    private static Vector3 CreateScale(int hazardLevel)
    {
        float level = Mathf.Clamp(hazardLevel, 0, 5);
        return new Vector3(9f + level * 1.4f, 0.12f, 6f + level * 0.8f);
    }

    private static Color CreateColor(int hazardLevel)
    {
        if (hazardLevel <= 0)
        {
            return new Color(0.55f, 0.55f, 0.55f, 0.34f);
        }

        if (hazardLevel <= 1)
        {
            return new Color(1f, 0.82f, 0.18f, 0.38f);
        }

        if (hazardLevel <= 3)
        {
            return new Color(1f, 0.42f, 0.12f, 0.42f);
        }

        return new Color(1f, 0.08f, 0.08f, 0.46f);
    }

    private static string BuildLabel(TsunamiHazardFixtureLoader.HazardZoneRecord zone)
    {
        var builder = new StringBuilder();
        AppendLine(builder, zone.zoneName);
        AppendLine(builder, $"Family: {zone.hazardFamily}");
        AppendLine(builder, $"Level: {zone.hazardLevel}");

        if (!string.IsNullOrWhiteSpace(zone.affectedZone))
        {
            AppendLine(builder, $"Area: {zone.affectedZone}");
        }

        if (zone.hasInundationDepth)
        {
            AppendLine(builder, $"Depth: {zone.inundationDepthMeters:0.#}m");
        }
        else
        {
            AppendLine(builder, "Depth: unknown");
        }

        if (zone.hasTsunamiHeight)
        {
            AppendLine(builder, $"Height: {zone.tsunamiHeightMeters:0.#}m");
        }

        if (!string.IsNullOrWhiteSpace(zone.sourceName))
        {
            AppendLine(builder, $"Source: {zone.sourceName}");
        }

        if (!string.IsNullOrWhiteSpace(zone.officialStatus))
        {
            AppendLine(builder, $"Status: {zone.officialStatus}");
        }

        if (!string.IsNullOrWhiteSpace(zone.notes))
        {
            AppendLine(builder, $"Notes: {zone.notes}");
        }

        return builder.ToString().TrimEnd();
    }

    private static void AppendLine(StringBuilder builder, string value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            builder.AppendLine(value.Trim());
        }
    }
}
