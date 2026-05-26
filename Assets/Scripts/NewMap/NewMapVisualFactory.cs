using UnityEngine;

public static class NewMapVisualFactory
{
    public static Material CreateMaterial(string name, Color color, bool transparent = false)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            shader = Shader.Find("Standard");
        }
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }
        if (shader == null)
        {
            shader = Shader.Find("Sprites/Default");
        }
        if (shader == null)
        {
            Debug.LogWarning("NewMap visual material could not resolve a shader. Renderers will keep their default material.");
            return null;
        }

        Material material = new Material(shader)
        {
            name = name,
            color = color
        };

        if (transparent)
        {
            material.SetFloat("_Surface", 1f);
            material.SetFloat("_Mode", 3f);
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
        }

        return material;
    }

    public static void CreateHumanoid(Transform parent, string rootName, Color color)
    {
        Material material = CreateMaterial(rootName + "_Material", color);
        GameObject visualRoot = new GameObject(rootName);
        visualRoot.transform.SetParent(parent, false);
        visualRoot.transform.localPosition = Vector3.zero;

        GameObject body = CreatePrimitive("Body", PrimitiveType.Cylinder, visualRoot.transform, new Vector3(0f, 0.95f, 0f), new Vector3(0.42f, 0.58f, 0.42f), material);
        GameObject head = CreatePrimitive("Head", PrimitiveType.Sphere, visualRoot.transform, new Vector3(0f, 1.72f, 0f), new Vector3(0.42f, 0.42f, 0.42f), material);
        GameObject leftLeg = CreatePrimitive("LeftLeg", PrimitiveType.Cylinder, visualRoot.transform, new Vector3(-0.14f, 0.32f, 0f), new Vector3(0.16f, 0.35f, 0.16f), material);
        GameObject rightLeg = CreatePrimitive("RightLeg", PrimitiveType.Cylinder, visualRoot.transform, new Vector3(0.14f, 0.32f, 0f), new Vector3(0.16f, 0.35f, 0.16f), material);

        RemoveCollider(body);
        RemoveCollider(head);
        RemoveCollider(leftLeg);
        RemoveCollider(rightLeg);
    }

    public static GameObject CreatePrimitive(
        string name,
        PrimitiveType primitiveType,
        Transform parent,
        Vector3 localPosition,
        Vector3 localScale,
        Material material)
    {
        GameObject gameObject = GameObject.CreatePrimitive(primitiveType);
        gameObject.name = name;
        gameObject.transform.SetParent(parent, false);
        gameObject.transform.localPosition = localPosition;
        gameObject.transform.localScale = localScale;

        Renderer renderer = gameObject.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        return gameObject;
    }

    public static void RemoveCollider(GameObject gameObject)
    {
        Collider collider = gameObject.GetComponent<Collider>();
        if (collider != null)
        {
            Object.Destroy(collider);
        }
    }
}
