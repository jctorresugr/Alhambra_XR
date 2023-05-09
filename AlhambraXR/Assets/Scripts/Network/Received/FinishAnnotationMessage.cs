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
    /// Points should be between (0, 0) and (annotationWidth, annotationHeight)
    /// </summary>
    public float[] points;

    /// <summary>
    /// The width of the stroke in pixel
    /// </summary>
    public float width;
}

/// <summary>
/// The data about one polygon
/// </summary>
[Serializable]
public class Polygon
{
    /// <summary>
    /// The points of the polygon
    /// Number of points: points.Length/2
    /// Position of point ID i: {x=points[2*i], y=points[2*i+1]}
    /// Points should be between (0, 0) and (annotationWidth, annotationHeight)
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

    /// <summary>
    /// The polygons to anchor back to the environment
    /// </summary>
    public Polygon[] polygons;

    /// <summary>
    /// The width of the image where the strokes were captured
    /// </summary>
    public int width;

    /// <summary>
    /// The height of the image where the strokes were captured
    /// </summary>
    public int height;

    /// <summary>
    /// The message associated with this annotation
    /// </summary>
    public String description;

    public int[] selectedJointID;
    public string[] createdJointName;
}