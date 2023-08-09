package com.alhambra.experiment.task;

import android.widget.Toast;

import com.alhambra.MainActivity;
import com.alhambra.Utils;
import com.alhambra.dataset.AnnotationDataset;
import com.alhambra.dataset.SelectionData;
import com.alhambra.dataset.data.Annotation;
import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.dataset.data.AnnotationJoint;
import com.alhambra.experiment.ExperimentDataCollection;
import com.alhambra.fragment.OverviewFragment;
import com.alhambra.interactions.IInteraction;
import com.alhambra.network.JSONUtils;
import com.alhambra.network.NetworkJsonParser;
import com.alhambra.network.receivingmsg.SelectionMessage;
import com.google.gson.JsonElement;
import com.sereno.math.BBox;

import java.io.File;
import java.util.ArrayList;
import java.util.HashSet;

public class ExperimentTaskInteraction extends IInteraction
        implements OverviewFragment.OverviewFragmentListener {

    protected ExperimentTaskList taskList;
    protected ExperimentTaskWindow window;

    public static class ScreenTextInfo{
        public String text;
        public float time;

        public ScreenTextInfo(String text, float time) {
            this.text = text;
            this.time = time;
        }
    }
    @Override
    protected void reg(MainActivity mainActivity) {
        taskList = JSONUtils.gson.fromJson(Utils.readWholeString(
                new File(mainActivity.getExternalFilesDir(null),"task.json"))
                ,ExperimentTaskList.class);
        window = new ExperimentTaskWindow(mainActivity);
        window.setExperimentTaskList(taskList);
        window.updateText();
        mainActivity.getOverviewFragment().addListener(this);
    }

    public void onSelection(int[] indexArr){
        if(taskList.isFinished() || !taskList.isStarted()){
            return;
        }
        AnnotationDataset annotationDataset = mainActivity.getAnnotationDataset();
        ArrayList<Annotation> annotations = annotationDataset.getAnnotations(indexArr);
        ArrayList<AnnotationID> annotationIDs = new ArrayList<AnnotationID>();
        for(Annotation a : annotations){
            annotationIDs.add(a.id);
        }
        int result = taskList.testChoices(annotationIDs);
        String showInfo = null;
        switch (result){
            case ExperimentTaskData.TEST_RIGHT:
                showInfo = "Correct!";
                break;
            case ExperimentTaskData.TEST_WRONG:
                showInfo = "wrong";
                break;
            case ExperimentTaskData.TEST_ALREADY_EXIST:
                showInfo = "Already selected!";
                break;
        }
        if(showInfo!=null){
            mainActivity.sendServerAction("ScreenTextTime", new ScreenTextInfo(showInfo,3.0f));
        }
        window.updateText();

        if(taskList.isFinished()){
            ExperimentDataCollection.add("experiment_result",taskList);
        }
    }

    @Override
    public void showAllAnnotation(OverviewFragment frag) {

    }

    @Override
    public void stopShowAllAnnotation(OverviewFragment frag) {

    }

    @Override
    public void onOverViewUIInit(OverviewFragment frag) {

    }

    @Override
    public void onBeginTask(OverviewFragment frag) {
        if(window.isShown()){
            taskList.forceNext();
            Toast.makeText(frag.getContext(),"Force next task "+ taskList.getCurrentTaskIndex(),Toast.LENGTH_SHORT).show();
            window.updateText();
        }else{
            window.show();
            window.updateText();
            taskList.markStart();
        }

    }

    @Override
    public void onSwitchScene(OverviewFragment frag) {

    }
}
