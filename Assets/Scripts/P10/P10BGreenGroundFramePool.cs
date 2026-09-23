using System.Collections.Generic;
using UnityEngine;

public class P10BGreenGroundFramePool
{
    private readonly List<GameObject> frameObjects = new List<GameObject>();
    private Material sharedFrameMaterial;

    public int CreatedCount => frameObjects.Count;

    public int Prewarm(int count, Transform parent)
    {
        count = Mathf.Max(0, count);
        int targetCount = Mathf.Max(CreatedCount, count);
        while (CreatedCount < targetCount)
        {
            GameObject frame = CreateFrameObject(parent);
            frame.SetActive(false);
            frameObjects.Add(frame);
        }

        return CreatedCount;
    }

    public GameObject Acquire(Transform parent)
    {
        for (int i = 0; i < frameObjects.Count; i++)
        {
            GameObject candidate = frameObjects[i];
            if (candidate != null && !candidate.activeSelf)
            {
                candidate.transform.SetParent(parent, false);
                candidate.SetActive(true);
                return candidate;
            }
        }

        GameObject frameObject = CreateFrameObject(parent);
        frameObjects.Add(frameObject);
        return frameObject;
    }

    private GameObject CreateFrameObject(Transform parent)
    {
        var frameObject = new GameObject("P10B_GreenGroundFrame");
        frameObject.transform.SetParent(parent, false);
        var line = frameObject.AddComponent<LineRenderer>();
        line.useWorldSpace = false;
        line.loop = false;
        line.positionCount = 5;
        line.textureMode = LineTextureMode.Stretch;
        Material material = GetSharedMaterial();
        if (material != null)
        {
            line.sharedMaterial = material;
        }

        return frameObject;
    }

    public void ReleaseAll()
    {
        for (int i = 0; i < frameObjects.Count; i++)
        {
            if (frameObjects[i] != null)
            {
                frameObjects[i].SetActive(false);
            }
        }
    }

    private Material GetSharedMaterial()
    {
        if (sharedFrameMaterial != null)
        {
            return sharedFrameMaterial;
        }

        Shader shader = Shader.Find("Sprites/Default");
        if (shader == null)
        {
            shader = Shader.Find("Unlit/Color");
        }

        if (shader == null)
        {
            return null;
        }

        sharedFrameMaterial = new Material(shader)
        {
            name = "P10B_GreenGroundFrame_Material"
        };
        return sharedFrameMaterial;
    }
}
