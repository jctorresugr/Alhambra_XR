package com.alhambra.interactions;

import android.util.Log;

import com.alhambra.MainActivity;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.dataset.data.AnnotationRenderInfo;
import com.alhambra.network.JSONUtils;
import com.google.gson.JsonElement;
import com.sereno.math.BBox;

import java.util.ArrayList;

//Same as C# DataSync.cs
public class DatasetSync extends IInteraction{

    private static final String TAG = DatasetSync.class.getSimpleName();

    public void OnReceiveSyncJoint(AnnotationDataset dataset, JsonElement jsonElement){
        AnnotationJoint joint = JSONUtils.gson.fromJson(jsonElement, AnnotationJoint.class);
        if(dataset.hasAnnotationJoint(joint.getId())) {
            Log.i("DatasetSync","Try to add duplication annotation joint "+joint.getId()+" replace its annotation links");
            AnnotationJoint oldJoint = dataset.getAnnotationJoint(joint.getId());
            oldJoint.removeAllAnnotation();
        }
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

    protected void reg(MainActivity activity){
        AnnotationDataset ad = mainActivity.getAnnotationDataset();
        mainActivity.regReceiveMessageListener("SyncAnnotationJoint", (mainActivity1, jsonElement) -> this.OnReceiveSyncJoint(ad, jsonElement));
        mainActivity.regReceiveMessageListener("AddAnnotationToJoint", (ma, je) -> this.onReceiveAddAnnotationToJoint(ad,je));
        mainActivity.regReceiveMessageListener("RemoveAnnotationFromJoint", (ma, je) -> this.onReceiveRemoveAnnotationFromJoint(ad,je));
        mainActivity.regReceiveMessageListener("AddAnnotationJoint", (ma, je) -> this.onReceiveAddAnnotationJoint(ad,je));
        mainActivity.regReceiveMessageListener("RemoveAnnotationJoint", (ma, je) -> this.onReceiveRemoveAnnotationJoint(ad,je));
        mainActivity.regReceiveMessageListener("UpdateAnnotationRenderInfo", (ma, je) -> this.onReceiveUpdateAnnotationRenderInfo(ad,je));
        mainActivity.regReceiveMessageListener("UpdateModelBounds", (ma, je) -> this.onReceiveUpdateModelBounds(ad,je));
    }

    private static final String LOG_TAG = "DatasetSync";

    // ===========================================================================
    // Sync functions

    public static class MessageAnnotationJointModify{
        public int jointID;
        public AnnotationID annotationID;

        public MessageAnnotationJointModify(int jointID, AnnotationID annotationID) {
            this.jointID = jointID;
            this.annotationID = annotationID;
        }
    }

    protected AnnotationJoint getAnnotationJoint(AnnotationDataset dataset, int jointID) {
        AnnotationJoint aj = dataset.getAnnotationJoint(jointID);
        if(aj==null){
            Log.w(LOG_TAG,"Unknown joint id: "+jointID);
        }
        return aj;
    }

    protected Annotation getAnnotation(AnnotationDataset dataset, AnnotationID annotationID) {
        Annotation a = dataset.getAnnotation(annotationID);
        if(a==null){
            Log.w(LOG_TAG,"Unknown annotation id: "+annotationID);
        }
        return a;
    }

    public void onReceiveAddAnnotationToJoint(AnnotationDataset dataset, JsonElement jsonElement){
        MessageAnnotationJointModify msg = JSONUtils.gson.fromJson(jsonElement, MessageAnnotationJointModify.class);
        Annotation annotation = getAnnotation(dataset, msg.annotationID);
        AnnotationJoint annotationJoint = getAnnotationJoint(dataset, msg.jointID);
        if(annotation==null || annotationJoint==null) {
            Log.w(LOG_TAG,"Failed: onReceiveAddAnnotationToJoint, null object");
            return;
        }
        annotationJoint.addAnnotation(annotation);
    }

    public void sendAddAnnotationToJoint(int jointID, AnnotationID annotationID) {
        MessageAnnotationJointModify msg = new MessageAnnotationJointModify(jointID,annotationID);
        mainActivity.sendServerAction("AddAnnotationToJoint",msg);
    }

    public void onReceiveRemoveAnnotationFromJoint(AnnotationDataset dataset, JsonElement jsonElement) {
        MessageAnnotationJointModify msg = JSONUtils.gson.fromJson(jsonElement, MessageAnnotationJointModify.class);
        Annotation annotation = getAnnotation(dataset, msg.annotationID);
        AnnotationJoint annotationJoint = getAnnotationJoint(dataset, msg.jointID);
        if(annotation==null || annotationJoint==null) {
            Log.w(LOG_TAG,"Failed: onReceiveRemoveAnnotationFromJoint, null object");
            return;
        }
        annotationJoint.removeAnnotation(annotation);
    }

    public void sendRemoveAnnotationFromJoint(int jointID, AnnotationID annotationID) {
        MessageAnnotationJointModify msg = new MessageAnnotationJointModify(jointID,annotationID);
        mainActivity.sendServerAction("RemoveAnnotationFromJoint",msg);
    }

    public void onReceiveRemoveAnnotationJoint(AnnotationDataset dataset, JsonElement jsonElement) {
        int msg = jsonElement.getAsInt();
        if(dataset.removeAnnotationJoint(msg)==null){
            Log.w(LOG_TAG,"Failed: onReceiveRemoveAnnotationJoint, null object");
        }
    }

    public void sendRemoveAnnotationJoint(AnnotationJoint annotationJoint) {
        mainActivity.sendServerAction("RemoveAnnotationJoint",annotationJoint);
    }

    public void onReceiveAddAnnotationJoint(AnnotationDataset dataset, JsonElement jsonElement) {
        AnnotationJoint msg = JSONUtils.gson.fromJson(jsonElement, AnnotationJoint.class);
        msg.syncAnnotations(dataset.getAnnotationList());
        dataset.addAnnotationJoint(msg);
    }

    public void sendAddAnnotationJoint(AnnotationJoint annotationJoint) {
        mainActivity.sendServerAction("AddAnnotationJoint",annotationJoint);
    }

    public static class MessageUpdateAnnotationRenderInfo{
        public AnnotationID id;
        public AnnotationRenderInfo annotationRenderInfo;
    }
    public void onReceiveUpdateAnnotationRenderInfo(AnnotationDataset dataset, JsonElement jsonElement){
        MessageUpdateAnnotationRenderInfo annotationRenderInfo = JSONUtils.gson.fromJson(jsonElement,MessageUpdateAnnotationRenderInfo.class);
        Annotation annotation = dataset.getAnnotation(annotationRenderInfo.id);
        if(annotation==null){
            Log.w(LOG_TAG, "Unknown annotation when try to update annotation render info: "+annotationRenderInfo.id);
            return;
        }
        if(annotationRenderInfo.annotationRenderInfo!=null){
            annotation.renderInfo=annotationRenderInfo.annotationRenderInfo;
            dataset.notifyAnnotationChange(annotation);
        }else{
            Log.w(LOG_TAG, "Null render info when try to update annotation render info: "+annotationRenderInfo.id);
        }
    }

    public void onReceiveUpdateModelBounds(AnnotationDataset dataset, JsonElement jsonElement){
        BBox bBox = JSONUtils.gson.fromJson(jsonElement,BBox.class);
        if(bBox!=null){
            dataset.setModelBounds(bBox);
        }
    }



}
