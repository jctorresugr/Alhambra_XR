package com.alhambra.interactions;

import com.alhambra.MainActivity;

public abstract class IInteraction {
    protected MainActivity mainActivity;
    public void regNetworkIO(MainActivity mainActivity) {
        this.mainActivity=mainActivity;
        reg(mainActivity);
    }
    abstract protected void reg(MainActivity mainActivity);
}
