using UnityEngine;
using UnityEngine.UI;

public class NavigationGuidanceDisplay : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private RectTransform directionArrow;
    [SerializeField] private Text targetText;
    [SerializeField] private Text distanceText;
    [SerializeField] private Text estimatedTimeText;
    [SerializeField] private Text statusText;

    public NavigationGuidanceResult LastAppliedResult { get; private set; }

    public void Apply(NavigationGuidanceResult result)
    {
        LastAppliedResult = result;

        if (root != null)
        {
            root.SetActive(result != null);
        }

        if (result == null)
        {
            SetText(targetText, string.Empty);
            SetText(distanceText, string.Empty);
            SetText(estimatedTimeText, string.Empty);
            SetText(statusText, string.Empty);
            SetArrowActive(false);
            return;
        }

        SetText(targetText, result.hasTarget ? result.targetName : string.Empty);
        SetText(distanceText, result.distanceText);
        SetText(estimatedTimeText, result.estimatedTimeText);
        SetText(statusText, result.statusText);
        ApplyArrow(result);
    }

    public void BindForTests(
        RectTransform arrow,
        Text target,
        Text distance,
        Text estimatedTime,
        Text status)
    {
        directionArrow = arrow;
        targetText = target;
        distanceText = distance;
        estimatedTimeText = estimatedTime;
        statusText = status;
    }

    private void ApplyArrow(NavigationGuidanceResult result)
    {
        bool hasDirection = result.hasTarget && result.directionToTarget.sqrMagnitude > 0.000001f;
        SetArrowActive(hasDirection);

        if (!hasDirection || directionArrow == null)
        {
            return;
        }

        float angle = Mathf.Atan2(result.directionToTarget.x, result.directionToTarget.z) * Mathf.Rad2Deg;
        directionArrow.localRotation = Quaternion.Euler(0f, 0f, -angle);
    }

    private void SetArrowActive(bool active)
    {
        if (directionArrow != null)
        {
            directionArrow.gameObject.SetActive(active);
        }
    }

    private static void SetText(Text text, string value)
    {
        if (text != null)
        {
            text.text = value ?? string.Empty;
        }
    }
}
