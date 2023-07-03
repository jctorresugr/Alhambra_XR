using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

//TODO: seperated from Main
public class ServerJsonParser : MonoBehaviour,
    AlhambraServer.IAlhambraServerListener,
    Client.IClientListener
{

    public delegate void ProcessMessageFunc(Client c, string msg);
    public Dictionary<string, ProcessMessageFunc> onReceiveMessage = new Dictionary<string, ProcessMessageFunc>();

    private AlhambraServer server;
    public void Init(AlhambraServer server)
    {
        this.server = server;
        server.AddListener(this);
    }

    public void SendASCIIStringToClients(string s)
    {
        server.SendASCIIStringToClients(s);
    }

    public void SendASCIIStringToClients(List<string> ss)
    {
        foreach(string s in ss)
        {
            server.SendASCIIStringToClients(s);
        }
    }

    private int packNumCount = 0;

    public void SendSeqMessage(PackedJSONMessages packed)
    {
        List<string> ss = SeqJSONMessage.GenerateSeqJSON(packed, packNumCount);
        SendASCIIStringToClients(ss);
        packNumCount++;
    }

    void Client.IClientListener.OnClose(Client c)
    {
    }

    void AlhambraServer.IAlhambraServerListener.OnConnectionStatus(
        AlhambraServer server, ConnectionStatus status)
    {
        if (status == ConnectionStatus.DISCONNECTED)
        {

        }else
        {
            server.TabletClient.AddListener(this);
        }
    }

    void Client.IClientListener.OnRead(Client c, string msg)
    {
        //Issue with the JSON utility of Unity: Need to deserialize once to know what to expect, and a second time to get the other attributes...
        //This adds a subtential overhead, but it does not require installation of a third party library
        CommonMessage commonMsg = CommonMessage.FromJSON(msg);

        //read packed message
        if (commonMsg.action == PackedJSONMessages.ACTION_NAME)
        {
            ReceivedMessage<PackedJSONMessages> messages = ReceivedMessage<PackedJSONMessages>.FromJSON(msg);
            List<string> actions = messages.data.actions;
            foreach (string action in actions)
            {
                (this as Client.IClientListener).OnRead(c, action);
            }
        }
        else
        {
            if(onReceiveMessage.ContainsKey(commonMsg.action))
            {
                ProcessMessageFunc processMessageFunc = onReceiveMessage[commonMsg.action];
                processMessageFunc.Invoke(c, msg);
            }else
            {
                Debug.LogWarning("Unregistered Action " + commonMsg.action);
            }
            
        }
    }
}
