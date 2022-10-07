using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System;
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

    /// <summary>
    /// The MonoBehaviour script handling the panel picking of the Alhambra model
    /// </summary>
    public PickPano m_pickPanoModel;

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
        CultureInfo.DefaultThreadCurrentCulture = System.Globalization.CultureInfo.InvariantCulture;

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


    public void OnConnectionStatus(AlhambraServer server, ConnectionStatus status)
    {
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
            Debug.Log(detailedMsg);
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

                //Create the texture that we will read, and a RenderTexture that the camera will render into
                Texture2D screenShot = new Texture2D(Camera.main.scaledPixelWidth, Camera.main.scaledPixelHeight, TextureFormat.RGBA32, false);
                RenderTexture rt = new RenderTexture(screenShot.width, screenShot.height, 24);

                //Render what the specific mesh from the camera position in the render texture (and thus in the linked screenShot texture)
                CommandBuffer buf = new CommandBuffer();
                buf.SetRenderTarget(rt, 0, CubemapFace.Unknown, -1); //-1 == all the color buffers (I guess, this is undocumented)
                buf.DrawMesh(m_pickPanoModel.Mesh, m_pickPanoModel.transform.localToWorldMatrix, m_pickPanoModel.GetComponent<Renderer>().material);
                Camera.main.AddCommandBuffer(CameraEvent.AfterDepthTexture, buf);
                Camera.main.Render();
                Camera.main.RemoveCommandBuffer(CameraEvent.AfterDepthTexture, buf);

                //Read back the pixels from the render texture to the texture
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, screenShot.width, screenShot.height), 0, 0);

                //Clean up our messes
                Camera.main.targetTexture = null;
                RenderTexture.active = null;
                Destroy(rt);

                //Copy some values for them to be usable in a separate thread (Task.Run)...
                byte[] pixels = new byte[4 * screenShot.width * screenShot.height];
                int width = screenShot.width;
                int height = screenShot.height;
                Vector3 cameraPos = Camera.main.transform.position;
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
