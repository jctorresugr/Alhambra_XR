using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class ColorExt
{
    public static Color32 Color32(this Color c)
    {
        byte cr = (byte)(c.r * 255);
        byte cg = (byte)(c.g * 255);
        byte cb = (byte)(c.b * 255);
        byte ca = (byte)(c.a * 255);

        return new Color32(cr, cg, cb, ca);
    }
}
