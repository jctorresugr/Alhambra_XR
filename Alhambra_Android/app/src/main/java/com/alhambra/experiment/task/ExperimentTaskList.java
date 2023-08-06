package com.alhambra.experiment.task;

import com.alhambra.dataset.data.AnnotationID;
import com.alhambra.experiment.ExperimentDataCollection;

import java.util.ArrayList;

public class ExperimentTaskList {

    public ArrayList<ExperimentTaskData> taskData = new ArrayList<>();

    protected transient int currentTaskIndex = 0;

    public ExperimentTaskData getCurrentTaskData(){
        if(currentTaskIndex <0|| currentTaskIndex >=taskData.size()){
            return null;
        }
        return taskData.get(currentTaskIndex);
    }

    public int totalTaskCount(){
        return taskData.size();
    }

    public int getCurrentTaskIndex(){
        return currentTaskIndex;
    }


    public int testChoice(AnnotationID annotationID){
        ExperimentTaskData currentTask = getCurrentTaskData();
        int result = currentTask.testChoice(annotationID);
        validCurrentTask();
        return result;
    }

    public int testChoices(ArrayList<AnnotationID> annotationIDs){
        ExperimentTaskData currentTask = getCurrentTaskData();
        int result = currentTask.testChoices(annotationIDs);
        validCurrentTask();
        return result;
    }

    protected void validCurrentTask(){
        ExperimentTaskData currentTask = getCurrentTaskData();
        if(currentTask.isFinished()){
            currentTaskIndex++;
            markStart();
            ExperimentDataCollection.add("experiment_middle_save",currentTask);
        }
    }

    public void markStart(){
        ExperimentTaskData currentTask = getCurrentTaskData();
        if(currentTask!=null){
            currentTask.markAsStart();
        }
    }

    public boolean isFinished(){
        return currentTaskIndex>=taskData.size();
    }

}
