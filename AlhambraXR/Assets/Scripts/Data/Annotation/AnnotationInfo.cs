using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Immutable class describing registered annotations
/// </summary>
public class AnnotationInfo
{
    /// <summary>
    /// The RGBA color representing this annotation in the target index texture
    /// </summary>
    private Color32 m_color;

    private AnnotationID m_annotationID;

    /// <summary>
    /// The snapshot RGBA32 pixel image
    /// </summary>
    private byte[]  m_snapshotRGBA;

    /// <summary>
    /// The snapshot image width
    /// </summary>
    private int     m_snapshotWidth;

    /// <summary>
    /// The snapshot image height
    /// </summary>
    private int     m_snapshotHeight;

    /// <summary>
    /// The textual description of the annotation
    /// </summary>
    private string m_description;

    private List<AnnotationJoint> joints;

    /// <summary>
    /// Constructor, initialize a read-only annotation
    /// </summary>
    /// <param name="color">The RGBA color representing this annotation in the target index texture</param>
    /// <param name="snapshotRGBA">The snapshot RGBA32 pixel image</param>
    /// <param name="snapshotWidth">The snapshot image width</param>
    /// <param name="snapshotHeight">The snapshot image height</param>
    /// <param name="description">The textual description of the annotation</param>
    public AnnotationInfo(Color32 color, byte[] snapshotRGBA, int snapshotWidth, int snapshotHeight, string description)
    {
        if (snapshotRGBA.Length < 4 * snapshotHeight * snapshotWidth)
            throw new InvalidOperationException($"The snapshot RGBA byte array contains {snapshotRGBA.Length} byte instead of {4*snapshotHeight*snapshotWidth}, according to the snapshot width and height");
        m_color          = color;
        m_annotationID = new AnnotationID(color);
        m_snapshotRGBA   = snapshotRGBA;
        m_snapshotWidth  = snapshotWidth;
        m_snapshotHeight = snapshotHeight;
        m_description    = description;
    }

    /// <summary>
    /// The RGBA color representing this annotation in the target index texture
    /// </summary>
    public Color32 Color
    {
        get => m_color;
    }

    public AnnotationID ID
    {
        get => m_annotationID;
    }

    /// <summary>
    /// The snapshot RGBA32 pixel image
    /// </summary>
    public byte[] SnapshotRGBA
    {
        get => m_snapshotRGBA;
    }

    /// <summary>
    /// The snapshot image width
    /// </summary>
    public int SnapshotWidth
    {
        get => m_snapshotWidth;
    }

    /// <summary>
    /// The snapshot image height
    /// </summary>
    public int SnapshotHeight
    {
        get => m_snapshotHeight;
    }

    /// <summary>
    /// The textual description of the annotation
    /// </summary>
    public string Description
    {
        get => m_description;
    }
}