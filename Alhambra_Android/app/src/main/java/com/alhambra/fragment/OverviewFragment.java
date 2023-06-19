package com.alhambra.fragment;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;
import androidx.fragment.app.Fragment;

import com.alhambra.R;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.view.MapView;

import java.util.ArrayList;

public class OverviewFragment extends AlhambraFragment {

    private Button m_showAllBtn;
    private Button m_stopShowAllBtn;
    private MapView m_mapView;
    private AnnotationDataset annotationDataset;
    public OverviewFragment() {
        super();
    }

    public interface OverviewFragmentListener
    {
        void showAllAnnotation(OverviewFragment frag);
        void stopShowAllAnnotation(OverviewFragment frag);
    }

    private ArrayList<OverviewFragmentListener> m_listeners = new ArrayList<>();


    /** @brief Add a new listener
     * @param l the new listener*/
    public void addListener(OverviewFragmentListener l)
    {
        m_listeners.add(l);
    }

    /** @brief Remove an old listener
     * @param l the listener to remove*/
    public void removeListener(OverviewFragmentListener l)
    {
        m_listeners.remove(l);
    }

    @Nullable
    @Override
    public View onCreateView(@NonNull LayoutInflater inflater, @Nullable ViewGroup container, @Nullable Bundle savedInstanceState) {
        View v = inflater.inflate(R.layout.overview_fragment,container,false);
        m_showAllBtn = v.findViewById(R.id.showAllBtn);
        m_stopShowAllBtn = v.findViewById(R.id.stopShowAllBtn);
        m_mapView = v.findViewById(R.id.annotationMapView2);
        m_mapView.setDataset(annotationDataset);
        m_showAllBtn.setOnClickListener(view -> {
            //TODO: add show all
            for(OverviewFragmentListener l:m_listeners) {
                l.showAllAnnotation(this);
            }
        });
        m_stopShowAllBtn.setOnClickListener(view->{
            for(OverviewFragmentListener l:m_listeners) {
                l.stopShowAllAnnotation(this);
            }
        });
        return v;
    }

    public AnnotationDataset getAnnotationDataset() {
        return annotationDataset;
    }

    public void setAnnotationDataset(AnnotationDataset annotationDataset) {
        this.annotationDataset = annotationDataset;
        if(m_mapView!=null){
            m_mapView.setDataset(annotationDataset);
        }
    }
}
