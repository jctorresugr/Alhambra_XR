package com.alhambra.view.graphics;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.view.MotionEvent;

import com.alhambra.MainActivity;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.view.base.CanvasInteractiveElement;
import com.sereno.math.BBox;
import com.sereno.math.MathUtils;
import com.sereno.math.TranslateMatrix;

public class CanvasAnnotation extends CanvasInteractiveElement {
    protected Annotation annotation;
    private static final float radius=30.0f;
    //private boolean highlight = false;


    //styles
    private static final Paint normalPaint;
    private static final Paint normalBorderPaint;
    private static final Paint mouseDownPaint;
    private static final Paint mouseDownBorderPaint;

    static{
        normalPaint =           newPaint(20,80,255,100,2.0f,Paint.Style.FILL);
        normalBorderPaint =     newPaint(10,60,155,255,10.0f,Paint.Style.STROKE);
        mouseDownPaint =        newPaint(120,180,255,255,2.0f,Paint.Style.FILL);
        mouseDownBorderPaint =  newPaint(110,120,205,255,10.0f,Paint.Style.STROKE);
    }




    public void setAnnotation(Annotation annotation) {
        this.annotation=annotation;
        BBox bounds = annotation.renderInfo.getBounds();
        setPos((bounds.min.x+bounds.max.x)*0.5f,(bounds.min.z+bounds.max.z)*0.5f);
    }

    public void setAnnotation(Annotation annotation, TranslateMatrix translateMatrix) {
        this.annotation=annotation;
        BBox bounds = annotation.renderInfo.getBounds();
        float x = (bounds.min.x+bounds.max.x)*0.5f;
        float y = (bounds.min.z+bounds.max.z)*0.5f;
        setPos(translateMatrix.transformPointX(x), translateMatrix.transformPointY(y));
    }

    @Override
    public void draw(Canvas canvas) {
        if(this.isMouseDown()){
            canvas.drawCircle(x,y,radius,mouseDownPaint);
            canvas.drawCircle(x,y,radius,mouseDownBorderPaint);
        }else{
            canvas.drawCircle(x,y,radius,normalPaint);
            canvas.drawCircle(x,y,radius,normalBorderPaint);
        }

        //Log.i("Draw","At: "+x+" \t|\t "+y);
    }

    @Override
    public boolean isInRange(float x0, float y0) {
        return MathUtils.distanceSqr(x0,y0,x,y)<radius*radius*2;
    }

    public Annotation getAnnotation() {
        return annotation;
    }

}
