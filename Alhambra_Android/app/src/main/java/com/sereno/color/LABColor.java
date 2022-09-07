package com.sereno.color;

public class LABColor
{
    public float l; /**!< The L component*/
    public float a; /**!< The A component*/
    public float b; /**!< The B component*/
    public float transparency; /**!< The alpha component [0, 1]*/

    /**
     * \brief  Constructor
     * @param _l L component
     * @param _a A component
     * @param _b B component
     * @param _transparency Transparency component
     */
    public LABColor(float _l, float _a, float _b, float _transparency)
    {
        l = _l;
        a = _a;
        b = _b;
        transparency = _transparency;
    }

    /**
     * \brief  Convert a XYZ color to LABColor
     * @param color the color to convert
     */
    public LABColor(XYZColor color)
    {
        setFromXYZ(color);
    }

    /**
     * \brief  Convert a RGB color to LABColor
     * @param color the color to convert
     */
    public LABColor(Color color)
    {
        setFromRGB(color);
    }

    @Override
    public Object clone()
    {
        return new LABColor(l, a, b, transparency);
    }

    /**
     * \brief  Convert a RGB Color and store the result in this object
     * @param color the RGB Color object
     */
    public void setFromRGB(Color color)
    {
        setFromXYZ(new XYZColor(color));
    }

    /**
     * \brief  Convert a XYZ Color and store the result in this object
     * @param xyz the XYZ Color object
     */
    public void setFromXYZ(XYZColor xyz)
    {
        float fX = f(xyz.x/XYZColor.REFERENCE.x);
        float fY = f(xyz.y/XYZColor.REFERENCE.y);
        float fZ = f(xyz.z/XYZColor.REFERENCE.z);

        l = 116*fY - 16.0f;
        a = 500*(fX - fY);
        b = 200*(fY - fZ);

        transparency = xyz.a;
    }

    /**
     * \brief  Convert this object to an XYZ Color object
     * \return   The XYZ Color object
     */
    public XYZColor toXYZ()
    {
        return new XYZColor(XYZColor.REFERENCE.x * invF((float)((l+16.0)/116.0 + a/500.0)),
                            XYZColor.REFERENCE.y * invF((float)((l+16.0)/116.0)),
                            XYZColor.REFERENCE.z * invF((float)((l+16.0)/116.0 - b/200.0)),
                            transparency);
    }

    /**
     * \brief  Convert this object to an RGB Color object
     * \return   The RGB Color object
     */
    public Color toRGB()
    {
        return toXYZ().toRGB();
    }

    /** \brief Multiply this color by a factor t components per components
     * @param t the factor (betwene 0 and 1)
     * \return the color once multiplied
     */
    public LABColor multiplyBy(float t)
    {
        return new LABColor(l*t, a*t, b*t, transparency*t);
    }

    /**\brief Add components per components this color by another
     * @param c the Color to add
     * \return the addition components per components*/
    public LABColor addBy(LABColor c)
    {
        return new LABColor(l+c.l, a+c.a, b+c.b, transparency+c.transparency);
    }

    /**\brief Linear interpolation
     * @param c1 left color (t=0)
     * @param c2 right color (t=1)
     * @param t the advancement (t between 0 and 1)
     * \return new LABColor
     */
    public static LABColor lerp(LABColor c1, LABColor c2, float t)
    {
        return c1.multiplyBy(1.0f-t).addBy(c2.multiplyBy(t));
    }

    /**
     * \brief  a private function which helps determining the three component value. 7.787*v+16.0/116.0 otherwise. theta = 6.0/29.0 -> theta^3 = 0.008856
     * @param v the value to convert
     * \return  v^(1.0/3.0) if v > 0.008856
     */
    private float f(float v) {return (float)(v > 0.008856 ? Math.pow(v, 1.0/3.0) : 7.787*v + 16.0f/116.0f);}

    /** \brief the inverse function which helps determining the three component value. 0.128418*(v-4.0/29.0) otherwise, thata = 6.0/29.0 -> 3*theta^2 = 0.128418
     * @param v the value to determine
     * \return v^/3.0 if v > 6.0/29.0*/
    private float invF(float v)  {return (float)(v > 6.0/29.0 ? v*v*v : 0.128418*(v - 4.0/29.0));}
}