package com.alhambra.fragment;

import android.content.Context;
import android.graphics.Bitmap;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.MotionEvent;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;

import com.alhambra.R;
import com.sereno.math.Quaternion;
import com.sereno.view.AnnotationCanvasData;
import com.sereno.view.AnnotationCanvasView;
import com.sereno.view.AnnotationGeometry;
import com.sereno.view.AnnotationStroke;
import com.sereno.view.ColorPickerView;

import java.lang.annotation.Annotation;
import java.util.ArrayList;
import java.util.List;

import androidx.annotation.NonNull;

/** The fragment specialized with the management of annotations*/
public class AnnotationFragment extends AlhambraFragment
{
    /** Interface used to catch events from the annotation fragment*/
    public interface IAnnotationFragmentListener
    {
        /** Method called when the Fragment wants data to start an annotation process
         * @param frag the fragment calling this method*/
        void askStartAnnotation(AnnotationFragment frag);

        /** Method called to confirm the current annotation task
         * @param frag the fragment calling this method*/
        void onConfirmAnnotation(AnnotationFragment frag);

        /** Method called to cancel the current annotation task
         * @param frag the fragment calling this method*/
        void onCancelAnnotation(AnnotationFragment frag);
    }

    /** The list of registered listeners*/
    private ArrayList<IAnnotationFragmentListener> m_listeners = new ArrayList<>();

    /** The camera position at the time of where the annotation image was taken. null if no image is currently being annotated*/
    private float[]    m_cameraPos = null;

    /** The camera orientation at the time of where the annotation image was taken. null if no image is currently being annotated*/
    private Quaternion m_cameraRot = null;

    /** The context associated with this fragment*/
    private Context m_ctx = null;

    /** The current ARGB8888 color to use for strokes*/
    private int m_currentStrokeColor = 0;

    /** The canvas view */
    private AnnotationCanvasView m_canvas = null;

    /** The text shown when the user is starting an annotation*/
    private TextView m_startAnnotationTxt = null;

    /** The color picker view*/
    private ColorPickerView m_colorPicker = null;

    /** The ComboBox listing all the drawing method (Strokes vs. Polygons)*/
    private Spinner m_drawingMethod = null;

    /** Button to start an annotation */
    private Button m_startAnnotationBtn = null;

    /** The EditText widget where the annotation description is written*/
    private EditText m_editText = null;

    /** Button to confirm the annotation*/
    private Button m_confirmBtn = null;

    /** Button to cancel the current annotation*/
    private Button m_cancelBtn  = null;

    /** Default constructor */
    public AnnotationFragment() { super(); }

    /** Initialize the layout of the fragment once the view is created*/
    private void initLayout(View v)
    {
        m_canvas             = v.findViewById(R.id.annotationCanvas);
        m_editText           = v.findViewById(R.id.annotationText);
        m_colorPicker        = v.findViewById(R.id.colorPicker);
        m_startAnnotationBtn = v.findViewById(R.id.startAnnotationButton);
        m_confirmBtn         = v.findViewById(R.id.confirmAnnotation);
        m_cancelBtn          = v.findViewById(R.id.cancelAnnotation);
        m_startAnnotationTxt = v.findViewById(R.id.tapToAddAnnotationTxt);
        m_drawingMethod      = v.findViewById(R.id.drawingMethod);
        m_currentStrokeColor = m_colorPicker.getModel().getColor().toRGB().toARGB8888();

        m_canvas.setOnTouchListener((view, motionEvent) -> {
            onTouchSwipingEvent(motionEvent);
            return false;
        });

        m_colorPicker.setOnTouchListener((view, motionEvent) -> {
            onTouchSwipingEvent(motionEvent);
            return false;
        });

        m_drawingMethod.setOnItemSelectedListener(new AdapterView.OnItemSelectedListener() {
            @Override
            public void onItemSelected(AdapterView<?> adapterView, View view, int position, long id)
            {
                m_canvas.getModel().setDrawingMethod(position);
            }

            @Override
            public void onNothingSelected(AdapterView<?> adapterView) {}
        });

        m_canvas.getModel().addListener(new AnnotationCanvasData.IAnnotationDataListener() {
            @Override
            public void onAddStroke(AnnotationCanvasData data, AnnotationStroke stroke) { stroke.setColor(m_currentStrokeColor); }

            @Override
            public void onClearGeometries(AnnotationCanvasData data, List<AnnotationGeometry> geometries) {}

            @Override
            public void onSetBackground(AnnotationCanvasData data, Bitmap background) {}

            @Override
            public void onSetDrawingMethod(AnnotationCanvasData data, int drawingMethod)
            {
                if(m_drawingMethod.getSelectedItemPosition() != drawingMethod)
                    m_drawingMethod.setSelection(drawingMethod);
            }
        });

        m_startAnnotationBtn.setOnClickListener(view -> {
            for(IAnnotationFragmentListener l : m_listeners)
                l.askStartAnnotation(AnnotationFragment.this);
            m_startAnnotationTxt.setVisibility(View.VISIBLE);
        });

        m_confirmBtn.setOnClickListener(view -> {
            for(IAnnotationFragmentListener l : m_listeners)
                l.onConfirmAnnotation(AnnotationFragment.this);
        });
        m_cancelBtn.setOnClickListener(view -> {
            for(IAnnotationFragmentListener l : m_listeners)
                l.onCancelAnnotation(AnnotationFragment.this);
        });

        m_colorPicker.getModel().addListener((data, color) -> m_currentStrokeColor = color);
        m_confirmBtn.setVisibility(View.GONE);
        m_cancelBtn.setVisibility(View.GONE);
        m_startAnnotationTxt.setVisibility(View.GONE);
        m_editText.setVisibility(View.GONE);
        m_colorPicker.setVisibility(View.GONE);
        m_drawingMethod.setVisibility(View.GONE);
    }

