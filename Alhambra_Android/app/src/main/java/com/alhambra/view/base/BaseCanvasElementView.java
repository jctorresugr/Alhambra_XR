package com.alhambra.view.base;

import android.content.Context;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.util.AttributeSet;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.view.MapView;

import java.util.ArrayList;
import java.util.Collections;
import java.util.List;

/**
 * TODO: document your custom view class.
 */
public class BaseCanvasElementView extends View implements View.OnTouchListener {

    protected ArrayList<CanvasBaseElement> elements = new ArrayList<>();
    public Matrix matrix;
    public boolean impedeEvents = false;
    public int cleanColor= Color.WHITE;
    private long lastTime;
    private long lastInterval;

    public void addElement(CanvasBaseElement e){
        e.index = elements.size();
        elements.add(e);
        e.parent = this;
    }

    public List<CanvasBaseElement> getElements() {
        return Collections.unmodifiableList(elements);
    }

    public void removeElement(CanvasBaseElement e){
        int ind = e.index;
        CanvasBaseElement element = elements.get(e.index);
        if(element==e){
            int lastIndex = elements.size()-1;
            if(ind == lastIndex){
                elements.remove(lastIndex);
            }else{
                elements.set(e.index,elements.get(lastIndex));
                elements.remove(lastIndex);
            }
            e.parent=null;
            e.index=-1;
        }
    }

    public BaseCanvasElementView(Context context) {
        super(context);
        init(null, 0);
    }

    public BaseCanvasElementView(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(attrs, 0);
    }

    public BaseCanvasElementView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        init(attrs, defStyle);
    }

    private void init(AttributeSet attrs, int defStyle) {
        this.setOnTouchListener(this);
        matrix = new Matrix();
    }

    public void redraw(){
        this.invalidate();
    }

    public long getNanoDeltaTime(){
        return lastInterval;
    }

    public float getDeltaTime(){
        return lastInterval*1e-09f;
    }


    @Override
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        canvas.setMatrix(matrix);
        canvas.drawColor(cleanColor);
        for(CanvasBaseElement e: elements){
            e.update();
            e.draw(canvas);
        }
        redraw();

        long time = System.nanoTime();
        lastInterval = time-lastTime;
        lastTime=time;
    }

    private final Matrix matrixInvCache = new Matrix();
    private final float[] pointsCache = new float[2];
    //dispatch Event
    @Override
    public boolean onTouch(View v, MotionEvent e) {
        float x = e.getX();
        float y = e.getY();
        matrix.invert(matrixInvCache);
        pointsCache[0]=x;
        pointsCache[1]=y;
        matrixInvCache.mapPoints(pointsCache);
        x=pointsCache[0];
        y=pointsCache[1];
        e.setLocation(x,y);
        Log.i("BaseCanvasView","Click "+x+" \t"+y+" "+e.getAction());
        for(CanvasBaseElement element: elements){
            if(element.isInRange(x,y)){
                element.triggerClick(v,e);
            }else{
                element.triggerLoseFocus(e);
            }
        }
        return impedeEvents;
    }


}