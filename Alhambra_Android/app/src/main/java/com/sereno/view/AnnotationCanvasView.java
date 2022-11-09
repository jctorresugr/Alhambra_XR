package com.sereno.view;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Path;
import android.graphics.Point;
import android.graphics.Rect;
import android.media.Image;
import android.util.AttributeSet;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;

import java.util.ArrayList;
import java.util.List;

import androidx.annotation.Nullable;

import com.alhambra.MainActivity;

public class AnnotationCanvasView extends View implements AnnotationCanvasData.IAnnotationDataListener, AnnotationGeometry.IAnnotationGeometryListener, AnnotationStroke.IAnnotationStrokeListener
{
    /** The internal data of the annotation view*/
    private AnnotationCanvasData m_model = new AnnotationCanvasData(1024, 1024);

    /** The paint object used to draw strokes on screen*/
    private Paint m_strokePaint = new Paint();

    private Rect m_backgroundSrcRect = null;

    public AnnotationCanvasView(Context context)
    {
        super(context);
        init(null);
    }

    public AnnotationCanvasView(Context context, @Nullable AttributeSet attrs)
    {
        super(context, attrs);
        init(attrs);
    }

    public AnnotationCanvasView(Context context, @Nullable AttributeSet attrs, int defStyleAttr)
    {
        super(context, attrs, defStyleAttr);
        init(attrs);
    }

    public AnnotationCanvasView(Context context, @Nullable AttributeSet attrs, int defStyleAttr, int defStyleRes)
    {
        super(context, attrs, defStyleAttr, defStyleRes);
        init(attrs);
    }

    /** Initialize the annotation view internal state*/
    private void init(AttributeSet attrs)
    {
        setFocusable(true);
        setFocusableInTouchMode(true);
        m_model.addListener(this);
        m_strokePaint.setStyle(Paint.Style.STROKE);
    }

    @Override
    public void onDraw(Canvas canvas)
    {
        if(m_model == null)
            return;

        int viewWidth   = getWidth();
        int viewHeight  = getHeight();
        int modelWidth  = m_model.getWidth();
        int modelHeight = m_model.getHeight();

        //Draw the background, if any
        if(m_model.getBackground() != null)
            canvas.drawBitmap(m_model.getBackground(), m_backgroundSrcRect, new Rect(0, 0, viewWidth, viewHeight), m_strokePaint);

        Path path = new Path();

        //Draw the strokes
        for(AnnotationGeometry g : m_model.getGeometries())
        {
            if(!g.isValid()) //Do not draw an invalid geometry...
                continue;

            boolean toClose = false;
            if(g instanceof AnnotationStroke)
            {
                AnnotationStroke s = (AnnotationStroke) g;
                m_strokePaint.setStrokeWidth(s.getWidth());
                m_strokePaint.setStyle(Paint.Style.STROKE);
            }

            else if(g instanceof AnnotationPolygon)
            {
                m_strokePaint.setStyle(Paint.Style.FILL);
                toClose = true;
            }
            else
            {
                Log.w(MainActivity.TAG, "Ignoring a geometry to be drawn, as its drawing type is unknown");
                continue;
            }
            //Parameterize the paint
            m_strokePaint.setColor(g.getColor());

            //Draw the path
            ArrayList<Point> points = g.getPoints();

            if(points.size() > 0)
                path.moveTo(points.get(0).x*viewWidth/modelWidth,
                            points.get(0).y*viewHeight/modelHeight);
            for(int i = 1; i < points.size(); i++)
                path.lineTo(points.get(i).x*viewWidth/modelWidth,
                            points.get(i).y*viewHeight/modelHeight);
            if(toClose)
                path.close();

            canvas.drawPath(path, m_strokePaint);
            path.reset();
        }
    }

    @Override
    public boolean onTouchEvent(MotionEvent e)
    {
        super.onTouchEvent(e);
        int viewWidth   = getWidth();
        int viewHeight  = getHeight();
        int modelWidth  = m_model.getWidth();
        int modelHeight = m_model.getHeight();

        if(m_model == null)
            return false;

        boolean addPoint = false;
        if(e.getAction() == MotionEvent.ACTION_DOWN)
        {
            if(m_model.getDrawingMethod() == AnnotationCanvasData.DRAWING_METHOD_STROKES)
            {
                addPoint = true;
                m_model.addStroke(new AnnotationStroke());
            }
            else if(m_model.getDrawingMethod() == AnnotationCanvasData.DRAWING_METHOD_POLYGONS)
            {
                addPoint = true;
                m_model.addPolygon(new AnnotationPolygon());
            }
        }

        //Just to tell that we can modify the stroke/polygon
        else if(e.getAction() == MotionEvent.ACTION_MOVE)
            addPoint = true;

        //Add the event point. The callback listeners will invalidate this view
        if(addPoint && m_model.getGeometries().size() > 0)
        {
            Point p = new Point((int)(e.getX()*modelWidth/viewWidth),
                                (int)(e.getY()*modelHeight/viewHeight));

            if(m_model.getDrawingMethod() == AnnotationCanvasData.DRAWING_METHOD_STROKES ||
               m_model.getDrawingMethod() == AnnotationCanvasData.DRAWING_METHOD_POLYGONS)
                m_model.getGeometries().get(m_model.getGeometries().size()-1).addPoint(p);

            return true;
        }

        return false;
    }

