using System.Collections.Generic;
using UnityEngine;

public static class ShelterGameplayDataMapper
{
    private const float FallbackStartX = 8f;
    private const float FallbackStartZ = -14f;
    private const float FallbackSpacingX = 8f;
    private const float FallbackSpacingZ = 10f;
    private const int FallbackColumns = 3;

    public static ShelterDataLoader.ShelterData[] MapRealSheltersToGameplayData(
        RealShelterDataLoader.RealShelterRecord[] realShelters)
    {
        if (realShelters == null || realShelters.Length == 0)
        {
            return new ShelterDataLoader.ShelterData[0];
        }

        var mappedShelters = new List<ShelterDataLoader.ShelterData>();

        for (int i = 0; i < realShelters.Length; i++)
        {
            ShelterDataLoader.ShelterData mappedShelter = MapRealShelterToGameplayData(realShelters[i], i);
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
            layoutPosition = ResolveLayoutPosition(realShelter, index),
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
}
