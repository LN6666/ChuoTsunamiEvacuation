using System.Collections;
using UnityEngine;

public class ClimbSimulation : MonoBehaviour
{
    private Coroutine activeClimb;
    private EvacuationGameManager activeGameManager;

    public bool IsClimbing => activeClimb != null;

    public void StartClimb(BuildingShelter shelter, EvacuationGameManager gameManager)
    {
        StartClimb(shelter, null, gameManager);
    }

    public void StartClimb(BuildingShelter shelter, ShelterEntranceTrigger shelterEntrance, EvacuationGameManager gameManager)
    {
        CancelClimb();

        if (shelter == null || gameManager == null)
        {
            Debug.LogWarning($"{nameof(ClimbSimulation)} could not start because shelter or game manager was missing.", this);
            return;
        }

        activeGameManager = gameManager;
        Debug.Log("Climb starts.");
        activeClimb = StartCoroutine(ClimbRoutine(shelter));
    }

    public void CancelClimb()
    {
        if (activeClimb != null)
        {
            StopCoroutine(activeClimb);
            activeClimb = null;
        }

        activeGameManager?.HandleClimbProgress(0f);
        activeGameManager = null;
    }

    private IEnumerator ClimbRoutine(BuildingShelter shelter)
    {
        float duration = shelter.ClimbTimeSeconds;
        float elapsed = 0f;

        activeGameManager?.HandleClimbProgress(0f);

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            activeGameManager?.HandleClimbProgress(elapsed / duration);
            yield return null;
        }

        EvacuationGameManager completedGameManager = activeGameManager;
        activeClimb = null;
        activeGameManager = null;
        Debug.Log("Climb completes.");
        completedGameManager?.HandleClimbCompleted(shelter);
    }
}
