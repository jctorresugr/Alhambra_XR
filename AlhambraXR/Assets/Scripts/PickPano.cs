using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.Physics;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Unity.Collections;
using UnityEngine;
using Unity.Burst;
using UnityEngine.Rendering;


public class PickPano : MonoBehaviour, IMixedRealityInputActionHandler, Model.IModelListener
{
    /// <summary>
    /// Class used to register annotations
    /// </summary>
    public class Annotation
    {
        /// <summary>
        /// The color of the annotation
        /// </summary>
        public Color32 Color { get; set; }

        /// <summary>
        /// The bounding box (min XYZ position) in the local space of the 3D model where this annotation belongs to.
        /// </summary>
        public float[] BoundingMin { get; set; }

        /// <summary>
        /// The bounding box (max XYZ position) in the local space of the 3D model where this annotation belongs to.
        /// </summary>
        public float[] BoundingMax { get; set; }

        /// <summary>
        /// The central position of this annotation in the local space of the 3D model.
        /// </summary>
        public Vector3 Center
        {
            get => new Vector3(0.5f * (BoundingMax[0] - BoundingMin[0]) + BoundingMin[0],
                               0.5f * (BoundingMax[1] - BoundingMin[1]) + BoundingMin[1],
                               0.5f * (BoundingMax[2] - BoundingMin[2]) + BoundingMin[2]);
        }

        /// <summary>
        /// Constructor. Initialize everything with default values.
        /// </summary>
        public Annotation()
        {
            BoundingMin = new float[3] { 0, 0, 0 };
            BoundingMax = new float[3] { 0, 0, 0 };
            Color       = new Color32(0, 0, 0, 0);
        }
    }

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

    /// <summary>
    /// all the registered annotation
    /// </summary>
    private List<Annotation> m_annotations = new List<Annotation>();

    /// <summary>
    /// The RGBAFloat texture to read to map UV mapping to 3D positions. Useful to know where annotations are anchored in the 3D space.
    /// Size: See _IndexTex.
    /// </summary>
    private float[] m_uvToPositionPixels = null;

    /// <summary>
    /// The Material to map UV coordinates to the 3D positions in local space.
    /// </summary>
    public Material UVToPosition;

    /// <summary>
    /// The camera used to render data on RenderTexture
    /// </summary>
    public Camera RTCamera;

