using System.Text;

public static class ShelterDebugMetadataFormatter
{
    public static string BuildMarkerLabel(ShelterDataLoader.ShelterData shelterData)
    {
        return BuildCompactMarkerLabel(shelterData);
    }

    public static string BuildCompactMarkerLabel(ShelterDataLoader.ShelterData shelterData)
    {
        if (shelterData == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        AppendLine(builder, shelterData.shelterName);
        AppendMetadata(builder, "Type", shelterData.facilityType);

        if (shelterData.capacity > 0)
        {
            AppendLine(builder, $"Cap: {shelterData.capacity}");
        }

        if (shelterData.safeFloor > 0)
        {
            AppendLine(builder, $"Safe floor: {shelterData.safeFloor}");
        }

        return builder.ToString().TrimEnd();
    }

    public static string BuildDetailedMarkerLabel(ShelterDataLoader.ShelterData shelterData)
    {
        if (shelterData == null)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        AppendLine(builder, shelterData.shelterName);
        AppendMetadata(builder, "Type", shelterData.facilityType);
        AppendMetadata(builder, "Address", shelterData.address);
        AppendMetadata(builder, "Source", FirstNonEmpty(shelterData.sourceType, shelterData.dataSource));

        if (shelterData.capacity > 0)
        {
            AppendLine(builder, $"Capacity: {shelterData.capacity}");
        }
        else
        {
            AppendLine(builder, "Capacity: unknown");
        }

        if (shelterData.safeFloor > 0)
        {
            AppendLine(builder, $"Safe floor estimate: {shelterData.safeFloor}");
        }

        AppendMetadata(builder, "Updated", shelterData.sourceUpdatedAt);
        AppendMetadata(builder, "Notes", shelterData.notes);
        return builder.ToString().TrimEnd();
    }

    private static void AppendMetadata(StringBuilder builder, string label, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        AppendLine(builder, $"{label}: {value.Trim()}");
    }

    private static void AppendLine(StringBuilder builder, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return;
        }

        builder.AppendLine(value.Trim());
    }

    private static string FirstNonEmpty(string first, string second)
    {
        if (!string.IsNullOrWhiteSpace(first))
        {
            return first.Trim();
        }

        return string.IsNullOrWhiteSpace(second) ? string.Empty : second.Trim();
    }
}
