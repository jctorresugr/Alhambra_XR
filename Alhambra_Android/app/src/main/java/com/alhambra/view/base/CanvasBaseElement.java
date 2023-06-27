package com.alhambra.view.base;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.ListenerSubscriber;
import com.alhambra.view.base.BaseCanvasElementView;

public abstract class CanvasBaseElement {


    protected float x,y;
    protected Paint paint;
    int index;
    protected BaseCanvasElementView parent;
    private boolean isFocused;

    public ListenerSubscriber<View.OnTouchListener> subscribeTouchEvent = new ListenerSubscriber<>();

    public CanvasBaseElement(){
        x=y=0;
    }

    public void setPos(float x, float y){
        this.x=x;
        this.y=y;
    }

    public void setPaint(Paint paint) {
        if(paint!=this.paint){
            this.paint=paint;
        }
    }

    public void onTouch(View v,MotionEvent e){};

    public void update(){};
    public abstract void draw(Canvas canvas);

    public void triggerClick(View v,MotionEvent e){
        isFocused=true;
        onTouch(v,e);
        subscribeTouchEvent.invoke(l->l.onTouch(v,e));
    }

    public void triggerLoseFocus(MotionEvent e){
        isFocused=false;
        onLoseFocus(e);
    }

    public boolean isFocused() {
        return isFocused;
    }

    public void onLoseFocus(MotionEvent e){

    }

    public abstract boolean isInRange(float x0, float y0);

    public float getX() {
        return x;
    }

    public float getY() {
        return y;
    }

    public Paint getPaint() {
        return paint;
    }

    public BaseCanvasElementView getParent() {
        return parent;
    }
}
