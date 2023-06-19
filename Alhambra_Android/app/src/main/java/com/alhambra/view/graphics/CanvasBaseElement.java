package com.alhambra.view.graphics;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.ListenerSubscriber;

public abstract class CanvasBaseElement {

    protected float x,y;
    protected Paint paint;
    private int dirtyFrames;
    int index;
    BaseCanvasElementView parent;
    private boolean isFocused;

    public void dirty(){
        dirtyFrames =1;
        if(parent!=null){
            parent.redraw();
        }
    }

    public void setDirty(int frames){
        dirtyFrames =frames;
        if(frames>0){
            if(parent!=null){
                parent.redraw();
            }
        }
    }

    public void setDirty(boolean flag){
        setDirty(flag?1:0);
    }

    public void decreaseDirty(){
        if(dirtyFrames>0){
            setDirty(dirtyFrames-1);
        }

    }



    public boolean isDirty(){
        return dirtyFrames >0;
    }


    public ListenerSubscriber<View.OnTouchListener> subscribeTouchEvent = new ListenerSubscriber<>();

    public CanvasBaseElement(){
        x=y=0;
        dirty();
    }

    public void setPos(float x, float y){
        this.x=x;
        this.y=y;
        dirty();
    }

    public void setPaint(Paint paint) {
        if(paint!=this.paint){
            this.paint=paint;
            dirty();
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
}
