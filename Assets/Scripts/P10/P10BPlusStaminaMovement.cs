using System;
using UnityEngine;
using UnityEngine.UI;

public interface IP10BPlusMovementSpeedProvider
{
    float ResolveMovementSpeed(float fallbackWalkSpeed, float fallbackSprintSpeed, bool sprintRequested, float deltaTime, out bool sprintActive);
}

[Serializable]
public class P10BPlusMovementConfig
{
    public string schemaVersion = "p10b_plus.movement_stamina_config.v1";
    public float normalWalkSpeedMetersPerSecond = 0.5f;
    public float sprintSpeedMetersPerSecond = 2.5f;
    public float initialStaminaFraction = 1f;
    public float slowDrainPerSecond = 0.08f;
    public float sustainedDrainPerSecond = 0.16f;
    public float exhaustionDrainPerSecond = 0.24f;
    public float sustainedSprintThresholdSeconds = 4f;
    public float exhaustionSprintThresholdSeconds = 10f;
    public float exhaustionLockSeconds = 15f;
    public float recovery30Seconds = 15f;
    public float recovery50Seconds = 45f;
    public float recovery100Seconds = 90f;
    public float recovery30Fraction = 0.3f;
    public float recovery50Fraction = 0.5f;
    public float recovery100Fraction = 1f;

    public bool IsSpeedConfigValid()
    {
        return normalWalkSpeedMetersPerSecond > 0f &&
            normalWalkSpeedMetersPerSecond <= 0.75f &&
            sprintSpeedMetersPerSecond >= 2f &&
            sprintSpeedMetersPerSecond <= 3f;
    }
}

[Serializable]
public class P10BPlusStaminaState
{
    public float staminaFraction = 1f;
    public bool sprintLocked;
    public bool sprinting;
    public float exhaustionLockRemainingSeconds;
    public float activeSprintSeconds;
    public float secondsSinceSprintStopped;

    public bool CanSprint => !sprintLocked && staminaFraction > 0f;
}

public class P10BPlusStaminaController
{
    private readonly P10BPlusMovementConfig config;
    private readonly P10BPlusStaminaState state;

    public P10BPlusStaminaController(P10BPlusMovementConfig movementConfig)
    {
        config = movementConfig ?? new P10BPlusMovementConfig();
        state = new P10BPlusStaminaState
        {
            staminaFraction = Mathf.Clamp01(config.initialStaminaFraction)
        };
    }

    public P10BPlusStaminaState State => state;

    public bool Tick(float deltaTime, bool sprintRequested)
    {
        deltaTime = Mathf.Max(0f, deltaTime);
        if (sprintRequested && state.CanSprint)
        {
            state.sprinting = true;
            state.secondsSinceSprintStopped = 0f;
            state.activeSprintSeconds += deltaTime;
            state.staminaFraction = Mathf.Clamp01(state.staminaFraction - GetDrainRate() * deltaTime);
            if (state.staminaFraction <= 0f)
            {
                state.staminaFraction = 0f;
                state.sprinting = false;
                state.sprintLocked = true;
                state.exhaustionLockRemainingSeconds = Mathf.Max(0f, config.exhaustionLockSeconds);
                state.activeSprintSeconds = 0f;
            }

            return state.sprinting;
        }

        state.sprinting = false;
        state.activeSprintSeconds = 0f;
        state.secondsSinceSprintStopped += deltaTime;
        Recover(deltaTime);
        return false;
    }

    public float GetDrainRateForTesting(float activeSprintSeconds)
    {
        state.activeSprintSeconds = Mathf.Max(0f, activeSprintSeconds);
        return GetDrainRate();
    }

    private float GetDrainRate()
    {
        if (state.activeSprintSeconds >= config.exhaustionSprintThresholdSeconds || state.staminaFraction <= 0.25f)
        {
            return Mathf.Max(config.exhaustionDrainPerSecond, config.sustainedDrainPerSecond);
        }

        if (state.activeSprintSeconds >= config.sustainedSprintThresholdSeconds)
        {
            return Mathf.Max(config.sustainedDrainPerSecond, config.slowDrainPerSecond);
        }

        return Mathf.Max(0f, config.slowDrainPerSecond);
    }

    private void Recover(float deltaTime)
    {
        if (state.sprintLocked)
        {
            state.exhaustionLockRemainingSeconds = Mathf.Max(0f, state.exhaustionLockRemainingSeconds - deltaTime);
            if (state.exhaustionLockRemainingSeconds <= 0f)
            {
                state.sprintLocked = false;
                state.staminaFraction = Mathf.Max(state.staminaFraction, Mathf.Clamp01(config.recovery30Fraction));
            }
        }

        if (state.secondsSinceSprintStopped >= config.recovery100Seconds)
        {
            state.staminaFraction = Mathf.Max(state.staminaFraction, Mathf.Clamp01(config.recovery100Fraction));
            return;
        }

        if (state.secondsSinceSprintStopped >= config.recovery50Seconds)
        {
            state.staminaFraction = Mathf.Max(state.staminaFraction, Mathf.Clamp01(config.recovery50Fraction));
            return;
        }

        if (state.secondsSinceSprintStopped >= config.recovery30Seconds && !state.sprintLocked)
        {
            state.staminaFraction = Mathf.Max(state.staminaFraction, Mathf.Clamp01(config.recovery30Fraction));
        }
    }
}

