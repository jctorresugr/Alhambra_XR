package com.sereno.view;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Path;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;

import com.sereno.color.HSVColor;
import com.alhambra.R;

import androidx.annotation.Nullable;

public class ColorPickerView extends View implements ColorPickerData.IColorPickerDataListener
{
    private enum ColorPickerTarget
    {
        SLIDER,
        PICKER
    }
    /** Default hue slider picker height*/
    private static final int PICKER_HEIGHT        = 50;

    /** Default space between the picker SV and slider*/
    private static final int PICKER_SPACE         = 10;

    /** Default circle radius targeting the current color*/
    private static final int PICKER_CIRCLE_RADIUS = 15;

    /** Default slider handler height*/
    private static final int SLIDER_HEIGHT        = 10;

    /** The model bound to this view*/
    private ColorPickerData m_model = new ColorPickerData();

    /** The paint object used to draw on the canvas*/
    private Paint m_paint = new Paint();

    /** circle radius targeting the current color*/
    private int m_circleRadius;

    /** hue slider picker height*/
    private int m_hueHeight;

    /** space between the picker SV and slider*/
    private int m_pickerSpace;

    /** Default slider handler height*/
    private int m_sliderHeight;

    /** What are we currently moving ?*/
    private ColorPickerTarget m_currentTargetSelection =  ColorPickerTarget.PICKER;

    public ColorPickerView(Context context)
    {
        super(context);
        init(null);
    }

    public ColorPickerView(Context context, @Nullable AttributeSet attrs)
    {
        super(context, attrs);
        init(attrs);
    }

    public ColorPickerView(Context context, @Nullable AttributeSet attrs, int defStyleAttr)
    {
        super(context, attrs, defStyleAttr);
        init(attrs);
    }

    public ColorPickerView(Context context, @Nullable AttributeSet attrs, int defStyleAttr, int defStyleRes)
    {
        super(context, attrs, defStyleAttr, defStyleRes);
        init(attrs);
    }

    /** Initialize the internal state of the color picker view
     * @param attrs the parameters key/value attributes*/
    private void init(AttributeSet attrs)
    {
        TypedArray ta = getContext().obtainStyledAttributes(attrs, R.styleable.ColorPickerView);
        m_circleRadius = ta.getDimensionPixelSize(R.styleable.ColorPickerView_circleRadius, PICKER_CIRCLE_RADIUS);
        m_hueHeight  = ta.getDimensionPixelSize(R.styleable.ColorPickerView_pickerHueHeight, PICKER_HEIGHT);
        m_pickerSpace  = ta.getDimensionPixelSize(R.styleable.ColorPickerView_pickerSpace, PICKER_SPACE);
        m_sliderHeight = ta.getDimensionPixelSize(R.styleable.ColorPickerView_sliderHeight, SLIDER_HEIGHT);
        ta.recycle();

        m_model.addListener(this);
        m_paint.setStrokeWidth(3.0f);
    }

