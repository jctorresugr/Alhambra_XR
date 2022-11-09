package com.sereno.view;

import android.graphics.Color;
import android.graphics.Point;

import java.util.ArrayList;

/** Class containing an annotation geometry information*/
public class AnnotationGeometry
{
    /** Listener containing methods to call when the internal state of the AnnotationGeometry object changes*/
    public interface IAnnotationGeometryListener
    {
        /** Method called when a new point has been added
         * @param geometry the annotation stroke adding the point
         * @param p the new point to add*/
        void onAddPoint(AnnotationGeometry geometry, Point p);

        /** Method called when the geometry color changes
         * @param geometry the annotation stroke adding the point
         * @param c the new geometry color*/
        void onSetColor(AnnotationGeometry geometry, int c);
    }

    /** List of points composing the geometry*/
    private ArrayList<Point> m_points = new ArrayList<>();

    /** Color of the geometry*/
    private int m_color = Color.BLACK;

    /** The listeners to call when the current state of the annotations changed*/
    private ArrayList<IAnnotationGeometryListener> m_listeners = new ArrayList<>();

    /** Constructor*/
    public AnnotationGeometry()
    {}

    /** Add a new listener to call if not already registered
     * @param l the new listener to add*/
    public void addListener(IAnnotationGeometryListener l)
    {
        if(!m_listeners.contains(l))
            m_listeners.add(l);
    }

    /** remove an already registered listener
     * @param l the listener to remove*/
    public void removeListener(IAnnotationGeometryListener l)
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

    /** Get the list of points describing the geometry
     * @return the list of points describing the geometry*/
    public ArrayList<Point> getPoints()
    {
        return m_points;
    }

    /** Get the geometry color
     * @return the geometry ARGB8888 color*/
    public int getColor()
    {
        return m_color;
    }

    /** Set the geometry color
     * @param color the new geometry ARGB8888 color to apply*/
    public void setColor(int color)
    {
        m_color = color;
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onSetColor(this, color);
    }

    /** Is the geometry correctly constructed (i.e., can it be drawn based on its stored data)?
     * @return true is yes, false otherwise*/
    public boolean isValid()
    {
        return true;
    }
}