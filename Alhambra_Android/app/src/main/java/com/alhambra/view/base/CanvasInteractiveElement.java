package com.alhambra.view.base;

import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.ListenerSubscriber;
import com.alhambra.view.base.CanvasBaseElement;

public abstract class CanvasInteractiveElement extends CanvasBaseElement {
    public interface OnClickCanvasElementListener{
        void onClick(CanvasInteractiveElement e);
    }
    public ListenerSubscriber<OnClickCanvasElementListener> onClickListeners = new ListenerSubscriber<>();
    private boolean mouseDown = false;
    @Override
    public void onTouch(View v, MotionEvent e) {
        switch(e.getAction()){
            case MotionEvent.ACTION_DOWN:
                setMouseDown(true);
                break;
            case MotionEvent.ACTION_UP:
                if(mouseDown){
                    onClick(e);
                }
                setMouseDown(false);
                break;
        }
    }

    public void onClick(MotionEvent e){
        onClickListeners.invoke(l->l.onClick(this));
        Log.i("CanvasElement","Clicked "+this);
    }

    public boolean isMouseDown() {
        return mouseDown;
    }

    public void setMouseDown(boolean mouseDown) {
        if(mouseDown!=this.mouseDown){
            dirty();
        }
        this.mouseDown = mouseDown;
    }

    @Override
    public void onLoseFocus(MotionEvent e) {
        setMouseDown(false);
    }

    protected static Paint newPaint(int r,int g,int b,int a, float stroke, Paint.Style style) {
        Paint p = new Paint();
        p.setColor(Color.argb(a,r,g,b));
        p.setStrokeWidth(stroke);
        p.setStyle(style);
        return p;
    }

    protected void translateAndRotate(Canvas canvas,float dx,float dy, float angle){
        canvas.translate(dx,dy);
        canvas.rotate(angle);
    }
}
