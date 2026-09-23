using System.Collections.Generic;
using System.Text;
using UnityEngine;

public sealed class NewMapShelterDirectLineController : MonoBehaviour
{
    private readonly List<LineEntry> entries = new List<LineEntry>();
    private NewMapShelterDirectLineConfig config;
    private Transform player;
    private bool visible;
    private float updateTimer;
    private string nearestTargetId = string.Empty;
    private string lastRankingText = string.Empty;

    public int LineCount => entries.Count;
    public string NearestTargetId => nearestTargetId;
    public string LastRankingText => lastRankingText;
    public float RankingAutoRefreshIntervalSeconds => config != null ? config.RankingAutoRefreshIntervalSeconds : 0.5f;

    public static readonly Color NearestLineColor = new Color(1f, 0.06f, 0.04f, 0.92f);
    public static readonly Color OfficialLineColor = new Color(0.12f, 0.95f, 0.28f, 0.72f);
    public static readonly Color NonOfficialLineColor = new Color(1f, 0.86f, 0.08f, 0.74f);

    public static NewMapShelterDirectLineController Create(
        Transform parent,
        NewMapPlayerController playerController,
        IEnumerable<NewMapRuntimeTarget> targets,
        NewMapShelterDirectLineConfig directLineConfig)
    {
        GameObject controllerObject = new GameObject("P10_ShelterDirectLineController");
        controllerObject.transform.SetParent(parent, false);
        NewMapShelterDirectLineController controller = controllerObject.AddComponent<NewMapShelterDirectLineController>();
        controller.Configure(playerController != null ? playerController.transform : null, targets, directLineConfig);
        return controller;
    }

    public void Configure(
        Transform playerTransform,
        IEnumerable<NewMapRuntimeTarget> targets,
        NewMapShelterDirectLineConfig directLineConfig)
    {
        player = playerTransform;
        config = directLineConfig ?? NewMapShelterDirectLineConfig.Default();
        entries.Clear();

        int cap = config.MaxDisplayedShelterLines;
        int included = 0;
        if (config.enableShelterDirectLines && targets != null)
        {
            foreach (NewMapRuntimeTarget target in targets)
            {
                if (!IsRankableTarget(target))
                {
                    continue;
                }

                if (cap > 0 && included >= cap)
                {
                    break;
                }

                entries.Add(CreateLineEntry(target));
                included++;
            }
        }

        SetVisible(false);
        ForceRefresh();
        Debug.Log(
            $"NewMap shelter direct lines configured enable={config.enableShelterDirectLines} " +
            $"lineCount={entries.Count} maxDisplayed={config.maxDisplayedShelterLines} " +
            $"updateInterval={config.LineUpdateIntervalSeconds:0.00}");
    }

    public void Tick(float deltaTime, bool shouldBeVisible)
    {
        SetVisible(shouldBeVisible);
        if (!visible || entries.Count == 0)
        {
            return;
        }

        updateTimer -= Mathf.Max(0f, deltaTime);
        if (updateTimer <= 0f)
        {
            ForceRefresh();
        }
    }

    public void SetVisible(bool value)
    {
        visible = value && config != null && config.enableShelterDirectLines && entries.Count > 0;
        if (gameObject.activeSelf != visible)
        {
            gameObject.SetActive(visible);
        }
    }

