using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using UnityEngine;

public class PickPano : MonoBehaviour, IMixedRealityInputActionHandler, IMixedRealityPointerHandler
{
    public Texture2D Image;
    public Texture2D Tex12;

    void Start()
    {
        PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOn, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityPointerHandler>(this);
    }

    void OnDestroy()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityPointerHandler>(this);
    }

    void Update()
    {
        foreach(var inputSource in CoreServices.InputSystem.DetectedInputSources)
        {
            if(inputSource.SourceType == InputSourceType.Hand)
            {
                Color? c = FindLayersAt(inputSource.Pointers[0]);
                if(c != null)
                    SetShaderLayerParams(c.Value);
            }
        }
    }

    public void OnActionStarted(BaseInputEventData eventData)
    {
        if (eventData.InputSource.SourceType == InputSourceType.Hand && eventData.MixedRealityInputAction.Description == "Select")
        {
            Color? c = FindLayersAt(eventData.InputSource.Pointers[0]);
            if (c != null)
                SetShaderLayerParams(c.Value);
        }
    }

    private Color? FindLayersAt(IMixedRealityPointer pointer)
    {
        //Get the general ray information
        Vector3 position = pointer.Position; //Pointer 0 is the default one (straight line)
        Quaternion rot = pointer.Rotation;
        Ray ray = new Ray(position, rot * Vector3.forward);

        //Collide the ray with the world, and check that we hit the correct gameObject
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << gameObject.layer))
        {
            if (hit.transform.name == gameObject.name)
            {
                //Get the position of the hit
                Vector2 point = hit.textureCoord;
                Debug.Log("u " + point.x + " v " + point.y);

                //Get the corresponding Layer information using the Image that encodes up to 4 layers
                Color c = Image.GetPixel(Mathf.FloorToInt(point.x * Image.width), Mathf.FloorToInt(point.y * Image.height));
                return c;
            }
        }

        return null;
    }

    private void SetShaderLayerParams(Color c)
    { 
        int ir = Mathf.RoundToInt(c.r * 255);
        int ig = Mathf.RoundToInt(c.g * 255);
        int ib = Mathf.RoundToInt(c.b * 255);
        int ia = Mathf.RoundToInt(c.a * 255);
        Debug.Log($" r {ir}, g {ig}, b {ib}, a {ia}");

        //Depending on where we hit, highlight a particular part of the game object if any layer information is there
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

        if (ir == 120)
        {
            gameObject.GetComponent<Renderer>().material.SetTexture("_DecalTex", Tex12);
        }
        else
        {
            gameObject.GetComponent<Renderer>().material.SetTexture("_DecalTex", null);
        }
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {}

    public void OnPointerDown(MixedRealityPointerEventData eventData)
    {
    }

    public void OnPointerDragged(MixedRealityPointerEventData eventData)
    {
        Debug.Log(eventData.InputSource.SourceType);
        if (eventData.InputSource.SourceType == InputSourceType.Hand)
            Debug.Log(eventData.selectedObject.name);
    }

    public void OnPointerUp(MixedRealityPointerEventData eventData)
    {}

    public void OnPointerClicked(MixedRealityPointerEventData eventData)
    {}
}
