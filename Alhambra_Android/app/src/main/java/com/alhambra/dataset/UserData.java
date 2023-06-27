package com.alhambra.dataset;

import com.alhambra.ListenerSubscriber;
import com.sereno.math.Vector3;

//Data for interaction
public class UserData {

    public interface OnUserDataChangeListener
    {
        void onUserDataChange(UserData data);
    }
    private Vector3 position = Vector3.getZero();
    private Vector3 rotation = Vector3.getZero();

    public ListenerSubscriber<OnUserDataChangeListener> dataChangeListeners = new ListenerSubscriber<>();

    public Vector3 getPosition() {
        return position;
    }

    public void setPosition(Vector3 position) {
        this.position = position;
        triggerDataChange();
    }

    public Vector3 getRotation() {
        return rotation;
    }

    public void setRotation(Vector3 rotation) {
        this.rotation = rotation;
        triggerDataChange();
    }

    public void triggerDataChange(){
        dataChangeListeners.invoke(l->l.onUserDataChange(this));
    }
}
