package com.alhambra.view.graphics;

import android.graphics.Bitmap;
import android.graphics.Canvas;
import android.graphics.Paint;
import android.graphics.Rect;

import com.alhambra.dataset.data.Annotation;
import com.alhambra.view.base.CanvasInteractiveElement;
import com.alhambra.view.base.DynamicFloat;
import com.alhambra.view.base.DynamicHSVColor;
import com.sereno.color.HSVColor;
import com.sereno.math.BBox;
import com.sereno.math.MathUtils;
import com.sereno.math.TranslateMatrix;

public class CanvasAnnotation extends CanvasInteractiveElement {
    protected Annotation annotation;
    private static final float radius=20.0f;
    private static final float radiusDown=60.0f;
    private static final float radiusSelected=45.0f;
    private static final float radiusHide=15.0f;
    public boolean hide=false;
    public boolean isMainSelected = false;
    //styles
    private static final HSVColor fillColor = HSVColor.createFromRGB(20,80,255,100);
    private static final HSVColor borderColor = HSVColor.createFromRGB(10,80,155,100);
    private static final HSVColor imageColor = HSVColor.createFromRGB(255,255,255,255);

    private static final HSVColor fillHideColor = HSVColor.createFromRGB(20,80,255,20);
    private static final HSVColor borderHideColor = HSVColor.createFromRGB(10,80,155,20);
    private static final HSVColor imageHideColor = HSVColor.createFromRGB(255,255,255,20);

    private static final HSVColor fillDownColor = HSVColor.createFromRGB(58,61,227,100);
    private static final HSVColor borderDownColor = HSVColor.createFromRGB(0,0,166,100);//166


    protected DynamicHSVColor fillDynColor = new DynamicHSVColor(fillColor);
    protected DynamicHSVColor borderDynColor = new DynamicHSVColor(borderColor);
    protected DynamicHSVColor imageDynColor = new DynamicHSVColor(imageColor);
    protected DynamicFloat radiusDyn = new DynamicFloat(radius);
    protected Paint fillPaint = newPaint(fillDynColor.curColor.toARGB(),2.0f,Paint.Style.FILL);
    protected Paint imagePaint = newPaint(imageDynColor.curColor.toARGB(),0.0f,Paint.Style.FILL);
    protected Paint borderPaint = newPaint(borderDynColor.curColor.toARGB(),10.0f,Paint.Style.STROKE);


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
        setTranslateMatrix(translateMatrix);
        setTranslatePos(x,y);
    }

    private Rect imageSourceRect;
    private Rect imageDestRect;
    @Override
    public void draw(Canvas canvas) {
        Bitmap bitmap = annotation.info.getBitmap();
        canvas.drawCircle(x,y,radiusDyn.currentValue,fillPaint);
        canvas.drawCircle(x,y,radiusDyn.currentValue,borderPaint);
        if(bitmap!=null){
            if(imageSourceRect==null){
                imageSourceRect = new Rect();
                imageSourceRect.top = 0;
                imageSourceRect.left = 0;
                imageSourceRect.right = bitmap.getWidth();
                imageSourceRect.bottom = bitmap.getHeight();
                imageDestRect = new Rect();
            }
            float largeSize = Math.max(bitmap.getWidth(),bitmap.getHeight());
            float scale = radiusDyn.currentValue*1.2f;
            float widthRatio = bitmap.getWidth()/largeSize*scale;
            float heightRatio = bitmap.getHeight()/largeSize*scale;
            imageDestRect.top    = (int) (y-heightRatio);
            imageDestRect.left   = (int) (x-widthRatio);
            imageDestRect.right  = (int) (x+widthRatio);
            imageDestRect.bottom = (int) (y+heightRatio);
            canvas.drawBitmap(bitmap,imageSourceRect,imageDestRect,imagePaint);
        }
    }

    @Override
    public void update() {
        super.update();
        if(this.isMouseDown()|| isMainSelected){
            fillDynColor.targetColor = fillDownColor;
            borderDynColor.targetColor = borderDownColor;
            if(isMainSelected){
                radiusDyn.targetValue = radiusSelected;
            }else{
                radiusDyn.targetValue = radiusDown;
            }
            imageDynColor.targetColor = imageColor;
        }else{
            if(hide){
                fillDynColor.targetColor = fillHideColor;
                borderDynColor.targetColor = borderHideColor;
                radiusDyn.targetValue = radiusHide;
                imageDynColor.targetColor = imageHideColor;
            }else{
                fillDynColor.targetColor = fillColor;
                borderDynColor.targetColor = borderColor;
                radiusDyn.targetValue = radius;
                imageDynColor.targetColor = imageColor;
            }
        }

        float t=parent.getDeltaTime();
        radiusDyn.update(t);
        fillDynColor.update(t);
        borderDynColor.update(t);
        imageDynColor.update(t);
        imagePaint.setColor(imageDynColor.curColor.toARGB());
        fillPaint.setColor(fillDynColor.curColor.toARGB());
        borderPaint.setColor(borderDynColor.curColor.toARGB());
    }

    @Override
    public boolean isInRange(float x0, float y0) {
        return MathUtils.distanceSqr(x0,y0,x,y)<radius*radius*2;
    }

    public Annotation getAnnotation() {
        return annotation;
    }

}
