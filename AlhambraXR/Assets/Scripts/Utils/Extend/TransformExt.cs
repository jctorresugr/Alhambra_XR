using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class TransformExt
{
    public static void SetGlobalScale(this Transform t, Vector3 scale)
    {
        //TODO: optimize computation
        Transform parent = t.parent;
        t.parent = null;
        t.localScale = scale;
        t.parent = parent;
    }
}
