package com.alhambra.interactions;

import android.util.Log;

import com.alhambra.MainActivity;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.data.AnnotationInfo;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.fragment.PreviewFragment;
import com.google.android.material.chip.Chip;

import java.util.HashSet;

public class SelectionInteraction extends IInteraction implements SelectionData.ISelectionDataChange, PreviewFragment.IPreviewFragmentListener {

    public SelectionData selectionData;
    public AnnotationDataset annotationDataset;

    public void sendHighlightGroups(HashSet<Integer> groups) {
        mainActivity.sendServerAction("HighlightGroups",groups);
    }

    @Override
    protected void reg(MainActivity mainActivity) {
        selectionData = mainActivity.selectionData;
        selectionData.subscriber.addListener(this);
        mainActivity.getPreviewFragment().addListener(this);
        annotationDataset = mainActivity.getAnnotationDataset();
    }

    @Override
    public void onGroupSelectionAdd(int newIndex, HashSet<Integer> groups) {
        processSearch();
    }

    @Override
    public void onGroupSelectionRemove(int newIndex, HashSet<Integer> groups) {
        processSearch();
    }

    @Override
    public void onGroupClear() {
        processSearch();
    }

    @Override
    public void onKeywordChange(String text) {
        processSearch();
    }

    public void processSearch(){
        annotationDataset.applySearch(selectionData);
        mainActivity.sendServerAction("selection", annotationDataset.index2AnnotationID(annotationDataset.getCurrentSelection()));
    }

    @Override
    public void onHighlightDataChunk(PreviewFragment fragment, AnnotationInfo annotationInfo) {

    }

    @Override
    public void onClickChip(Chip chip, int annotationJoint) {
        AnnotationDataset annotationDataset = mainActivity.getAnnotationDataset();
        AnnotationJoint joint = annotationDataset.getAnnotationJoint(annotationJoint);
        if(joint==null){
            Log.w("SelectionInteraction","Cannot resolve joint "+annotationJoint);
            return;
        }
        if(chip.isChecked()) {
            selectionData.addSelectedGroup(annotationJoint);
        }else{
            selectionData.removeSelectedGroup(annotationJoint);
        }

    }
}
