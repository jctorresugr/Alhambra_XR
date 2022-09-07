package com.sereno.view;

import com.sereno.color.HSVColor;

import java.util.ArrayList;

public class ColorPickerData
{
    public interface IColorPickerDataListener
    {
        /** Method called when the color picker data color changes
         * @param data the ColorPickerData calling this method
         * @param color the new color applied*/
        void onSetColor(ColorPickerData data, int color);
    }

    /** The ARGB 8888 color*/
    private HSVColor m_color = new HSVColor(0.0f, 0.0f, 0.0f, 1.0f);

    /** The listeners to call when the current state of the annotations changed*/
    private ArrayList<IColorPickerDataListener> m_listeners = new ArrayList<>();

    /** Add a new listener to call if not already registered
     * @param l the new listener to add*/
    public void addListener(IColorPickerDataListener l)
    {
        if(!m_listeners.contains(l))
            m_listeners.add(l);
    }

    /** remove an already registered listener
     * @param l the listener to remove*/
    public void removeListener(IColorPickerDataListener l)
    {
        m_listeners.remove(l);
    }

    /** Set the color
     * @param color the new color to apply*/
    public void setColor(HSVColor color)
    {
        if(m_color.equals(color))
            return;
        m_color = color;
        for(int i = 0; i < m_listeners.size(); i++)
            m_listeners.get(i).onSetColor(this, color.toRGB().toARGB8888());
    }

    /** Get the current color
     * @return the current color*/
    public HSVColor getColor()
    {
        return m_color;
    }
}
