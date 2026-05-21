using UnityEngine;

public class NavigationGuidanceController : MonoBehaviour
{
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Transform targetTransform;
    [SerializeField] private BuildingShelter targetShelter;
    [SerializeField] private NavigationGuidanceDisplay display;
    [SerializeField] private float walkingSpeedMetersPerSecond =
        NavigationGuidanceCalculator.DefaultWalkingSpeedMetersPerSecond;
    [SerializeField] private bool updateEveryFrame = true;

    public static bool AffectsGameplayRules => false;
    public NavigationGuidanceResult LastGuidance { get; private set; }

    private void Start()
    {
        RefreshGuidance();
    }

    private void Update()
    {
        if (updateEveryFrame)
        {
            RefreshGuidance();
        }
    }

    public void SetPlayerTransform(Transform player)
    {
        playerTransform = player;
    }

    public void SetTargetTransform(Transform target)
    {
        targetTransform = target;
        targetShelter = target != null ? target.GetComponentInParent<BuildingShelter>() : null;
    }

    public void SetTargetShelter(BuildingShelter shelter)
    {
        targetShelter = shelter;
        targetTransform = shelter != null ? shelter.transform : null;
    }

    public void SetDisplay(NavigationGuidanceDisplay guidanceDisplay)
    {
        display = guidanceDisplay;
    }

    public void SetWalkingSpeedMetersPerSecond(float walkingSpeed)
    {
        walkingSpeedMetersPerSecond = walkingSpeed;
    }

    public NavigationGuidanceResult RefreshGuidance()
    {
        Vector3 playerPosition = playerTransform != null ? playerTransform.position : transform.position;
        LastGuidance = NavigationGuidanceCalculator.BuildGuidance(
            playerPosition,
            ResolveTargetInfo(),
            walkingSpeedMetersPerSecond);

        if (display != null)
        {
            display.Apply(LastGuidance);
        }

        return LastGuidance;
    }

    private NavigationTargetInfo ResolveTargetInfo()
    {
        if (targetShelter != null && NavigationTargetInfo.TryFromShelter(targetShelter, out NavigationTargetInfo shelterInfo))
        {
            return shelterInfo;
        }

        if (targetTransform != null)
        {
            BuildingShelter shelter = targetTransform.GetComponentInParent<BuildingShelter>();
            if (shelter != null && NavigationTargetInfo.TryFromShelter(shelter, out NavigationTargetInfo parentShelterInfo))
            {
                return parentShelterInfo;
            }

            return NavigationTargetInfo.FromTransform(targetTransform);
        }

        return null;
    }
}
