using UnityEngine;

public class P8RiskFrontDebugStatus : MonoBehaviour
{
    public const bool AffectsGameplaySuccessFailure = false;

    [SerializeField] private TextMesh label;
    [SerializeField] private bool createLabelIfMissing = true;

    public string LastStatus { get; private set; } = string.Empty;

    public void SetStatus(string status, bool failSafe)
    {
        EnsureLabel();
        LastStatus = (failSafe ? "Fail-safe hidden. " : string.Empty) +
                     P8RiskFrontVisualConfig.CinematicDisclaimer + " " +
                     (status ?? string.Empty);

        if (label != null)
        {
            label.text = LastStatus;
        }
    }

    private void EnsureLabel()
    {
        if (label != null || !createLabelIfMissing)
        {
            return;
        }

        var labelObject = new GameObject("P8_RiskFront_CinematicDisclaimer_Label");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = Vector3.up * 2f;
        label = labelObject.AddComponent<TextMesh>();
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.25f;
        label.color = Color.cyan;
    }
}
