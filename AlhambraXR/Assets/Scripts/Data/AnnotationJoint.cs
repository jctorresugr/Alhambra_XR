using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AnnotationJoint
{
    public Vector3 position;
    public Bounds range;
    public string name;
    protected HashSet<Annotation> annotations;
    private int m_id;
    public int ID
    {
        get => m_id;
    }

    public AnnotationJoint(int id, string name)
    {
        m_id = id;
        this.name = name;
    }

    public void AddAnnotation(Annotation annotation)
    {
        annotations.Add(annotation);
        range.Encapsulate(annotation.renderInfo.Bounds);
    }
}
