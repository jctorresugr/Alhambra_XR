package com.sereno.color;

public class LUVColor
{
    public float l; /**!< The L component*/
    public float u; /**!< The U component*/
    public float v; /**!< The V component*/
    public float a; /**!< The alpha component*/

    /**
     * \brief  Constructor
     * @param _l L component
     * @param _u U component
     * @param _v V component
     */
    public LUVColor(float _l, float _u, float _v, float _a)
    {
        l = _l;
        u = _u;
        v = _v;
        a = _a;
    }

    /**
     * \brief  Constructor. Convert a RGB Color to a LUV Color
     * @param color the RGB Color to convert
     */
    public LUVColor(Color color)
    {
        setFromRGB(color);
    }

    /**
     * \brief  Constructor. Convert a XYZ Color to a XYZ Color
     * @param color the XYZ Color to convert
     */
    public LUVColor(XYZColor color)
    {
        setFromXYZ(color);
    }

    @Override
    public Object clone()
    {
        return new LUVColor(l, u, v, a);
    }

    /**
     * \brief  Convert a RGB Value
     * @param color the value to convert
     */
    public void setFromRGB(Color color)
    {
        setFromXYZ(new XYZColor(color));
    }

    /**
     * \brief  Convert a XYZ Value
     * @param xyz the xyz value to convert
     */
    public void setFromXYZ(XYZColor xyz)
    {
        float un = 4*XYZColor.REFERENCE.x/(XYZColor.REFERENCE.x+15*XYZColor.REFERENCE.y+3*XYZColor.REFERENCE.z);
        float vn = 9*XYZColor.REFERENCE.y/(XYZColor.REFERENCE.x+15*XYZColor.REFERENCE.y+3*XYZColor.REFERENCE.z);

        float y = xyz.y/XYZColor.REFERENCE.y;

        if(y < 0.008856f)      //(6/29)**3 =   0.008856
            l = 903.296296f*y; //(29/3)**3 = 903.296296
        else
            l = 116.0f*(float)(Math.pow(y, 1.0f/3.0f)) - 16.0f;

        u = 13.0f*l * (4.0f*xyz.x/(xyz.x + 15.0f*xyz.y + 3.0f*xyz.z) - un);
        v = 13.0f*l * (9.0f*xyz.y/(xyz.x + 15.0f*xyz.y + 3.0f*xyz.z) - vn);

        a = xyz.a;
    }

    /**
     * \brief  Convert this object to a XYZ colorspace
     * \return   the XYZ color value
     */
    public XYZColor toXYZ()
    {
        float un = 4*XYZColor.REFERENCE.x/(XYZColor.REFERENCE.x+15*XYZColor.REFERENCE.y+3*XYZColor.REFERENCE.z);
        float vn = 9*XYZColor.REFERENCE.y/(XYZColor.REFERENCE.x+15*XYZColor.REFERENCE.y+3*XYZColor.REFERENCE.z);

        float uprime = u/(13.0f*l) + un;
        float vprime = v/(13.0f*l) + vn;

        float z = 0.0f;
        float y = 0.0f;
        float x = 0.0f;

        if(l <= 8.0)
            y = XYZColor.REFERENCE.y*l*(0.001107056f); //0.001107056 = (3.0/29.0)**3
        else
        {
            float lprime = (l+16.0f)/116.0f;
            y = XYZColor.REFERENCE.y*lprime*lprime*lprime;
        }
        x = y*9*uprime/(4*vprime);
        z = y*(12 - 3*uprime - 20*vprime)/(4*vprime);

        return new XYZColor(x, y, z, a);
    }

    /**
     * \brief  Convert this object to a RGB colorspace
     * \return   the RGB color value
     */
    public Color toRGB()
    {
        return toXYZ().toRGB();
    }

    /** \brief Multiply this color by a factor t components per components
     * @param t the factor (between 0 and 1)
     * \return the color once multiplied
     */
    public LUVColor multiplyBy(float t)
    {
        return new LUVColor(l*t, u*t, v*t, a*t);
    }

    /**\brief Add components per components this color by another
     * @param c the Color to add
     * \return the addition components per components*/
    public LUVColor addBy(LUVColor c)
    {
        return new LUVColor(l+c.l, u+c.u, v+c.v, a+c.a);
    }

    /**\brief Linear interpolation
     * @param c1 left color (t=0)
     * @param c2 right color (t=1)
     * @param t the advancement (t between 0 and 1)
     * \return new LUVColor
     */
    public static LUVColor lerp(LUVColor c1, LUVColor c2, float t)
    {
        return c1.multiplyBy(1.0f-t).addBy(c2.multiplyBy(t));
    }
}
