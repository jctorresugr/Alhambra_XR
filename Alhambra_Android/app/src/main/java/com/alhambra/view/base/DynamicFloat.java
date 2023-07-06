package com.alhambra.view.base;

import com.sereno.math.MathUtils;

public class DynamicFloat {
    public float currentValue;
    public float targetValue;

    public void update(float time){
        currentValue = MathUtils.interpolate(currentValue,targetValue,Math.min(time*10.0f,1.0f));
    }

    public DynamicFloat() {
    }

    public DynamicFloat(float targetValue) {
        this.targetValue = targetValue;
    }
}
