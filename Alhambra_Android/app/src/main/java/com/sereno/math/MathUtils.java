package com.sereno.math;

public class MathUtils {

    public static float distanceSqr(float x0,float y0, float x1,float y1) {
        float dx = x0-x1;
        float dy = y0-y1;
        return dx*dx+dy*dy;
    }

    public static float distance(float x0,float y0, float x1,float y1) {
        return (float) Math.sqrt(distanceSqr(x0,y0,x1,y1));
    }
}
