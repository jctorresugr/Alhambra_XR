package com.alhambra.dataset.data;

import androidx.annotation.Nullable;

/**
 * Annotation ID
 */
public class AnnotationID {
    private final int m_layer;
    private final int m_index;

    public AnnotationID(int layer, int id) {
        this.m_layer = layer;
        this.m_index = id;
    }

    public int getLayer() {
        return m_layer;
    }

    public int getId() {
        return m_index;
    }

    @Override
    public boolean equals(@Nullable Object obj) {
        if(obj instanceof AnnotationID)
        {
            AnnotationID o = (AnnotationID) obj;
            return o.m_layer == m_layer && m_index ==o.m_index;
        }else if(obj instanceof Annotation)
        {
            Annotation a = (Annotation) obj;
            return a.id.m_layer == m_layer && m_index ==a.id.m_index;
        }
        return super.equals(obj);
    }

    @Override
    public int hashCode() {
        return m_layer | (m_index <<2);
    }

    @Override
    public String toString() {
        return "AID{" +
                "L" + m_layer +
                ", " + m_index +
                '}';
    }
}
