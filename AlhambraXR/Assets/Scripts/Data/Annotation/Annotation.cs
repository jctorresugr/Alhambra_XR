using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public class Annotation
{
    // pure data
    [SerializeField]
    public AnnotationInfo info = null;
    [SerializeField]
    public AnnotationRenderInfo renderInfo = new AnnotationRenderInfo();

    [NonSerialized]
    internal List<AnnotationJoint> joints = new List<AnnotationJoint>();

    // control data
    public IReadOnlyList<AnnotationJoint> Joints => joints;

    [SerializeField]
    public AnnotationID ID
    {
        get;
    }

    public Annotation(AnnotationID _id)
    {
        ID = _id;
    }

    public void RemoveJoint(AnnotationJoint joint)
    {
        joint.RemoveAnnotation(this);
    }

    public void AddJoint(AnnotationJoint joint)
    {
        joint.AddAnnotation(this);
    }

    public AnnotationJoint FindJoint(int jointID)
    {
        return joints.Find(x => x.ID == jointID);
    }

    public bool HasJoint(int jointID)
    {
        return FindJoint(jointID) != null;
    }

    public bool IsValid
    {
        get => info != null && ID.IsValid;
    }

    

}
