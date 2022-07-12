using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//public interface IMixedRealityFocusHandler : UnityEngine.EventSystems.IEventSystemHandler

using Microsoft.MixedReality.Toolkit.Input;

public class onFocus : MonoBehaviour , IMixedRealityFocusHandler, IMixedRealityPointerHandler
{
    GameObject Modelo;
    public Texture2D indexTexture;
    Color c;
    int ir, ig, ib, ia;

    Renderer rend;
    Material m_Material;



    private void Awake()
    {
        Debug.Log("Awake");
        Modelo = GameObject.Find("CuartoDoradoSTa");
        rend = GetComponent<Renderer>();
        m_Material = GetComponent<Renderer>().material;

        // Use the BlinkSurface shader on the material
        m_Material.shader = Shader.Find("Custom/BlinkSurface");

    }

  

    void IMixedRealityFocusHandler.OnFocusEnter(FocusEventData eventData)
    {
        //   if (Input.GetMouseButtonDown(0))
        //   {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == "CuartoDoradoSTa")
            {
                Vector2 punto = hit.textureCoord;
                Debug.Log("u " + punto.x + " v " + punto.y);
                c = indexTexture.GetPixel(Mathf.FloorToInt(punto.x * indexTexture.width), Mathf.FloorToInt(punto.y * indexTexture.height));
                ir = Mathf.RoundToInt(c.r * 255);
                ig = Mathf.RoundToInt(c.g * 255);
                ib = Mathf.RoundToInt(c.b * 255);
                ia = Mathf.RoundToInt(c.a * 255);
                Debug.Log("OnFocus ir " + ir + " ig " + ig + " ib " + ib + " ia " + ia);

                //           rend.material.shader = Shader.Find("Custom/BlinkSurface");

                //   gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ir);

                if (ir > 0)
                {
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ir);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 0);
                }
                else if (ig > 0)
                {
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ig);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 1);
                }
                else if (ib > 0)
                {
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ib);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 2);
                }
                else gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 4);

                /*             if (id == 120)
                            {
                                Modelo.GetComponent<Renderer>().material.SetTexture("_DecalTex", tex12);
                            }
                             else
                             {
                                 Modelo.GetComponent<Renderer>().material.SetTexture("_DecalTex", null);
                             }
                         }*/


            }
        }

    }

    void IMixedRealityFocusHandler.OnFocusExit(FocusEventData eventData)
    {
        Debug.Log(" OnFocusExit");
        gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 4);

    }

    void IMixedRealityPointerHandler.OnPointerDown(
         MixedRealityPointerEventData eventData)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            if (hit.transform.name == "CuartoDoradoSTa")
            {
                Vector2 punto = hit.textureCoord;
                Debug.Log("u " + punto.x + " v " + punto.y);
                c = indexTexture.GetPixel(Mathf.FloorToInt(punto.x * indexTexture.width), Mathf.FloorToInt(punto.y * indexTexture.height));
                ir = Mathf.RoundToInt(c.r * 255);
                ig = Mathf.RoundToInt(c.g * 255);
                ib = Mathf.RoundToInt(c.b * 255);
                ia = Mathf.RoundToInt(c.a * 255);
                Debug.Log("PointerDown ir " + ir + " ig " + ig + " ib " + ib + " ia " + ia);

                //           rend.material.shader = Shader.Find("Custom/BlinkSurface");

                //   gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ir);

                if (ir > 0)
                {
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ir);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 0);
                }
                else if (ig > 0)
                {
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ig);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 1);
                }
                else if (ib > 0)
                {
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID", ib);
                    gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 2);
                }
                else gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 4);
            }

        }
    }
    void IMixedRealityPointerHandler.OnPointerUp(
       MixedRealityPointerEventData eventData)
    {
                Debug.Log(" PointerUP Exit");
                gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", 4);
            }

    void IMixedRealityPointerHandler.OnPointerDragged(
         MixedRealityPointerEventData eventData)
    {
    }

    void IMixedRealityPointerHandler.OnPointerClicked(MixedRealityPointerEventData eventData)
    {
        
    }
}
