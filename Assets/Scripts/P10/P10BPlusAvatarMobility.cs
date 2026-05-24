using System;
using UnityEngine;

public enum P10BPlusAvatarPresentation
{
    Male,
    Female
}

[Serializable]
public class P10BPlusAvatarMobilityConfig
{
    public string schemaVersion = "p10b_plus.avatar_mobility_config.v1";
    public bool avatarPresentationRandomizedFiftyFifty = true;
    public bool mobilityProfileSeparateFromAvatarPresentation = true;
    public bool enableGenderSpeedModifier;
    public float femalePresentationSpeedMultiplier = 0.85f;
    public string genderSpeedModifierPolicy = "disabled_by_default_optional_scenario_assumption_not_real_world_claim";
    public P10BPlusMobilityProfile[] mobilityProfiles = Array.Empty<P10BPlusMobilityProfile>();

    public bool IsEthicallySafeDefault()
    {
        return avatarPresentationRandomizedFiftyFifty &&
            mobilityProfileSeparateFromAvatarPresentation &&
            !enableGenderSpeedModifier &&
            femalePresentationSpeedMultiplier > 0f &&
            genderSpeedModifierPolicy.IndexOf("not_real_world_claim", StringComparison.OrdinalIgnoreCase) >= 0;
    }
}

[Serializable]
public class P10BPlusMobilityProfile
{
    public string profileId = "standard";
    public float speedMultiplier = 1f;
    public string notes = string.Empty;
}

public static class P10BPlusPlayerProfileResolver
{
    public static P10BPlusAvatarPresentation ResolveBalancedPresentation(int deterministicSeed, int playerIndex)
    {
        int value = Mathf.Abs(deterministicSeed + playerIndex);
        return value % 2 == 0 ? P10BPlusAvatarPresentation.Male : P10BPlusAvatarPresentation.Female;
    }

    public static float ResolveSpeedMultiplier(
        P10BPlusAvatarMobilityConfig config,
        P10BPlusAvatarPresentation presentation,
        string mobilityProfileId)
    {
        config = config ?? new P10BPlusAvatarMobilityConfig();
        float multiplier = ResolveMobilityProfileMultiplier(config, mobilityProfileId);
        if (config.enableGenderSpeedModifier && presentation == P10BPlusAvatarPresentation.Female)
        {
            multiplier *= Mathf.Clamp(config.femalePresentationSpeedMultiplier, 0.1f, 2f);
        }

        return Mathf.Max(0f, multiplier);
    }

    private static float ResolveMobilityProfileMultiplier(P10BPlusAvatarMobilityConfig config, string mobilityProfileId)
    {
        if (config.mobilityProfiles != null)
        {
            for (int i = 0; i < config.mobilityProfiles.Length; i++)
            {
                P10BPlusMobilityProfile profile = config.mobilityProfiles[i];
                if (profile != null && string.Equals(profile.profileId, mobilityProfileId, StringComparison.Ordinal))
                {
                    return profile.speedMultiplier;
                }
            }
        }

        return 1f;
    }
}