public static class P10BPlusMovementSpeedResolver
{
    public static float ResolveSpeed(
        P10BPlusMovementConfig movementConfig,
        P10BPlusWeatherConfig weatherConfig,
        P10BPlusAvatarMobilityConfig avatarConfig,
        P10BPlusAvatarPresentation presentation,
        string mobilityProfileId,
        P10BPlusWeatherMode weatherMode,
        bool sprintActive)
    {
        movementConfig = movementConfig ?? new P10BPlusMovementConfig();
        float baseSpeed = sprintActive
            ? movementConfig.sprintSpeedMetersPerSecond
            : movementConfig.normalWalkSpeedMetersPerSecond;
        float weatherMultiplier = P10BPlusWeatherMovementModifier.GetMultiplier(weatherConfig, weatherMode);
        float mobilityMultiplier = P10BPlusPlayerProfileResolver.ResolveSpeedMultiplier(
            avatarConfig,
            presentation,
            mobilityProfileId);
        return Mathf.Max(0f, baseSpeed * weatherMultiplier * mobilityMultiplier);
    }
}

public class P10BPlusMovementRuntimeAdapter : MonoBehaviour, IP10BPlusMovementSpeedProvider
{
    [SerializeField] private P10BPlusMovementConfig movementConfig = new P10BPlusMovementConfig();
    [SerializeField] private P10BPlusWeatherConfig weatherConfig = new P10BPlusWeatherConfig();
    [SerializeField] private P10BPlusAvatarMobilityConfig avatarMobilityConfig = new P10BPlusAvatarMobilityConfig();
    [SerializeField] private P10BPlusWeatherMode weatherMode = P10BPlusWeatherMode.ClearDay;
    [SerializeField] private P10BPlusAvatarPresentation avatarPresentation = P10BPlusAvatarPresentation.Male;
    [SerializeField] private string mobilityProfileId = "standard";

    private P10BPlusStaminaController staminaController;

    public P10BPlusStaminaState StaminaState => EnsureStamina().State;

    public void Configure(
        P10BPlusMovementConfig newMovementConfig,
        P10BPlusWeatherConfig newWeatherConfig,
        P10BPlusAvatarMobilityConfig newAvatarConfig,
        P10BPlusWeatherMode newWeatherMode,
        P10BPlusAvatarPresentation newPresentation,
        string newMobilityProfileId)
    {
        movementConfig = newMovementConfig ?? new P10BPlusMovementConfig();
        weatherConfig = newWeatherConfig ?? new P10BPlusWeatherConfig();
        avatarMobilityConfig = newAvatarConfig ?? new P10BPlusAvatarMobilityConfig();
        weatherMode = newWeatherMode;
        avatarPresentation = newPresentation;
        mobilityProfileId = string.IsNullOrWhiteSpace(newMobilityProfileId) ? "standard" : newMobilityProfileId;
        staminaController = new P10BPlusStaminaController(movementConfig);
    }

    public float ResolveMovementSpeed(float fallbackWalkSpeed, float fallbackSprintSpeed, bool sprintRequested, float deltaTime, out bool sprintActive)
    {
        sprintActive = EnsureStamina().Tick(deltaTime, sprintRequested);
        return P10BPlusMovementSpeedResolver.ResolveSpeed(
            movementConfig,
            weatherConfig,
            avatarMobilityConfig,
            avatarPresentation,
            mobilityProfileId,
            weatherMode,
            sprintActive);
    }

    private P10BPlusStaminaController EnsureStamina()
    {
        return staminaController ?? (staminaController = new P10BPlusStaminaController(movementConfig));
    }
}

public class P10BPlusStaminaHud : MonoBehaviour
{
    [SerializeField] private Text staminaText;

    public void Refresh(P10BPlusStaminaState state)
    {
        if (staminaText == null)
        {
            staminaText = GetComponent<Text>();
        }

        if (staminaText == null || state == null)
        {
            return;
        }

        bool visible = state.sprinting || state.sprintLocked || state.staminaFraction < 1f;
        staminaText.gameObject.SetActive(visible);
        staminaText.text = "Stamina: " + Mathf.RoundToInt(Mathf.Clamp01(state.staminaFraction) * 100f) + "%";
        P10BPlusUiLayoutSafety.ApplyDynamicTextSafety(staminaText);
    }
}
