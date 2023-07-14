package com.alhambra.interactions;

import android.widget.RelativeLayout;

import com.alhambra.MainActivity;
import com.alhambra.interactions.GestureAnalyze.FingerState;
import com.sereno.math.Vector3;

import java.util.ArrayList;

public class MoveInteraction extends IInteraction implements GestureAnalyze.OnDoubleGestureDetect {
    public GestureAnalyze gestureAnalyze;

    private int state = STATE_NORMAL;
    private static final int STATE_NORMAL=0;
    private static final int STATE_TWO=1;
    private static final int STATE_THREE=2;


    @Override
    protected void reg(MainActivity mainActivity) {
        gestureAnalyze = new GestureAnalyze();
        RelativeLayout globalRelativeLayout = mainActivity.getGlobalRelativeLayout();
        globalRelativeLayout.setClickable(true);
        gestureAnalyze.reg(globalRelativeLayout);
        gestureAnalyze.subscriber.addListener(this);
    }

    @Override
    public void doubleDown(FingerState f0, FingerState f1) {
        setState(STATE_TWO);
    }

    @Override
    public void doubleParallelMove(FingerState f0, FingerState f1) {
        setState(STATE_TWO);
        float dx0 = f0.move.x - f0.lastMove.x;
        float dx1 = f1.move.x - f1.lastMove.x;
        float dy0 = f0.move.y - f0.lastMove.y;
        float dy1 = f1.move.y - f1.lastMove.y;
        float dxc = (dx0+dx1)*0.5f;
        float dyc = (dy0+dy1)*0.5f;
        mainActivity.sendServerAction("TabletControllerMove",new Vector3(-dyc,0.0f,dxc));
    }

    @Override
    public void multipleDown(int downCount, ArrayList<FingerState> downStates, ArrayList<FingerState> moveStates) {
        if(downCount==3){
            setState(STATE_THREE);
        }
    }

    @Override
    public void multipleMove(int moveCount, ArrayList<FingerState> moveStates) {
        if(moveCount==3){
            setState(STATE_THREE);
            float totalDx = 0.0f;
            float totalDy = 0.0f;
            for(FingerState f: moveStates){
                float dx = f.move.x-f.lastMove.x;
                float dy = f.move.y-f.lastMove.y;
                totalDx+=dx;
                totalDy+=dy;
            }
            totalDx/=3.0f;
            totalDy/=3.0f;
            mainActivity.sendServerAction("TabletControllerMove",new Vector3(0.0f,-totalDy,0.0f));
        }
    }

    @Override
    public void multipleUp(FingerState f0, int leftFingers) {
        if(leftFingers==3){
            setState(STATE_THREE);
        }else if (leftFingers==2){
            setState(STATE_TWO);
        }else if (leftFingers<2){
            setState(STATE_NORMAL);
        }
    }
    
    public void setState(int state){
        this.state=state;
    }
}
