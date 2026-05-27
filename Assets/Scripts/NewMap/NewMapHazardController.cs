using UnityEngine;

public sealed class NewMapHazardController : MonoBehaviour
{
    private GameObject lightCurtain;
    private GameObject debrisWarning;
    private Vector3 curtainStart;
    private Vector3 curtainEnd;
    private Vector3 debrisCenter;
    private Transform hazardRoot;
    private Transform debrisRoot;
    private float stageElapsed;
    private float frontDurationSeconds = 90f;
    private float debrisExposureSeconds;
    private Bounds debrisBounds;

    public NewMapTsunamiStage Stage { get; private set; } = NewMapTsunamiStage.Inactive;
    public bool RiskChecksActive => Stage == NewMapTsunamiStage.FrontApproaching;
    public float NormalizedFrontProgress => Mathf.Clamp01(stageElapsed / Mathf.Max(1f, frontDurationSeconds));
    public float DebrisExposureSeconds => debrisExposureSeconds;
    public Vector3 DebrisCenterForDiagnostics => debrisBounds.center;
    public bool Stage2VisualsBuiltForDiagnostics => lightCurtain != null && debrisWarning != null;
    public bool LightCurtainVisibleForDiagnostics => lightCurtain != null && lightCurtain.activeSelf;

    public static NewMapHazardController Create(Transform hazardRoot, Transform debrisRoot, Vector3 spawnPosition)
    {
        GameObject controllerObject = new GameObject("NewMap_HazardController");
        controllerObject.transform.SetParent(hazardRoot, false);
        NewMapHazardController controller = controllerObject.AddComponent<NewMapHazardController>();
        controller.Build(hazardRoot, debrisRoot, spawnPosition);
        return controller;
    }

    public void SetStage(NewMapTsunamiStage stage)
    {
        Stage = stage;
        stageElapsed = 0f;
        debrisExposureSeconds = 0f;

        if (stage == NewMapTsunamiStage.FrontApproaching)
        {
            EnsureStage2VisualsBuilt();
        }

        if (lightCurtain != null)
        {
            lightCurtain.SetActive(stage == NewMapTsunamiStage.FrontApproaching);
            lightCurtain.transform.position = curtainStart;
        }

        if (debrisWarning != null)
        {
            debrisWarning.SetActive(stage == NewMapTsunamiStage.FrontApproaching);
        }
    }

    public void Tick(float deltaTime)
    {
        if (Stage != NewMapTsunamiStage.FrontApproaching)
        {
            return;
        }

        stageElapsed += Mathf.Max(0f, deltaTime);
        if (lightCurtain != null)
        {
            lightCurtain.transform.position = Vector3.Lerp(curtainStart, curtainEnd, NormalizedFrontProgress);
        }
    }

    public bool IsPlayerReachedByFront(Vector3 playerPosition)
    {
        if (!RiskChecksActive)
        {
            return false;
        }

        EnsureStage2VisualsBuilt();
        return lightCurtain != null && playerPosition.x < lightCurtain.transform.position.x - 0.5f;
    }

    public bool IsPlayerInDebrisExposure(Vector3 playerPosition, float deltaTime, out string reason)
    {
        reason = string.Empty;
        if (!RiskChecksActive)
        {
            debrisExposureSeconds = Mathf.Max(0f, debrisExposureSeconds - deltaTime);
            return false;
        }

        EnsureStage2VisualsBuilt();
        if (!debrisBounds.Contains(playerPosition))
        {
            debrisExposureSeconds = Mathf.Max(0f, debrisExposureSeconds - deltaTime);
            return false;
        }

        debrisExposureSeconds += deltaTime;
        if (debrisExposureSeconds < 4f)
        {
            return false;
        }

        reason = "collapse_debris_exposure: player remained in the marked debris hazard area during Stage 2.";
        return true;
    }

    private void Build(Transform hazardRoot, Transform debrisRoot, Vector3 spawnPosition)
    {
        this.hazardRoot = hazardRoot;
        this.debrisRoot = debrisRoot;
        curtainStart = spawnPosition + new Vector3(-70f, 14f, 0f);
        curtainEnd = spawnPosition + new Vector3(70f, 14f, 0f);
        debrisCenter = spawnPosition + new Vector3(10f, 1f, 10f);
        debrisBounds = new Bounds(debrisCenter, new Vector3(7f, 2f, 7f));
        SetStage(NewMapTsunamiStage.Inactive);
    }

    private void EnsureStage2VisualsBuilt()
    {
        if (lightCurtain != null && debrisWarning != null)
        {
            return;
        }

        Material curtainMaterial = NewMapVisualFactory.CreateMaterial("NewMap_LightCurtain_Material", new Color(0.05f, 0.75f, 1f, 0.32f), true);
        Material debrisMaterial = NewMapVisualFactory.CreateMaterial("NewMap_DebrisWarning_Material", new Color(1f, 0.45f, 0.08f, 0.45f), true);

        lightCurtain = GameObject.CreatePrimitive(PrimitiveType.Cube);
        lightCurtain.name = "NewMap_Stage2_LightCurtain_RiskFront";
        lightCurtain.transform.SetParent(hazardRoot, true);
        lightCurtain.transform.position = curtainStart;
        lightCurtain.transform.localScale = new Vector3(0.6f, 28f, 130f);
        SetMaterial(lightCurtain, curtainMaterial);
        NewMapVisualFactory.RemoveCollider(lightCurtain);

        debrisWarning = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debrisWarning.name = "NewMap_CollapseDebris_ExposureWarning";
        debrisWarning.transform.SetParent(debrisRoot, true);
        debrisWarning.transform.position = debrisCenter;
        debrisWarning.transform.localScale = new Vector3(7f, 2f, 7f);
        SetMaterial(debrisWarning, debrisMaterial);
        NewMapVisualFactory.RemoveCollider(debrisWarning);
        debrisBounds = new Bounds(debrisWarning.transform.position, debrisWarning.transform.localScale);

        bool visible = Stage == NewMapTsunamiStage.FrontApproaching;
        lightCurtain.SetActive(visible);
        debrisWarning.SetActive(visible);
    }

    private static void SetMaterial(GameObject gameObject, Material material)
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }
    }
}
