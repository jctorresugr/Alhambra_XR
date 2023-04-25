package com.alhambra.dataset.data;

import com.sereno.math.BBox;
import com.sereno.math.Vector3;

import java.util.ArrayList;
import java.util.HashSet;

public class AnnotationJoint {
    public Vector3 position;
    public BBox range;
    public String name;
    private int id;

    private final HashSet<Annotation> annotations = new HashSet<>();
    private ArrayList<AnnotationID> annotationsID = new ArrayList<>();

    public AnnotationJoint(int id, String name)
    {
        this.id=id;
        this.name=name;
    }

    public boolean HasAnnotation(Annotation annot){
        return annotations.contains(annot);
    }

    public void addAnnotation(Annotation annotation){
        if(HasAnnotation(annotation)){
            return;
        }
        annotations.add(annotation);
        annotationsID.add(annotation.id);
    }

    public void removeAnnotation(Annotation annotation){
        annotations.remove(annotation);
        annotationsID.remove(annotation.id);
    }
}
