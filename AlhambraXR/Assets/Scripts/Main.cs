using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// The equivalent of the "Main" specifically designed to work with Unity
/// </summary>
public class Main : MonoBehaviour, AlhambraServer.IAlhambraServerListener, PickPano.IPickPanoListener, Client.IClientListener, Model.IModelListener, IMixedRealityInputActionHandler
{
    /// <summary>
    /// The server application
    /// </summary>
    private AlhambraServer m_server = new AlhambraServer();
    
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
    private Model m_model = new Model();

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

    private Queue<Action> m_tasksInMainThread = new Queue<Action>();

    /// <summary>
    /// The MonoBehaviour script handling the panel picking of the Alhambra model
    /// </summary>
    public PickPano m_pickPanoModel;

    /// <summary>
    /// The material to display UV mapping of a particular mesh
    /// </summary>
    public Material UVMaterial;

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


    private void Awake()
    {
        //Parameterize Unity
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
        m_persistantPath = Application.persistentDataPath;
        CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture; //Useful to enforce dots, and not commas, on double/float values when we print them
        RTCamera.CopyFrom(Camera.main);
        RTCamera.name = "RTCamera";
        RTCamera.targetDisplay = -1;

        m_server.Launch();
        m_server.AddListener(this);
        m_model.AddListener(this);
        m_pickPanoModel.Init(m_model);
        m_pickPanoModel.AddListener(this);

        //Default text helpful to bind headset to tablet
        m_updateIPTexts = true;
        m_enableIPTexts = !m_server.Connected;
        m_updateRandomText = true;
        m_enableRandomText = false;

        //Necessary because Main cannot be focused
        CoreServices.InputSystem?.RegisterHandler<IMixedRealityInputActionHandler>(this);
    }

    private void Start()
    {}

    private void Update()
    {
        lock(this)
        {
            HandleIPTxt();
            HandleRandomText();
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

        //Handle the highlight action type
        if(commonMsg.action == "highlight")
        {
            ReceivedMessage<HighlightMessage> detailedMsg = ReceivedMessage<HighlightMessage>.FromJSON(msg);
            m_model.CurrentAction    = CurrentAction.IN_HIGHLIGHT;
            m_model.CurrentHighlight = new PairLayerID() { Layer = detailedMsg.data.layer, ID = detailedMsg.data.id };
        }

        else if(commonMsg.action == "startAnnotation")
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
                m_model.CurrentHighlight = new PairLayerID() { Layer = -1, ID = -1 };
            }
        }

