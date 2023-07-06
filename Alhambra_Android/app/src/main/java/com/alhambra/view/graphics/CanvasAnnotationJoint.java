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
import com.alhambra.view.base.DynamicFloat;
import com.alhambra.view.base.DynamicHSVColor;
import com.sereno.color.HSVColor;
import com.sereno.math.BBox;
import com.sereno.math.MathUtils;
import com.sereno.math.TranslateMatrix;

import java.util.ArrayList;
import java.util.List;

public class CanvasAnnotationJoint extends CanvasInteractiveElement {

    public boolean hide=false;
    public boolean showStroke = false;
    protected AnnotationJoint annotationJoint;
    private static final float radius=30.0f;
    private static final float radiusDown=90.0f;
    private static final float radiusHide=22.5f;

    //styles
    private static final HSVColor fillColor = HSVColor.createFromRGB(20,255,80,100);
    private static final HSVColor borderColor = HSVColor.createFromRGB(10,155,60,255);
    private static final HSVColor textColor = HSVColor.createFromRGB(110,205,110,255);
    private static final HSVColor linkColor = HSVColor.createFromRGB(90,255,100,255);

    private static final HSVColor fillHideColor = HSVColor.createFromRGB(20,255,80,20);
    private static final HSVColor borderHideColor = HSVColor.createFromRGB(10,155,60,20);
    private static final HSVColor textHideColor = HSVColor.createFromRGB(110,205,110,125);
    private static final HSVColor linkHideColor = HSVColor.createFromRGB(90,205,100,0);

    private static final HSVColor fillDownColor = HSVColor.createFromRGB(255,40,155,100);
    private static final HSVColor borderDownColor = HSVColor.createFromRGB(75,20,55,100);
    private static final HSVColor textDownColor = HSVColor.createFromRGB(80,125,60,255);
    private static final HSVColor linkDownColor = HSVColor.createFromRGB(90,255,100,255);

    protected DynamicHSVColor fillDynColor = new DynamicHSVColor(fillColor);
    protected DynamicHSVColor borderDynColor = new DynamicHSVColor(borderColor);
    protected DynamicHSVColor textDynColor = new DynamicHSVColor(textColor);
    protected DynamicHSVColor linkDynColor = new DynamicHSVColor(linkColor);
    protected DynamicFloat radiusDyn = new DynamicFloat(radius);
    protected Paint fillPaint = newPaint(fillDynColor.curColor.toARGB(),2.0f,Paint.Style.FILL);
    protected Paint borderPaint = newPaint(borderDynColor.curColor.toARGB(),10.0f,Paint.Style.STROKE);
    protected Paint textPaint = newPaint(textDynColor.curColor.toARGB(),3.0f,Paint.Style.STROKE);
    protected Paint linkPaint = newPaint(linkDynColor.curColor.toARGB(),3.0f,Paint.Style.STROKE);
    {
        textPaint.setTextSize(20.0f);
    }
    private final ArrayList<CanvasAnnotation> canvasAnnotations = new ArrayList<>();

    @Override
    public void update() {
        super.update();
        if(this.isMouseDown()){
            fillDynColor.targetColor =     fillDownColor;
            borderDynColor.targetColor = borderDownColor;
            textDynColor.targetColor =     textDownColor;
            radiusDyn.targetValue =      radiusDown;
        }else{
            if(hide){
                fillDynColor.targetColor =     fillHideColor;
                borderDynColor.targetColor = borderHideColor;
                textDynColor.targetColor =     textHideColor;
                radiusDyn.targetValue =      radiusHide;
            }else{
                fillDynColor.targetColor = fillColor;
                borderDynColor.targetColor = borderColor;
                textDynColor.targetColor = textColor;
                radiusDyn.targetValue = radius;
            }
        }
        if(showStroke){
            linkDynColor.targetColor =     linkColor;
        }else{
            linkDynColor.targetColor =     linkHideColor;
        }

        float t=parent.getDeltaTime();
        fillDynColor.update(t);
        borderDynColor.update(t);
        textDynColor.update(t);
        linkDynColor.update(t);
        radiusDyn.update(t);
        fillPaint.setColor(fillDynColor.curColor.toARGB());
        borderPaint.setColor(borderDynColor.curColor.toARGB());
        textPaint.setColor(textDynColor.curColor.toARGB());
        linkPaint.setColor(linkDynColor.curColor.toARGB());
    }

    @Override
    public void draw(Canvas canvas) {
        float radius = radiusDyn.currentValue;
        canvas.drawCircle(x,y,radius,fillPaint);
        canvas.drawCircle(x,y,radius,borderPaint);
        for (CanvasAnnotation ca: canvasAnnotations) {
            canvas.drawLine(x,y,ca.getX(),ca.getY(),linkPaint);
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
        setTranslateMatrix(translateMatrix);
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
