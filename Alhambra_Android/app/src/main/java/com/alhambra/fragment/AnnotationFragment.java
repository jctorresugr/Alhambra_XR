package com.alhambra.fragment;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.drawable.BitmapDrawable;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;

import com.alhambra.R;
import com.sereno.color.Color;
import com.sereno.view.AnnotationCanvasData;
import com.sereno.view.AnnotationCanvasView;
import com.sereno.view.AnnotationStroke;
import com.sereno.view.ColorPickerData;
import com.sereno.view.ColorPickerView;

import androidx.annotation.NonNull;

public class AnnotationFragment extends AlhambraFragment
{
    /** The context associated with this fragment*/
    private Context m_ctx = null;

    private int m_currentStrokeColor = 0;

    private AnnotationCanvasView m_canvas = null;
    private ColorPickerView m_colorPicker = null;

    /** Default constructor */
    public AnnotationFragment() { super(); }

    /** Initialize the layout of the fragment once the view is created*/
    private void initLayout(View v)
    {
        m_canvas             = v.findViewById(R.id.annotationCanvas);
        m_colorPicker        = v.findViewById(R.id.colorPicker);
        m_currentStrokeColor = m_colorPicker.getModel().getColor().toRGB().toARGB8888();

        m_canvas.setOnTouchListener((view, motionEvent) -> {
            onTouchSwippingEvent(motionEvent);
            return false;
        });

        m_colorPicker.setOnTouchListener((view, motionEvent) -> {
            onTouchSwippingEvent(motionEvent);
            return false;
        });

        m_canvas.getModel().addListener(new AnnotationCanvasData.IAnnotationDataListener() {
            @Override
            public void onAddStroke(AnnotationCanvasData data, AnnotationStroke stroke) {
                stroke.setColor(m_currentStrokeColor);
            }

            @Override
            public void onSetBackground(AnnotationCanvasData data, Bitmap background) {

            }
        });

        m_colorPicker.getModel().addListener(new ColorPickerData.IColorPickerDataListener()
        {
            @Override
            public void onSetColor(ColorPickerData data, int color)
            {
                m_currentStrokeColor = color;
            }
        });
    }

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceStates)
    {
        View v = inflater.inflate(R.layout.annotation_fragment, container, false);
        m_ctx = getContext();
        initLayout(v);
        return v;
    }

    public boolean startNewAnnotation(int width, int height, byte[] argbImg)
    {
        if(argbImg.length < 4*width*height)
            return false;

        //Need to convert the byte array to the int array...
        int[] argb8888Colors = new int[width*height];
        for(int i = 0; i < width*height; i++)
            argb8888Colors[i] = (argbImg[4*i+0] << 24) +
                                (argbImg[4*i+1] << 16) +
                                (argbImg[4*i+2] << 8)  +
                                (argbImg[4*i+3]);

        m_canvas.getModel().setBackground(Bitmap.createBitmap(argb8888Colors, width, height, Bitmap.Config.ARGB_8888));
        return true;
    }

    /** Function to enable or disable the swipping based on a motion event
     * @param motionEvent the motion event received*/
    private void onTouchSwippingEvent(MotionEvent motionEvent)
    {
        if(motionEvent.getAction() == MotionEvent.ACTION_DOWN)
            callOnDisableSwipping();
        else if(motionEvent.getAction() == MotionEvent.ACTION_UP)
            callOnEnableSwipping();
    }
}
