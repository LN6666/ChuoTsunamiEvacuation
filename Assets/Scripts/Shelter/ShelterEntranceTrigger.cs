using UnityEngine;

[RequireComponent(typeof(Collider))]
public class ShelterEntranceTrigger : MonoBehaviour
{
    [SerializeField] private BuildingShelter shelter;
    [SerializeField] private EvacuationGameManager gameManager;
    [SerializeField] private GameUIManager gameUIManager;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private KeyCode interactKey = KeyCode.E;

    private bool playerInRange;

    public BuildingShelter Shelter => shelter;

    private void Reset()
    {
        ConfigureTrigger();
        shelter = GetComponentInParent<BuildingShelter>();
    }

    private void Awake()
    {
        ConfigureTrigger();

        if (shelter == null)
        {
            shelter = GetComponentInParent<BuildingShelter>();
        }
    }

    private void ConfigureTrigger()
    {
        Collider triggerCollider = GetComponent<Collider>();
        triggerCollider.isTrigger = true;
    }

    private void Update()
    {
        if (!playerInRange || !Input.GetKeyDown(interactKey))
        {
            return;
        }

        Debug.Log("E is pressed near shelter.");

        if (gameManager == null)
        {
            Debug.LogWarning($"{nameof(ShelterEntranceTrigger)} on {name} has no game manager assigned.", this);
            return;
        }

        gameManager.TryEnterShelter(shelter, this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInRange = true;
        Debug.Log("Player enters ShelterEntrance.");
        gameUIManager?.ShowInteractionPrompt(shelter);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag(playerTag))
        {
            return;
        }

        playerInRange = false;
        gameUIManager?.HideInteractionPrompt();
    }
}
