package com.alhambra.dataset.data;

import androidx.annotation.Nullable;

/**
 * Annotation ID
 */
public class AnnotationID {
    private final int layer;
    private final int id;

    public AnnotationID(int layer, int id) {
        this.layer = layer;
        this.id = id;
    }

    public int getLayer() {
        return layer;
    }

    public int getId() {
        return id;
    }

    @Override
    public boolean equals(@Nullable Object obj) {
        if(obj instanceof AnnotationID)
        {
            AnnotationID o = (AnnotationID) obj;
            return o.layer==layer && id==o.id;
        }
        return super.equals(obj);
    }
}
