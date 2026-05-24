using System;
using UnityEngine;

public class P9RouteGuidanceProxyMarker : MonoBehaviour
{
    public const bool RoutesAreOfficial = P9RuntimePolicy.ClaimsOfficialRoutes;
    public const bool AffectsGameplaySuccessFailure = P9RuntimePolicy.AffectsGameplaySuccessFailure;

    [SerializeField] private string routeProxyId = string.Empty;
    [SerializeField] private string targetProxyId = string.Empty;
    [SerializeField] private bool routesAreEstimatedPrototypeGuidance = true;
    [SerializeField] private bool routeRoadGeometryValidated;
    [SerializeField] private string guidanceStatus = "estimated_prototype_guidance";
    [SerializeField] private string notes = string.Empty;

    public string RouteProxyId => routeProxyId;
    public string TargetProxyId => targetProxyId;
    public bool RoutesAreEstimatedPrototypeGuidance => routesAreEstimatedPrototypeGuidance;
    public bool RouteRoadGeometryValidated => routeRoadGeometryValidated;
    public string GuidanceStatus => guidanceStatus;
    public string Notes => notes;

    public void ConfigureForEstimatedGuidance(string id, string targetId, Vector3 position, string markerNotes)
    {
        routeProxyId = id ?? string.Empty;
        targetProxyId = targetId ?? string.Empty;
        routesAreEstimatedPrototypeGuidance = true;
        routeRoadGeometryValidated = false;
        guidanceStatus = "estimated_prototype_guidance_not_official_route";
        notes = markerNotes ?? string.Empty;
        transform.position = position;
    }

    public P9RouteGuidanceProxySnapshot CreateSnapshot()
    {
        return new P9RouteGuidanceProxySnapshot
        {
            routeProxyId = routeProxyId ?? string.Empty,
            targetProxyId = targetProxyId ?? string.Empty,
            position = transform.position,
            routesAreOfficial = RoutesAreOfficial,
            routesAreEstimatedPrototypeGuidance = routesAreEstimatedPrototypeGuidance,
            routeRoadGeometryValidated = routeRoadGeometryValidated,
            affectsGameplaySuccessFailure = AffectsGameplaySuccessFailure,
            guidanceStatus = guidanceStatus ?? string.Empty
        };
    }
}

[Serializable]
public class P9RouteGuidanceProxySnapshot
{
    public string routeProxyId = string.Empty;
    public string targetProxyId = string.Empty;
    public Vector3 position;
    public bool routesAreOfficial;
    public bool routesAreEstimatedPrototypeGuidance;
    public bool routeRoadGeometryValidated;
    public bool affectsGameplaySuccessFailure;
    public string guidanceStatus = string.Empty;
}