    @Override
    public void onDraw(Canvas canvas)
    {
        HSVColor hsvRGB = m_model.getColor();

        //Draw picker color palette
        int handlerWidth = (int)(2.0f*m_sliderHeight*Math.tan(30.0/180.0*Math.PI));

        int pickerWidth  = getWidth() - handlerWidth;
        int pickerHeight = (int)Math.max(0.0, getHeight() - m_hueHeight - m_pickerSpace - m_sliderHeight);
        HSVColor hsv = new HSVColor(0, 0, 0, 1.0f);

        m_paint.setStyle(Paint.Style.FILL);
        for(int i = 0; i < pickerWidth; i+=3)
        {
            for(int j = 0; j < pickerHeight; j+=3)
            {
                hsv.v = ((float)j)/(pickerHeight-1);
                hsv.s = ((float)i)/(pickerWidth-1);
                hsv.h = hsvRGB.h;

                int intColor = hsv.toRGB().toARGB8888();
                m_paint.setColor(intColor);
                canvas.drawRect(i+handlerWidth/2.0f, j, i+handlerWidth/2.0f+Math.min(3, pickerWidth-i-1), j+Math.min(3, pickerHeight-j-1), m_paint);
            }
        }

        HSVColor hsvRGBClone = new HSVColor(hsvRGB.h, 1.0f, 1.0f, 1.0f);
        int rgbHandler = hsvRGBClone.toRGB().toARGB8888();

        //Draw the value slider
        for(int i = pickerWidth-1; i >= 0; i-=2)
        {
            hsvRGBClone.h = 360.0f*((float)i)/(pickerWidth-1);
            m_paint.setColor(hsvRGBClone.toRGB().toARGB8888());
            canvas.drawRect(i+handlerWidth/2.0f, pickerHeight+m_pickerSpace, i+handlerWidth/2.0f+Math.min(2, pickerWidth-i-1), pickerHeight+m_pickerSpace+m_hueHeight, m_paint);
        }

        //Draw the handler
        Path path = new Path();
        path.moveTo(pickerWidth * hsvRGB.h/360.0f+handlerWidth/2.0f, getHeight()-m_sliderHeight);
        path.lineTo(pickerWidth * hsvRGB.h/360.0f, getHeight());
        path.lineTo(pickerWidth * hsvRGB.h/360.0f+handlerWidth, getHeight());
        path.close();
        m_paint.setColor(rgbHandler);
        canvas.drawPath(path, m_paint);

        //Draw the circle where the actual picker is
        int x = (int)(hsvRGB.s*pickerWidth);
        int y = (int)(hsvRGB.v*pickerHeight);

        m_paint.setStyle(Paint.Style.STROKE);
        m_paint.setColor(android.graphics.Color.BLACK);
        canvas.drawCircle(x, y, m_circleRadius, m_paint);
    }

    @Override
    public boolean onTouchEvent(MotionEvent event)
    {
        super.onTouchEvent(event);

        HSVColor hsvRGB = null;
        int handlerWidth = (int)(2.0f*m_sliderHeight*Math.tan(30.0/180.0*Math.PI));
        int pickerWidth = getWidth()-handlerWidth;

        float x = Math.max(0.0f, Math.min(event.getX() - handlerWidth/2.0f, pickerWidth));

        //Determine what we are moving
        if(event.getAction() == MotionEvent.ACTION_DOWN)
        {
            if(event.getY() > getHeight() - m_hueHeight)
                m_currentTargetSelection = ColorPickerTarget.SLIDER;
            else if(event.getY() < getHeight() - m_hueHeight)
                m_currentTargetSelection = ColorPickerTarget.PICKER;
        }

        //Move the target
        if(event.getAction() == MotionEvent.ACTION_DOWN || event.getAction() == MotionEvent.ACTION_MOVE)
        {
            //Check the slider area
            if(m_currentTargetSelection == ColorPickerTarget.SLIDER)
            {
                hsvRGB   = (HSVColor)m_model.getColor().clone();
                hsvRGB.h = 360.0f*x/pickerWidth;
            }

            //Check the picker area
            else if(m_currentTargetSelection == ColorPickerTarget.PICKER)
            {
                float y = Math.max(0.0f, Math.min(event.getY(), getHeight() - m_hueHeight - m_pickerSpace));
                hsvRGB   = (HSVColor)m_model.getColor().clone();
                hsvRGB.v = y / (getHeight() - m_hueHeight - m_pickerSpace);
                hsvRGB.s = x / pickerWidth;
            }
        }

        //Update the color if necessary
        if(hsvRGB != null)
        {
            m_model.setColor(hsvRGB);
            return true;
        }
        return false;
    }

    /** Get the model bound to this color picker view
     * @return the model*/
    public ColorPickerData getModel()
    {
        return m_model;
    }

    /** Set the model bound to this color picker view
     * @param model the new model. Must be different from NULL*/
    public void setModel(ColorPickerData model)
    {
        m_model.removeListener(this);
        m_model = model;
        m_model.addListener(this);
    }

    @Override
    public void onSetColor(ColorPickerData data, int color)
    {
        invalidate();
    }
}