    public void ForceRefresh()
    {
        updateTimer = config != null ? config.LineUpdateIntervalSeconds : 0.2f;
        nearestTargetId = string.Empty;
        if (player == null || entries.Count == 0)
        {
            lastRankingText = BuildRankingText(new List<NewMapShelterLineSnapshot>());
            return;
        }

        float bestDistance = float.MaxValue;
        LineEntry nearest = null;
        for (int i = 0; i < entries.Count; i++)
        {
            LineEntry entry = entries[i];
            bool rankable = IsRankableTarget(entry.Target);
            if (entry.LineObject != null)
            {
                entry.LineObject.SetActive(visible && rankable);
            }

            if (!rankable)
            {
                continue;
            }

            Vector3 start = player.position + Vector3.up * config.LineHeightOffsetMeters;
            Vector3 end = entry.Target.Anchor.position + Vector3.up * config.LineHeightOffsetMeters;
            entry.DistanceMeters = Vector3.Distance(player.position, entry.Target.Anchor.position);
            if (entry.Line != null)
            {
                entry.Line.SetPosition(0, start);
                entry.Line.SetPosition(1, end);
            }

            if (entry.DistanceMeters < bestDistance)
            {
                bestDistance = entry.DistanceMeters;
                nearest = entry;
            }
        }

        if (nearest != null && nearest.Target != null)
        {
            nearestTargetId = nearest.Target.Id ?? string.Empty;
        }

        for (int i = 0; i < entries.Count; i++)
        {
            LineEntry entry = entries[i];
            if (!IsRankableTarget(entry.Target))
            {
                continue;
            }

            Color color = entry == nearest
                ? NearestLineColor
                : (entry.Target.IsOfficialShelter ? OfficialLineColor : NonOfficialLineColor);
            ApplyColor(entry, color);
        }

        lastRankingText = BuildRankingText(GetSortedSnapshots());
    }

    public NewMapShelterLineSnapshot[] GetSortedSnapshots()
    {
        var snapshots = new List<NewMapShelterLineSnapshot>();
        for (int i = 0; i < entries.Count; i++)
        {
            LineEntry entry = entries[i];
            if (!IsRankableTarget(entry.Target))
            {
                continue;
            }

            snapshots.Add(CreateSnapshot(entry));
        }

        snapshots.Sort((left, right) => left.DistanceMeters.CompareTo(right.DistanceMeters));
        for (int i = 0; i < snapshots.Count; i++)
        {
            NewMapShelterLineSnapshot snapshot = snapshots[i];
            snapshot.Rank = i + 1;
            snapshot.IsNearest = i == 0;
            snapshots[i] = snapshot;
        }

        return snapshots.ToArray();
    }

    public bool TryGetLineColorForDiagnostics(string targetId, out Color color)
    {
        color = Color.clear;
        for (int i = 0; i < entries.Count; i++)
        {
            LineEntry entry = entries[i];
            if (entry.Target != null && entry.Target.Id == targetId)
            {
                color = entry.CurrentColor;
                return true;
            }
        }

        return false;
    }

    public int CountLineCollidersForDiagnostics()
    {
        return GetComponentsInChildren<Collider>(true).Length;
    }

    private LineEntry CreateLineEntry(NewMapRuntimeTarget target)
    {
        GameObject lineObject = new GameObject("P10_ShelterDirectLine_" + SanitizeName(target.Id));
        lineObject.transform.SetParent(transform, true);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.positionCount = 2;
        line.widthMultiplier = config.LineWidthMeters;
        line.numCapVertices = 2;
        line.sharedMaterial = NewMapVisualFactory.CreateMaterial(
            lineObject.name + "_Material",
            target.IsOfficialShelter ? OfficialLineColor : NonOfficialLineColor,
            true);

        var entry = new LineEntry
        {
            Target = target,
            LineObject = lineObject,
            Line = line,
            CurrentColor = target.IsOfficialShelter ? OfficialLineColor : NonOfficialLineColor
        };
        ApplyColor(entry, entry.CurrentColor);
        return entry;
    }

    private NewMapShelterLineSnapshot CreateSnapshot(LineEntry entry)
    {
        NewMapRuntimeTarget target = entry.Target;
        return new NewMapShelterLineSnapshot
        {
            TargetId = target.Id ?? string.Empty,
            DisplayName = string.IsNullOrWhiteSpace(target.DisplayName) ? target.Id : target.DisplayName,
            DistanceMeters = entry.DistanceMeters,
            Status = ResolveStatusLabel(target),
            Availability = ResolveAvailabilityLabel(target),
            IsOfficialShelter = target.IsOfficialShelter,
            LineColor = entry.CurrentColor
        };
    }

