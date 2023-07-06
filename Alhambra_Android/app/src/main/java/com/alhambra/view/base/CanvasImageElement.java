package com.alhambra.view.base;

import android.graphics.Canvas;
import android.graphics.drawable.Drawable;

public class CanvasImageElement extends CanvasBaseElement{

    protected Drawable drawable;
    public void setDrawable(Drawable newDrawable){
        if(drawable==newDrawable){
            return;
        }
        this.drawable=newDrawable;
    }

    @Override
    public void draw(Canvas canvas) {
        if(drawable!=null){
            drawable.draw(canvas);
        }

    }

    @Override
    public boolean isInRange(float x0, float y0) {
        return false;
    }
}
