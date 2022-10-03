package com.alhambra.network.receivingmsg;

import android.util.Base64;

import org.json.JSONException;
import org.json.JSONObject;

import androidx.annotation.NonNull;

public class AnnotationMessage
{
    /** The base64 image data*/
    private final byte[] m_image;

    /** The width of the image*/
    private final int    m_width;

    /** The height of the image*/
    private final int    m_height;

    /** Constructor
     * @param data the JSONObject representing the "data" entry of the received JSON object from the network*/
    public AnnotationMessage(@NonNull JSONObject data) throws JSONException
    {
        m_width  = data.getInt("width");
        m_height = data.getInt("height");
        m_image  = Base64.decode(data.getString("base64"), Base64.DEFAULT);
    }

    /** Get the width of the received image
     * @return the width of the image*/
    public int getWidth() {return m_width;}

    /** Get the height of the received image
     * @return the height of the image*/
    public int getHeight() {return m_height;}

    /** Get the received bitmap image in a RGBA8888 of the received image in a column-major fashion.
     * The size of the array is not checked by this class. However, the expected size of the array is: 4*getWidth()*getHeight()
     * @return the RGBA8888 bitmap image*/
    public byte[] getBitmap() {return m_image;}
}
