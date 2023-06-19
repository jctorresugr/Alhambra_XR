package com.alhambra.view.graphics;

import android.content.Context;
import android.content.res.TypedArray;
import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Matrix;
import android.graphics.Paint;
import android.graphics.PorterDuff;
import android.graphics.drawable.Drawable;
import android.text.TextPaint;
import android.util.AttributeSet;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.R;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.view.graphics.CanvasAnnotation;
import com.alhambra.view.graphics.CanvasBaseElement;
import com.sereno.math.BBox;

import java.util.ArrayList;
import java.util.Collection;

/**
 * TODO: document your custom view class.
 */
public class BaseCanvasElementView extends View implements View.OnTouchListener {

    protected ArrayList<CanvasBaseElement> elements = new ArrayList<>();

    public void addElement(CanvasBaseElement e){
        e.index = elements.size();
        elements.add(e);
        e.parent = this;
        e.dirty();
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
    }

    public void redraw(){
        this.invalidate();
    }


    @Override
    protected void onDraw(Canvas canvas) {
        super.onDraw(canvas);
        //this.getHeight();
        //canvas.drawColor(Color.WHITE, PorterDuff.Mode.CLEAR);
        canvas.drawColor(Color.WHITE);
        boolean continueDraw = false;
        for(CanvasBaseElement e: elements){
            e.update();
            //if(e.isDirty()){
                e.draw(canvas);
                e.decreaseDirty();
                if(e.isDirty()){
                    continueDraw=true;
                }
            //}
        }
        if(continueDraw){
            redraw();
        }
    }

    //dispatch Event
    @Override
    public boolean onTouch(View v, MotionEvent e) {
        float x = e.getX();
        float y = e.getY();
        for(CanvasBaseElement element: elements){
            if(element.isInRange(x,y)){
                element.triggerClick(v,e);
            }else{
                element.triggerLoseFocus(e);
            }
        }
        return true;
    }

}