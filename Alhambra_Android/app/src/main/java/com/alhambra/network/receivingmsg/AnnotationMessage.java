package com.alhambra.network.receivingmsg;

import android.util.Base64;

import org.json.JSONException;
import org.json.JSONObject;

import androidx.annotation.NonNull;

import com.sereno.math.Quaternion;
import com.sereno.math.Vector3;

public class AnnotationMessage
{
    /** The base64 image data*/
    private final byte[] m_image;

    /** The width of the image*/
    private final int    m_width;

    /** The height of the image*/
    private final int    m_height;

    /** The camera position of the HoloLens when the annotation screenshot was taken*/
    private final float[] m_cameraPos;

    /** The camera orientation of the HoloLens when the annotation screenshot was taken*/
    private final Quaternion m_cameraRot;

    /** Constructor
     * @param data the JSONObject representing the "data" entry of the received JSON object from the network*/
    public AnnotationMessage(@NonNull JSONObject data) throws JSONException
    {
        m_width  = data.getInt("width");
        m_height = data.getInt("height");
        m_image  = Base64.decode(data.getString("base64"), Base64.DEFAULT);
        m_cameraPos = JSONUtils.jsonArrayToFloatArray(data.getJSONArray("cameraPos"));
        if(m_cameraPos.length != 3)
            throw new JSONException("The length of the JSON array corresponding to a Vector3 is " + m_cameraPos.length);
        m_cameraRot = new Quaternion(JSONUtils.jsonArrayToFloatArray(data.getJSONArray("cameraRot")));
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

    /** Get the camera position of the HoloLens when the annotation screenshot was taken.
     * @return the [x, y, z] camera position of the HoloLens*/
    public float[] getCameraPos() {return m_cameraPos;}

    /** Get the camera orientation of the HoloLens when the annotation screenshot was taken.
     * @return the quaternion representing the camera orientation of the HoloLens*/
    public Quaternion getCameraRot() {return m_cameraRot;}
}
