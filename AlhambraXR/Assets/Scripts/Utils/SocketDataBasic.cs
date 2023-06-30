using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SocketDataBasic : MonoBehaviour
{
    public ServerJsonParser server;

    /*
    protected void Awake()
    {
        Utils.EnsureComponent(this, ref main);
    }*/

    protected void RegReceiveInfo(string name, ServerJsonParser.ProcessMessageFunc func)
    {
        if(server.onReceiveMessage.ContainsKey(name))
        {
            Debug.LogWarning("main.onReceiveMessage already has registered" + name);
        }
        server.onReceiveMessage[name] = func;
    }

    protected void SendClientString(string content)
    {
        server.SendASCIIStringToClients(content);
    }

    protected void SendClientAction<T>(string actionName, T data)
    {
        server.SendASCIIStringToClients(JSONMessage.ActionJSON(actionName, data));
    }

    protected void SendClientFailure<T>(T data)
    {
        server.SendASCIIStringToClients(JSONMessage.FailJSON(data));
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
        server.SendASCIIStringToClients(JSONMessage.FailJSON(temp));
    }


    // reg


    protected static string ProcessMethodName(string name)
    {
        if (name.StartsWith("OnReceive"))
        {
            return name.Substring(9);
        }
        else if (name.StartsWith("Send"))
        {
            return name.Substring(4);
        }
        else if (name.StartsWith("Get"))
        {
            return name.Substring(3);
        }
        Debug.LogWarning("Problem with method name: " + name);
        return name;
    }


    // C# cannot infer generic type like C++ :(
    protected void FastReg<T>(Action<T> action)
    {
        string name = ProcessMethodName(action.Method.Name);
        //name = name.Replace("OnReceive", "");
        RegReceiveInfo(name, (c, msg) => Parse(c, msg, action));
        Debug.Log("Fast reg socket message: " + name);
    }

    protected void Parse<T>(Client c, string msg, Action<T> func)
    {
        Debug.Log("Process " + msg);
        func(JsonUtility.FromJson<ReceivedMessage<T>>(msg).data);
    }
}
