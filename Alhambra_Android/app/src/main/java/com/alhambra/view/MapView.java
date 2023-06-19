package com.alhambra.view;

import android.content.Context;
import android.graphics.Color;
import android.graphics.Paint;
import android.util.AttributeSet;
import android.util.Log;
import android.view.View;
import android.view.ViewTreeObserver;

import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.view.graphics.BaseCanvasElementView;
import com.alhambra.view.graphics.CanvasAnnotation;
import com.sereno.math.BBox;
import com.sereno.math.TranslateMatrix;

import java.util.HashMap;

public class MapView extends BaseCanvasElementView
        implements
        AnnotationDataset.IDatasetListener,
        View.OnLayoutChangeListener {

    //data
    private AnnotationDataset m_dataset;
    private HashMap<AnnotationID,CanvasAnnotation> annotationElements = new HashMap<>();

    private Paint annotationPaint;

    public TranslateMatrix translateInfo; //TODO: getter setter for interaction

    public void init(){
        annotationPaint = new Paint();
        annotationPaint.setColor(Color.YELLOW);
        annotationPaint.setStrokeWidth(2.0f);
        annotationPaint.setStyle(Paint.Style.FILL);
        translateInfo = new TranslateMatrix();
        translateInfo.scaleX=9000.0f;
        translateInfo.scaleY=9000.0f;
        translateInfo.translateX=400.0f;
        translateInfo.translateY=900.0f;
        this.addOnLayoutChangeListener(this);
    }
    public AnnotationDataset getDataset() {
        return m_dataset;
    }

    public void setDataset(AnnotationDataset m_dataset) {
        if(m_dataset!=null){
            m_dataset.removeListener(this);
        }
        this.m_dataset = m_dataset;
        this.elements.clear();
        annotationElements.clear();
        if(m_dataset==null){
            return;
        }
        recalculateBounds(m_dataset.getModelBounds());
        m_dataset.addListener(this);
        updateDataset();
    }

    public void regenerateElement(){
        this.elements.clear();
        annotationElements.clear();
        updateDataset();
    }

    public MapView(Context context) {
        super(context);
        init();
    }

    public MapView(Context context, AttributeSet attrs) {
        super(context, attrs);
        init();
    }

    public MapView(Context context, AttributeSet attrs, int defStyle) {
        super(context, attrs, defStyle);
        init();
    }

    public void updateDataset(){
        for (Annotation annotation : m_dataset.getAnnotationList()) {
            updateElement(annotation);
        }
    }

    public CanvasAnnotation addElement(Annotation annotation) {
        if(annotation.renderInfo==null){
            return null;
        }
        CanvasAnnotation curElement = annotationElements.get(annotation.id);
        if(curElement==null) {
            curElement = new CanvasAnnotation();
            curElement.setAnnotation(annotation,translateInfo);
            curElement.setPaint(annotationPaint);
            annotationElements.put(annotation.id,curElement);
            this.addElement(curElement);
            curElement.dirty();
        }else{
            curElement.setAnnotation(annotation,translateInfo);
        }
        return curElement;
    }

    public CanvasAnnotation removeElement(Annotation annotation){
        CanvasAnnotation canvasAnnotation = annotationElements.get(annotation.id);
        if(canvasAnnotation==null){
            return null;
        }else{
            annotationElements.remove(annotation.id);
            this.removeElement(canvasAnnotation);
        }
        return canvasAnnotation;
    }

    public void updateElement(Annotation annotation){
        CanvasAnnotation canvasAnnotation = annotationElements.get(annotation.id);
        if(canvasAnnotation!=null){
            canvasAnnotation.setAnnotation(annotation,translateInfo);
        }else{
            addElement(annotation);
        }
    }

    @Override
    public void onSetMainEntryIndex(AnnotationDataset annotationDataset, int index) {

    }

    @Override
    public void onSetSelection(AnnotationDataset annotationDataset, int[] selections) {

    }

    @Override
    public void onAddDataChunk(AnnotationDataset annotationDataset, Annotation annotation) {
        addElement(annotation);
    }

    @Override
    public void onRemoveDataChunk(AnnotationDataset annotationDataset, Annotation annotation) {
        removeElement(annotation);
    }

    @Override
    public void onAnnotationChange(Annotation annotation) {
        updateElement(annotation);
    }

    @Override
    public void onAddJoint(AnnotationJoint annotationJoint) {

    }

    @Override
    public void onRemoveJoint(AnnotationJoint annotationJoint) {

    }

    @Override
    public void onChangeJoint(AnnotationJoint annotationJoint) {

    }

    @Override
    public void onModelBoundsChange(AnnotationDataset annotationDataset, BBox bounds) {
        refreshBoundsInfo();
    }

    protected void recalculateBounds(BBox bounds){
        final float paddingRatio=0.1f;
        final float paddingLeftRatio=1.0f-paddingRatio;
        float x0 = bounds.min.x;
        float x1 = bounds.max.x;
        float z0 = bounds.min.z;
        float z1 = bounds.max.z;

        float w = getWidth()*paddingLeftRatio;
        float h = getHeight()*paddingLeftRatio;

        translateInfo.scaleX = w/Math.max(x1-x0,1e-5f);
        translateInfo.scaleY = h/Math.max(z1-z0,1e-5f);
        translateInfo.translateX = -x0*translateInfo.scaleX+paddingRatio*0.5f*w;
        translateInfo.translateY = -z0*translateInfo.scaleY+paddingRatio*0.5f*h;
        Log.i("TranslateInfo","screen size: "+w+" "+h);
        Log.i("TranslateInfo","scale "+translateInfo.scaleX+" "+translateInfo.scaleY+
                " | trans "+translateInfo.translateX+" "+translateInfo.translateY);
    }

    public void refreshBoundsInfo(){
        recalculateBounds(m_dataset.getModelBounds());
        regenerateElement();
    }

    @Override
    public void onLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom) {
        refreshBoundsInfo();

    }
}
