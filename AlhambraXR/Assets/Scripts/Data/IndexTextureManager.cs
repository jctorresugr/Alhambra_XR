using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndexTextureManager : MonoBehaviour
{
    public Material material;
    public DataManager data;
    internal void Init()
    {
        data.OnIndexTextureChange += ProcessIndexTexture;
    }

    private void OnDestroy()
    {
        data.OnIndexTextureChange -= ProcessIndexTexture;
    }


    public void ProcessIndexTexture(Texture2D newTexture)
    {
        material.SetTexture("Index Texture", newTexture);
    }
}
