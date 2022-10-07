using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

[Serializable]
public class Stroke
{
    public float[] points;
}

[Serializable]
public class FinishAnnotationMessage
{
    public float[]  cameraPos;
    public float[]  cameraRot;
    public bool     confirm;
    public Stroke[] strokes;
}
