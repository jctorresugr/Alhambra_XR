package com.sereno.color;

public class MSHColor
{
    public float m; /**!< m component*/
    public float s; /**!< s component*/
    public float h; /**!< h component*/
    public float a; /**!< alpha component*/

    /**
     * \brief  Constructor
     * @param _m The M component
     * @param _s The S component
     * @param _h The H component
     * @param _a The alpha component
     */
    public MSHColor(float _m, float _s, float _h, float _a)
    {
        m = _m;
        s = _s;
        h = _h;
        a = _a;
    }

    /**
     * \brief  Constructor. Convert a LAB Color value to MSH color value
     * @param color the value to convert
     */
    public MSHColor(LABColor color)
    {
        setFromLAB(color);
    }

    /**
     * \brief  Constructor. Convert a XYZ Color value to MSH color value
     * @param color the value to convert
     */
    public MSHColor(XYZColor color)
    {
        setFromXYZ(color);
    }

    /**
     * \brief  Constructor. Convert a RGB Color value to MSH color value
     * @param color the value to convert
     */
    public MSHColor(Color color)
    {
        setFromRGB(color);
    }

    public Object clone()
    {
        return new MSHColor(m, s, h, a);
    }

    /**
     * \brief  Convert a LAB color value to MSH color value
     * @param color the color to convert
     */
    public void setFromLAB(LABColor color)
    {
        m = (float)Math.sqrt(color.l*color.l + color.a*color.a + color.b*color.b);
        s = (float)Math.acos(color.l/m);
        h = (float)Math.atan2(color.b, color.a);
        a = color.transparency;
    }

    /**
     * \brief  Convert a RGB color value to MSH color value
     * @param color the color to convert
     */
    public void setFromRGB(Color color)
    {
        setFromLAB(new LABColor(color));
    }

    /**
     * \brief  Convert a XYZ color value to MSH color value
     * @param xyz the color to convert
     */
    public void setFromXYZ(XYZColor xyz)
    {
        setFromLAB(new LABColor(xyz));
    }

    /**
     * \brief  Convert this object to a LAB colorspace value
     * \return   the LABColor corresponding to this object
     */
    public LABColor toLAB()
    {
        float l  = (float)(m * Math.cos(s));
        float _a = (float)(m * Math.sin(s) * Math.cos(h));
        float b  = (float)(m * Math.sin(s) * Math.sin(h));
        return new LABColor(l, _a, b, a);
    }

    /**
     * \brief  Convert this object to a RGB colorspace value
     * \return   the RGBColor corresponding to this object
     */
    public Color toRGB()
    {
        return toLAB().toRGB();
    }

    /**
     * \brief  Convert this object to a XYZ colorspace value
     * \return   the XYZColor corresponding to this object
     */
    public XYZColor toXYZ()
    {
        return toLAB().toXYZ();
    }

    /** \brief Multiply this color by a factor t components per components
     * @param t the factor (betwene 0 and 1)
     * \return the color once multiplied
     */
    public MSHColor multiplyBy(float t)
    {
        return new MSHColor(m*t, s*t, h*t, a*t);
    }

    /**\brief Add components per components this color by another
     * @param c the Color to add
     * \return the addition components per components*/
    public MSHColor addBy(MSHColor c)
    {
        return new MSHColor(m+c.m, s+c.s, h+c.h, a+c.a);
    }

    /**
     * \brief  Interpolate from c1 to c2 at step interp (between 0 and 1)
     *
     * @param c1 the left color (interp = 0)
     * @param c2 the right color (interp = 1)
     * @param interp the step
     *
     * \return  the color once interpolated. The medium color is usually white
     */
    public static MSHColor fromColorInterpolation(Color c1, Color c2, float interp)
    {
        MSHColor m1 = new MSHColor(c1);
        MSHColor m2 = new MSHColor(c2);

        float radDiff = Math.abs(m1.h - m2.h);

        if(m1.s > 0.05 &&
           m2.s > 0.05 &&
           radDiff > Math.PI/3.0f)
        {
            float midM = (float)(Math.max(Math.max(m1.m, m2.m), 98.0f));
            if(interp < 0.5f)
            {
                m2.m = midM;
                m2.s = 0;
                m2.h = 0;
                interp *= 2.0f;
            }
            else
            {
                m1.m = midM;
                m1.s = 0;
                m1.h = 0;
                interp = 2.0f*interp - 1.0f;
            }
        }

        if(m1.s < 0.05 && m2.s > 0.05)
            m1.h = adjustHue(m2, m1.m);
        else if(m1.s > 0.05 && m2.s < 0.05)
            m2.h = adjustHue(m1, m2.m);

        return m1.multiplyBy(1.0f-interp).addBy(m2.multiplyBy(interp));
    }

    /**
     * \brief  Adjust the hue
     * @param sat The saturated color
     * @param m The unsaturated M component
     * \return  the hue adjusted
     */
    public static float adjustHue(MSHColor color, float m)
    {
        if(color.m >= m)
            return color.h;

        float hSpin = (float)(color.s * Math.sqrt(m*m - color.m*color.m) / (color.m*Math.sin(color.s)));
        if(hSpin > -Math.PI/3.0)
            return color.h + hSpin;
        return color.h - hSpin;
    }

}