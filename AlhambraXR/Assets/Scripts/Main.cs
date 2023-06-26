using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Unity.Burst;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The equivalent of the "Main" specifically designed to work with Unity
/// </summary>
public class Main : MonoBehaviour, 
    AlhambraServer.IAlhambraServerListener, 
    PickPano.IPickPanoListener, 
    Client.IClientListener, 
    SelectionModelData.ISelectionModelDataListener, 
    IMixedRealityInputActionHandler
{
    
    /// <summary>
    /// The server application
    /// </summary>
    private AlhambraServer m_server = new AlhambraServer();
    public AlhambraServer Server => m_server;

    /// <summary>
    /// Store all data here!
    /// </summary>
    public DataManager data;
    
    /// <summary>
    /// Should we enable the IPText?
    /// </summary>
    private bool m_enableIPTexts = false;

    /// <summary>
    /// Should we update the text values?
    /// </summary>
    private bool m_updateIPTexts = false;

    /// <summary>
    /// The model of the application
    /// </summary>
    private SelectionModelData m_model = new SelectionModelData();

    /// <summary>
    /// The Random string value
    /// </summary>
    private String m_randomStr = "";

    /// <summary>
    /// Should we enable the Random Text?
    /// </summary>
    private bool m_enableRandomText = false;

    /// <summary>
    /// Should we update the random text values?
    /// </summary>
    private bool m_updateRandomText = false;

    /// <summary>
    /// Save Application.persistentDataPath for it to be used asynchronously...
    /// </summary>
    private String m_persistantPath = "";

    /// <summary>
    /// The tasks to run in the main thread
    /// </summary>
    private Queue<Action> m_tasksInMainThread = new Queue<Action>();

    /// <summary>
    /// The current annotation highlighted
    /// </summary>
    private AnnotationRenderInfo m_curAnnotation = null;

    /// <summary>
    /// The MonoBehaviour script handling the panel picking of the Alhambra model
    /// </summary>
    public PickPano PickPanoModel;

    /// <summary>
    /// The pointing arrow game object
    /// </summary>
    public GameObject PointingArrow;

    /// <summary>
    /// The material to display UV mapping of a particular mesh
    /// </summary>
    public Material UVMaterial;

    /// <summary>
    /// The material used to render the mask from stroke annotations (using the stencil buffer)
    /// </summary>
    public Material StencilMaskMaterial;

    /// <summary>
    /// Camera used with RenderTexture to retrieve information
    /// </summary>
    public Camera RTCamera;

    /// <summary>
    /// The IP header text being displayed
    /// </summary>
    public UnityEngine.UI.Text IPHeaderText;

    /// <summary>
    /// The IP value text being displayed
    /// </summary>
    public UnityEngine.UI.Text IPValueText;

    /// <summary>
    /// Any text that has to be displayed
    /// </summary>
    public UnityEngine.UI.Text RandomText;

    public DataSync dataSync;

    public SelectionModelData SelectionData => m_model;

    /// <summary>
    /// A better navigator? in progress
    /// </summary>
    //public LineNavigator lineNavigator;

    public AnnotationJointRenderBuilder annotationJointRenderBuilder;

    public delegate void ProcessMessageFunc(Client c, string msg);

    public Dictionary<string, ProcessMessageFunc> onReceiveMessage;

    //public LineNavigatorManager lineNavigatorManager;

    public NavigationManager navigationManager;
    public AnnotationRenderBuilder annotationRenderBuilder;
    //public VolumeNavigation volumeNavigation;

    //public VolumeAnalyze volumeAnalyze;


    private void Awake()
    {
        Debug.Log("Main Awake");
        onReceiveMessage = new Dictionary<string, ProcessMessageFunc>();
        //Parameterize Unity
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        m_persistantPath = Application.persistentDataPath;
        CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture; //Useful to enforce dots, and not commas, on double/float values when we print them
        PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOn, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Right);
        PointerUtils.SetHandRayPointerBehavior(PointerBehavior.AlwaysOn, Microsoft.MixedReality.Toolkit.Utilities.Handedness.Left);

        //Parameterize the RenderTexture Camera
        RTCamera.CopyFrom(Camera.main);
        RTCamera.name = "RTCamera";
        RTCamera.targetDisplay = -1;
        RTCamera.stereoTargetEye = StereoTargetEyeMask.Left;

        //Test that the plateforms handles some texture reading and format
        if (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat) ||
            !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) ||
            !SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat, UnityEngine.Experimental.Rendering.FormatUsage.ReadPixels) ||

            !SystemInfo.SupportsTextureFormat(TextureFormat.RGBA32) ||
            !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32) ||
            !SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R8G8B8A8_UNorm, UnityEngine.Experimental.Rendering.FormatUsage.ReadPixels))
        {
            Debug.LogWarning("Issue with ARGBFloat or ARGB32 textures... This device cannot anchor annotations correctly.");
        }

        data.Init();
        // init ui first, add listeners

        
        m_model.AddListener(this);
        PickPanoModel.Init(m_model);
        PickPanoModel.AddListener(this);

        data.LoadDefaultData();
        data.modelBounds = PickPanoModel.Mesh.bounds;

        navigationManager.Init(m_model);

        annotationRenderBuilder.Init();
        annotationRenderBuilder.DrawAllAnnotation();

        annotationJointRenderBuilder.Init();
        annotationJointRenderBuilder.DrawAllAnnotationJoints();

        m_server.Launch();
        m_server.AddListener(this);

        //Default text helpful to bind headset to tablet
        m_updateIPTexts = true;
        m_enableIPTexts = !m_server.Connected;
        m_updateRandomText = true;
        m_enableRandomText = false;

        //Necessary because Main cannot be focused
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
        //Debug messages
        //OnRead(null, "{\"action\": \"highlight\", \"data\": { \"layer\": 0, \"id\": 112} }");

        

    }

    private void Start()
    {}

    private void Update()
    {
        lock(this)
        {
            HandleIPTxt();
            HandleRandomText();
            HandlePointingArrow();
            HandleTasks();
        }
    }

    private void OnDestroy()
    {
        CoreServices.InputSystem?.UnregisterHandler<IMixedRealityInputActionHandler>(this);
        m_server.Close();
    }

    /// <summary>
    /// Handles the server status texts
    /// </summary>
    private void HandleIPTxt()
    {
        //Update the displayed text requiring networking attention
        if (m_updateIPTexts)
        {
            //Enable/Disable the IP Text
            IPHeaderText.enabled = m_enableIPTexts;
            IPValueText.enabled  = m_enableIPTexts;

            //If we should enable the text, set the text value
            if (m_enableIPTexts)
            {
                IPHeaderText.text = "Headset IP address:";
                IPValueText.text  = $"{ServerSocket.DeviceServerAddress}:{AlhambraServer.SERVER_PORT}";
            }
            m_updateIPTexts = false;
        }
    }

    /// <summary>
    /// Handle the random text to display
    /// </summary>
    private void HandleRandomText()
    {
        //Update the displayed text requiring networking attention
        if (m_updateRandomText)
        {
            //Enable/Disable the random text
            RandomText.enabled = m_enableRandomText;

            //If we should enable the text, set the text value
            if (m_enableRandomText)
            {
                RandomText.text = m_randomStr;
            }
            m_updateRandomText = false;
        }
    }

    /// <summary>
    /// Handle the 3D arrow that points toward annotations
    /// </summary>
    private void HandlePointingArrow()
    {
        if(m_curAnnotation != null)
        {
            PointingArrow.SetActive(true);
            Vector3 annotationPos = PickPanoModel.transform.localToWorldMatrix.MultiplyPoint3x4(m_curAnnotation.Center);
            PointingArrow.transform.rotation = Quaternion.LookRotation(annotationPos - Camera.main.transform.position, new Vector3(0, 1, 0));
        }
        else
        {
            PointingArrow.SetActive(false);
        }
            
    }

    /// <summary>
    /// Handle tasks to run in the main thread.
    /// </summary>
    private void HandleTasks()
    {
        while(m_tasksInMainThread.Count > 0)
        {
            Action tasks = m_tasksInMainThread.Dequeue();
            tasks.Invoke();
        }
    }

    public void OnConnectionStatus(AlhambraServer server, ConnectionStatus status)
    {
        //Show the IP address of the device in case of a disconnection. Otherwise, hide it.
        Debug.Log("Connection status:" + status);
        if(status == ConnectionStatus.DISCONNECTED)
        {
            lock(this)
            { 
                this.m_updateIPTexts = true;
                this.m_enableIPTexts = true;
            }
        }
        else
        {
            lock (this)
            {
                this.m_updateIPTexts = true;
                this.m_enableIPTexts = false;
            }
            server.TabletClient.AddListener(this);

            //Send annotation data
            PackedJSONMessages pack = new PackedJSONMessages();
            lock (this)
            {
                pack.AddString(DataSync.GetUpdateModelBounds(data.modelBounds));
                foreach(Annotation annot in data.Annotations)
                {
                    if (annot.isLocalData)
                    {
                        pack.AddString(DataSync.GetUpdateAnnotationRenderInfo(annot));
                        continue;
                    }
                    pack.AddString(JSONMessage.AddAnnotationToJSON(annot));
                    //TODO: resolve uncomment annotation
                    //m_server.SendASCIIStringToClients(JSONMessage.AddAnnotationToJSON(annot));
                    Debug.Log("Sync Annotation " + annot.ID);
                }

                foreach (AnnotationJoint joint in data.AnnotationJoints)
                {
                    pack.AddAction("SyncAnnotationJoint", joint);
                    //m_server.SendASCIIStringToClients(JSONMessage.ActionJSON("SyncAnnotationJoint", joint));
                    Debug.Log("Sync AnnotationJoint " + joint.ID);
                } 
            }
            m_server.SendASCIIStringToClients(pack.ToString());
        }
    }

    public void OnSelection(PickPano pano, Color c)
    {
        m_server.SendASCIIStringToClients(JSONMessage.SelectionToJSON(c));
    }

    public void OnClose(Client c)
    {}

    public void OnRead(Client c, string msg)
    {
        //Issue with the JSON utility of Unity: Need to deserialize once to know what to expect, and a second time to get the other attributes...
        //This adds a subtential overhead, but it does not require installation of a third party library
        CommonMessage commonMsg = CommonMessage.FromJSON(msg);

        //read packed message
        if(commonMsg.action == PackedJSONMessages.ACTION_NAME)
        {
            ReceivedMessage<PackedJSONMessages> messages = ReceivedMessage<PackedJSONMessages>.FromJSON(msg);
            List<string> actions = messages.data.actions;
            foreach(string action in actions)
            {
                OnRead(c, action);
            }
        }
        //Handle the highlight action type
        else if (commonMsg.action == "highlight")
        {
            ReceivedMessage<HighlightMessage> detailedMsg = ReceivedMessage<HighlightMessage>.FromJSON(msg);
            m_model.CurrentAction = CurrentAction.IN_HIGHLIGHT;
            m_model.CurrentHighlightMain = new AnnotationID(detailedMsg.data.layer, detailedMsg.data.id);
            m_model.CurrentHighlightSecond = AnnotationID.INVALID_ID;
            m_model.ClearSelectedAnnotations();
            if(m_model.CurrentHighlightMain.IsValid)
            {
                m_model.AddSelectedAnnotations(m_model.CurrentHighlightMain);
            }
            
            //lineNavigatorManager.SetAnnotations(data.FindAnnotationID(m_model.CurrentHighlightMain));
        }

        else if (commonMsg.action == "startAnnotation")
        {
            //Display instruction to the user
            lock (this)
            {
                m_updateRandomText = true;
                m_enableRandomText = true;
                m_randomStr = "Tap to start annotating\non the tablet";
            }

            //Change the current action and remove highlighting if needed
            CurrentAction oldAction = m_model.CurrentAction;
            m_model.CurrentAction = CurrentAction.START_ANNOTATION;
            if (oldAction != CurrentAction.IN_HIGHLIGHT)
            {
                m_model.CurrentHighlightMain = AnnotationID.INVALID_ID;
                m_model.CurrentHighlightSecond = AnnotationID.INVALID_ID;
            }
        }

        else if (commonMsg.action == "finishAnnotation")
        {
            ReceivedMessage<FinishAnnotationMessage> detailedMsg = ReceivedMessage<FinishAnnotationMessage>.FromJSON(msg);
            HandleFinishAction(detailedMsg);
        }

        else if (commonMsg.action == "showAllAnnotation")
        {
            HandleShowAll();
        }

        else if(commonMsg.action == "stopShowAllAnnotation")
        {
            HandleStopShowAll();
        }else
        {
            ProcessMessageFunc processMessageFunc = onReceiveMessage[commonMsg.action];
            if(processMessageFunc==null)
            {
                Debug.LogWarning("Unknown Action " + commonMsg.action);
                return;
            }
            processMessageFunc.Invoke(c, msg);
        }
        //TODO: write here!!!!
    }

    /// <summary>
    /// Handle the "finishAction" received by the tablet client
    /// </summary>
    /// <param name="detailedMsg">The message the client sent to anchor and generate a new annotation</param>
    private void HandleFinishAction(ReceivedMessage<FinishAnnotationMessage> detailedMsg)
    {
        lock (this)
        {
            //Purpose: Get the UV mapping of the scene at the particular pos/rotation
            m_tasksInMainThread.Enqueue(() =>
            {
                RTCamera.transform.position = new Vector3(detailedMsg.data.cameraPos[0], detailedMsg.data.cameraPos[1], detailedMsg.data.cameraPos[2]);
                RTCamera.transform.rotation = new Quaternion(detailedMsg.data.cameraRot[1], detailedMsg.data.cameraRot[2], detailedMsg.data.cameraRot[3], detailedMsg.data.cameraRot[0]);

                Mesh mesh = GenerateMeshFromStrokesPolygons(detailedMsg.data.strokes, detailedMsg.data.polygons, detailedMsg.data.width, detailedMsg.data.height); //Generate the mesh from the strokes and polygons.

                //Create the texture that we will read, and a RenderTexture that the camera will render into for UV mappings
                Texture2D uvScreenShot = new Texture2D(2048, 2048, TextureFormat.RGBAFloat, false);
                RenderTexture uvRT = new RenderTexture(uvScreenShot.width, uvScreenShot.height, 24, RenderTextureFormat.ARGBFloat);
                uvRT.Create();

                //Create the texture that we will read, and a RenderTexture that the camera will render into for the generated color texture
                Texture2D colorScreenShot = new Texture2D(2048, 2048, TextureFormat.RGBA32, false);
                RenderTexture colorRT = new RenderTexture(colorScreenShot.width, colorScreenShot.height, 24, RenderTextureFormat.ARGB32);
                colorRT.Create();

                //Render the specific mesh from the camera position in the render texture (and thus in the linked screenShot texture)
                CommandBuffer buf = new CommandBuffer();
                buf.SetRenderTarget(new RenderTargetIdentifier[2] { uvRT, colorRT }, uvRT, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
                buf.DrawMesh(mesh, Matrix4x4.identity, StencilMaskMaterial, 0, -1);
                buf.DrawMesh(PickPanoModel.Mesh, PickPanoModel.transform.localToWorldMatrix, UVMaterial, 0, -1);
                RTCamera.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
                RTCamera.Render();
                RTCamera.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);

                //Read back the pixels from the render texture to the texture
                RenderTexture.active = uvRT;
                uvScreenShot.ReadPixels(new Rect(0, 0, uvScreenShot.width, uvScreenShot.height), 0, 0);

                //Clean up our messes
                RTCamera.targetTexture = null;
                RenderTexture.active = null;
                Destroy(uvRT);

                //Anchor the annotation in the PickPano object
                Color32 annotationColor;
                if (PickPanoModel.AddAnnotation(uvScreenShot, out annotationColor))
                {
                    //Retrieve the color of the annotation
                    RenderTexture.active = colorRT;
                    colorScreenShot.ReadPixels(new Rect(0, 0, colorScreenShot.width, colorScreenShot.height), 0, 0);

                    //Clean up our messes
                    RTCamera.targetTexture = null;
                    RenderTexture.active = null;

                    //Get the minimum snapshot to save memory and screen space in the saving of this annotation RGBA image
                    int newWidth;
                    int newHeight;
                    byte[] newRGBA = MinimumRectImage(colorScreenShot.GetRawTextureData<byte>(), colorScreenShot.width, colorScreenShot.height, out newWidth, out newHeight);

                    //Save that information for later reuses (e.g., to send back on disconnection)
                    AnnotationInfo annot = new AnnotationInfo(annotationColor, newRGBA, newWidth, newHeight, detailedMsg.data.description);
                    Annotation annotation;
                    List<AnnotationJoint> newJoints;
                    lock (this)
                    {
                        data.AddAnnotationInfo(annot);
                        annotation = data.FindAnnotationID(annot.ID);
                        // add joint id to the annotation
                        foreach (int jointID in detailedMsg.data.selectedJointID)
                        {
                            AnnotationJoint selectedJoint = data.FindJointID(jointID);
                            if (selectedJoint == null)
                            {
                                Debug.LogWarning("HandleAnnotationFinishAction: Cannot find selected joint id " + jointID);
                                continue;
                            }
                            selectedJoint.AddAnnotation(annotation);
                        }
                        newJoints = new List<AnnotationJoint>();
                        // create new joint, and add it to the annotation
                        foreach (string name in detailedMsg.data.createdJointName)
                        {
                            AnnotationJoint newJoint = data.AddAnnotationJoint(name);
                            newJoint.AddAnnotation(annotation);
                            newJoints.Add(newJoint);
                        }
                    }
                    //Send the information back to the tablet, if any.
                    Task.Run(() =>
                    {
                        // send annotation information
                        PackedJSONMessages pack = new PackedJSONMessages();
                        pack.AddString(JSONMessage.AddAnnotationToJSON(annotation));
                        //m_server.SendASCIIStringToClients(JSONMessage.AddAnnotationToJSON(annotation));
                        // send add joint
                        foreach (int jointID in detailedMsg.data.selectedJointID)
                        {
                            AnnotationJoint selectedJoint = data.FindJointID(jointID);
                            if (selectedJoint == null)
                            {
                                Debug.LogWarning("HandleAnnotationFinishAction: Cannot find selected joint id " + jointID);
                                continue;
                            }
                            AnnotationID annotationID = annotation.ID;
                            //dataSync.SendAddAnnotationToJoint(jointID, annotationID);
                            pack.AddString(dataSync.GetAddAnnotationToJoint(jointID, annotationID));
                        }
                        // send new joint, add annot to new joint
                        foreach (AnnotationJoint newJoint in newJoints)
                        {
                            //dataSync.SendAddAnnotationJoint(newJoint);
                            pack.AddString(dataSync.GetAddAnnotationJoint(newJoint));
                            //dataSync.SendAddAnnotationToJoint(newJoint.ID, annotation.ID);
                            pack.AddString(dataSync.GetAddAnnotationToJoint(newJoint.ID, annotation.ID));
                        }
                        m_server.SendASCIIStringToClients(pack.ToString());


                    });
                }
                Destroy(colorRT);
            });
        }
    }

    /// <summary>
    /// Get the minimum rect containing an original RGBA32 image (i.e., remove useless alpha bands)
    /// </summary>
    /// <param name="rgba">The original RGBA32 image (its native array)</param>
    /// <param name="width">The width of the image</param>
    /// <param name="height">The height of the image</param>
    /// <param name="newTextureWidth">The width of the newly generated texture, if any. This variable is set to -1 if no pixel is lighten</param>
    /// <param name="newTextureHeight">The height of the newly generated texture, if any. This variable is set to -1 if no pixel is lighten</param>
    /// <returns>null if no pixel are not transparent, this function returns the RGBA32 (column-major) pixels of the minimum image containing the original one without loosing information (i.e., removing alpha bands). 
    /// newTextureWidth and newTextureHeight contains the respective width and height of this newly generated image</returns>
    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
    private byte[] MinimumRectImage(NativeArray<byte> rgba, int width, int height, out int newTextureWidth, out int newTextureHeight)
    {
        int[] colorTextureRect = new int[4] { width, height, -1, -1 };
        newTextureWidth  = -1;
        newTextureHeight = -1;

        //Parallelize the computation to earn time
        Parallel.For(0, height,
            () => new int[4] { width, height, -1, -1 },
            (j, state, partialRes) =>
            {
                bool found = false;
                int firstX = 0;
                int lastX = 0;
                for (int i = 0; i < width; i++)
                {
                    if (rgba[4 * (j * width + i) + 3] != 0) //Check Alpha
                                {
                        if (!found)
                        {
                            found = true;
                            firstX = i;
                        }
                        lastX = i;
                    }
                }

                if (found)
                {
                    partialRes[0] = Math.Min(partialRes[0], firstX);
                    partialRes[1] = Math.Min(partialRes[1], j);
                    partialRes[2] = Math.Max(partialRes[2], lastX);
                    partialRes[3] = Math.Max(partialRes[3], j);
                }
                return partialRes;
            },
            (partialRes) => //Merge results
                        {
                lock (colorTextureRect)
                {
                    colorTextureRect[0] = Math.Min(colorTextureRect[0], partialRes[0]);
                    colorTextureRect[1] = Math.Min(colorTextureRect[1], partialRes[1]);
                    colorTextureRect[2] = Math.Max(colorTextureRect[2], partialRes[2]);
                    colorTextureRect[3] = Math.Max(colorTextureRect[3], partialRes[3]);
                }
            }
        );

        //Check that the texture contains something...
        if (colorTextureRect[0] == width)
        {
            Debug.Log("No annotation were performed... Exit.");
            return null;
        }

        //Create a subtexture
        int newWidth  = (colorTextureRect[2] - colorTextureRect[0]);
        int newHeight = (colorTextureRect[3] - colorTextureRect[1]);
        byte[] newRGBA = new byte[4 * newWidth * newHeight];

        Parallel.For(0, newHeight,
            (j, state) =>
            {
                for (int i = 0; i < newWidth; i++)
                {
                    int destIdx = j * newWidth + i;
                    int srcIdx = (j + colorTextureRect[1]) * width + (i + colorTextureRect[0]);

                    for (int k = 0; k < 4; k++)
                        newRGBA[4 * destIdx + k] = rgba[4 * srcIdx + k];
                }
            }
        );
        newTextureWidth  = newWidth;
        newTextureHeight = newHeight;
        return newRGBA;
    }

    /// <summary>
    /// Generate a mesh containing triangles (which form quads) from a list of strokes and triangulate a set of polygons
    /// </summary>
    /// <param name="strokes">The list of strokes to convert to triangles</param>
    /// <param name="polygons">The list of polygons to convert to triangles</param>
    /// <param name="imgWidth">The original width of the image where the strokes were drawn upon, to normalize the coordinate of the different points of the strokes</param>
    /// <param name="imgHeight">The original height of the image where the strokes were drawn upon, to normalize the coordinate of the different points of the strokes</param>
    /// <returns>The generated mesh containing triangles representing strokes with a given width in the camera space</returns>
    [BurstCompile(FloatPrecision.Medium, FloatMode.Fast)]
    private Mesh GenerateMeshFromStrokesPolygons(Stroke[] strokes, Polygon[] polygons, int imgWidth, int imgHeight)
    {
        //Create triangles from strokes and polygons
        Mesh triangles = new Mesh();
        List<Vector3> vertices = new List<Vector3>();
        List<int> indices = new List<int>();

        //Create quads from strokes...
        foreach (Stroke stroke in strokes)
        {
            float lineWidth = stroke.width / Math.Min(imgWidth, imgHeight) * 2.0f;
            for (int i = 0; i < stroke.points.Length - 4; i += 2)
            {
                //Get the points of the line
                float xStart = 2 * stroke.points[i]     / imgWidth - 1;
                float yStart = 2 * stroke.points[i + 1] / imgHeight - 1;
                float xEnd   = 2 * stroke.points[i + 2] / imgWidth - 1;
                float yEnd   = 2 * stroke.points[i + 3] / imgHeight - 1;

                //Lines are too close to differenciate them...
                if (xEnd == xStart && yEnd == yStart)
                    continue;

                //Get the normal of the line
                float normalX = yStart - yEnd;
                float normalY = xEnd - xStart;
                float mag = (float)Math.Sqrt(normalX * normalX + normalY * normalY);
                normalX /= mag;
                normalY /= mag;

                //Add the four points generated from the line that has a width "lineWidth"
                vertices.Add(new Vector3(xStart - normalX / 2.0f * lineWidth,
                                         yStart - normalY / 2.0f * lineWidth,
                                         0));

                vertices.Add(new Vector3(xStart + normalX / 2.0f * lineWidth,
                                         yStart + normalY / 2.0f * lineWidth,
                                         0));

                vertices.Add(new Vector3(xEnd - normalX / 2.0f * lineWidth,
                                         yEnd - normalY / 2.0f * lineWidth,
                                         0));

                vertices.Add(new Vector3(xEnd + normalX / 2.0f * lineWidth,
                                         yEnd + normalY / 2.0f * lineWidth,
                                         0));

                //Put the indices

                //First triangle
                indices.Add(vertices.Count - 4);
                indices.Add(vertices.Count - 3);
                indices.Add(vertices.Count - 1);

                //Second triangle
                indices.Add(vertices.Count - 4);
                indices.Add(vertices.Count - 1);
                indices.Add(vertices.Count - 2);
            }
        }

        //Create triangles from polygons...
        foreach(Polygon polygon in polygons)
        {
            int originalCount = vertices.Count;

            Vector2[] points = new Vector2[polygon.points.Length / 2];
            for (int i = 0; i < points.Length; i++)
                points[i] = new Vector2(polygon.points[2 * i], polygon.points[2 * i + 1]);

            foreach (Vector2 p in points)
                vertices.Add(new Vector3(2 * p.x / imgWidth - 1, 
                                         2 * p.y / imgHeight - 1,
                                         0));

            int[] polygonIndices = Triangulator.Triangulate(points);
            foreach(int i in polygonIndices)
                indices.Add(originalCount + i);
        }

        //Finally, send all the data to the graphics card
        triangles.vertices = vertices.ToArray();
        triangles.indexFormat = IndexFormat.UInt32;
        triangles.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
        triangles.RecalculateBounds();

        return triangles;
    }

    public void OnSetCurrentAction(SelectionModelData model, CurrentAction action)
    {
        if(action != CurrentAction.IN_HIGHLIGHT)
            lock(this)
                m_curAnnotation = null; //Nothing to highlight. Cancel the current annotation
    }

    public void OnSetCurrentHighlight(SelectionModelData model, AnnotationID mainID, AnnotationID secondID)
    {
        if(model.CurrentAction == CurrentAction.IN_HIGHLIGHT)
        {
            //AnnotationRenderInfo annot = null;

            Annotation annot = data.FindAnnotationID(mainID);
            //Search per layer the annotation data corresponding the demand of highlight
            if(annot!=null)
            {
                lock (this)
                    m_curAnnotation = annot.renderInfo;
            }
            else
            {
                lock (this)
                    m_curAnnotation = null;
            }
            
        }
    }

    public void OnActionStarted(BaseInputEventData eventData)
    {}

    public void OnActionEnded(BaseInputEventData eventData)
    {
        if (eventData.InputSource.SourceType == InputSourceType.Hand && eventData.MixedRealityInputAction.Description == "Select")
        {
            if (m_model.CurrentAction == CurrentAction.START_ANNOTATION)
            {
                //Go back to the default state
                m_model.CurrentAction = CurrentAction.DEFAULT;
                lock (this)
                {
                    m_updateRandomText = true;
                    m_enableRandomText = false;
                }

                RTCamera.transform.position = Camera.main.transform.position;
                RTCamera.transform.rotation = Camera.main.transform.rotation;
                Texture2D screenShot = RenderPickPanoMeshInRT(RTCamera, PickPanoModel.GetComponent<Renderer>().material);

                //Copy some values for them to be usable in a separate thread (Task.Run)...
                byte[] pixels = screenShot.GetRawTextureData();
                int width     = screenShot.width;
                int height    = screenShot.height;
                Vector3    cameraPos = Camera.main.transform.position;
                Quaternion cameraRot = Camera.main.transform.rotation;

                //...and send them asynchronously to the tablet client
                Task.Run(() =>
                {
                    //SavePPMImageForDebug(pixels, width, height, "image.ppm");
                    m_server.SendASCIIStringToClients(JSONMessage.StartAnnotationToJSON(pixels, width, height, cameraPos, cameraRot));
                });
            }
        }
    }

    /// <summary>
    /// Render the PickPanoMesh in a RenterTexture and returns the texture containing the data
    /// </summary>
    /// <param name="cam">The Camera to use to render the PickPano Mesh</param>
    /// <param name="mat">The material to use to render the PickPano Mesh</param>
    /// <returns>The RGBA32 Texture resized using the Camera settings where the mesh is drawn into.</returns>
    private Texture2D RenderPickPanoMeshInRT(Camera cam, Material mat)
    {
        //Create the texture that we will read, and a RenderTexture that the camera will render into
        Texture2D screenShot = new Texture2D(cam.scaledPixelWidth, cam.scaledPixelHeight, TextureFormat.RGBA32, false);
        RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 24, RenderTextureFormat.ARGB32);
        rt.vrUsage = VRTextureUsage.OneEye;
        rt.Create();

        //Render what the specific mesh from the camera position in the render texture (and thus in the linked screenShot texture)
        CommandBuffer buf = new CommandBuffer();
        buf.SetRenderTarget(rt, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
        buf.DrawMesh(PickPanoModel.Mesh, PickPanoModel.transform.localToWorldMatrix, mat);
        cam.AddCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);
        cam.Render();
        cam.RemoveCommandBuffer(CameraEvent.BeforeForwardOpaque, buf);

        //Read back the pixels from the render texture to the texture
        RenderTexture.active = rt;
        screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);

        //Clean up our messes
        cam.targetTexture = null;
        RenderTexture.active = null;
        Destroy(rt);

        return screenShot;
    }

    /// <summary>
    /// Save binary data on disk
    /// </summary>
    /// <param name="data">The binary data</param>
    /// <param name="fileName">The output file name. The final destination is {m_persistantPath}/{fileName}</param>
    public void SaveBinaryDataOnDisk(byte[] data, String fileName)
    {
        String destination = $"{m_persistantPath}/{fileName}";
        Debug.Log($"Saving file to {destination}");
        FileStream file;

        if (File.Exists(destination))
            file = File.OpenWrite(destination);
        else
            file = File.Create(destination);
        file.Write(data, 0, data.Length);
        file.Close();
    }

    /// <summary>
    /// For debug purpose, save a RGBA32 image in a PPM file for later analyses.
    /// </summary>
    /// <param name="pixels">The RGBA32 pixel data, column-major ordered. Expected size: width*height*4</param>
    /// <param name="width">The width of the image</param>
    /// <param name="height">The height of the image</param>
    /// <param name="fileName">The filename of the image to be put in m_persistantPath directory</param>
    private void SavePPMImageForDebug(byte[] pixels, int width, int height, String fileName)
    {
        String destination = $"{m_persistantPath}/{fileName}";
        Debug.Log($"Saving image to {destination}");
        FileStream file;

        if (File.Exists(destination))
            file = File.OpenWrite(destination);
        else
            file = File.Create(destination);

        using (StreamWriter sw = new StreamWriter(file))
        {
            sw.Write($"P3\n{width} {height}\n255");
            for (Int32 j = height - 1; j >= 0; j--)
            {
                for (Int32 i = 0; i < width; i++)
                {
                    if ((i + width * (height - 1 - j)) % (70 / 12) == 0) //70 -- maximum line in caracters for PPM. 12: 3 caracters per components, 3 components, 2 spaces
                        sw.Write("\n");
                    for (int k = 0; k < 3; k++)
                    {
                        sw.Write($"{pixels[j * width * 4 + i * 4 + k]} ");
                    }
                }
            }
        }
        file.Close();
    }

    private void HandleShowAll()
    {
        m_model.CurrentAction = CurrentAction.IN_HIGHLIGHT;
        m_model.CurrentHighlightMain = AnnotationID.LIGHTALL_ID;
        m_model.CurrentHighlightSecond = AnnotationID.LIGHTALL_ID;
    }

    private void HandleStopShowAll()
    {
        m_model.CurrentAction = CurrentAction.DEFAULT;
        m_model.CurrentHighlightMain = AnnotationID.INVALID_ID;
        m_model.CurrentHighlightSecond = AnnotationID.INVALID_ID;
    }

    void SelectionModelData.ISelectionModelDataListener.OnSetSelectedAnnotations(SelectionModelData model, List<AnnotationID> ids)
    {
    }

    void PickPano.IPickPanoListener.OnHover(PickPano pano, Color c)
    {
    }
}