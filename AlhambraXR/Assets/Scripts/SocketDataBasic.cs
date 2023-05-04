using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketDataBasic : MonoBehaviour
{
    public Main main;

    protected void Awake()
    {
        Utils.EnsureComponent(this, ref main);
    }

    protected void RegReceiveInfo(string name, Main.ProcessMessageFunc func)
    {
        if(main.onReceiveMessage.ContainsKey(name))
        {
            Debug.LogWarning("main.onReceiveMessage already has registered" + name);
        }
        main.onReceiveMessage[name] = func;
    }

    protected void SendClientString(string content)
    {
        main.Server.SendASCIIStringToClients(content);
    }

    protected void SendClientAction<T>(string actionName, T data)
    {
        main.Server.SendASCIIStringToClients(JSONMessage.ActionJSON(actionName, data));
    }

    protected void SendClientFailure<T>(T data)
    {
        main.Server.SendASCIIStringToClients(JSONMessage.FailJSON(data));
    }

    [Serializable]
    public struct FailureMessage<T>
    {
        [SerializeField]
        string msg;
        [SerializeField]
        T data;

        public FailureMessage(string msg, T data)
        {
            this.msg = msg;
            this.data = data;
        }
    }
    protected void SendClientFailure<T>(string msg,T data)
    {
        SendClientFailure(new FailureMessage<T>(msg, data));
    }

    // ugly coding, just for prototype, refine it.
    private static List<int> temp = new List<int>();
    protected void SendClientFailure()
    {
        main.Server.SendASCIIStringToClients(JSONMessage.FailJSON(temp));
    }
}
