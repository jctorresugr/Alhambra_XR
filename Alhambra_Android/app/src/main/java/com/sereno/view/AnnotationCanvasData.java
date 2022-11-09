package com.sereno.view;

import android.graphics.Bitmap;
import android.graphics.Color;

import java.util.ArrayList;
import java.util.List;

public class AnnotationCanvasData
{
    /** Listener of the class AnnotationData*/
    public interface IAnnotationDataListener
    {
        /** Method called when the color to apply to newly added geometries has been set
         * @param data the AnnotationData firing this call
         * @param color the ARGB8888 32-bits color to apply*/
        void onSetCurrentColor(AnnotationCanvasData data, int color);

        /** Method called when a new stroke has been added
         * @param data the AnnotationData firing this call
         * @param stroke  the stroke added*/
        void onAddStroke(AnnotationCanvasData data, AnnotationStroke stroke);

        /** Method called when a new polygon has been added
         * @param data the AnnotationData firing this call
         * @param polygon  the polygon added*/
        void onAddPolygon(AnnotationCanvasData data, AnnotationPolygon polygon);

        /** Method called when all the geometries are erased
         * @param data the AnnotationData firing this call
         * @param geometries the old geometries getting cleared. You may want to remove some registered listeners*/
        void onClearGeometries(AnnotationCanvasData data, List<AnnotationGeometry> geometries);

        /** Method called when a new background is set
         * @param data the AnnotationData firing this call
         * @param background the new background of the canvas*/
        void onSetBackground(AnnotationCanvasData data, Bitmap background);

        /** Method called when the drawing method is set
         * @param data the AnnotationData firing this call
         * @param drawingMethod the new drawing method to apply*/
        void onSetDrawingMethod(AnnotationCanvasData data, int drawingMethod);
    }

    public static final int DRAWING_METHOD_STROKES  = 0;
    public static final int DRAWING_METHOD_POLYGONS = 1;

    /** the texture width where this annotation belongs to*/
    private int    m_width;

    /** the texture height where this annotation belongs to*/
    private int    m_height;

    /** The current color to use for newly created geometries*/
    private int    m_curColor = Color.BLACK;

    /** The background Bitmap associated with the annotation canvas view */
    private Bitmap m_background = null;

    /** the current drawing method. Useful to know what type of data to push (e.g., strokes or polygons?) */
    private int m_drawingMethod = DRAWING_METHOD_STROKES;

    /** List of m_geometries*/
    private ArrayList<AnnotationGeometry> m_geometries = new ArrayList<>();

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

    /** Get the current ARGB8888 color to use for newly added geometries
     * @return the ARGB8888 32-bits color to apply*/
    public int getCurrentColor() {return m_curColor;}

    /** Set the current ARGB8888 color to use for newly added geometries
     * @param color the ARGB8888 32-bits color to apply*/
    public void setCurrentColor(int color)
    {
        if(m_curColor != color)
        {
            m_curColor = color;
            for(int i = 0; i < m_listeners.size(); i++)
                m_listeners.get(i).onSetCurrentColor(this, color);
        }
    }

    /** Add a new stroke to the annotation stroke list
     * @param s the new stroke to add*/
    public void addStroke(AnnotationStroke s)
    {
        m_geometries.add(s);
        s.setColor(m_curColor);
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onAddStroke(this, s);
    }

    /** Add a new polygon to the annotation polygon list
     * @param p the new polygon to add*/
    public void addPolygon(AnnotationPolygon p)
    {
        m_geometries.add(p);
        p.setColor(m_curColor);
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onAddPolygon(this, p);
    }

    /** Get the list of geometries. Do not modify the list!
     * @return the list of geometries. Do not modify the list! (but each AnnotationGeometry can be)*/
    public ArrayList<AnnotationGeometry> getGeometries()
    {
        return m_geometries;
    }

    /** Clear all the registered geometries*/
    public void clearGeometries()
    {
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onClearGeometries(this, m_geometries);
        m_geometries.clear();
    }

    /** Get the current background of the canvas
     * @return the current image background, or null if no background*/
    public Bitmap getBackground() {return m_background;}

    /** Set the background of the canvas
     * @param background the new background to use, or null if no background should be used*/
    public void setBackground(Bitmap background)
    {
        m_background = background;
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onSetBackground(this, background);
    }

    /** Get the current drawing method. Useful to know what type of data to push (e.g., strokes or polygons?)
     * @return the current drawing method. See static field "DRAWING_METHOD*" */
    public int getDrawingMethod() {return m_drawingMethod;}

    /** Set the current drawing method. Useful to know what type of data to push (e.g., strokes or polygons?)
     * @param drawingMethod the new current drawing method to apply. See static field "DRAWING_METHOD*" */
    public void setDrawingMethod(int drawingMethod)
    {
        if(m_drawingMethod == drawingMethod)
            return;
        m_drawingMethod = drawingMethod;
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onSetDrawingMethod(this, drawingMethod);
    }

}
