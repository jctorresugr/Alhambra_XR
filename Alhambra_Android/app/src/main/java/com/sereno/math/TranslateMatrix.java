package com.sereno.math;

import android.graphics.Matrix;

public class TranslateMatrix {
    public float scaleX,scaleY;
    public float translateX,translateY;

    public TranslateMatrix(){
        scaleX=scaleY=1.0f;
        translateX=translateY=0.0f;
    }

    public Matrix computeMatrix(){
        Matrix m = new Matrix();
        m.preScale(scaleX,scaleY);
        m.preTranslate(translateX,translateY);
        return m;
    }

    public void transformPoint(float x,float y, float[] output) {
        output[0] = (x*scaleX)+translateX;
        output[1] = (y*scaleY)+translateY;
    }

    public float transformPointX(float x) {
        return (x*scaleX)+translateX;
    }

    public float transformPointY(float y) {
        return (y*scaleY)+translateY;
    }
}
