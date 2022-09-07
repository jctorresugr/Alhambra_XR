package com.alhambra.network;

import android.util.Base64;

import org.json.JSONException;
import org.json.JSONObject;

import androidx.annotation.NonNull;

public class AnnotationMessage
{
    private byte[] m_image;
    private int    m_width;
    private int    m_height;

    /** Constructor
     * @param data the JSONObject representing the "data" entry of the received JSON object from the network*/
    public AnnotationMessage(@NonNull JSONObject data) throws JSONException
    {
        m_width  = data.getInt("width");
        m_height = data.getInt("height");
        m_image  = Base64.decode(data.getString("base64"), Base64.DEFAULT);
    }

    public int getWidth() {return m_width;}
    public int getHeight() {return m_height;}
    public byte[] getBitmap() {return m_image;}
}
