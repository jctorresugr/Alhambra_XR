package com.alhambra.dataset.data;

import android.graphics.Bitmap;
import android.graphics.drawable.Drawable;

/** This class describes the chunk of data as saved in the database
 * Each chunk of data is readonly once it is created.*/
public class AnnotationInfo
{
    /** The index of this chunk of data*/
    int    m_index = 0;

    private AnnotationID m_annotationID = null;

    /** The color representing this chunk of data*/
    private int    m_color = 0;

    /** The text associated with this chunk of data*/
    private String m_text = "";

    /** The image associated with this chunk of data*/
    private Drawable m_drawable = null;

    private Bitmap m_bitmap =null;

    /** Constructor
     * @param index the Index of this chunk of data in Android fragment list
     * @param id  The ID of this chunk of data inside its layer ( part of Unity Shader's ID )
     * @param layer The layer ID to which this chunk of data belongs to ( part of Unity Shader's ID )
     * @param color  The color representing this chunk of data (the whole Unity Shader's ID)
     * @param text The text associated with this chunk of data
     * @param img The image drawable describing this data chunk*/
    public AnnotationInfo(int index, int layer, int id, int color, String text, Drawable img)
    {
        m_index    = index;
        m_annotationID = new AnnotationID(layer,id);
        m_color    = color;
        m_text     = text;
        m_drawable = img;
    }

    public AnnotationInfo(int index, int layer, int id, int color, String text, Drawable img, Bitmap bitmap)
    {
        m_index    = index;
        m_annotationID = new AnnotationID(layer,id);
        m_color    = color;
        m_text     = text;
        m_drawable = img;
        m_bitmap=bitmap;
    }

    /** Get the index of this chunk of data*/
    public int getIndex() {return m_index;}

    /** Get the ID of this chunk of data*/
    public int getID() {return m_annotationID.getId();}

    public AnnotationID getAnnotationID() {return m_annotationID;}

    /** Get the layer ID to which this chunk of data belongs to*/
    public int getLayer() {return m_annotationID.getLayer();}

    /** Get the color representing this chunk of data*/
    public int getColor() {return m_color;}

    /** Get the text associated with this chunk of data*/
    public String getText() {return m_text;}

    /** Get the image drawable describing this data chunk*/
    public Drawable getImage() {return m_drawable;}

    public Bitmap getBitmap() {
        return m_bitmap;
    }
}