package com.alhambra.network.receivingmsg;

import android.util.Base64;

import androidx.annotation.NonNull;

import com.alhambra.dataset.data.AnnotationRenderInfo;
import com.alhambra.network.JSONUtils;

import org.json.JSONException;
import org.json.JSONObject;

/** Class to handle the "addAnnotation" message as sent by the server*/
public class AddAnnotationMessage
{
    /** The base64 snapshot image data*/
    private final byte[] m_image;

    /** The width of the snapshot image*/
    private final int    m_width;

    /** The height of the snapshot image*/
    private final int    m_height;

    /** The color of the annotation (which encodes the layer + ID of the data chunk)*/
    private final byte[] m_color;

    /** The textual description of the annotation*/
    private final String m_desc;

    private AnnotationRenderInfo renderInfo;

    /** Constructor
     * @param data the JSONObject representing the "data" entry of the received JSON object from the network*/
    public AddAnnotationMessage(@NonNull JSONObject data) throws JSONException
    {
        m_width  = data.getInt("snapshotWidth");
        m_height = data.getInt("snapshotHeight");
        m_image  = Base64.decode(data.getString("snapshotBase64"), Base64.DEFAULT);
        m_color  = JSONUtils.jsonArrayToByteArray(data.getJSONArray("annotationColor"));
        m_desc   = data.getString("description");
        renderInfo = JSONUtils.gson.fromJson(data.getString("renderInfo"),AnnotationRenderInfo.class);

        if(m_color.length != 4)
            throw new JSONException("The length of the JSON array corresponding to a Color (length 4) is: " + m_color.length);
    }

    public AnnotationRenderInfo getRenderInfo(){
        return renderInfo;
    }

    /** Get the width of the received snapshot image
     * @return the width of the snapshot image*/
    public int getSnapshotWidth() {return m_width;}

    /** Get the height of the received snapshot image
     * @return the height of the snapshot image*/
    public int getSnapshotHeight() {return m_height;}

    /** Get the received bitmap image in a RGBA8888 of the received snapshot image in a column-major fashion.
     * The size of the array is not checked by this class. However, the expected size of the array is: 4*getWidth()*getHeight()
     * @return the RGBA8888 bitmap image*/
    public byte[] getSnapshotBitmap() {return m_image;}

    /** Get the RGBA color describing the annotation
     * @return the array containing the color*/
    public byte[] getColor() {return m_color;}

    /** Get the ARGB8888 packed in an Integer describing the annotation
     * @return the ARGB8888 android format for colors */
    public int getARGB8888Color()
    {
        return (m_color[3] << 24) +
               (m_color[0] << 16) +
               (m_color[1] << 8)  +
               (m_color[2]);
    }

    /** Get the textual description of the annotation
     * @return the textual description of the annotation*/
    public String getDescription()
    {
        return m_desc;
    }
}
