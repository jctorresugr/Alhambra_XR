package com.alhambra.view.graphics;

import android.graphics.Canvas;
import android.graphics.Paint;
import android.util.Log;
import android.view.MotionEvent;
import android.view.View;

import com.alhambra.Utils;
import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.view.MapView;
import com.alhambra.view.base.CanvasBaseElement;
import com.alhambra.view.base.CanvasInteractiveElement;
import com.sereno.math.BBox;
import com.sereno.math.MathUtils;
import com.sereno.math.TranslateMatrix;

import java.util.ArrayList;
import java.util.List;

public class CanvasAnnotationJoint extends CanvasInteractiveElement {

    protected AnnotationJoint annotationJoint;
    private static final float radius=40.0f;

    private static final Paint normalPaint;
    private static final Paint normalBorderPaint;
    private static final Paint mouseDownPaint;
    private static final Paint mouseDownBorderPaint;
    private static final Paint linkPaint;
    private static final Paint textPaint;

    private final ArrayList<CanvasAnnotation> canvasAnnotations = new ArrayList<>();

    static{
        normalPaint =           newPaint(20,255,80,100,2.0f,Paint.Style.FILL);
        normalBorderPaint =     newPaint(10,155,60,255,10.0f,Paint.Style.STROKE);
        mouseDownPaint =        newPaint(180,255,120,255,2.0f,Paint.Style.FILL);
        mouseDownBorderPaint =  newPaint(110,205,110,255,10.0f,Paint.Style.STROKE);
        linkPaint =  newPaint(110,205,110,255,3.0f,Paint.Style.STROKE);
        textPaint =  newPaint(110,205,110,255,3.0f,Paint.Style.STROKE);
        textPaint.setTextSize(20);
    }
    @Override
    public void draw(Canvas canvas) {
        if(this.isMouseDown()){
            //draw lines
            for (CanvasAnnotation ca: canvasAnnotations) {
                canvas.drawLine(x,y,ca.getX(),ca.getY(),linkPaint);
            }
            canvas.drawCircle(x,y,radius,mouseDownPaint);
            canvas.drawCircle(x,y,radius,mouseDownBorderPaint);
        }else{
            canvas.drawCircle(x,y,radius,normalPaint);
            canvas.drawCircle(x,y,radius,normalBorderPaint);
        }
        Utils.drawCenterText(canvas, annotationJoint.getName(),x,y-radius*1.5f,textPaint);
    }

    @Override
    public boolean isInRange(float x0, float y0) {
        return MathUtils.distanceSqr(x0,y0,x,y)<radius*radius*2;
    }

    public AnnotationJoint getAnnotationJoint() {
        return annotationJoint;
    }

    public void setAnnotationJoint(AnnotationJoint annotationJoint, TranslateMatrix translateInfo) {
        this.annotationJoint = annotationJoint;
        BBox r = annotationJoint.getRange();
        this.x = translateInfo.transformPointX(r.getCenterX());
        this.y = translateInfo.transformPointY(r.getCenterZ());
        List<AnnotationID> annotationsID = annotationJoint.getAnnotationsID();
        if(canvasAnnotations.size()!=annotationsID.size()){
            canvasAnnotations.clear();
            if(this.parent==null){
                Log.w("CanvasAnnotationJoint","Cannot find parent!");
                return;
            }
            if(this.parent instanceof MapView){
                MapView mv = (MapView) this.parent;
                for(AnnotationID id: annotationsID){
                    CanvasAnnotation annotationElement = mv.getAnnotationElement(id);
                    if(annotationElement!=null){
                        canvasAnnotations.add(annotationElement);
                    }
                }
            }else{
                //this.parent.elements.stream().filter()
                for (CanvasBaseElement element : this.parent.getElements()) {
                    if(element instanceof CanvasAnnotation){
                       CanvasAnnotation ca = (CanvasAnnotation) element;
                        for(AnnotationID id: annotationsID){
                            if(ca.getAnnotation().id.equals(id)){
                                canvasAnnotations.add(ca);
                            }
                        }

                    }
                }

            }
        }
    }

    @Override
    public void onTouch(View v, MotionEvent e) {
        super.onTouch(v, e);
    }

    @Override
    public void onLoseFocus(MotionEvent e) {
        super.onLoseFocus(e);
    }

}
