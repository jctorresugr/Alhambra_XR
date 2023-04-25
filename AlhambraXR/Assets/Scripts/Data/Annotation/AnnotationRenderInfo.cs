using System;
using UnityEngine;
/// <summary>
/// Class used to register annotations
/// </summary>
[Serializable]
public class AnnotationRenderInfo
{
    /// <summary>
    /// The color of the annotation
    /// </summary>
    [SerializeField]
    public Color32 Color { get; set; }


    [SerializeField]
    public Bounds Bounds;
    /*
    public Bounds Bounds
    {
        get => m_bounds;
        set => m_bounds = Bounds;
    }*/

    /// <summary>
    /// The bounding box (min XYZ position) in the local space of the 3D model where this annotation belongs to.
    /// </summary>
    public Vector3 BoundingMin
    {
        get => Bounds.min;
        set => Bounds.min = value;
    }

    /// <summary>
    /// The bounding box (max XYZ position) in the local space of the 3D model where this annotation belongs to.
    /// </summary>
    public Vector3 BoundingMax
    {
        get => Bounds.max;
        set => Bounds.max = value;
    }

    [SerializeField]
    public Vector3 Normal { get; set; }

    /// <summary>
    /// The central position of this annotation in the local space of the 3D model.
    /// </summary>
    public Vector3 Center
    {
        get => new Vector3(0.5f * (BoundingMax[0] - BoundingMin[0]) + BoundingMin[0],
                           0.5f * (BoundingMax[1] - BoundingMin[1]) + BoundingMin[1],
                           0.5f * (BoundingMax[2] - BoundingMin[2]) + BoundingMin[2]);
    }

    /// <summary>
    /// Constructor. Initialize everything with default values.
    /// </summary>
    public AnnotationRenderInfo()
    {
       //Bounds.SetMinMax(new Vector3(float.MaxValue, float.MaxValue, float.MaxValue),
         //   new Vector3(float.MinValue, float.MinValue, float.MinValue));
        //Color = new Color32(0, 0, 0, 0);
    }
}