package com.alhambra.view.base;

import com.sereno.color.HSVColor;

public class DynamicHSVColor {
    public HSVColor targetColor = new HSVColor(1,1,1,1);
    public HSVColor curColor = new HSVColor(1,1,1,1);


    public void update(float time){
        curColor.interpolateTo(targetColor,Math.min(time*3.0f,1.0f));
    }

    public DynamicHSVColor() {
    }

    public DynamicHSVColor(HSVColor targetColor) {
        this.targetColor = targetColor;
    }
}
