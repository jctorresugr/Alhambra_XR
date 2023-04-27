package com.alhambra.controls;

import android.util.Log;

import com.alhambra.MainActivity;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.network.JSONUtils;
import com.google.gson.JsonElement;

import java.util.ArrayList;

public class DatasetSync {

    private static final String TAG = DatasetSync.class.getSimpleName();
    public MainActivity mainActivity;

    public void OnReceiveSyncJoint(AnnotationDataset dataset, JsonElement jsonElement){
        AnnotationJoint joint = JSONUtils.gson.fromJson(jsonElement, AnnotationJoint.class);
        ArrayList<Annotation> annotations = new ArrayList<>();
        for(AnnotationID annotationID: joint.getAnnotationsID()){
            Annotation annotation = dataset.getAnnotation(annotationID);
            if(annotation==null){
                Log.w(TAG,"Joint require Annotation "+annotationID+" which cannot be found in dataset");
                continue;
            }
            annotations.add(annotation);
        }
        joint.syncAnnotations(annotations);
        dataset.addAnnotationJoint(joint);
    }

    public void reg(MainActivity activity){
        mainActivity=activity;
        mainActivity.regReceiveMessageListener("SyncAnnotationJoint",
                (mainActivity1, jsonElement) -> this.OnReceiveSyncJoint(mainActivity.getAnnotationDataset(), jsonElement));
    }
}
