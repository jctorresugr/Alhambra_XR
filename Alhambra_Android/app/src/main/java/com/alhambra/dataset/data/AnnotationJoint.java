package com.alhambra.dataset.data;

import com.sereno.math.BBox;
import com.sereno.math.Vector3;

import java.util.ArrayList;
import java.util.Collections;
import java.util.HashSet;
import java.util.Iterator;
import java.util.List;

public class AnnotationJoint {
    private Vector3 position;
    private BBox range;
    private String name;
    private int id;

    private transient HashSet<Annotation> annotations = new HashSet<>();
    private final ArrayList<AnnotationID> annotationsID = new ArrayList<>();

    public AnnotationJoint(int id, String name)
    {
        this.id=id;
        this.name=name;
    }

    public boolean HasAnnotation(Annotation annot){
        return annotations.contains(annot);
    }

    public List<AnnotationID> getAnnotationsID(){
        return Collections.unmodifiableList(annotationsID);
    }



    public void addAnnotation(Annotation annotation){
        if(HasAnnotation(annotation)){
            return;
        }
        annotations.add(annotation);
        annotationsID.add(annotation.id);
        annotation.joints.add(this);
    }

    public void removeAnnotation(Annotation annotation){
        annotations.remove(annotation);
        annotationsID.remove(annotation.id);
        annotation.joints.remove(this);
    }

    public void removeAllAnnotation() {
        for(Annotation a:annotations) {
            a.removeAnnotationJoint(this);
        }
        annotations.clear();
        annotationsID.clear();
    }

    private boolean hasAnnotationID(AnnotationID id) {
        return annotationsID.contains(id);
    }

    // invoke this after deserialize
    public void syncAnnotations(List<Annotation> annotations){
        this.annotations = new HashSet<Annotation>();
        for(Annotation annot: annotations) {
            if(hasAnnotationID(annot.id)) {
                this.annotations.add(annot);
                if(!annot.hasJoint(this)){
                    annot.joints.add(this);
                }
            }
        }
    }

    public int getId() {
        return id;
    }

    public Vector3 getPosition() {
        return position;
    }

    public BBox getRange() {
        return range;
    }

    public String getName() {
        return name;
    }
}
