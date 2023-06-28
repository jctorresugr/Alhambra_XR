package com.alhambra.dataset.data;

import com.google.gson.annotations.SerializedName;
import com.sereno.color.Color;
import com.sereno.math.BBox;
import com.sereno.math.Vector3;

public class AnnotationRenderInfo {
    private Color color;
    @SerializedName("Bounds")
    private BBox bounds;
    private Vector3 normal;

    private Vector3 tangent;

    private Vector3 averagePosition;

    private int pointCount;

    public Color getColor() {
        return color;
    }

    public BBox getBounds() {
        return bounds;
    }

    public Vector3 getNormal() {
        return normal;
    }

    public Vector3 getTangent() {
        return tangent;
    }

    public Vector3 getAveragePosition() {
        return averagePosition;
    }

    public int getPointCount() {
        return pointCount;
    }
}
