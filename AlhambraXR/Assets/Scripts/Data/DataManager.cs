using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Global Data management, all annotation data stores here
/// </summary>
public class DataManager : MonoBehaviour
{
    public List<Annotation> annotations;
    public List<AnnotationJoint> annotationJoints;
    private bool isInited = false;

    public void Init()
    {
        if(isInited)
        {
            return;
        }
        isInited = true;
        annotationJoints = new List<AnnotationJoint>();
        annotations = new List<Annotation>();
    }

    public void Awake()
    {
        Init();
    }

    public Annotation FindID(AnnotationID id)
    {
        return annotations.Find(x => x.ID == id);
    }

    public Annotation AddAnnotation(AnnotationID id)
    {
        Annotation annot = FindID(id);
        if (annot==null)
        {
            annot = new Annotation(id);
            annotations.Add(annot);
            return annot;
        }
        return null;
    }

    // compatible with old code
    public void AddAnnoationRenderInfo(AnnotationRenderInfo renderInfo)
    {
        AnnotationID id = new AnnotationID(renderInfo.Color);
        Annotation annot = FindID(id);
        if (annot==null)
        {
            Annotation newAnnotation = new Annotation(id);
            newAnnotation.renderInfo = renderInfo;
            newAnnotation.info = new AnnotationInfo(renderInfo.Color, new byte[4], 1, 1, "Unknown annotation");
            annotations.Add(newAnnotation);
            Debug.Log("Add annotation (render) " + id);
        }
        else
        {
            annot.renderInfo = renderInfo;
        }
    }

    public void AddAnnotationInfo(AnnotationInfo info)
    {
        AnnotationID id = new AnnotationID(info.Color);
        Annotation annot = FindID(id);
        if (annot == null)
        {
            Annotation newAnnotation = new Annotation(id);
            newAnnotation.info = info;
            newAnnotation.renderInfo = new AnnotationRenderInfo();
            annotations.Add(newAnnotation);
            Debug.Log("Add annotation (info) " + id);
        }
        else
        {
            annot.info = info;
        }
    }

    
}
