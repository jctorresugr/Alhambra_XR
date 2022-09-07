package com.sereno.color;

public class HSVColor
{
    public float h; /**!< The Hue between 0 and 360°*/
    public float s; /**!< The Saturation*/
    public float v; /**!< The value*/
    public float a; /**!< The alpha*/

    /** \brief Constructor
     * @param _h the hue
     * @param _s the saturation
     * @param _v the value
     * @param _a the alpha */
    public HSVColor(float _h, float _s, float _v, float _a)
    {
        h = _h;
        s = _s;
        v = _v;
        a = _a;
    }

    /** \brief Constructor
     * @param c the color to convert */
    public HSVColor(Color c)
    {
        setFromRGB(c);
    }

    /** \brief Set the HSV colorspace value from RGB colorspace value
     * @param color the color to convert*/
    public void setFromRGB(Color color)
    {
        float max = (float)Math.max(Math.max(color.r, color.g), color.b);
        float min = (float)Math.min(Math.min(color.r, color.g), color.b);
        float c   = max-min;

        //Compute the Hue
        if(c == 0)
            h = 0;
        else if(max == color.r)
            h = (color.g - color.b)/c + 6;
        else if(max == color.g)
            h = (color.b - color.r)/c + 2;
        else if(max == color.b)
            h = (color.r - color.g)/c + 4;
        h *= 60.0f;
        while(h >= 360.0f)
            h -= 360.0f;

        //Compute the Saturation
        if(max == 0)
            s = 0;
        else
            s = c/max;

        //Compute the Value
        v = max;

        a = color.a;
    }

    /** Is the hue defined?
     * @return the hue if defined*/
    public boolean isHueDefined()
    {
        return s == 0.0f || v == 0.0f;
    }

    /** \brief Convert from the HSV colorspace to the RGB colorspace
     * \return the color in RGB space */
    public Color toRGB()
    {
        float c  = v*s;
        float h2 = (h/60.0f);
        float x  = c*(1.0f-Math.abs(h2%2.0f - 1.0f));
        float m  = v - c;
        switch((int)h2)
        {
            case 0:
                return new Color(c+m, x+m, m, a);
            case 1:
                return new Color(x+m, c+m, m, a);
            case 2:
                return new Color(m, c+m, x+m, a);
            case 3:
                return new Color(m, x+m, c+m, a);
            case 4:
                return new Color(x+m, m, c+m, a);
            default:
                return new Color(c+m, m, x+m, a);
        }
    }

    @Override
    public boolean equals(Object o)
    {
        if(o == this)
            return true;

        else if(o instanceof HSVColor)
        {
            HSVColor col = (HSVColor)o;
            return col.h == h && col.s == s && col.v == v && col.a == a;
        }
        return false;
    }

    public Object clone()
    {
        return new HSVColor(h, s, v, a);
    }
}