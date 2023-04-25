package com.alhambra.dataset.data;

import androidx.annotation.Nullable;

import java.util.HashSet;

public class Annotation {
    public AnnotationInfo info;
    public AnnotationRenderInfo renderInfo;
    public final AnnotationID id;
    public HashSet<AnnotationJoint> joints;

    public Annotation(AnnotationID id)
    {
        this.id=id;
        joints = new HashSet<>();
    }

    public void addAnnotationJoint(AnnotationJoint joint){
        joint.addAnnotation(this);
    }

    public void removeAnnotationJoint(AnnotationJoint joint){
        joint.removeAnnotation(this);
    }

    @Override
    public boolean equals(@Nullable Object obj) {
        if(obj instanceof Annotation)
        {
            Annotation a = (Annotation) obj;
            return a.id.equals(id);
        }else if (obj instanceof AnnotationID){
            AnnotationID aid = (AnnotationID) obj;
            return aid.equals(id);
        }
        return super.equals(obj);
    }

    @Override
    public int hashCode() {
        return id.hashCode();
    }
}
