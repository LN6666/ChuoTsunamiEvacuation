using UnityEngine;

public static class P5DRoutePreviewTransformValidator
{
    public const string UnityDebugCoordinateSystem = "UNITY_DEBUG";

    private const float MinPlausibleSpanMeters = 0.25f;
    private const float DefaultMaxPlausibleSpanMeters = 5000f;
    private const float DefaultMaxEndpointDistanceMeters = 20f;

    public class ValidationResult
    {
        public bool canRender;
        public string reason = string.Empty;
        public Vector3[] localPositions = new Vector3[0];
        public float startDistanceMeters;
        public float endDistanceMeters;
        public float horizontalSpanMeters;
        public string routeDisclaimer = P5CStaticDataLoader.EstimatedPrototypeRouteLabel;
    }

    public static ValidationResult ValidateForSelectedRoutePreview(
        P5CStaticDataLoader.RouteSampleRecord route,
        Vector3 expectedStartLocalPosition,
        Vector3 expectedEndLocalPosition)
    {
        return ValidateForSelectedRoutePreview(
            route,
            expectedStartLocalPosition,
            expectedEndLocalPosition,
            DefaultMaxEndpointDistanceMeters,
            DefaultMaxPlausibleSpanMeters);
    }

    public static ValidationResult ValidateForSelectedRoutePreview(
        P5CStaticDataLoader.RouteSampleRecord route,
        Vector3 expectedStartLocalPosition,
        Vector3 expectedEndLocalPosition,
        float maxEndpointDistanceMeters,
        float maxPlausibleSpanMeters)
    {
        var result = new ValidationResult();

        if (route == null || route.geometry == null || !route.geometry.HasCoordinates)
        {
            result.reason = "route geometry is missing or has fewer than two points";
            return result;
        }

        string coordinateSystem = route.geometry.coordinateReferenceSystem;
        if (!string.Equals(coordinateSystem, UnityDebugCoordinateSystem, System.StringComparison.OrdinalIgnoreCase))
        {
            result.reason =
                $"route geometry is {FirstNonEmpty(coordinateSystem, "unknown CRS")}; no verified WGS84-to-Unity/PLATEAU transform exists";
            return result;
        }

        Vector3[] positions = ConvertUnityDebugCoordinates(route.geometry.coordinates);
        if (positions.Length < 2)
        {
            result.reason = "route geometry did not produce enough Unity preview points";
            return result;
        }

        if (!AllFinite(positions))
        {
            result.reason = "route geometry contains invalid coordinates";
            return result;
        }

        result.horizontalSpanMeters = CalculateHorizontalSpan(positions);
        if (result.horizontalSpanMeters < MinPlausibleSpanMeters)
        {
            result.reason = "route geometry collapsed to a point";
            return result;
        }

        if (result.horizontalSpanMeters > Mathf.Max(MinPlausibleSpanMeters, maxPlausibleSpanMeters))
        {
            result.reason = "route geometry span is too large for the current Unity preview layout";
            return result;
        }

        result.startDistanceMeters = HorizontalDistance(positions[0], expectedStartLocalPosition);
        result.endDistanceMeters = HorizontalDistance(positions[positions.Length - 1], expectedEndLocalPosition);
        float endpointLimit = Mathf.Max(0.01f, maxEndpointDistanceMeters);

        if (result.startDistanceMeters > endpointLimit)
        {
            result.reason = "route start is not near the expected origin proxy";
            return result;
        }

        if (result.endDistanceMeters > endpointLimit)
        {
            result.reason = "route end is not near the selected shelter proxy";
            return result;
        }

        result.localPositions = positions;
        result.canRender = true;
        result.reason = "route geometry passed Unity preview validation";
        return result;
    }

    private static Vector3[] ConvertUnityDebugCoordinates(Vector2[] coordinates)
    {
        if (coordinates == null)
        {
            return new Vector3[0];
        }

        var positions = new Vector3[coordinates.Length];
        for (int i = 0; i < coordinates.Length; i++)
        {
            positions[i] = new Vector3(coordinates[i].x, 0.12f, coordinates[i].y);
        }

        return positions;
    }

    private static bool AllFinite(Vector3[] positions)
    {
        foreach (Vector3 position in positions)
        {
            if (float.IsNaN(position.x) || float.IsInfinity(position.x) ||
                float.IsNaN(position.y) || float.IsInfinity(position.y) ||
                float.IsNaN(position.z) || float.IsInfinity(position.z))
            {
                return false;
            }
        }

        return true;
    }

    private static float CalculateHorizontalSpan(Vector3[] positions)
    {
        if (positions == null || positions.Length == 0)
        {
            return 0f;
        }

        float minX = positions[0].x;
        float maxX = positions[0].x;
        float minZ = positions[0].z;
        float maxZ = positions[0].z;

        foreach (Vector3 position in positions)
        {
            minX = Mathf.Min(minX, position.x);
            maxX = Mathf.Max(maxX, position.x);
            minZ = Mathf.Min(minZ, position.z);
            maxZ = Mathf.Max(maxZ, position.z);
        }

        return Mathf.Sqrt(Mathf.Pow(maxX - minX, 2f) + Mathf.Pow(maxZ - minZ, 2f));
    }

    private static float HorizontalDistance(Vector3 first, Vector3 second)
    {
        return Vector2.Distance(new Vector2(first.x, first.z), new Vector2(second.x, second.z));
    }

    private static string FirstNonEmpty(string value, string fallback)
    {
        return string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
    }
}
