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
    public bool autoPosition = true;

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
        Debug.Log("Add annotation " + annotation.ID + " to joint " + ID);
        annotations.Add(annotation);
        if(annotations.Count==1)
        {
            range.SetMinMax(annotation.renderInfo.BoundingMin, annotation.renderInfo.BoundingMax);
        }
        else
        {
            range.Encapsulate(annotation.renderInfo.Bounds);
        }
        annotation.joints.Add(this);
        if(autoPosition)
        {
            position = range.center;
        }
        OnJointAddAnnotationEvent?.Invoke(this, annotation);
    }

    public void RemoveAnnotation(Annotation annotation)
    {
        bool result = annotations.Remove(annotation);
        if(result)
        {
            Debug.Log("Remove annotation " + annotation.ID + " from joint " + ID);

            //update range
            if (annotations.Count > 0)
            {
                range.SetMinMax(annotations[0].renderInfo.BoundingMin, annotations[0].renderInfo.BoundingMax);
                foreach(Annotation a in annotations)
                {
                    range.Encapsulate(a.renderInfo.Bounds);
                }
            }
                
            if (autoPosition)
            {
                position = range.center;
            }
            OnJointRemoveAnnotationEvent?.Invoke(this,annotation);
        }
    }
}
