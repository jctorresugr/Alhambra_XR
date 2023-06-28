using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BoundsExt
{
    public static Bounds INVALID_BOUNDS = new Bounds(Vector3.zero,new Vector3(float.MaxValue,float.MaxValue,float.MaxValue));

    public static void Add(this ref Bounds bounds, Vector3 pos)
    {
        if(bounds!=INVALID_BOUNDS)
        {
            bounds.Encapsulate(pos);
        }else
        {
            bounds.SetMinMax(pos, pos);
        }
    }

    public static void Add(this ref Bounds bounds, Bounds addBound)
    {
        if (bounds != INVALID_BOUNDS)
        {
            bounds.Encapsulate(addBound);
        }
        else
        {
            bounds.SetMinMax(addBound.center, addBound.center);
            bounds.extents = addBound.extents;
        }
    }
}
