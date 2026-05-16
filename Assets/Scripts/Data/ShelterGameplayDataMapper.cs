using System.Collections.Generic;
using UnityEngine;

public static class ShelterGameplayDataMapper
{
    private const float FallbackStartX = 8f;
    private const float FallbackStartZ = -14f;
    private const float FallbackSpacingX = 8f;
    private const float FallbackSpacingZ = 10f;
    private const int FallbackColumns = 3;
    private const float PositionEpsilon = 0.0001f;

    public static ShelterDataLoader.ShelterData[] MapRealSheltersToGameplayData(
        RealShelterDataLoader.RealShelterRecord[] realShelters)
    {
        if (realShelters == null || realShelters.Length == 0)
        {
            return new ShelterDataLoader.ShelterData[0];
        }

        var mappedShelters = new List<ShelterDataLoader.ShelterData>();
        Dictionary<string, int> unityPositionCounts = CountUnityPositionKeys(realShelters);

        for (int i = 0; i < realShelters.Length; i++)
        {
            ShelterDataLoader.ShelterData mappedShelter = MapRealShelterToGameplayData(
                realShelters[i],
                i,
                ShouldUseFallbackLayout(realShelters[i], unityPositionCounts));
            if (mappedShelter != null)
            {
                mappedShelters.Add(mappedShelter);
            }
        }

        return mappedShelters.ToArray();
    }

    public static ShelterDataLoader.ShelterData MapRealShelterToGameplayData(
        RealShelterDataLoader.RealShelterRecord realShelter,
        int index)
    {
        return MapRealShelterToGameplayData(realShelter, index, false);
    }

    private static ShelterDataLoader.ShelterData MapRealShelterToGameplayData(
        RealShelterDataLoader.RealShelterRecord realShelter,
        int index,
        bool forceFallbackLayout)
    {
        if (realShelter == null || string.IsNullOrWhiteSpace(realShelter.shelterId))
        {
            return null;
        }

        int safeFloor = Mathf.Max(0, realShelter.safeFloor);
        float climbTimeSeconds = realShelter.climbTimeSeconds > 0f
            ? realShelter.climbTimeSeconds
            : Mathf.Max(1, safeFloor) * 3f;

        return new ShelterDataLoader.ShelterData
        {
            shelterId = realShelter.shelterId,
            shelterName = string.IsNullOrWhiteSpace(realShelter.shelterName)
                ? realShelter.shelterId
                : realShelter.shelterName,
            shelterRank = "Real",
            isOfficialShelter = realShelter.isOfficialShelter,
            canEnter = realShelter.canEnter,
            entryDelaySeconds = Mathf.Max(0f, realShelter.entryDelaySeconds),
            climbTimeSeconds = Mathf.Max(0.1f, climbTimeSeconds),
            crowdingDelaySeconds = Mathf.Max(0f, realShelter.crowdingDelaySeconds),
            failureReason = realShelter.canEnter
                ? "This shelter is not available."
                : "This real sample shelter is marked not enterable for debug validation.",
            sourceType = string.IsNullOrWhiteSpace(realShelter.sourceType)
                ? ShelterSourceConfigLoader.RealSampleSourceMode
                : realShelter.sourceType,
            facilityType = string.IsNullOrWhiteSpace(realShelter.facilityType)
                ? "unknown"
                : realShelter.facilityType,
            layoutPosition = forceFallbackLayout
                ? CreateDeterministicFallbackLayout(index)
                : ResolveLayoutPosition(realShelter, index),
            realFacilityName = realShelter.shelterName,
            address = realShelter.address,
            latitude = realShelter.latitude,
            longitude = realShelter.longitude,
            coordinateSystem = string.IsNullOrWhiteSpace(realShelter.coordinateSystem)
                ? "EPSG:4326"
                : realShelter.coordinateSystem,
            plateauBuildingId = string.Empty,
            safeFloor = safeFloor,
            capacity = Mathf.Max(0, realShelter.capacity),
            dataSource = realShelter.dataSource,
            sourceUrl = realShelter.sourceUrl,
            sourceUpdatedAt = realShelter.sourceUpdatedAt,
            notes = realShelter.notes
        };
    }

    public static ShelterDataLoader.LayoutPosition ResolveLayoutPosition(
        RealShelterDataLoader.RealShelterRecord realShelter,
        int index)
    {
        if (realShelter != null && realShelter.HasUnityPosition)
        {
            return new ShelterDataLoader.LayoutPosition
            {
                x = realShelter.unityPosition.x,
                y = realShelter.unityPosition.y,
                z = realShelter.unityPosition.z
            };
        }

        return CreateDeterministicFallbackLayout(index);
    }

    public static ShelterDataLoader.LayoutPosition CreateDeterministicFallbackLayout(int index)
    {
        int safeIndex = Mathf.Max(0, index);
        int column = safeIndex % FallbackColumns;
        int row = safeIndex / FallbackColumns;

        return new ShelterDataLoader.LayoutPosition
        {
            x = FallbackStartX + column * FallbackSpacingX,
            y = 0f,
            z = FallbackStartZ + row * FallbackSpacingZ
        };
    }

    private static Dictionary<string, int> CountUnityPositionKeys(RealShelterDataLoader.RealShelterRecord[] realShelters)
    {
        var counts = new Dictionary<string, int>();

        foreach (RealShelterDataLoader.RealShelterRecord realShelter in realShelters)
        {
            if (realShelter == null || !realShelter.HasUnityPosition)
            {
                continue;
            }

            string key = CreatePositionKey(realShelter.unityPosition.ToVector3());
            if (!counts.ContainsKey(key))
            {
                counts[key] = 0;
            }

            counts[key]++;
        }

        return counts;
    }

    private static bool ShouldUseFallbackLayout(
        RealShelterDataLoader.RealShelterRecord realShelter,
        Dictionary<string, int> unityPositionCounts)
    {
        if (realShelter == null || !realShelter.HasUnityPosition)
        {
            return true;
        }

        Vector3 unityPosition = realShelter.unityPosition.ToVector3();
        if (!IsZeroPosition(unityPosition))
        {
            return false;
        }

        string key = CreatePositionKey(unityPosition);
        return unityPositionCounts != null &&
            unityPositionCounts.TryGetValue(key, out int count) &&
            count > 1;
    }

    private static bool IsZeroPosition(Vector3 position)
    {
        return Mathf.Abs(position.x) <= PositionEpsilon &&
            Mathf.Abs(position.y) <= PositionEpsilon &&
            Mathf.Abs(position.z) <= PositionEpsilon;
    }

    private static string CreatePositionKey(Vector3 position)
    {
        return $"{position.x:0.####},{position.y:0.####},{position.z:0.####}";
    }
}
