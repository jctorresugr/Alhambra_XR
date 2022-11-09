package com.sereno.view;

import java.util.ArrayList;

/** Class containing an annotation stroke information*/
public class AnnotationStroke extends AnnotationGeometry
{
    /** Listener containing methods to call when the internal state of the AnnotationStroke object changes*/
    public interface IAnnotationStrokeListener
    {
        /** Method called when the stroke width changes
         * @param stroke the annotation stroke changing width
         * @param w the new stroke width*/
        void onSetWidth(AnnotationStroke stroke, float w);
    }

    /** Width of the stroke in pixels*/
    private float m_width = 10.0f;

    /** The listeners to call when the current state of the annotations changed*/
    private ArrayList<IAnnotationStrokeListener> m_listeners = new ArrayList<>();

    /** Constructor*/
    public AnnotationStroke()
    {}

    /** Add a new listener to call if not already registered
     * @param l the new listener to add*/
    public void addListener(IAnnotationStrokeListener l)
    {
        if(!m_listeners.contains(l))
            m_listeners.add(l);
    }

    /** remove an already registered listener
     * @param l the listener to remove*/
    public void removeListener(IAnnotationStrokeListener l)
    {
        m_listeners.remove(l);
    }

    /** Get the stroke width
     * @return the stroke width in pixels*/
    public float getWidth()
    {
        return m_width;
    }

    /** Set the stroke width
     * @param width the new stroke width in pixels to apply*/
    public void setWidth(int width)
    {
        m_width = width;
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onSetWidth(this, width);
    }
}