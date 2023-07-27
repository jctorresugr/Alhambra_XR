package com.alhambra.interactions;

import androidx.viewpager.widget.ViewPager;

import com.alhambra.MainActivity;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.view.MapView;

public class MinimapInteraction extends IInteraction implements MapView.OnClickSymbols {

    private MapView mapView;
    public SelectionData selectionData;
    public ViewPager viewPager;

    @Override
    protected void reg(MainActivity mainActivity) {

    }

    @Override
    public void onClickAnnotation(Annotation annotation) {
        //mainActivity.sendServerAction("TeleportAnnotation",annotation.id);
        AnnotationDataset annotationDataset = mainActivity.getAnnotationDataset();
        annotationDataset.setMainEntryIndex(annotation.info.getIndex());
    }

    @Override
    public void onClickAnnotationJoint(AnnotationJoint annotationJoint) {
        //mainActivity.sendServerAction("TeleportAnnotationJoint",annotationJoint.getId());
        selectionData.clearSelectedGroup();
        selectionData.addSelectedGroup(annotationJoint.getId());
    }

    public void setMapView(MapView mapView) {
        if(this.mapView!=mapView){
            if(this.mapView!=null){
                this.mapView.onClickSymbolsListeners.removeListener(this);
            }
            mapView.onClickSymbolsListeners.addListener(this);
        }
        this.mapView = mapView;

    }
}
