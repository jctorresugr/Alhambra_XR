using System;

/// <summary>
/// Data part of the message to highlight a particular data chunk
/// </summary>
[Serializable]
public struct HighlightMessage
{
    /// <summary>
    /// The layer of the data chunk
    /// </summary>
    public int layer;

    /// <summary>
    /// The ID  of the data chunk inside the specified layer
    /// </summary>
    public int id;
}