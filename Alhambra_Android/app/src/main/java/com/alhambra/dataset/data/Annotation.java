package com.alhambra.dataset.data;

import androidx.annotation.Nullable;

import java.util.Collections;
import java.util.HashSet;
import java.util.Set;

public class Annotation {
    public AnnotationInfo info;
    public AnnotationRenderInfo renderInfo;
    public final AnnotationID id;
    transient HashSet<AnnotationJoint> joints;

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

    public Set<AnnotationJoint> getAnnotationJoints(){
        return Collections.unmodifiableSet(joints);
    }

    @Override
    public int hashCode() {
        return id.hashCode();
    }

    public boolean hasJoint(AnnotationJoint aj) {
        return joints.contains(aj);
    }
}
