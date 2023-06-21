using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class LineRenderExt
{
    public static void AssignPositionData(this LineRenderer render, Vector3[] positions)
    {
        render.positionCount = positions.Length;
        render.SetPositions(positions);
    }

    public static void AssignPositionData(this LineRenderer render, Vector3 p0, Vector3 p1)
    {
        render.positionCount = 2;
        render.SetPositions(new Vector3[2] { p0,p1});
    }
}
