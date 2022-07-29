using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class pickPano : MonoBehaviour, IMixedRealityInputHandler
{
    GameObject Modelo;
    public Texture2D Imagen, tex12;
    Color c;
    int id;

    // Start is called before the first frame update
    void Start()
    {
        Modelo = GameObject.Find("CuartoDoradoSTa");

    }

    // Update is called once per frame
    void Update()
    {}

    public void OnInputUp(InputEventData eventData)
    {}

    public void OnInputDown(InputEventData eventData)
    {
        Debug.Log(eventData.MixedRealityInputAction.Description);
        if (eventData.InputSource.SourceType == InputSourceType.Hand && eventData.MixedRealityInputAction.Description == "Select")
        {
            Vector3    position = eventData.InputSource.Pointers[0].Position; //Pointer 0 is the default one (straight line)
            Quaternion rot      = eventData.InputSource.Pointers[0].Rotation;

            Ray ray = new Ray(position, rot*Vector3.forward);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                if (hit.transform.name == "CuartoDoradoSTa")
                {
                    Vector2 punto = hit.textureCoord;
                    Debug.Log("u " + punto.x + " v " + punto.y);
                    c = Imagen.GetPixel(Mathf.FloorToInt(punto.x * Imagen.width), Mathf.FloorToInt(punto.y * Imagen.height));
                    id = Mathf.RoundToInt(c.r * 255);
                    Debug.Log(" r " + Mathf.RoundToInt(c.r * 255) + " g " + Mathf.RoundToInt(c.g * 255) + " b " + Mathf.RoundToInt(c.b * 255) + " a " + Mathf.RoundToInt(c.a * 255));
                    if (id == 120)
                    {
                        Modelo.GetComponent<Renderer>().material.SetTexture("_DecalTex", tex12);
                    }
                    else
                    {
                        Modelo.GetComponent<Renderer>().material.SetTexture("_DecalTex", null);
                    }
                }
            }
        }
    }
}

