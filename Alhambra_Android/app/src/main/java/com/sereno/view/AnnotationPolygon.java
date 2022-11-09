package com.sereno.view;

public class AnnotationPolygon extends AnnotationGeometry
{
    @Override
    public boolean isValid()
    {
        return getPoints().size() >= 3;
    }
}