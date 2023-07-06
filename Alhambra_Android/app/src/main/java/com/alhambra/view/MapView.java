package com.alhambra.view;

import android.content.Context;
import android.util.AttributeSet;
import android.util.Log;
import android.view.View;

import com.alhambra.ListenerSubscriber;
import com.alhambra.MainActivity;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.UserData;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.view.base.BaseCanvasElementView;
import com.alhambra.view.base.CanvasImageElement;
import com.alhambra.view.graphics.CanvasAnnotation;
import com.alhambra.view.graphics.CanvasAnnotationJoint;
import com.alhambra.view.graphics.CanvasUser;
import com.sereno.math.BBox;
import com.sereno.math.TranslateMatrix;

import java.util.HashMap;
import java.util.HashSet;
import java.util.Set;

public class MapView extends BaseCanvasElementView
        implements
        AnnotationDataset.IDatasetListener,
        View.OnLayoutChangeListener, SelectionData.ISelectionDataChange {


    public interface OnClickSymbols{
        void onClickAnnotation(Annotation annotation);
        void onClickAnnotationJoint(AnnotationJoint annotationJoint);
    }
    //data
    private AnnotationDataset m_dataset;
    private UserData userData;
    private SelectionData selectionData;

    //canvas element
    private CanvasUser canvasUser;
    private CanvasImageElement canvasBackground;
    private final HashMap<AnnotationID,CanvasAnnotation> annotationElements = new HashMap<>();
    private final HashMap<Integer, CanvasAnnotationJoint> annotationJointElements = new HashMap<>();

    //settings and listeners
    public boolean fixedBounds =false;
    public TranslateMatrix translateInfo; //TODO: getter setter for interaction
    public ListenerSubscriber<OnClickSymbols> onClickSymbolsListeners = new ListenerSubscriber<>();

    public void init(){
        translateInfo = new TranslateMatrix();
        canvasBackground = new CanvasImageElement();
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
        this.annotationJointElements.clear();
        if(m_dataset==null){
            return;
        }
        recalculateBounds(m_dataset.getModelBounds());
        m_dataset.addListener(this);
        updateDataset();
    }

    public void setSelectionData(SelectionData selectionData){
        if(this.selectionData!=null){
            this.selectionData.subscriber.removeListener(this);
        }
        this.selectionData = selectionData;
        selectionData.subscriber.addListener(this);
    }

    public UserData getUserData() {
        return userData;
    }

    public void setUserData(UserData userData) {
        if(this.userData==userData){
            return;
        }
        this.userData = userData;
        canvasUser = new CanvasUser();
        this.addElement(canvasUser);
        canvasUser.setUserData(userData);
        canvasUser.setTranslateMatrix(this.translateInfo);
    }

    public void regenerateElement(){
        this.elements.clear();
        annotationElements.clear();
        annotationJointElements.clear();
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
        this.addElement(canvasBackground);

        for (Annotation annotation : m_dataset.getAnnotationList()) {
            updateElement(annotation);
        }
        for(AnnotationJoint annotationJoint: m_dataset.getAnnotationJoint()){
            updateElement(annotationJoint);
        }

        if(canvasUser!=null){
            canvasUser.setTranslateMatrix(translateInfo);
            this.addElement(canvasUser);
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
            annotationElements.put(annotation.id,curElement);
            this.addElement(curElement);
            curElement.onClickListeners.addListener(
                    l->onClickSymbolsListeners.invoke(i->i.onClickAnnotation(((CanvasAnnotation)l).getAnnotation()))
            );
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

    public CanvasAnnotationJoint removeElement(AnnotationJoint annotationJoint){
        CanvasAnnotationJoint canvasAnnotation = annotationJointElements.get(annotationJoint.getId());
        if(canvasAnnotation==null){
            return null;
        }else{
            annotationJointElements.remove(annotationJoint.getId());
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

    public void updateElement(AnnotationJoint annotationJoint){
        CanvasAnnotationJoint canvasAnnotationJoint = annotationJointElements.get(annotationJoint.getId());
        if(canvasAnnotationJoint!=null){
            canvasAnnotationJoint.setAnnotationJoint(annotationJoint,translateInfo);
        }else{
            addElement(annotationJoint);
        }
    }

    public CanvasAnnotationJoint addElement(AnnotationJoint annotationJoint){
        if(annotationJoint==null){
            return null;
        }
        CanvasAnnotationJoint canvasAnnotationJoint = annotationJointElements.get(annotationJoint.getId());
        if(canvasAnnotationJoint==null){
            canvasAnnotationJoint = new CanvasAnnotationJoint();
            this.addElement(canvasAnnotationJoint);
            annotationJointElements.put(annotationJoint.getId(),canvasAnnotationJoint);
            canvasAnnotationJoint.setAnnotationJoint(annotationJoint,translateInfo);
            canvasAnnotationJoint.onClickListeners.addListener(
                    l->onClickSymbolsListeners.invoke(i->i.onClickAnnotationJoint(((CanvasAnnotationJoint)l).getAnnotationJoint()))
            );
        }else{
            canvasAnnotationJoint.setAnnotationJoint(annotationJoint,translateInfo);
        }
        return canvasAnnotationJoint;
    }

    @Override
    public void onSetMainEntryIndex(AnnotationDataset annotationDataset, int index) {
        updateSelectionVisualEffect();
    }

    @Override
    public void onSetSelection(AnnotationDataset annotationDataset, int[] selections) {
        updateSelectionVisualEffect();
    }

    public void updateSelectionVisualEffect(){
        updateSelectionVisualEffect(this.m_dataset,this.selectionData);
    }
    public void updateSelectionVisualEffect(AnnotationDataset annotationDataset, SelectionData selectionData){
        if(selectionData.isEmptyFilter() && annotationDataset.isEmptySelection()){
            setAnnotationJointsHide(false);
            setAnnotationsHide(false);
        } else {
            int[] currentSelection = annotationDataset.getCurrentSelection();
            //annot
            if(annotationDataset.isEmptySelection()){
                setAnnotationsHide(false);
            }else{
                setAnnotationsHide(true);
                for(int index: currentSelection) {
                    setAnnotationHide(index,false);
                }
            }
            setAnnotationHide(annotationDataset.getMainEntryIndex(),false);
            //joint
            Set<Integer> selectedGroups = selectionData.getSelectedGroups();
            if(selectedGroups.size()==0){
                if(selectionData.isEmptyKeyWords()){
                    setAnnotationJointsHide(false);
                }else{
                    setAnnotationJointsHide(true);
                }

            }else{
                setAnnotationJointsHide(true);
                for (int index : selectedGroups) {
                    CanvasAnnotationJoint caj = annotationJointElements.get(index);
                    if(caj!=null){
                        caj.hide=false;
                    }
                }
            }
            for(CanvasAnnotationJoint caj: annotationJointElements.values()){
                caj.showStroke=false;
            }
            for (int index : selectedGroups) {
                CanvasAnnotationJoint caj = annotationJointElements.get(index);
                if(caj!=null){
                    caj.showStroke=true;
                }
            }

        }
    }

    protected void setAnnotationJointsHide(boolean state){
        for(CanvasAnnotationJoint caj: annotationJointElements.values()){
            caj.hide=state;
        }
    }

    protected void setAnnotationHide(int index, boolean state){
        Annotation annotation = m_dataset.getAnnotation(index);
        if (annotation != null) {
            CanvasAnnotation canvasAnnotation = annotationElements.get(annotation.id);
            if (canvasAnnotation != null) {
                canvasAnnotation.hide = false;
            }
        }
    }

    protected void setAnnotationsHide(boolean state){
        for (CanvasAnnotation ca : annotationElements.values()) {
            ca.hide=state;
        }
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
        addElement(annotationJoint);
    }

    @Override
    public void onRemoveJoint(AnnotationJoint annotationJoint) {
        removeElement(annotationJoint);
    }

    @Override
    public void onChangeJoint(AnnotationJoint annotationJoint) {
        updateElement(annotationJoint);
    }

    @Override
    public void onModelBoundsChange(AnnotationDataset annotationDataset, BBox bounds) {
        refreshBoundsInfo();
    }

    private final float[] tempPoint = new float[2];

    protected float[] point(float x, float y)
    {
        tempPoint[0]=x;
        tempPoint[1]=y;
        return tempPoint;
    }
    protected void recalculateBounds(BBox bounds){
        if(bounds==null || bounds.min==null){
            return;
        }
        final float paddingRatio=0.03f;
        final float paddingLeftRatio=1.0f-paddingRatio;
        float x0 = bounds.min.x;
        float x1 = bounds.max.x;
        float z0 = bounds.min.z;
        float z1 = bounds.max.z;

        //map points
        matrix.mapPoints(point(x0,z0));
        x0 = tempPoint[0];
        z0 = tempPoint[1];

        matrix.mapPoints(point(x1,z1));
        x1 = tempPoint[0];
        z1 = tempPoint[1];

        if(x0>x1) {
            float t=x0;
            x0=x1;
            x1=t;
        }
        if(z0>z1) {
            float t=z0;
            z0=z1;
            z1=t;
        }

        float w = getWidth()*paddingLeftRatio;
        float h = getHeight()*paddingLeftRatio;

        translateInfo.scaleX = w/Math.max(x1-x0,1e-5f);
        translateInfo.scaleY = h/Math.max(z1-z0,1e-5f);
        translateInfo.scaleX = translateInfo.scaleY = Math.min(translateInfo.scaleX,translateInfo.scaleY);
        translateInfo.translateX = -bounds.min.x*translateInfo.scaleX+paddingRatio*0.5f*w;
        translateInfo.translateY = -bounds.min.z*translateInfo.scaleY+paddingRatio*0.5f*h;
        /*
        Log.i("TranslateInfo","screen size: "+w+" "+h);
        Log.i("TranslateInfo","scale "+translateInfo.scaleX+" "+translateInfo.scaleY+
                " | trans "+translateInfo.translateX+" "+translateInfo.translateY);
        */
    }

    public void refreshBoundsInfo(){
        recalculateBounds(m_dataset.getModelBounds());
        regenerateElement();
    }

    @Override
    public void onLayoutChange(View v, int left, int top, int right, int bottom, int oldLeft, int oldTop, int oldRight, int oldBottom) {
        if(fixedBounds){
            return;
        }
        refreshBoundsInfo();

    }

    public CanvasAnnotation getAnnotationElement(AnnotationID annotationID) {
        return annotationElements.get(annotationID);
    }

    public CanvasAnnotationJoint getAnnotationJointElement(int jointID) {
        return annotationJointElements.get(jointID);
    }


    @Override
    public void onGroupSelectionAdd(int newIndex, HashSet<Integer> groups) {
        updateSelectionVisualEffect();
    }

    @Override
    public void onGroupSelectionRemove(int newIndex, HashSet<Integer> groups) {
        updateSelectionVisualEffect();
    }

    @Override
    public void onGroupClear() {
        updateSelectionVisualEffect();
    }

    @Override
    public void onKeywordChange(String text) {
        updateSelectionVisualEffect();
    }
}
