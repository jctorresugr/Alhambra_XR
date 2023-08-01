package com.alhambra.experiment.task;

import com.alhambra.dataset.data.AnnotationID;

import java.util.ArrayList;

public class ExperimentTaskData {

    public static final int TEST_RIGHT=0;
    public static final int TEST_WRONG=1;
    public static final int TEST_ALREADY_EXIST=2;
    protected String description;
    protected ArrayList<AnnotationID> requiredAnnotation = new ArrayList<>();

    protected ArrayList<AnnotationID> finishedAnnotation = new ArrayList<>();
    protected ArrayList<AnnotationID> errorAnnotation = new ArrayList<>();

    protected static class TaskResponseRecord{
        public long time = System.currentTimeMillis();
        public ArrayList<AnnotationID> ids;
        public int flag;

        public TaskResponseRecord(ArrayList<AnnotationID> ids, int flag) {
            this.ids = ids;
            this.flag=flag;
        }
    }
    protected long startTime;
    protected ArrayList<TaskResponseRecord> operationRecord = new ArrayList<>();

    public boolean isFinished(){
        return finishedAnnotation.size()==requiredAnnotation.size();
    }

    public void markAsStart(){
        startTime = System.currentTimeMillis();
    }
    public int testChoice(AnnotationID annotationID){
        ArrayList<AnnotationID> ids = new ArrayList<>();
        ids.add(annotationID);
        return testChoices(ids);
    }

    public int testChoices(ArrayList<AnnotationID> annotationIDs){
        int existCount = 0;
        int errorCount = 0;
        int rightCount = 0;

        for(AnnotationID annotationID:annotationIDs){

            loopAnnotation:
            for (AnnotationID reqID : requiredAnnotation) {
                if(reqID.equals(annotationID)){
                    for(AnnotationID finID: finishedAnnotation){
                        if(finID.equals(annotationID)){
                            existCount++;
                            break loopAnnotation;
                        }
                    }
                    finishedAnnotation.add(annotationID);
                    rightCount++;
                    break loopAnnotation;
                }
            }
            errorCount++;
        }
        int response = 0;
        if(rightCount>0){
            response= TEST_RIGHT;
        }else if (existCount>0){
            response= TEST_ALREADY_EXIST;
        }else{
            response= TEST_WRONG;
        }
        operationRecord.add(new TaskResponseRecord(annotationIDs,response));
        return response;
    }

    public String getDescription() {
        return description;
    }

    public void setDescription(String description) {
        this.description = description;
    }

    public ArrayList<AnnotationID> getRequiredAnnotation() {
        return requiredAnnotation;
    }

    public void setRequiredAnnotation(ArrayList<AnnotationID> requiredAnnotation) {
        this.requiredAnnotation = requiredAnnotation;
    }

    public int getAlreadyFinished() {
        return finishedAnnotation.size();
    }

    public int getRequiredCount(){
        return requiredAnnotation.size();
    }


    public ArrayList<AnnotationID> getFinishedAnnotation() {
        return finishedAnnotation;
    }

    public void setFinishedAnnotation(ArrayList<AnnotationID> finishedAnnotation) {
        this.finishedAnnotation = finishedAnnotation;
    }



}
