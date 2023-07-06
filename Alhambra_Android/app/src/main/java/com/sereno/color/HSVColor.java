package com.sereno.color;

import com.sereno.math.MathUtils;

public class HSVColor
{
    /** The Hue between 0 and 360°*/
    public float h;
    /** The Saturation*/
    public float s;
    /** The value*/
    public float v;
    /** The alpha*/
    public float a;

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

    /**
     * \brief Set the HSV colorspace value from RGB colorspace value
     * @param r Color red
     * @param g Color green
     * @param b Color blue
     * @param a Color alpha
     */
    public void setFromRGB(float r,float g,float b,float a)
    {
        float max = Math.max(Math.max(r, g), b);
        float min = Math.min(Math.min(r, g), b);
        float c   = max-min;

        //Compute the Hue
        if(c == 0)
            h = 0;
        else if(max == r)
            h = (g - b)/c + 6;
        else if(max == g)
            h = (b - r)/c + 2;
        else if(max == b)
            h = (r - g)/c + 4;
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

        this.a=a;
    }
    
    public void setFromRGB(Color color){
        setFromRGB(color.r,color.g,color.b,color.a);
    }

    public static HSVColor createFromRGB(float r, float g,float b,float a){
        HSVColor color = new HSVColor(0,0,0,0);
        color.setFromRGB(r,g,b,a);
        return color;
    }

    public static HSVColor createFromRGB(int r, int g,int b,int a){
        HSVColor color = new HSVColor(0,0,0,0);
        float div = 1.0f/255.0f;
        color.setFromRGB(r*div,g*div,b*div,a*div);
        return color;
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

    public int toARGB(){
        float c  = v*s;
        float h2 = (h/60.0f);
        float x  = c*(1.0f-Math.abs(h2%2.0f - 1.0f));
        float m  = v - c;
        switch((int)h2)
        {
            case 0:
                return Color.toARGB8888(c+m, x+m, m, a);
            case 1:
                return Color.toARGB8888(x+m, c+m, m, a);
            case 2:
                return Color.toARGB8888(m, c+m, x+m, a);
            case 3:
                return Color.toARGB8888(m, x+m, c+m, a);
            case 4:
                return Color.toARGB8888(x+m, m, c+m, a);
            default:
                return Color.toARGB8888(c+m, m, x+m, a);
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

    public void interpolateTo(HSVColor color2,float factor){
        h = MathUtils.interpolate(h,color2.h,factor);
        s = MathUtils.interpolate(s,color2.s,factor);
        v = MathUtils.interpolate(v,color2.v,factor);
        a = MathUtils.interpolate(a,color2.a,factor);
    }

    public Object clone()
    {
        return new HSVColor(h, s, v, a);
    }
}