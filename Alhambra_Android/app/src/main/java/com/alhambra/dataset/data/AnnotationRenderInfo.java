package com.alhambra.dataset.data;

import com.google.gson.annotations.SerializedName;
import com.sereno.color.Color;
import com.sereno.math.BBox;
import com.sereno.math.Vector3;

public class AnnotationRenderInfo {
    @SerializedName("color")
    private Color m_color;
    @SerializedName("Bounds")
    private BBox m_bounds;
    @SerializedName("normal")
    private Vector3 m_normal;

    public AnnotationRenderInfo(Color m_color, BBox m_bounds, Vector3 m_normal) {
        this.m_color = m_color;
        this.m_bounds = m_bounds;
        this.m_normal = m_normal;
    }

    public Color getColor() {
        return m_color;
    }

    public BBox getBounds() {
        return m_bounds;
    }

    public Vector3 getNormal() {
        return m_normal;
    }
}
