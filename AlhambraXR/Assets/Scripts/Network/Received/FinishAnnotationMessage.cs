using System;

/// <summary>
/// The data about one stroke
/// </summary>
[Serializable]
public class Stroke
{
    /// <summary>
    /// The points of the stroke
    /// Number of points: points.Length/2
    /// Position of point ID i: {x=points[2*i], y=points[2*i+1]}
    /// </summary>
    public float[] points;
}

/// <summary>
/// Data part of the message to finish the annotation process
/// </summary>
[Serializable]
public class FinishAnnotationMessage
{
    /// <summary>
    /// The camera position to anchor back the annotation
    /// </summary>
    public float[]  cameraPos;

    /// <summary>
    /// The camera orientation to anchor back the annotation
    /// </summary>
    public float[]  cameraRot;

    /// <summary>
    /// Is the annotation validated (true) or cancelled (false)?
    /// </summary>
    public bool     confirm;

    /// <summary>
    /// The strokes to anchor back to the environment
    /// </summary>
    public Stroke[] strokes;
}
