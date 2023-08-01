using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerDisplayText : SocketDataBasic
{
    public ScreenText screenText;
    protected void Start()
    {
        FastReg<ScreenTextInfo>(OnReceiveScreenTextTime);
        FastReg<string>(OnReceiveScreenText);
    }

    [Serializable]
    public class ScreenTextInfo
    {
        public string text;
        public float time;
    }

    public void OnReceiveScreenTextTime(ScreenTextInfo screenTextInfo)
    {
        screenText.SetText(screenTextInfo.text, screenTextInfo.time);
    }

    public void OnReceiveScreenText(string s)
    {
        screenText.SetText(s);
    }
}
