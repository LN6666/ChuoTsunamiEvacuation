using UnityEngine;

public class P5DRoutePreviewMetadata : MonoBehaviour
{
    [SerializeField] private string routeId;
    [SerializeField] private string shelterId;
    [SerializeField] private string routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel;
    [SerializeField] private string osmAttribution;
    [SerializeField] private string validationReason;

    public string RouteId => routeId;
    public string ShelterId => shelterId;
    public string RouteDisclaimer => routeDisclaimer;
    public string OsmAttribution => osmAttribution;
    public string ValidationReason => validationReason;

    public void Apply(
        P5CStaticDataLoader.RouteSampleRecord route,
        string selectedShelterId,
        string attribution,
        P5DRoutePreviewTransformValidator.ValidationResult validationResult)
    {
        routeId = route != null ? route.routeId : string.Empty;
        shelterId = selectedShelterId ?? string.Empty;
        routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel;
        osmAttribution = attribution ?? string.Empty;
        validationReason = validationResult != null ? validationResult.reason : string.Empty;
    }
}
