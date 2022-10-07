using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using System.Collections.Generic;
using UnityEngine;

public class PickPano : MonoBehaviour, IMixedRealityInputActionHandler, Model.IModelListener
{
    /// <summary>
    /// Listener for events launched from PickPano
    /// </summary>
    public interface IPickPanoListener
    {
        /// <summary>
        /// Method called when a new selection has been performed
        /// </summary>
        /// <param name="pano">The PickPano object calling this method</param>
        /// <param name="c">
        /// The 3-layers (one per channel. R == Layer1, G == Layer2, B == Layer3) selection data as defined by the index texture. 
        /// Use 'c[i]*255' to retrieve the ID of the selection (if ID > 0) for the layer i. If no layer is selected, all c[i], 0 <= i < 3, equal 0
        /// </param>
        void OnSelection(PickPano pano, Color c);
    }

    /// <summary>
    /// The image containing the layer information
    /// </summary>
    public Texture2D Image;

    /// <summary>
    /// The model to use. Should be set up by the main application
    /// </summary>
    private Model m_model = null;
    
    /// <summary>
    /// The listeners objects listening for selection events
    /// </summary>
    private HashSet<IPickPanoListener> m_listeners = new HashSet<IPickPanoListener>();

    /// <summary>
    /// Should we update the graphical parameters about the data chunk to highlight?
    /// </summary>
    private bool m_updateHighlight = false;

    public void Init(Model model)
    {
        m_model = model;
        model.AddListener(this);
    }

    void Start()
    {
        PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOn, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
        //CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
    }

    void OnDestroy()
    {
        //CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
    }

    void Update()
    {
        if(m_model.CurrentAction == CurrentAction.DEFAULT)
        { 
            foreach(var inputSource in CoreServices.InputSystem.DetectedInputSources)
            {
                if(inputSource.SourceType == InputSourceType.Hand)
                {
                    Color? c = FindLayersAt(inputSource.Pointers[0]);
                    if(c != null)
                    {
                        if(SetShaderLayerParams(c.Value))
                            break;
                    }
                }
            }
        }

        if(m_updateHighlight)
        {
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_ID",    m_model.CurrentHighlight.ID);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_Layer", m_model.CurrentHighlight.Layer);
            m_updateHighlight = false;
        }
    }

    /// <summary>
    /// Add a new listener to notify events
    /// </summary>
    /// <param name="l">The new listener to notify on events</param>
    public void AddListener(IPickPanoListener l)
    {
        m_listeners.Add(l);
    }

    /// <summary>
    /// Remove an already registered listener
    /// </summary>
    /// <param name="l">The listener to unregister</param>
    public void RemoveListener(IPickPanoListener l)
    {
        m_listeners.Remove(l);
    }
    /// <summary>
    /// Find the pixel color containing the layer information that the user is currently pointing at
    /// The color is determined from the Index texture.
    /// </summary>
    /// <param name="pointer">The ray pointer being used</param>
    /// <returns>
    /// null if the user is not pointing towards the Model object, else, the pixel color in the index texture corresponding to what the user is pointing at.
    /// The color defines 3-layers (one per channel. R == Layer1, G == Layer2, B == Layer3) data information as defined by the index texture. 
    /// Use 'c[i]*255' to retrieve the ID of the selection (if ID > 0) for the layer i. If no layer is selected, all c[i], 0 <= i < 3, equal 0
    /// </returns>
    private Color? FindLayersAt(IMixedRealityPointer pointer)
    {
        //Get the general ray information
        RayStep rayStep = pointer.Rays[0];
        Ray ray = new Ray(rayStep.Origin, rayStep.Direction);

        //Collide the ray with the world, and check that we hit the correct gameObject
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, 1 << gameObject.layer))
        {
            if (hit.transform.name == gameObject.name)
            {
                //Check that we have a mesh collider
                MeshCollider meshCollider = hit.collider as MeshCollider;
                if (meshCollider == null)
                {
                    Debug.Log("Issue with mesh colliders... Return.");
                    return null;
                }

                //Get the position of the hit
                Vector2 point = hit.textureCoord;
                //Debug.Log("u " + point.x + " v " + point.y);
                //GameObject.Find("Origin").transform.position = hit.point;


                //Get the corresponding Layer information using the Image that encodes up to 4 layers
                Color c = Image.GetPixel((int)(point.x * Image.width), (int)(point.y * Image.height));
                return c;
            }
        }

        return null;
    }

    /// <summary>
    /// Set the shader parameters in order to highlight a specific layer
    /// </summary>
    /// <param name="c">The color in the index texture of a specific area to highlight</param>
    /// <returns>true if something is to be highlighted, false otherwise</returns>
    private bool SetShaderLayerParams(Color c)
    {
        bool ret = false;
        int ir = Mathf.RoundToInt(c.r * 255);
        int ig = Mathf.RoundToInt(c.g * 255);
        int ib = Mathf.RoundToInt(c.b * 255);
        int ia = Mathf.RoundToInt(c.a * 255);
        //Debug.Log($" r {ir}, g {ig}, b {ib}, a {ia}");

        //Depending on where we hit, highlight a particular part of the game object if any layer information is there
        if (ir > 0)
        {
            m_model.CurrentHighlight = new PairLayerID() { ID = ir, Layer = 0 };
            ret = true;
        }
        else if (ig > 0)
        {
            m_model.CurrentHighlight = new PairLayerID() { ID = ig, Layer = 1 };
            ret = true;
        }
        else if (ib > 0)
        {
            m_model.CurrentHighlight = new PairLayerID() { ID = ib, Layer = 2 };
            ret = true;
        }
        else
        {
            m_model.CurrentHighlight = new PairLayerID() { ID = -1, Layer = -1 };
        }

        return ret;
    }

    /*********************************************/
    /* IMixedRealityInputActionHandler interface */
    /*********************************************/

    public void OnActionStarted(BaseInputEventData eventData)
    {
        if(eventData.InputSource.SourceType == InputSourceType.Hand && eventData.MixedRealityInputAction.Description == "Select")
        {
            if(m_model.CurrentAction == CurrentAction.IN_HIGHLIGHT) //Stop the highlighting from outside
            {
                m_model.CurrentAction = CurrentAction.DEFAULT;
                return; 
            }

            //Accept to select layers ONLY in the default setting
            if(m_model.CurrentAction != CurrentAction.DEFAULT)
                return;

            Color? c = FindLayersAt(eventData.InputSource.Pointers[0]);
            if (c != null)
            {
                SetShaderLayerParams(c.Value);
                foreach (IPickPanoListener l in m_listeners)
                    l.OnSelection(this, c.Value);
            }
        }
    }

    public void OnActionEnded(BaseInputEventData eventData)
    {}

    /*********************************************/
    /********** IModelListener interface *********/
    /*********************************************/

    public void OnSetCurrentAction(Model model, CurrentAction action)
    {
        m_updateHighlight = true;
    }

    public void OnSetCurrentHighlight(Model model, PairLayerID id)
    {
        m_updateHighlight = true;
    }

    public Mesh Mesh
    {
        get => gameObject.GetComponent<MeshFilter>().mesh;
    }
}