        else if(commonMsg.action == "finishAnnotation")
        {
            ReceivedMessage<FinishAnnotationMessage> detailedMsg = ReceivedMessage<FinishAnnotationMessage>.FromJSON(msg);
            lock(this)
            {
                //Purpose: Get the UV mapping of the scene at the particular pos/rotation
                m_tasksInMainThread.Enqueue(() =>
                {
                    RTCamera.transform.position = new Vector3(detailedMsg.data.cameraPos[0], detailedMsg.data.cameraPos[1], detailedMsg.data.cameraPos[2]);
                    RTCamera.transform.rotation = new Quaternion(detailedMsg.data.cameraRot[1], detailedMsg.data.cameraRot[2], detailedMsg.data.cameraRot[3], detailedMsg.data.cameraRot[0]);

                    //Create quad from lines
                    Mesh lines = new Mesh();
                    List<Vector3> vertices = new List<Vector3>();
                    List<int>     indices  = new List<int>();
                    float lineWidth = 0.01f;//width in screen space. Should be parametreable at one point...

                    foreach(Stroke stroke in detailedMsg.data.strokes)
                    {
                        for(int i = 0; i < stroke.points.Length-4; i += 2)
                        {
                            //Get the points of the line
                            float xStart = 2 * stroke.points[i]   / detailedMsg.data.width  - 1;
                            float yStart = 2 * stroke.points[i+1] / detailedMsg.data.height - 1;
                            float xEnd   = 2 * stroke.points[i+2] / detailedMsg.data.width  - 1;
                            float yEnd   = 2 * stroke.points[i+3] / detailedMsg.data.height - 1;

                            //Lines are too close...
                            if(xEnd == xStart && yEnd == yStart)
                                continue;

                            //Get the normal of the line
                            float normalX = yStart - yEnd;
                            float normalY = xEnd   - xStart;
                            float mag      = (float)Math.Sqrt(normalX*normalX + normalY*normalY);
                            normalX /= mag;
                            normalY /= mag;

                            //Add the four points generated from the line that has a width "lineWidth"
                            vertices.Add(new Vector3(xStart - normalX/2.0f*lineWidth,
                                                     yStart - normalY/2.0f*lineWidth,
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
                            indices.Add(vertices.Count-4);
                            indices.Add(vertices.Count-3);
                            indices.Add(vertices.Count-1);

                            //Second triangle
                            indices.Add(vertices.Count - 4);
                            indices.Add(vertices.Count - 1);
                            indices.Add(vertices.Count - 2);
                        }
                    }
                    lines.vertices    = vertices.ToArray();
                    lines.indexFormat = IndexFormat.UInt32;
                    lines.SetIndices(indices.ToArray(), MeshTopology.Triangles, 0);
                    lines.RecalculateBounds();

                    if (!SystemInfo.SupportsTextureFormat(TextureFormat.RGBAFloat) ||
                        !SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat) ||
                        !SystemInfo.IsFormatSupported(UnityEngine.Experimental.Rendering.GraphicsFormat.R32G32B32A32_SFloat, UnityEngine.Experimental.Rendering.FormatUsage.ReadPixels))
                        Debug.Log("Issue with float...");

                    //Create the texture that we will read, and a RenderTexture that the camera will render into
                    Texture2D screenShot = new Texture2D(2048, 2048, TextureFormat.RGBAFloat, false);
                    RenderTexture rt     = new RenderTexture(screenShot.width, screenShot.height, 24, RenderTextureFormat.ARGBFloat);
                    rt.Create();

                    //Render what the specific mesh from the camera position in the render texture (and thus in the linked screenShot texture)
                    CommandBuffer buf = new CommandBuffer();
                    buf.SetRenderTarget(rt, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
                    buf.DrawMesh(lines, Matrix4x4.identity, StencilMaskMaterial, 0, -1);
                    buf.DrawMesh(m_pickPanoModel.Mesh, m_pickPanoModel.transform.localToWorldMatrix, UVMaterial, 0, -1);
                    RTCamera.AddCommandBuffer(CameraEvent.AfterDepthTexture, buf);
                    RTCamera.Render();
                    RTCamera.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, buf);

                    //Read back the pixels from the render texture to the texture
                    RenderTexture.active = rt;
                    screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);

                    //Clean up our messes
                    RTCamera.targetTexture = null;
                    RenderTexture.active = null;
                    Destroy(rt);

                    //Anchor the annotation in the PickPano object
                    m_pickPanoModel.AddAnnotation(screenShot);
                });
            }
        }
    }

    public void OnSetCurrentAction(Model model, CurrentAction action)
    {}

    public void OnSetCurrentHighlight(Model model, PairLayerID id)
    {}

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

                Texture2D screenShot = RenderPickPanoMeshInRT(Camera.main, m_pickPanoModel.GetComponent<Renderer>().material);

                //Copy some values for them to be usable in a separate thread (Task.Run)...
                byte[] pixels = new byte[4 * screenShot.width * screenShot.height];
                int width = screenShot.width;
                int height = screenShot.height;
                Vector3    cameraPos = Camera.main.transform.position;
                Quaternion cameraRot = Camera.main.transform.rotation;
                screenShot.GetRawTextureData().CopyTo(pixels, 0);

                //...and send them asynchronously to the tablet client
                Task.Run(() =>
                {
                    //SavePPMImageForDebug(pixels, width, height, "image.ppm");
                    m_server.SendASCIIStringToClients(JSONMessage.StartAnnotation(pixels, width, height, cameraPos, cameraRot));
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
        RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 24);

        //Render what the specific mesh from the camera position in the render texture (and thus in the linked screenShot texture)
        CommandBuffer buf = new CommandBuffer();
        buf.SetRenderTarget(rt, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
        buf.DrawMesh(m_pickPanoModel.Mesh, m_pickPanoModel.transform.localToWorldMatrix, mat);
        cam.AddCommandBuffer(CameraEvent.AfterDepthTexture, buf);
        cam.Render();
        cam.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, buf);

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
}
