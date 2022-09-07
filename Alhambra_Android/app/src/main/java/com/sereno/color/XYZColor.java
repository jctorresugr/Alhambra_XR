package com.sereno.color;

public class XYZColor
{
    float x; /**!< The X component*/
    float y; /**!< The Y component*/
    float z; /**!< The Z component*/
    float a; /**!< The A component*/

    /**
     * \brief  Constructor
     * @param _x x component
     * @param _y y component
     * @param _z z component
     * @param _a a component
     */
    public XYZColor(float _x, float _y, float _z, float _a)
    {
        x = _x;
        y = _y;
        z = _z;
        a = _a;
    }

    /**
     * \brief  Convert a RGB color to a XYZ color object
     * @param color the color to convert
     */
    public XYZColor(Color color)
    {
        setFromRGB(color);
    }

    @Override
    public Object clone()
    {
        return new XYZColor(x, y, z, a);
    }

    /**
     * \brief  Convert a RGB color
     * @param color the object to convert
     */
    public void setFromRGB(Color color)
    {
        x = (float)(color.r*0.4124 + color.g*0.3576 + color.b*0.1805);
        y = (float)(color.r*0.2126 + color.g*0.7152 + color.b*0.0722);
        z = (float)(color.r*0.0193 + color.g*0.1192 + color.b*0.9505);
        a = color.a;

    }

    /**
     * \brief  Convert this object in a RGB colorspace object
     * \return  An RGB color
     */
    public Color toRGB()
    {
        return new Color(Math.min(1.0f,  3.2405f*x - 1.5371f*y - 0.4985f*z),
                         Math.min(1.0f, -0.9692f*x + 1.8760f*y + 0.0415f*z),
                         Math.min(1.0f,  0.0556f*x - 0.2040f*y + 1.0572f*z),
                         a);

    }

    static final XYZColor REFERENCE = new XYZColor(0.9505f, 1.0f, 1.0890f, 1.0f);
}
