package com.alhambra.experiment.task;

import android.content.Context;
import android.util.AttributeSet;
import android.view.LayoutInflater;
import android.widget.TextView;

import androidx.constraintlayout.widget.ConstraintLayout;

import com.alhambra.R;
import com.alhambra.view.base.DragViewLayout;

public class ExperimentTaskWindow extends DragViewLayout {
    public ExperimentTaskWindow(Context context) {
        super(context);
        init(context);
    }

    public ExperimentTaskWindow(Context context, AttributeSet attrs) {
        super(context, attrs);
        init(context);
    }

    public ExperimentTaskWindow(Context context, AttributeSet attrs, int defStyleAttr) {
        super(context, attrs, defStyleAttr);
        init(context);
    }

    protected TextView taskText;
    protected ExperimentTaskList experimentTaskList;

    protected void init(Context context){
        LayoutInflater inflater = LayoutInflater.from(context);
        ConstraintLayout constraintLayout = (ConstraintLayout) inflater.inflate(R.layout.float_window_experiment_task,null,false);
        addView(constraintLayout);
        taskText = findViewById(R.id.taskDescriptionTextView);
    }

    public void updateText(){
        ExperimentTaskData currentTaskData = experimentTaskList.getCurrentTaskData();
        if(currentTaskData==null){
            taskText.setText("Finish All tasks! Total tasks:"+experimentTaskList.totalTaskCount());
        }else{
            ExperimentTaskData currentTask = experimentTaskList.getCurrentTaskData();
            taskText.setText(
                    "Task No."+ +experimentTaskList.getCurrentTaskIndex()+"  |  (Total:"+ experimentTaskList.totalTaskCount()+") \n"+
                            "Require Annotations: ("+ currentTask.getAlreadyFinished()+" / "+currentTask.getRequiredCount()+") \n"
                            +currentTaskData.description
            );
        }
    }

    public ExperimentTaskList getExperimentTaskList() {
        return experimentTaskList;
    }

    public void setExperimentTaskList(ExperimentTaskList experimentTaskList) {
        this.experimentTaskList = experimentTaskList;
    }
}
