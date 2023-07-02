using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

// if you care about sequence of message, use this!
[Serializable]
public class PackedJSONMessages
{
    public const string ACTION_NAME = "PackedMessage";
    [SerializeField]
    public List<string> actions = new List<string>();

    public override string ToString()
    {
        return JSONMessage.ActionJSON(ACTION_NAME, this);
    }

    public void AddAction<T>(string actionName, T obj)
    {
        actions.Add(JSONMessage.ActionJSON(actionName, obj));
    }

    public void AddString(string json)
    {
        actions.Add(json);
    }

    
}
