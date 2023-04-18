package com.alhambra.dataset.data;

import com.sereno.math.BBox;
import com.sereno.math.Vector3;

import java.util.HashSet;

public class AnnotationJointData {
    private final HashSet<AnnotationInfo> annotations = new HashSet<>();
    public Vector3 position;
    public BBox bbox;
    public String name;
}