    @Override
    protected void onMeasure(int widthMeasureSpec, int heightMeasureSpec)
    {
        //The layout parameters
        int widthMode = MeasureSpec.getMode(widthMeasureSpec);
        int widthSize = MeasureSpec.getSize(widthMeasureSpec);
        int heightMode = MeasureSpec.getMode(heightMeasureSpec);
        int heightSize = MeasureSpec.getSize(heightMeasureSpec);

        //The final width and height
        int width, height;

        //Used for aspect ratio
        boolean adaptWidth  = false;
        boolean adaptHeight = false;

        if(widthMode != MeasureSpec.EXACTLY) {
            if (widthMode == MeasureSpec.AT_MOST)
                adaptWidth = true;
            else
                widthSize = getModel().getWidth();
        }
        if(heightMode != MeasureSpec.EXACTLY) {
            if (heightMode == MeasureSpec.AT_MOST)
                adaptHeight = true;
            else
                heightSize = getModel().getHeight();
        }

        //Conserve aspect ratio if possible

        //If we should adapt both the width and height: see which one we conserve as fixed and adapt later the other dimension
        if(adaptWidth && adaptHeight)
        {
            if((float)heightSize/widthSize > (float)getModel().getHeight()/getModel().getWidth()) //Height is bigger: fix the height and adapt the width later
                adaptHeight = false;
            else //Otherwise, fix the width and adapt the height later
                adaptWidth = false;
        }

        if(adaptWidth)
        {
            height = heightSize;
            width  = getModel().getWidth()*heightSize/getModel().getHeight(); //The height is already fixed. Adapt the width
        }
        else if(adaptHeight)
        {
            width = widthSize;
            height = getModel().getHeight()*widthSize/getModel().getWidth();
        }
        else //Both dimensions are fixed
        {
            width = widthSize;
            height = heightSize;
        }

        setMeasuredDimension(width, height);
    }

    /** Set the AnnotationData model
     * @param model the new AnnotationData model*/
    public void setModel(AnnotationCanvasData model)
    {
        if(m_model != null)
            m_model.removeListener(this);
        m_model = model;
        if(m_model != null)
        {
            m_model.addListener(this);
        }
        invalidate();
    }

    /** Get the AnnotationData model
     * @return the AnnotationData model*/
    public AnnotationCanvasData getModel()
    {
        return m_model;
    }

    @Override
    public void onSetCurrentColor(AnnotationCanvasData data, int color) {}

    @Override
    public void onAddStroke(AnnotationCanvasData data, AnnotationStroke stroke)
    {
        stroke.addListener((AnnotationGeometry.IAnnotationGeometryListener)this);
        stroke.addListener((AnnotationStroke.IAnnotationStrokeListener)this);
        invalidate();
    }

    @Override
    public void onAddPolygon(AnnotationCanvasData data, AnnotationPolygon polygon)
    {
        polygon.addListener(this);
        invalidate();
    }

    @Override
    public void onClearGeometries(AnnotationCanvasData data, List<AnnotationGeometry> geometries)
    {
        for(AnnotationGeometry g : geometries)
        {
            g.removeListener(this);
            if(g instanceof AnnotationStroke)
                ((AnnotationStroke)g).removeListener((AnnotationStroke.IAnnotationStrokeListener)this);
        }
        invalidate();
    }

    @Override
    public void onSetBackground(AnnotationCanvasData data, Bitmap background)
    {
        if(m_model.getBackground() == null)
        {
            invalidate();
            return;
        }
        m_backgroundSrcRect = new Rect(0, 0,
                                       m_model.getBackground().getWidth(), m_model.getBackground().getHeight());
        invalidate();
    }

    @Override
    public void onSetDrawingMethod(AnnotationCanvasData data, int drawingMethod)
    {}

    @Override
    public void onAddPoint(AnnotationGeometry geometry, Point p)
    {
        invalidate();
    }

    @Override
    public void onSetColor(AnnotationGeometry geometry, int c)
    {
        invalidate();
    }

    @Override
    public void onSetWidth(AnnotationStroke stroke, float w)
    {
        invalidate();
    }
}
