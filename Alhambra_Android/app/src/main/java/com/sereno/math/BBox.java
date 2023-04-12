package com.sereno.math;

/**
 * Bounding box
 */
public class BBox {
    public Vector3 min;
    public Vector3 max;

    public void addPoint(float x, float y, float z)
    {
        min.x = Math.min(min.x,x);
        min.y = Math.min(min.y,y);
        min.z = Math.min(min.z,z);

        max.x = Math.max(max.x,x);
        max.y = Math.max(max.y,y);
        max.z = Math.max(max.z,z);
    }

    public void addPoint(Vector3 v){
        addPoint(v.x,v.y,v.z);
    }
}
