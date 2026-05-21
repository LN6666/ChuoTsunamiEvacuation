using UnityEngine;

[DisallowMultipleComponent]
public class NpcStateLabel : MonoBehaviour
{
    [SerializeField] private NpcEvacuationAgent agent;
    [SerializeField] private Vector3 offset = new Vector3(0f, 1.6f, 0f);
    [SerializeField] private bool faceMainCamera = true;
    [SerializeField] private TextMesh textMesh;

    private void Awake()
    {
        if (agent == null)
        {
            agent = GetComponentInParent<NpcEvacuationAgent>();
        }

        EnsureTextMesh();
        Refresh();
    }

    private void LateUpdate()
    {
        Refresh();
        FaceCamera();
    }

    public void Bind(NpcEvacuationAgent targetAgent)
    {
        agent = targetAgent;
        EnsureTextMesh();
        Refresh();
    }

    private void EnsureTextMesh()
    {
        if (textMesh != null)
        {
            textMesh.transform.localPosition = offset;
            return;
        }

        GameObject labelObject = new GameObject("P6B_NPC_StateLabel");
        labelObject.transform.SetParent(transform, false);
        labelObject.transform.localPosition = offset;
        textMesh = labelObject.AddComponent<TextMesh>();
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.characterSize = 0.16f;
        textMesh.fontSize = 18;
        textMesh.color = Color.white;
    }

    private void Refresh()
    {
        if (textMesh == null)
        {
            return;
        }

        textMesh.text = agent != null ? agent.GetDebugText() : "NPC: unknown\nTarget: none";
    }

    private void FaceCamera()
    {
        if (!faceMainCamera || textMesh == null || Camera.main == null)
        {
            return;
        }

        Vector3 direction = textMesh.transform.position - Camera.main.transform.position;
        if (direction.sqrMagnitude > 0.0001f)
        {
            textMesh.transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up);
        }
    }
}
