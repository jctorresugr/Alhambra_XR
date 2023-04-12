using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

struct AnnotationID
{
    public int layer;
    public int index;

    public AnnotationID(int layer, int index)
    {
        this.layer = layer;
        this.index = index;
    }

    public AnnotationID(Color32 color)
    {
        if(color.r>0)
        {
            layer = 0;
            index = color.r;
        }
        else if (color.g > 0)
        {
            layer = 1;
            index = color.g;
        }
        else if (color.b > 0)
        {
            layer = 2;
            index = color.b;
        }
        else
        {
            layer = -1; //Invalid annotation ID
            index = 0;
        }
    }
}
