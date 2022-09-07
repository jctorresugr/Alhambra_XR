package com.sereno.view;


import android.graphics.Color;
import android.graphics.Point;

import java.util.ArrayList;

/** Class containing an annotation stroke information*/
public class AnnotationStroke
{
    /** Listener containing methods to call when the internal state of the AnnotationStroke object changes*/
    public interface IAnnotationStrokeListener
    {
        /** Method called when a new point has been added
         * @param p the new point to add
         * @param stroke the annotation stroke adding the point*/
        void onAddPoint(AnnotationStroke stroke, Point p);

        /** Method called when the stroke color changes
         * @param stroke the annotation stroke changing color
         * @param c the new stroke color*/
        void onSetColor(AnnotationStroke stroke, int c);

        /** Method called when the stroke width changes
         * @param stroke the annotation stroke changing width
         * @param c the new stroke width*/
        void onSetWidth(AnnotationStroke stroke, float w);
    }

    /** List of points composing the stroke*/
    private ArrayList<Point> m_points = new ArrayList<>();

    /** Color of the stroke*/
    private int m_color = Color.BLACK;

    /** Width of the stroke*/
    private float m_width = 5.0f;

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

    /** Add a new point on the points list
     * @param p the new point to add*/
    public void addPoint(Point p)
    {
        m_points.add(p);
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onAddPoint(this, p);
    }

    /** Get the list of points describing the stroke
     * @return the list of points describing the stroke*/
    public ArrayList<Point> getPoints()
    {
        return m_points;
    }

    /** Get the stroke color
     * @return the stroke color*/
    public int getColor()
    {
        return m_color;
    }

    /** Set the stroke color
     * @param color the new stroke color to apply*/
    public void setColor(int color)
    {
        m_color = color;
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onSetColor(this, color);
    }

    /** Get the stroke width
     * @return the stroke width*/
    public float getWidth()
    {
        return m_width;
    }
}