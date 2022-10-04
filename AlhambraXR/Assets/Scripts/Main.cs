using System;
using UnityEngine;

/// <summary>
/// The equivalent of the "Main" specifically designed to work with Unity
/// </summary>
public class Main : MonoBehaviour, IAlhambraServerListener, PickPano.IPickPanoListener, IClientListener
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
        m_server.Launch();
        m_server.AddListener(this);
        m_pickPanoModel.AddListener(this);

        //Default text helpful to bind headset to tablet
        m_updateIPTexts = true;
        m_enableIPTexts = !m_server.Connected;
        
        m_updateRandomText = true;
        m_enableRandomText = false;
    }

    private void Start()
    {}

    private void Update()
    {
        HandleIPTxt();
        HandleRandomText();
    }

    private void OnDestroy()
    {
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
            //Enable/Disable the IP Text
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
            this.m_updateIPTexts = true;
            this.m_enableIPTexts = true;
        }
        else
        {
            this.m_updateIPTexts = true;
            this.m_enableIPTexts = false;

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
            m_pickPanoModel.HighlightDataChunk(detailedMsg.data.layer, detailedMsg.data.id);
        }
    }
}
