using System;
using UnityEngine;

/// <summary>
/// Class to handle received JSON message from the bound tablet
/// </summary>
/// <typeparam name="T">An object representing the "data" part of the JSON message</typeparam>
public class ReceivedMessage<T> : CommonMessage
{
    /// <summary>
    /// The data of the message
    /// </summary>
    public T data;

    /// <summary>
    /// Deserialize a JSON string to a ReceivedMessage<T> object
    /// </summary>
    /// <param name="json">The string to deserialize</param>
    /// <returns>The object deserialize on success</returns>
    public static new ReceivedMessage<T> FromJSON(String json)
    {
        return JsonUtility.FromJson<ReceivedMessage<T>>(json);
    }
}

public class CommonMessage
{
    /// <summary>
    /// The type of the message
    /// </summary>
    public String action;

    public static CommonMessage FromJSON(String json)
    {
        return JsonUtility.FromJson<CommonMessage>(json);
    }
}