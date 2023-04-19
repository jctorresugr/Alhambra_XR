using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class AnnotationJoint
{
    public delegate void AnnotationAndJointChangeFunc(AnnotationJoint annotationJoint, Annotation annotation);

    // pure data
    public Vector3 position;
    public Bounds range;
    public string name;
    private int m_id;
    private List<Annotation> annotations = new List<Annotation>();

    // control data
    public event AnnotationAndJointChangeFunc OnJointAddAnnotationEvent;
    public event AnnotationAndJointChangeFunc OnJointRemoveAnnotationEvent;
    public IReadOnlyList<Annotation> Annotations => annotations;

    public int ID
    {
        get => m_id;
    }

    public AnnotationJoint(int id, string name)
    {
        m_id = id;
        this.name = name;
    }

    public Annotation FindAnnotation(AnnotationID annotationID)
    {
        return annotations.Find(x => x.ID == annotationID);
    }

    public bool HasAnnotation(AnnotationID annotationID)
    {
        return FindAnnotation(annotationID) != null;
    }

    public void AddAnnotation(Annotation annotation)
    {
        if(HasAnnotation(annotation.ID))
        {
            return;
        }
        annotations.Add(annotation);
        range.Encapsulate(annotation.renderInfo.Bounds);
        annotation.joints.Add(this);
        OnJointAddAnnotationEvent?.Invoke(this, annotation);
    }

    public void RemoveAnnotation(Annotation annotation)
    {
        bool result = annotations.Remove(annotation);
        if(result)
        {
            OnJointRemoveAnnotationEvent?.Invoke(this,annotation);
        }
    }
}
