package com.sereno.view;

import android.graphics.Bitmap;

import java.util.ArrayList;

public class AnnotationCanvasData
{
    /** Listener of the class AnnotationData*/
    public interface IAnnotationDataListener
    {
        /** Method called when a new stroke has been added
         * @param data the AnnotationData firing this call
         * @param stroke  the stroke added*/
        void onAddStroke(AnnotationCanvasData data, AnnotationStroke stroke);

        /** Method called when a new background is set
         * @param data the AnnotationData firing this call
         * @param background the new background of the canvas*/
        void onSetBackground(AnnotationCanvasData data, Bitmap background);
    }

    private int    m_width;
    private int    m_height;
    private Bitmap m_background = null;

    /** List of strokes*/
    private ArrayList<AnnotationStroke> m_strokes = new ArrayList<>();

    /** The listeners to call when the current state of the annotations changed*/
    private ArrayList<IAnnotationDataListener> m_listeners = new ArrayList<>();

    /** Constructor
     * @param width  the texture width where this annotation belongs to
     * @param height the texture height where this annotation belongs to*/
    public AnnotationCanvasData(int width, int height)
    {
        m_width  = width;
        m_height = height;
    }

    /** Add a new listener to call if not already registered
     * @param l the new listener to add*/
    public void addListener(IAnnotationDataListener l)
    {
        if(!m_listeners.contains(l))
            m_listeners.add(l);
    }

    /** remove an already registered listener
     * @param l the listener to remove*/
    public void removeListener(IAnnotationDataListener l)
    {
        m_listeners.remove(l);
    }

    /** Get the texture width where this annotation belongs to
     * @return the texture width*/
    public int getWidth()
    {
        return m_width;
    }

    /** Get the texture height where this annotation belongs to
     * @return the texture height*/
    public int getHeight()
    {
        return m_height;
    }

    /** Add a new stroke to the annotation stroke list
     * @param s the new stroke to add*/
    public void addStroke(AnnotationStroke s)
    {
        m_strokes.add(s);
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onAddStroke(this, s);
    }

    /** Get the list of strokes. Do not modify the list!
     * @return the list of strokes. Do not modify the list! (but each AnnotationStroke can be)*/
    public ArrayList<AnnotationStroke> getStrokes()
    {
        return m_strokes;
    }



    /** Get the current background of the canvas
     * @return the current image background*/
    public Bitmap getBackground() {return m_background;}

    /** Set the background of the canvas
     * @param background the new background to use*/
    public void setBackground(Bitmap background)
    {
        m_background = background;
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onSetBackground(this, background);
    }
}
