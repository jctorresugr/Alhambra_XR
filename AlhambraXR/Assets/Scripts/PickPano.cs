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
using System.Collections.Concurrent;

public class PickPano : MonoBehaviour, IMixedRealityInputActionHandler, SelectionModelData.ISelectionModelDataListener
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
        void OnHover(PickPano pano, Color c);//
    }

    /// <summary>
    /// The model to use. Should be set up by the main application
    /// </summary>
    private SelectionModelData m_model = null;

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
    //private List<AnnotationRenderInfo> m_annotations = new List<AnnotationRenderInfo>();

    public DataManager data;

    /// <summary>
    /// The RGBAFloat texture to read to map UV mapping to 3D positions. Useful to know where annotations are anchored in the 3D space.
    /// Size: See _IndexTex.
    /// </summary>
    private float[] m_uvToPositionPixels = null;

    private float[] m_uvToNormalPixels = null;

    /// <summary>
    /// The Material to map UV coordinates to the 3D positions in local space.
    /// </summary>
    public Material UVToPosition;

    public Material UVToNormal;
    public Material UVToTangent;

    /// <summary>
    /// The camera used to render data on RenderTexture
    /// </summary>
    public Camera RTCamera;
    private float[] m_uvToTangentPixels;

    /// <summary>
    /// Initialize this GameObject and link it with the other components of the Scene
    /// </summary>
    /// <param name="model">The Model object containing the data of the overall application</param>
    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
    public void Init(SelectionModelData model)
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
        buf.DrawMesh(Mesh, Matrix4x4.identity, UVToPosition, 0, -1); // This shader maps [0,1] to [-1,1]
        RTCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
        RTCamera.Render();
        RTCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);

        //Read back the pixels from the render texture to the texture
        RenderTexture.active = colorRT;
        colorScreenShot.ReadPixels(new Rect(0, 0, colorScreenShot.width, colorScreenShot.height), 0, 0);


        //Save the pixels for later usages
        m_uvToPositionPixels = colorScreenShot.GetRawTextureData<float>().ToArray();

        //-----
        // added by Yucheng: generate normal map
        CommandBuffer buf2 = new CommandBuffer();
        buf.SetRenderTarget(colorRT, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
        buf.DrawMesh(Mesh, Matrix4x4.identity, UVToNormal, 0, -1); 
        RTCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
        RTCamera.Render();
        RTCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);

        //Read back the pixels from the render texture to the texture
        RenderTexture.active = colorRT;
        colorScreenShot.ReadPixels(new Rect(0, 0, colorScreenShot.width, colorScreenShot.height), 0, 0);
        m_uvToNormalPixels = colorScreenShot.GetRawTextureData<float>().ToArray();

        //-----
        // added by Yucheng: generate tangent map
        CommandBuffer buf3 = new CommandBuffer();
        buf.SetRenderTarget(colorRT, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
        buf.DrawMesh(Mesh, Matrix4x4.identity, UVToTangent, 0, -1);
        RTCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
        RTCamera.Render();
        RTCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);

        //Read back the pixels from the render texture to the texture
        RenderTexture.active = colorRT;
        colorScreenShot.ReadPixels(new Rect(0, 0, colorScreenShot.width, colorScreenShot.height), 0, 0);
        m_uvToTangentPixels = colorScreenShot.GetRawTextureData<float>().ToArray();

        //Clean up our messes
        RTCamera.targetTexture = null;
        RenderTexture.active = null;
        Destroy(colorRT);

        /*********************************************************************/
        /*************Second -- Determine the known annotations***************/
        /*********************************************************************/

        //Read the index texture to know which colors (at which positions) are used (parallelize the reading to speed up the process)
        NativeArray<byte> srcRGBA = indexTexture.GetRawTextureData<byte>();

        
        Parallel.For(0, indexTexture.width * indexTexture.height,
            () => new List<AnnotationRenderInfo>(),
            (i, state, partialRes) =>
            {
                int idx = 4*i;
                if (srcRGBA[idx + 3] > 0 && m_uvToPositionPixels[idx+3] > 0) // if exists annotation in this pixel
                {
                    AnnotationRenderInfo srcAnnot = partialRes.Find((annot) => annot.Color.r == srcRGBA[idx + 0] &&
                                                                     annot.Color.g == srcRGBA[idx + 1] &&
                                                                     annot.Color.b == srcRGBA[idx + 2]); // find annotation we stored here

                    Vector3 pos = new Vector3(m_uvToPositionPixels[idx + 0], m_uvToPositionPixels[idx + 1], m_uvToPositionPixels[idx + 2]);
                    if (srcAnnot == null) // cannot find :(
                        partialRes.Add(new AnnotationRenderInfo() // add the annotation to the record list
                        {
                            Color = new Color32(srcRGBA[idx + 0],
                                                srcRGBA[idx + 1],
                                                srcRGBA[idx + 2], 255),
                            BoundingMin = pos,
                            BoundingMax = pos,
                            Normal = new Vector3(m_uvToNormalPixels[idx + 0], m_uvToNormalPixels[idx + 1], m_uvToNormalPixels[idx + 2]),
                            Tangent = new Vector3(m_uvToTangentPixels[idx + 0], m_uvToTangentPixels[idx + 1], m_uvToTangentPixels[idx + 2]),
                            averagePosition = pos,
                            pointCount = 1
                        });
                    else
                    {
                        //lock(srcAnnot)
                        {
                            srcAnnot.Bounds.Encapsulate(pos);
                            srcAnnot.Normal += new Vector3(m_uvToNormalPixels[idx + 0], m_uvToNormalPixels[idx + 1], m_uvToNormalPixels[idx + 2]);
                            srcAnnot.Tangent += new Vector3(m_uvToTangentPixels[idx + 0], m_uvToTangentPixels[idx + 1], m_uvToTangentPixels[idx + 2]);
                            srcAnnot.averagePosition += pos;
                            srcAnnot.pointCount++;
                        }

                    }
                }

                return partialRes;
            },
            (partialRes) =>
            {
                lock(this)
                { 
                    foreach(AnnotationRenderInfo annot in partialRes)
                    {
                        AnnotationID c = (AnnotationID)annot.Color;

                        Annotation srcAnnot = data.FindAnnotationID(c);

                        if (srcAnnot == null)
                        {
                            data.AddAnnoationRenderInfo(annot);
                        }
                            //m_annotations.Add(annot);
                        else //Merge annotations in the parallel computation
                        {
                            srcAnnot.renderInfo.Bounds.Encapsulate(annot.Bounds);
                            srcAnnot.renderInfo.Normal += annot.Normal;
                            srcAnnot.renderInfo.Tangent += annot.Tangent;
                            srcAnnot.renderInfo.averagePosition += annot.averagePosition;
                            srcAnnot.renderInfo.pointCount += annot.pointCount;
                            
                        }
                    }
                }
            }
        );

        Parallel.ForEach(data.Annotations,
            (annot) =>
            {
                AnnotationRenderInfo renderInfo = annot.renderInfo;
                renderInfo.Normal = renderInfo.Normal.normalized;
                renderInfo.Tangent = renderInfo.Tangent.normalized;
                renderInfo.averagePosition /= renderInfo.pointCount;
                Debug.Log($"Tangent&Normal {annot.ID} >>\t A{renderInfo.averagePosition} >>\t C{renderInfo.pointCount} >>> T{renderInfo.Tangent} >>\t N{renderInfo.Normal}");
            });

        ConcurrentDictionary<AnnotationID,XYZCoordinate> annotCoord = new ConcurrentDictionary<AnnotationID, XYZCoordinate>();

        foreach (Annotation annot in data.Annotations)
        {
            AnnotationID id = annot.ID;
            AnnotationRenderInfo renderInfo = annot.renderInfo;
            XYZCoordinate xYZCoordinate = new XYZCoordinate(renderInfo.Normal, renderInfo.Tangent);
            xYZCoordinate.translatePos = renderInfo.averagePosition;
            annotCoord.TryAdd(id, xYZCoordinate);
        }
        

        Parallel.For(0, indexTexture.width * indexTexture.height,
            (i) =>
            {
                int idx = 4 * i;
                if (srcRGBA[idx + 3] > 0 && m_uvToPositionPixels[idx + 3] > 0) // if exists annotation in this pixel
                {
                    Vector3 pos = new Vector3(m_uvToPositionPixels[idx + 0], m_uvToPositionPixels[idx + 1], m_uvToPositionPixels[idx + 2]);

                    int cr = srcRGBA[idx + 0];
                    int cg = srcRGBA[idx + 1];
                    int cb = srcRGBA[idx + 2];
                    void checkAnnotation(AnnotationID id)
                    {
                        Annotation annotation = data.FindAnnotationID(id);
                        if (annotation != null && annotation.renderInfo!=null)
                        {
                            XYZCoordinate coord = annotCoord[id];
                            Vector3 localPos = coord.Projection(pos);
                            
                            lock (annotation)
                            {
                                if(
                                annotation.renderInfo.OBBBounds.min==Vector3.zero &&
                                annotation.renderInfo.OBBBounds.max==Vector3.zero
                                )
                                {
                                    annotation.renderInfo.OBBBounds.SetMinMax(localPos, localPos);
                                    Debug.Log($"UpdateAnnotOBB {id}");
                                }else
                                {
                                    annotation.renderInfo.OBBBounds.Encapsulate(localPos);
                                }
                                
                            }
                            
                        }
                    }
                    if (cr > 0)
                    {
                        checkAnnotation(new AnnotationID(0, cr));
                    }
                    if (cg > 0)
                    {
                        checkAnnotation(new AnnotationID(1, cg));
                    }
                    if (cb > 0)
                    {
                        checkAnnotation(new AnnotationID(2, cb));
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
                        foreach(var l in m_listeners)
                        {
                            l.OnHover(this, c.Value);
                        }
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
        AnnotationID? ret;

        int ir = Mathf.RoundToInt(c.r * 255);
        int ig = Mathf.RoundToInt(c.g * 255);
        int ib = Mathf.RoundToInt(c.b * 255);
        int ia = Mathf.RoundToInt(c.a * 255);

        //Debug.Log($" r {ir}, g {ig}, b {ib}, a {ia}");

        //Depending on where we hit, highlight a particular part of the game object if any layer information is there
        if (ir > 0)
            ret = new AnnotationID(0, ir);// { ID = ir, Layer = 0 };
        else if (ig > 0)
            ret = new AnnotationID(1, ig);// { ID = ig, Layer = 1 };
        else if (ib > 0)
            ret = new AnnotationID(2, ib);// { ID = ib, Layer = 2 };
        else
            ret = AnnotationID.INVALID_ID;// { ID = -1, Layer = -1 };

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
                //if(!m_annotations.Exists((annot) => annot.Color.r == r))
                if(!data.ExistAnnotation((annot) => annot.renderInfo.Color.r == r))
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
                //if(!m_annotations.Exists((annot) => annot.Color.r == 0 &&
                //                                    annot.Color.g == g))
                if(!data.ExistAnnotation((annot) => annot.renderInfo.Color.r == 0 &&
                                                    annot.renderInfo.Color.g == g))
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
                //if(!m_annotations.Exists((annot) => annot.Color.r == 0 &&
                //                                    annot.Color.g == 0 &&
                //                                    annot.Color.b == b))
                if (!data.ExistAnnotation((annot) => annot.renderInfo.Color.r == 0 &&
                                                     annot.renderInfo.Color.g == 0 &&
                                                     annot.renderInfo.Color.b == b))
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
        AnnotationRenderInfo annot = new AnnotationRenderInfo() 
        { 
            Color = layerColor, 
            //BoundingMin = new float[3] { float.MaxValue, float.MaxValue, float.MaxValue }, 
            //BoundingMax = new float[3] { float.MinValue, float.MinValue, float.MinValue },
            Normal = Vector3.zero
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
                    if(annot.Bounds.min[0]==float.MinValue)
                    {
                        annot.Bounds.SetMinMax(new Vector3(partialRes[0], partialRes[1], partialRes[2]), new Vector3(partialRes[3], partialRes[4], partialRes[5]));
                    }
                    annot.Bounds.Encapsulate(new Vector3(partialRes[0], partialRes[1], partialRes[2]));
                    annot.Bounds.Encapsulate(new Vector3(partialRes[3], partialRes[4], partialRes[5]));
                    /*for(int i = 0; i < 3; i++)
                    {
                        annot.BoundingMin[i] = Math.Min(annot.BoundingMin[i], partialRes[i]);
                        annot.BoundingMax[i] = Math.Max(annot.BoundingMax[i], partialRes[i+3]);
                    }*/
                }
            }
        );

        Parallel.For(0, uvHeight,
            () => new Vector3(0,0,0),
            (j, state, partialRes) =>
            {
                for (int i = 0; i < uvWidth; i++)
                {
                    if (newRGBA[j * uvWidth + i].a == 0) //Transparent UV
                        continue;

                    int srcJ = (int)(newRGBA[j * uvWidth + i].g * srcHeight);
                    int srcI = (int)(newRGBA[j * uvWidth + i].r * srcWidth);
                    int srcIdx = (srcJ * srcWidth + srcI)*4;

                    if (m_uvToPositionPixels[srcIdx + 3] == 0) //Unknown 3D position
                        continue;

                    partialRes += new Vector3(m_uvToNormalPixels[srcIdx], m_uvToNormalPixels[srcIdx+1], m_uvToNormalPixels[srcIdx+2]);

                }
                return partialRes;
            },
            (partialRes) =>
            {
                lock (annot)
                {
                    annot.Normal += partialRes;
                }
            }
        );
        annot.Normal = annot.Normal.normalized;
        Debug.Log($"Finish to add annotation Color {layerColor}");
        //m_annotations.Add(annot);
        data.AddAnnoationRenderInfo(annot);

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

    public void OnSetCurrentAction(SelectionModelData model, CurrentAction action)
    {
        m_updateHighlight = true;
    }

    public void OnSetCurrentHighlight(SelectionModelData model, AnnotationID mainID, AnnotationID secondMainID)
    {
        m_updateHighlight = true;
    }

    void SelectionModelData.ISelectionModelDataListener.OnSetSelectedAnnotations(SelectionModelData model, List<AnnotationID> ids)
    {
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
    //public List<AnnotationRenderInfo> Annotations
    //{
    //    get => m_annotations;
    //}
}