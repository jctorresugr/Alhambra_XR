package com.alhambra.fragment;

import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.Button;

import androidx.annotation.NonNull;
import androidx.annotation.Nullable;

import com.alhambra.R;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.UserData;
import com.alhambra.view.MapView;

import java.util.ArrayList;

public class OverviewFragment extends AlhambraFragment {

    private Button m_showAllBtn;
    private Button m_stopShowAllBtn;
    private Button m_beginTaskBtn;
    private Button m_switchSceneBtn;
    private MapView m_mapView;
    private AnnotationDataset annotationDataset;
    private UserData userData;
    private SelectionData selectionData;
    public OverviewFragment() {
        super();
    }

    public interface OverviewFragmentListener
    {
        void showAllAnnotation(OverviewFragment frag);
        void stopShowAllAnnotation(OverviewFragment frag);
        void onOverViewUIInit(OverviewFragment frag);
        void onBeginTask(OverviewFragment frag);
        void onSwitchScene(OverviewFragment frag);
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
        m_beginTaskBtn = v.findViewById(R.id.beginTaskButton);
        m_mapView = v.findViewById(R.id.annotationMapView2);
        m_switchSceneBtn = v.findViewById(R.id.switchSceneButton);
        m_mapView.setUserData(userData);
        m_mapView.setSelectionData(selectionData);
        m_mapView.setDataset(annotationDataset);
        m_mapView.impedeEvents=true;
        m_showAllBtn.setOnClickListener(view -> {
            for(OverviewFragmentListener l:m_listeners) {
                l.showAllAnnotation(this);
            }
        });
        m_stopShowAllBtn.setOnClickListener(view->{
            for(OverviewFragmentListener l:m_listeners) {
                l.stopShowAllAnnotation(this);
            }
        });
        m_beginTaskBtn.setOnClickListener(view->{
            for(OverviewFragmentListener l:m_listeners) {
                l.onBeginTask(this);
            }
        });
        m_switchSceneBtn.setOnClickListener(view->{
            for(OverviewFragmentListener l:m_listeners) {
                l.onSwitchScene(this);
            }
        });

        return v;
    }

    @Override
    public void onViewCreated(@NonNull View view, @Nullable Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        for(OverviewFragmentListener l:m_listeners) {
            l.onOverViewUIInit(this);
        }
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

    public void setSelectionData(SelectionData selectionData) {
        this.selectionData=selectionData;
        if(m_mapView!=null){
            m_mapView.setSelectionData(selectionData);
        }
    }

    public UserData getUserData() {
        return userData;
    }

    public void setUserData(UserData userData) {
        this.userData = userData;
        if(m_mapView!=null){
            m_mapView.setUserData(userData);
        }
    }

    public MapView getMapView() {
        return m_mapView;
    }
}
