package com.alhambra.interactions;

import android.util.Log;

import com.alhambra.MainActivity;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.data.AnnotationInfo;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.fragment.PreviewFragment;
import com.google.android.material.chip.Chip;

import java.util.ArrayList;
import java.util.HashSet;
import java.util.stream.Collectors;

public class SelectionInteraction extends IInteraction implements SelectionData.ISelectionDataChange, PreviewFragment.IPreviewFragmentListener {

    public SelectionData selectionData;

    public void sendHighlightGroups(HashSet<Integer> groups) {
        mainActivity.sendServerAction("HighlightGroups",groups);

    }

    @Override
    protected void reg(MainActivity mainActivity) {
        selectionData = mainActivity.selectionData;
        selectionData.subscriber.addListener(this);
        mainActivity.getPreviewFragment().addListener(this);
    }

    @Override
    public void onGroupSelectionAdd(int newIndex, HashSet<Integer> groups) {
        sendHighlightGroups(groups);
    }

    @Override
    public void onGroupSelectionRemove(int newIndex, HashSet<Integer> groups) {
        sendHighlightGroups(groups);
    }

    @Override
    public void onGroupClear() {
        sendHighlightGroups(new HashSet<>());
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
