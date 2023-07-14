package com.alhambra.view.base;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.ListenerSubscriber;
import com.alhambra.view.base.BaseCanvasElementView;
import com.sereno.math.TranslateMatrix;

public abstract class CanvasBaseElement {

    // basic position
    protected float x,y;
    // index in the CanvasElementView
    int index;
    // parent
    protected BaseCanvasElementView parent;
    protected TranslateMatrix translateMatrix;
    // when the interaction is in the range, this will be true.
    private boolean isFocused;

    public ListenerSubscriber<View.OnTouchListener> subscribeTouchEvent = new ListenerSubscriber<>();

    public CanvasBaseElement(){
        x=y=0;
    }

    public void setPos(float x, float y){
        this.x=x;
        this.y=y;
    }

    public void setTranslatePos(float x,float y){
        if(translateMatrix!=null){
            setPos(translateMatrix.transformPointX(x), translateMatrix.transformPointY(y));
        }else{
            setPos(x,y);
        }
    }

    public void onTouch(View v,MotionEvent e){};

    //invoke every frame
    public void update(){};
    //invoke when draw (every frame)
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

    // judge if the click is inside the range of the element
    public abstract boolean isInRange(float x0, float y0);

    public float getX() {
        return x;
    }

    public float getY() {
        return y;
    }

    public TranslateMatrix getTranslateMatrix() {
        return translateMatrix;
    }

    public void setTranslateMatrix(TranslateMatrix translateMatrix) {
        this.translateMatrix = translateMatrix;
    }

    public BaseCanvasElementView getParent() {
        return parent;
    }

    protected static Paint newPaint(int r,int g,int b,int a, float stroke, Paint.Style style) {
        Paint p = new Paint();
        p.setColor(android.graphics.Color.argb(a,r,g,b));
        p.setStrokeWidth(stroke);
        p.setStyle(style);
        return p;
    }

    protected static Paint newPaint(int color, float stroke, Paint.Style style) {
        Paint p = new Paint();
        p.setColor(color);
        p.setStrokeWidth(stroke);
        p.setStyle(style);
        return p;
    }
}
