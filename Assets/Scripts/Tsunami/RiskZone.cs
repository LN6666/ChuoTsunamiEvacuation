using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RiskZone : MonoBehaviour
{
    [SerializeField] private EvacuationGameManager gameManager;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string failureReason = "The tsunami risk boundary reached you.";
    [SerializeField] private bool failureOnEnter = true;

    private void Reset()
    {
        ConfigureTrigger();
    }

    private void Awake()
    {
        ConfigureTrigger();
    }

    private void ConfigureTrigger()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!failureOnEnter || !other.CompareTag(playerTag))
        {
            return;
        }

        Debug.Log("Player enters RiskZone.");
        gameManager?.HandlePlayerEnteredRiskZone(failureReason);
    }
}
