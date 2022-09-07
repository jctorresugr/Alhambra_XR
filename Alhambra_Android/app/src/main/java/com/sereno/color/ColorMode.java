package com.sereno.color;

/** \brief Defines ColorMode usable*/
public class ColorMode
{
    public static final int RAINBOW          = 0; /**!< Rainbow            colormode*/
    public static final int GRAYSCALE        = 1; /**!< Grayscale          colormode*/
    public static final int WARM_COLD_CIELAB = 2; /**!< CIELAB blue to red colormode*/
    public static final int WARM_COLD_CIELUV = 3; /**!< CIELUV blue to red colormode*/
    public static final int WARM_COLD_MSH    = 4; /**!< MSH blue to red    colormode*/

    public static final Color coldRGB = new Color(59.0f/255.0f, 76.0f/255.0f, 192.0f/255.0f, 1.0f);
    public static final Color warmRGB = new Color(180.0f/255.0f, 4.0f/255.0f, 38.0f/255.0f, 1.0f);

    public static final LABColor coldLAB  = new LABColor(coldRGB);
    public static final LABColor whiteLAB = new LABColor(Color.WHITE);
    public static final LABColor warmLAB  = new LABColor(warmRGB);

    public static final LUVColor coldLUV  = new LUVColor(coldRGB);
    public static final LUVColor whiteLUV = new LUVColor(Color.WHITE);
    public static final LUVColor warmLUV  = new LUVColor(warmRGB);

    /** Return the color corresponding to its normalized value "t" and its color mode
     * @param t the normalized value of the color (between 0.0 and 1.0)
     * @param mode the color mode to apply. See ColorMode static values for insights. If the value is unknown, the function returns a default value.
     * @return The RGB corresponding color. Default value: coldRGB*/
    public static Color computeRGBColor(float t, int mode)
    {
        Color c = coldRGB;

        switch(mode)
        {
            case ColorMode.RAINBOW:
            {
                c = new HSVColor(260.0f * t, 1.0f, 1.0f, 1.0f).toRGB();
                break;
            }
            case ColorMode.GRAYSCALE:
            {
                c = new Color(t, t, t, 1.0f);
                break;
            }
            case ColorMode.WARM_COLD_CIELAB:
            {
                if(t < 0.5)
                    c = LABColor.lerp(ColorMode.coldLAB, ColorMode.whiteLAB, 2.0f*t).toRGB();
                else
                    c = LABColor.lerp(ColorMode.whiteLAB, ColorMode.warmLAB, 2.0f*t-1.0f).toRGB();
                break;
            }
            case ColorMode.WARM_COLD_CIELUV:
            {
                if(t < 0.5)
                    c = LUVColor.lerp(ColorMode.coldLUV, ColorMode.whiteLUV, 2.0f*t).toRGB();
                else
                    c = LUVColor.lerp(ColorMode.whiteLUV, ColorMode.warmLUV, 2.0f*t-1.0f).toRGB();
                break;
            }
            case ColorMode.WARM_COLD_MSH:
            {
                c = MSHColor.fromColorInterpolation(ColorMode.coldRGB, ColorMode.warmRGB, t).toRGB();
                break;
            }
            default:
                return c;
        }

        return c;
    }
}