    /** @brief Add a new listener
     * @param l the new listener*/
    public void addListener(IAnnotationFragmentListener l)
    {
        m_listeners.add(l);
    }

    /** @brief Remove an old listener
     * @param l the listener to remove*/
    public void removeListener(IAnnotationFragmentListener l)
    {
        m_listeners.remove(l);
    }

    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, ViewGroup container, Bundle savedInstanceStates)
    {
        View v = inflater.inflate(R.layout.annotation_fragment, container, false);
        m_ctx = getContext();
        initLayout(v);
        return v;
    }

    /** Start a new annotation
     * @param width the width of the background to annotate
     * @param height the height of the background to annotate
     * @param argbImg the ARGB8888 image to annotate
     * @param cameraPos the camera position at the time of where the argbImg was taken
     * @param cameraRot the camera orientation at the time of where the argbImg was taken*/
    public boolean startNewAnnotation(int width, int height, byte[] argbImg, float[] cameraPos, Quaternion cameraRot)
    {
        //Check that the incoming data has the correct size
        if(argbImg.length < 4*width*height)
            return false;

        //Need to convert the byte array to the int array...
        int[] argb8888Colors = new int[width*height];
        for(int j = 0; j < height; j++)
            for(int i = 0; i < width; i++)
            {
                int srcIdx = j*width+i;
                int newIdx = (height-1-j)*width+i;
                argb8888Colors[newIdx] = (argbImg[4*srcIdx+3] << 24) +
                                         (argbImg[4*srcIdx+0] << 16) +
                                         (argbImg[4*srcIdx+1] << 8)  +
                                         (argbImg[4*srcIdx+2]);
            }

        m_canvas.getModel().setBackground(Bitmap.createBitmap(argb8888Colors, width, height, Bitmap.Config.ARGB_8888));
        m_canvas.getModel().clearGeometries();
        m_confirmBtn.setVisibility(View.VISIBLE);
        m_cancelBtn.setVisibility(View.VISIBLE);
        m_editText.setVisibility(View.VISIBLE);
        m_colorPicker.setVisibility(View.VISIBLE);
        m_drawingMethod.setVisibility(View.VISIBLE);
        m_cameraPos = cameraPos;
        m_cameraRot = cameraRot;
        m_startAnnotationTxt.setVisibility(View.GONE);
        return true;
    }

    /** Clear the annotation snapshot on the fragment's view*/
    public void clearAnnotation()
    {
        m_canvas.getModel().clearGeometries();
        m_canvas.getModel().setBackground(null);
        m_cancelBtn.setVisibility(View.GONE);
        m_confirmBtn.setVisibility(View.GONE);
        m_editText.setVisibility(View.GONE);
        m_colorPicker.setVisibility(View.GONE);
        m_drawingMethod.setVisibility(View.GONE);
    }

    /** The camera position at the time of where the annotation image was taken. null if no image is currently being annotated
     * @return the [x, y, z] position Vector3*/
    public float[] getCameraPos() {return m_cameraPos;}

    /** The camera orientation at the time of where the annotation image was taken. null if no image is currently being annotated
     * @return the Quaternion representing the orientation*/
    public Quaternion getCameraRot() {return m_cameraRot;}

    /** Get the annotation canvas data model
     * @return the annotation canvas data model*/
    public AnnotationCanvasData getAnnotationCanvasData() {return m_canvas.getModel();}

    /** Get the annotation description
     * @return the String describing the annotation*/
    public String getAnnotationDescription() {return m_editText.getText().toString();}

    /** Function to enable or disable the swiping based on a motion event
     * @param motionEvent the motion event received*/
    private void onTouchSwipingEvent(MotionEvent motionEvent)
    {
        if(motionEvent.getAction() == MotionEvent.ACTION_DOWN)
            callOnDisableSwiping();
        else if(motionEvent.getAction() == MotionEvent.ACTION_UP)
            callOnEnableSwiping();
    }
}
