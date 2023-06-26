package com.alhambra.view.graphics;

import android.graphics.Canvas;
import android.graphics.Color;
import android.graphics.Paint;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.ListenerSubscriber;

public abstract class CanvasInteractiveElement extends CanvasBaseElement {
    private boolean mouseDown = false;
    @Override
    public void onTouch(View v, MotionEvent e) {
        switch(e.getAction()){
            case MotionEvent.ACTION_DOWN:
                setMouseDown(true);
                break;
            case MotionEvent.ACTION_UP:
                setMouseDown(false);
                break;
        }
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
}