    /// <summary>
    /// Initialize this GameObject and link it with the other components of the Scene
    /// </summary>
    /// <param name="model">The Model object containing the data of the overall application</param>
    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
    public void Init(Model model)
    {
        m_model = model;
        model.AddListener(this);
        Texture2D indexTexture = (Texture2D)gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_IndexTex");

        /*********************************************************************/
        /************First -- Determine the 3D position at each UV************/
        /***(this supposes that not two triangles share the same UV mapping***/
        /*********************************************************************/

        //Create a texture to map UV data to position data
        Texture2D colorScreenShot = new Texture2D(indexTexture.width, indexTexture.height, TextureFormat.RGBAFloat, false);
        RenderTexture colorRT = new RenderTexture(colorScreenShot.width, colorScreenShot.height, 24, RenderTextureFormat.ARGBFloat);
        colorRT.Create();

        //Render the specific mesh from the camera position in the render texture (and thus in the linked screenShot texture)
        CommandBuffer buf = new CommandBuffer();
        buf.SetRenderTarget(colorRT, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
        buf.DrawMesh(Mesh, Matrix4x4.identity, UVToPosition, 0, -1);
        RTCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
        RTCamera.Render();
        RTCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);

        //Read back the pixels from the render texture to the texture
        RenderTexture.active = colorRT;
        colorScreenShot.ReadPixels(new Rect(0, 0, colorScreenShot.width, colorScreenShot.height), 0, 0);

        //Clean up our messes
        RTCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(colorRT);

        //Save the pixels for later usages
        m_uvToPositionPixels = colorScreenShot.GetRawTextureData<float>().ToArray();

        /*********************************************************************/
        /*************Second -- Determine the known annotations***************/
        /*********************************************************************/

        //Read the index texture to know which colors (at which positions) are used (parallelize the reading to speed up the process)
        NativeArray<byte> srcRGBA = indexTexture.GetRawTextureData<byte>();

        Parallel.For(0, indexTexture.width * indexTexture.height,
            () => new List<Annotation>(),
            (i, state, partialRes) =>
            {
                int idx = 4*i;
                if (srcRGBA[idx + 3] > 0 && m_uvToPositionPixels[idx+3] > 0)
                {
                    Annotation srcAnnot = partialRes.Find((annot) => annot.Color.r == srcRGBA[idx + 0] &&
                                                                     annot.Color.g == srcRGBA[idx + 1] &&
                                                                     annot.Color.b == srcRGBA[idx + 2]);

                    if (srcAnnot == null)
                        partialRes.Add(new Annotation()
                        {
                            Color = new Color32(srcRGBA[idx + 0],
                                                srcRGBA[idx + 1],
                                                srcRGBA[idx + 2], 255),
                            BoundingMin = new float[3] { m_uvToPositionPixels[idx + 0], m_uvToPositionPixels[idx + 1], m_uvToPositionPixels[idx + 2] },
                            BoundingMax = new float[3] { m_uvToPositionPixels[idx + 0], m_uvToPositionPixels[idx + 1], m_uvToPositionPixels[idx + 2] }
                        });
                    else
                    {
                        for(int j = 0; j < 3; j++)
                        {
                            srcAnnot.BoundingMin[j] = Math.Min(srcAnnot.BoundingMin[j], m_uvToPositionPixels[idx + j]);
                            srcAnnot.BoundingMax[j] = Math.Max(srcAnnot.BoundingMax[j], m_uvToPositionPixels[idx + j]);
                        }
                    }
                }

                return partialRes;
            },
            (partialRes) =>
            {
                lock(this)
                { 
                    foreach(Annotation annot in partialRes)
                    {
                        Color32 c = annot.Color;
                        Annotation srcAnnot = m_annotations.Find((annot) => annot.Color.r == c.r && annot.Color.g == c.g && annot.Color.b == c.b);
                        if(srcAnnot == null)
                            m_annotations.Add(annot);
                        else //Merge annotations in the parallel computation
                        {
                            for(int i = 0; i < 3; i++) 
                            {
                                srcAnnot.BoundingMax[i] = Math.Max(srcAnnot.BoundingMax[i], annot.BoundingMax[i]);
                                srcAnnot.BoundingMin[i] = Math.Min(srcAnnot.BoundingMin[i], annot.BoundingMin[i]);
                            }
                        }
                    }
                }
            }
        );

        //Debug purposes, to check that annotations are anchored at their correct position.
        //GameObject originGO = GameObject.Find("Origin");
        //foreach(Annotation annot in m_annotations)
        //{
        //    GameObject go         = GameObject.Instantiate(originGO);
        //    go.transform.position = transform.localToWorldMatrix.MultiplyPoint3x4(annot.Center);
        //}
    }                                                                                               