    private string BuildRankingText(IList<NewMapShelterLineSnapshot> snapshots)
    {
        var builder = new StringBuilder();
        builder.AppendLine("Shelter distance ranking (straight-line)");
        builder.AppendLine("R: show/hide | nearest line is red | estimated prototype guidance only");

        if (snapshots == null || snapshots.Count == 0)
        {
            builder.AppendLine("No active targets.");
            return builder.ToString();
        }

        for (int i = 0; i < snapshots.Count; i++)
        {
            NewMapShelterLineSnapshot snapshot = snapshots[i];
            string nearest = i == 0 ? "Nearest | " : string.Empty;
            builder
                .Append(snapshot.Rank)
                .Append(". ")
                .Append(nearest)
                .Append(snapshot.DisplayName)
                .Append(" | id=")
                .Append(snapshot.TargetId)
                .Append(" | ")
                .Append(snapshot.DistanceMeters.ToString("0.0"))
                .Append(" m | ")
                .Append(snapshot.Status)
                .Append(" | ")
                .AppendLine(snapshot.Availability);
        }

        return builder.ToString();
    }

    private static void ApplyColor(LineEntry entry, Color color)
    {
        entry.CurrentColor = color;
        if (entry.Line == null)
        {
            return;
        }

        entry.Line.startColor = color;
        entry.Line.endColor = color;
        Material material = entry.Line.sharedMaterial;
        if (material == null)
        {
            return;
        }

        if (material.HasProperty("_BaseColor"))
        {
            material.SetColor("_BaseColor", color);
        }
        else if (material.HasProperty("_Color"))
        {
            material.color = color;
        }
    }

    private static bool IsRankableTarget(NewMapRuntimeTarget target)
    {
        return target != null &&
            target.ActiveInGame &&
            target.Anchor != null &&
            target.SafeFloorAvailable &&
            !target.EntranceBlocked;
    }

    private static string ResolveStatusLabel(NewMapRuntimeTarget target)
    {
        if (target == null)
        {
            return "Unknown";
        }

        if (target.IsOfficialShelter)
        {
            return "Official";
        }

        string category = target.Category ?? string.Empty;
        if (category.Contains("humanitarian_candidate"))
        {
            return "Humanitarian";
        }

        if (category.Contains("proxy"))
        {
            return "Proxy";
        }

        return "Non-official";
    }

    private static string ResolveAvailabilityLabel(NewMapRuntimeTarget target)
    {
        if (target == null || !target.ActiveInGame)
        {
            return "Unavailable";
        }

        if (target.EntranceBlocked)
        {
            return "Unavailable: blocked";
        }

        if (!target.SafeFloorAvailable)
        {
            return "Unavailable: no safe floor";
        }

        return "Available";
    }

    private static string SanitizeName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return "unknown";
        }

        char[] chars = value.ToCharArray();
        for (int i = 0; i < chars.Length; i++)
        {
            char c = chars[i];
            if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
            {
                chars[i] = '_';
            }
        }

        return new string(chars);
    }

    private sealed class LineEntry
    {
        public NewMapRuntimeTarget Target;
        public GameObject LineObject;
        public LineRenderer Line;
        public Color CurrentColor;
        public float DistanceMeters;
    }
}

public struct NewMapShelterLineSnapshot
{
    public int Rank;
    public string TargetId;
    public string DisplayName;
    public float DistanceMeters;
    public string Status;
    public string Availability;
    public bool IsOfficialShelter;
    public bool IsNearest;
    public Color LineColor;
}

[System.Serializable]
public sealed class NewMapShelterDirectLineConfig
{
    public bool enableShelterDirectLines = true;
    public int maxDisplayedShelterLines;
    public float lineUpdateIntervalSeconds = 0.2f;
    public float rankingAutoRefreshIntervalSeconds = 0.5f;
    public float directLineHeightOffsetMeters = 0.35f;
    public float directLineWidthMeters = 0.08f;

    public int MaxDisplayedShelterLines => Mathf.Clamp(maxDisplayedShelterLines, 0, 10000);
    public float LineUpdateIntervalSeconds => Mathf.Clamp(lineUpdateIntervalSeconds, 0.02f, 5f);
    public float RankingAutoRefreshIntervalSeconds => Mathf.Clamp(rankingAutoRefreshIntervalSeconds, 0.1f, 5f);
    public float LineHeightOffsetMeters => Mathf.Clamp(directLineHeightOffsetMeters, 0.05f, 5f);
    public float LineWidthMeters => Mathf.Clamp(directLineWidthMeters, 0.015f, 1f);

    public static NewMapShelterDirectLineConfig Default()
    {
        return new NewMapShelterDirectLineConfig();
    }
}
