using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractionBasic : MonoBehaviour
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
}