    void Start()
    {
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
                        Handedness hand = Handedness.RIGHT;
                        if(inputSource.Pointers[0].Controller.ControllerHandedness == Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left)
                            hand = Handedness.LEFT;
                        SetShaderLayerParams(c.Value, hand);
                    }
                }
            }
        }

        if(m_updateHighlight)
        {
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_IDRight",    m_model.CurrentHighlightMain.ID);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_LayerRight", m_model.CurrentHighlightMain.Layer);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_IDLeft",     m_model.CurrentHighlightSecond.ID);
            gameObject.GetComponent<Renderer>().sharedMaterial.SetFloat("_LayerLeft",  m_model.CurrentHighlightSecond.Layer);
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
                Texture2D indexTexture = (Texture2D)gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_IndexTex");
                Color c = indexTexture.GetPixel((int)(point.x * indexTexture.width), (int)(point.y * indexTexture.height));
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
    private bool SetShaderLayerParams(Color c, Handedness hand)
    {
        PairLayerID? ret;

        int ir = Mathf.RoundToInt(c.r * 255);
        int ig = Mathf.RoundToInt(c.g * 255);
        int ib = Mathf.RoundToInt(c.b * 255);
        int ia = Mathf.RoundToInt(c.a * 255);

        //Debug.Log($" r {ir}, g {ig}, b {ib}, a {ia}");

        //Depending on where we hit, highlight a particular part of the game object if any layer information is there
        if (ir > 0)
            ret = new PairLayerID() { ID = ir, Layer = 0 };
        else if (ig > 0)
            ret = new PairLayerID() { ID = ig, Layer = 1 };
        else if (ib > 0)
            ret = new PairLayerID() { ID = ib, Layer = 2 };
        else
            ret = new PairLayerID() { ID = -1, Layer = -1 };

        if (hand == Handedness.RIGHT)
            m_model.CurrentHighlightMain = ret.Value;
        else if (hand == Handedness.LEFT)
            m_model.CurrentHighlightSecond = ret.Value;

        return ret.Value.Layer != -1;
    }

    /// <summary>
    /// Adds a new annotation in the Index Texture associated with the GameObject
    /// </summary>
    /// <param name="uvMapping">A texture containing the UV mapping of the Index Texture to anchor the annotation into</param>
    /// <param name="outputColor">The color of the annotation inside the Index Texture. Every pixel of that color refers to the same annotation (the newly created one)</param>
    /// <returns>True on success (we found a suitable color for the annotation), false otherwise. If false, outputColor == Color32(0,0,0,0).</returns>
    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
    public bool AddAnnotation(Texture2D uvMapping, out Color32 outputColor)
    {
        //Get the pixels of the uvMapping to anchor, and the texture that contains the annotation
        NativeArray<Color> newRGBA = uvMapping.GetRawTextureData<Color>();
        Texture2D srcAnnotationTexture = (Texture2D)gameObject.GetComponent<MeshRenderer>().sharedMaterial.GetTexture("_IndexTex");
        NativeArray<byte>  srcRGBA = srcAnnotationTexture.GetRawTextureData<byte>();

        //Save some texture variables to let us read texture data in separate thread...
        int uvWidth = uvMapping.width;
        int uvHeight = uvMapping.height;
        int srcWidth = srcAnnotationTexture.width;
        int srcHeight = srcAnnotationTexture.height;

        //The purpose is to find on which layer to anchor this annotation
        int     layer      = 2;
        Color32 layerColor = new Color32(0, 0, 0, 255);

        for(int j = 0; j < uvMapping.height; j++)
        {
            for(int i = 0; i < uvMapping.width; i++)
            {
                if (newRGBA[j * uvWidth + i].a == 0) //Transparent UV
                    continue;

                int srcJ = (int)(newRGBA[j * uvWidth + i].g * srcHeight);
                int srcI = (int)(newRGBA[j * uvWidth + i].r * srcWidth);
                int b    = srcRGBA[4 * (srcJ * srcAnnotationTexture.width + srcI) + 2];
                if(b > 0)
                {
                    int g = srcRGBA[4 * (srcJ * srcAnnotationTexture.width + srcI) + 1];
                    if(g > 0)
                    {
                        layer        = 0;
                        layerColor.g = (byte)g;
                        layerColor.b = (byte)b;
                        goto endFindLayer; //Final depth -- Cannot go further
                    }
                    layer = Math.Min(1, layer); //Normally 1 is always the min, as the "0" breaks the for loop
                    layerColor.b = (byte)b;
                }
            }
        }
        endFindLayer:

        //Define the final color
        //Brute force all the possibilities depending on the layer
        if(layer == 0)
        {
            int r = 1;
            for(; r < (1<<8); r++)
            { 
                if(!m_annotations.Exists((annot) => annot.Color.r == r))
                {
                    layerColor.r = (byte)r;
                    break;
                }
            }

            if(r == (1<<8))
            {
                Debug.Log($"Issue: We excided the number of possible values in Layer 0 for colors g == {layerColor.g} and b == {layerColor.b}. Cancelling the annotation");
                outputColor = new Color32(0, 0, 0, 0);
                return false;
            }
        }
        else if(layer == 1)
        {
            int g = 1;
            for (; g < (1 << 8); g++) //Search that layer (g, b) does not exist first
            {
                if(!m_annotations.Exists((annot) => annot.Color.r == 0 &&
                                                    annot.Color.g == g))
                {
                    layerColor.g = (byte)g;
                    break;
                }
            }

            if(g == (1<<8))
            {
                Debug.Log($"Issue: We excided the number of possible values in Layer 1 for color b == {layerColor.b}. Cancelling the annotation...");
                outputColor = new Color32(0, 0, 0, 0);
                return false;
            }
        }
        else if(layer == 2)
        {
            int b = 1;
            for (; b < (1 << 8); b++) //Search that layer (b) does not exist first
            {
                if(!m_annotations.Exists((annot) => annot.Color.r == 0 &&
                                                    annot.Color.g == 0 &&
                                                    annot.Color.b == b))
                {
                    layerColor.b = (byte)b;
                    break;
                }
            }

            if (b == (1<<8))
            {
                Debug.Log($"Issue: We excided the number of possible values in Layer 2. Cancelling the annotation...");
                outputColor = new Color32(0, 0, 0, 0);
                return false;
            }
        }

        //Initialize the new annotation to register
        Annotation annot = new Annotation() 
        { 
            Color = layerColor, 
            BoundingMin = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue }, 
            BoundingMax = new float[3] { float.MinValue, float.MinValue, float.MinValue }
        };

        //Now that we know on which layer to put that annotation, anchor it
        //Since every pixel are independent, and that multiple write is not an issue, parallelize the code
        Parallel.For(0, uvHeight, new ParallelOptions { MaxDegreeOfParallelism = 8 },
            () => new float[6] { float.MaxValue, float.MaxValue, float.MaxValue, 
                                 float.MinValue, float.MinValue, float.MinValue },
            (j, state, partialRes) =>
            {
                for(int i = 0; i < uvWidth; i++)
                {
                    if(newRGBA[j * uvWidth + i].a == 0) //Transparent UV
                        continue;

                    int srcJ   = (int)(newRGBA[j * uvWidth + i].g * srcHeight);
                    int srcI   = (int)(newRGBA[j * uvWidth + i].r * srcWidth);
                    int srcIdx = 4 * (srcJ * srcWidth + srcI);

                    if(m_uvToPositionPixels[srcIdx + 3] == 0) //Unknown 3D position
                        continue;

                    for(int k = 0; k < 4; k++)
                        srcRGBA[srcIdx + k] = layerColor[k];
                    
                    for(int k = 0; k < 3; k++)
                    {
                        partialRes[k]   = Math.Min(m_uvToPositionPixels[srcIdx + k], partialRes[k]);
                        partialRes[k+3] = Math.Max(m_uvToPositionPixels[srcIdx + k], partialRes[k+3]);
                    }
                }
                return partialRes;
            },
            (partialRes) =>
            {
                lock(annot)
                {
                    for(int i = 0; i < 3; i++)
                    {
                        annot.BoundingMin[i] = Math.Min(annot.BoundingMin[i], partialRes[i]);
                        annot.BoundingMax[i] = Math.Max(annot.BoundingMax[i], partialRes[i+3]);
                    }
                }
            }
        );
        Debug.Log($"Finish to add annotation Color {layerColor}");
        m_annotations.Add(annot);

        srcAnnotationTexture.Apply();
        gameObject.GetComponent<MeshRenderer>().sharedMaterial.SetTexture("_IndexTex", srcAnnotationTexture);

        //Debug purposes
        //GameObject originGO = GameObject.Find("Origin");
        //GameObject go = GameObject.Instantiate(originGO);
        //go.transform.position = transform.localToWorldMatrix.MultiplyPoint3x4(annot.Center);

        //Return
        outputColor = layerColor;
        return true;
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
                Handedness hand = Handedness.RIGHT;
                if (eventData.InputSource.Pointers[0].Controller.ControllerHandedness == Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left)
                    hand = Handedness.LEFT;
                SetShaderLayerParams(c.Value, hand);
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

    public void OnSetCurrentHighlight(Model model, PairLayerID mainID, PairLayerID secondMainID)
    {
        m_updateHighlight = true;
    }

    /// <summary>
    /// The Mesh of the object linked to this GameObject. Useful, for instance, to draw this mesh on a dedicated RenderTexture
    /// </summary>
    public Mesh Mesh
    {
        get => gameObject.GetComponent<MeshFilter>().mesh;
    }

    /// <summary>
    /// The list of all registered annotations
    /// </summary>
    public List<Annotation> Annotations
    {
        get => m_annotations;
    }
}