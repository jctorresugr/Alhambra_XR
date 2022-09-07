package com.sereno.color;

public class Color
{
    public float r; /**!< Red component [0, 1]*/
    public float g; /**!< Green component [0, 1]*/
    public float b; /**!< Blue component [0, 1]*/
    public float a; /**!< Alpha component [0, 1]*/

    /** \brief Create a color
     * red, green, blue and alpha must to be between 0.0f and 1.0f
     * @param _r red component
     * @param _g green component
     * @param _b blue component
     * @param _a alpha component*/
    public Color(float _r, float _g, float _b, float _a)
    {
        r = _r > 0.0f ? (_r < 1.0f ? _r : 1.0f) : 0.0f;
        g = _g > 0.0f ? (_g < 1.0f ? _g : 1.0f) : 0.0f;
        b = _b > 0.0f ? (_b < 1.0f ? _b : 1.0f) : 0.0f;
        a = _a > 0.0f ? (_a < 1.0f ? _a : 1.0f) : 0.0f;
    }

    public static Color fromARGB8888(int argb)
    {
        return new Color(((argb >> 16)&0xff)/255.0f,
                         ((argb >> 8 )&0xff)/255.0f,
                         ((argb      )&0xff)/255.0f,
                         ((argb >> 24)&0xff)/255.0f);
    }

    /** Get a Int32 ARGB 8888 color
     * @return the color into a int format*/
    public int toARGB8888()
    {
        return ((int)(255*a) << 24) +
               ((int)(255*r) << 16) +
               ((int)(255*g) << 8)  +
               ((int)(255*b));
    }

    @Override
    public Object clone()
    {
        return new Color(r, g, b, a);
    }

    @Override
    public boolean equals(Object o)
    {
        if(o == this)
            return true;
        else if(o instanceof Color)
        {
            Color cmp = (Color)o;
            return (cmp.a == a && cmp.r == r && cmp.g == g && cmp.b ==b);
        }
        return false;

    }

    static final Color WHITE   = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    static final Color BLACK   = new Color(0.0f, 0.0f, 0.0f, 0.0f);
    static final Color RED     = new Color(1.0f, 0.0f, 0.0f, 1.0f);
    static final Color GREEN   = new Color(0.0f, 1.0f, 0.0f, 1.0f);
    static final Color BLUE    = new Color(0.0f, 0.0f, 1.0f, 1.0f);
    static final Color YELLOW  = new Color(1.0f, 1.0f, 0.0f, 1.0f);
    static final Color CYAN    = new Color(0.0f, 1.0f, 1.0f, 1.0f);
    static final Color MAGENTA = new Color(1.0f, 0.0f, 1.0f, 1.0f);

}
