using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Utils
{
    public static T EnsureComponent<T>(MonoBehaviour obj, ref T comp)
    {
        if(comp==null)
        {
            comp = obj.GetComponent<T>();
            if (comp == null)
            {
                Debug.LogWarning("Cannot init component for " + obj.ToString() + ", expect component: " + comp.GetType().Name);
            }
        }
        return comp;
    }
}
