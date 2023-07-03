package com.alhambra.interactions;

import android.util.Log;
import android.view.MotionEvent;
import android.view.View;

import androidx.annotation.NonNull;
import androidx.core.view.MotionEventCompat;

import com.alhambra.ListenerSubscriber;

import java.util.ArrayList;
import java.util.HashMap;

//REFER:https://developer.android.com/develop/ui/views/touch-and-input/gestures/multi
public class GestureAnalyze implements View.OnTouchListener {

    @Override
    public boolean onTouch(View v, MotionEvent event) {
        supplyEvent(event);
        return false;
    }

    public interface OnDoubleGestureDetect{
        void doubleDown(FingerState f0,FingerState f1);
        void doubleParallelMove(FingerState f0,FingerState f1);
    }

    public final ListenerSubscriber<OnDoubleGestureDetect> subscriber = new ListenerSubscriber<>();

    private final HashMap<Integer,FingerState> states;
    private final ArrayList<FingerState> moveStates= new ArrayList<>();
    private final ArrayList<FingerState> downStates= new ArrayList<>();

    //settings

    //ms: two tap time gap tolerance
    public long doubleTapInterval = 500;
    public float doubleParallelMoveCosDegree = (float) Math.cos(Math.toRadians(45.0f));

    public static class FingerState{
        public MotionState down=null;
        MotionState checkMove=null;
        public MotionState lastMove=null;
        public MotionState move=null;
        public MotionState up=null;
        public int index;

    }
    public static class MotionState{
        float x,y;
        long time;
        public MotionState(MotionEvent e,int pointerIndex){
            //try {
                x = e.getX(pointerIndex);
                y = e.getY(pointerIndex);
                time = e.getEventTime();
            //}catch (IllegalArgumentException ignored){
                //???
            //}
        }

        public static MotionState getMotionState(MotionEvent e,int pointerIndex) {
            return new MotionState(e,pointerIndex);
        }

        public static MotionState getMotionState(MotionEvent e) {
            return new MotionState(e,0);
        }

        @NonNull
        @Override
        public String toString() {
            return "> x= "+x+"\t,y= "+y+"\t,t="+time;
        }
    }

    public GestureAnalyze(){
        states = new HashMap<>();
    }

    public void reg(View view){
        view.setOnTouchListener(this);
    }

    //TODO: resolve pointer
    public boolean supplyEvent(MotionEvent e) {
        boolean result = false;
        int pointerCount = e.getPointerCount();
        int index = e.getActionIndex();
        int action = e.getActionMasked();

        Log.i("GestureA", "pc:" + pointerCount + "\t ac:" + action + " \tind:" + index);
        int pointerId = e.getPointerId(0);
        int actionState=e.getAction();
        while(true) {
            switch (actionState) {
                case MotionEvent.ACTION_DOWN:
                case MotionEvent.ACTION_POINTER_DOWN:
                    FingerState fingerState = new FingerState();
                    fingerState.down = MotionState.getMotionState(e);
                    fingerState.index = pointerId;
                    states.put(fingerState.index, fingerState);
                    downStates.add(fingerState);
                    Log.i("GestureA", "Down \t" + fingerState.down);

                    //judge double tap
                    if (downStates.size() == 2) {
                        triggerDoubleDown(downStates.get(0), downStates.get(1));
                    } else if (downStates.size() == 1 && moveStates.size() == 1) {
                        FingerState oldState = moveStates.get(0);
                        if (fingerState.down.time - oldState.down.time < doubleTapInterval) {
                            triggerDoubleDown(oldState, fingerState);
                            result = true;
                        }
                    }
                    break;
                case MotionEvent.ACTION_MOVE:
                    for (int i = 0; i < pointerCount; i++) {
                        int pid = e.getPointerId(i);
                        FingerState moveState = states.get(pid);
                        if (moveState != null) {
                            if (moveState.move == null) {
                                downStates.remove(moveState);
                                moveStates.add(moveState);
                                moveState.checkMove = moveState.lastMove = moveState.down;
                            } else {
                                moveState.lastMove = moveState.move;
                            }
                            moveState.move = MotionState.getMotionState(e,i);
                            if (moveState.move.time - moveState.checkMove.time > 200) {
                                moveState.checkMove = moveState.lastMove;
                            }
                            Log.i("GestureA", "<"+pid+">Move \t" + moveState.move);
                        }
                    }


                    //judge double move (parallel move)
                    if (moveStates.size() == 2) {
                        FingerState move0 = moveStates.get(0);
                        FingerState move1 = moveStates.get(1);
                        float dx0 = move0.move.x - move0.checkMove.x;
                        float dx1 = move1.move.x - move1.checkMove.x;
                        float dy0 = move0.move.y - move0.checkMove.y;
                        float dy1 = move1.move.y - move1.checkMove.y;
                        float dis0 = (float) Math.sqrt(dx0 * dx0 + dy0 * dy0);
                        float dis1 = (float) Math.sqrt(dx1 * dx1 + dy1 * dy1);
                        float cosDeg = (float) ((dx0 * dx1 + dy0 * dy1) / (dis0 * dis1));
                        if (cosDeg > doubleParallelMoveCosDegree) {
                            triggerDoubleMove(move0, move1);
                            result = true;
                        }
                    }
                    break;
                case MotionEvent.ACTION_UP:
                case MotionEvent.ACTION_POINTER_UP:
                    FingerState upState = states.get(pointerId);
                    if (upState != null) {
                        moveStates.remove(upState);
                        upState.up = MotionState.getMotionState(e);
                        Log.i("GestureA", "Up   \t" + upState.up);
                        states.remove(upState.index);
                    }

                    break;
            }
            int newState = e.getActionMasked();
            if(newState!=actionState){
                actionState=newState;
                pointerId = e.getPointerId(e.getActionIndex());
            }else{
                break;
            }
        }
        return result;
    }


    public void triggerDoubleDown(FingerState f0, FingerState f1){
        Log.i("GestureA","Down!");
        subscriber.invoke(l->l.doubleDown(f0,f1));
    }

    public void triggerDoubleMove(
            FingerState f0, FingerState f1){
        Log.i("GestureA","Parallel Move!");
        subscriber.invoke(l->l.doubleParallelMove(f0,f1));
    }
}
