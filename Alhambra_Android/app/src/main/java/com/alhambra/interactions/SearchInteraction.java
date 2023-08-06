package com.alhambra.interactions;

import android.text.Editable;
import android.text.TextWatcher;

import com.alhambra.MainActivity;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationInfo;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.experiment.ExperimentDataCollection;
import com.alhambra.experiment.task.ExperimentTaskInteraction;
import com.sereno.math.BBox;

import java.util.HashSet;

public class SearchInteraction extends IInteraction implements TextWatcher, AnnotationDataset.IDatasetListener, SelectionData.ISelectionDataChange {

    public SelectionData selectionData;
    private boolean isSetting=false;

    @Override
    protected void reg(MainActivity mainActivity) {
        mainActivity.filterEditor.addTextChangedListener(this);
        mainActivity.getAnnotationDataset().addListener(this);
        selectionData = mainActivity.selectionData;
        selectionData.subscriber.addListener(this);
    }

    @Override
    public void beforeTextChanged(CharSequence s, int start, int count, int after) {

    }

    @Override
    public void onTextChanged(CharSequence s, int start, int before, int count) {
        if(isSetting){
            return;
        }
        selectionData.setKeyword(s.toString());
    }

    @Override
    public void afterTextChanged(Editable s) {

    }


    @Override
    public void onSetMainEntryIndex(AnnotationDataset annotationDataset, int index) {

    }

    @Override
    public void onSetSelection(AnnotationDataset annotationDataset, int[] selections) {

    }

    @Override
    public void onAddDataChunk(AnnotationDataset annotationDataset, Annotation annotation) {

    }

    @Override
    public void onRemoveDataChunk(AnnotationDataset annotationDataset, Annotation annotation) {

    }

    @Override
    public void onAnnotationChange(Annotation annotation) {

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

    }

    //----------------------
    @Override
    public void onGroupSelectionAdd(int newIndex, HashSet<Integer> groups) {

    }

    @Override
    public void onGroupSelectionRemove(int newIndex, HashSet<Integer> groups) {

    }

    @Override
    public void onGroupClear() {

    }

    @Override
    public void onKeywordChange(String text) {
        if(isSetting){
            return;
        }
        isSetting=true;
        if(!mainActivity.filterEditor.getText().toString().equals(text)) {
            mainActivity.filterEditor.setText(text);
            ExperimentDataCollection.add("all_search_textEdit",text.toString());
        }
        isSetting=false;
    }
}
