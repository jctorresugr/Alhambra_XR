package com.alhambra.interactions;

import android.widget.RelativeLayout;

import com.alhambra.MainActivity;
import com.alhambra.interactions.GestureAnalyze.FingerState;
import com.sereno.math.Vector3;

public class MoveInteraction extends IInteraction implements GestureAnalyze.OnDoubleGestureDetect {
    public GestureAnalyze gestureAnalyze;
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

    }

    @Override
    public void doubleParallelMove(FingerState f0, FingerState f1) {
        float dx0 = f0.move.x - f0.lastMove.x;
        float dx1 = f1.move.x - f1.lastMove.x;
        float dy0 = f0.move.y - f0.lastMove.y;
        float dy1 = f1.move.y - f1.lastMove.y;
        float dxc = (dx0+dx1)*0.5f;
        float dyc = (dy0+dy1)*0.5f;
        mainActivity.sendServerAction("TabletControllerMove",new Vector3(-dyc,0.0f,dxc));
    }
}
