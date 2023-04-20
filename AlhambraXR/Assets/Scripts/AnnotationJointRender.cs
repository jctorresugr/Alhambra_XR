using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manage Annotation Joint Object
/// </summary>
public class AnnotationJointRender : MonoBehaviour
{
    private AnnotationJoint data;
    private Dictionary<Annotation,GameObject> indicators;
    public GameObject templateIndicator;
    public ReferenceTransform refTrans;

    public float offsetRadius = 0.05f;

    public void Init(AnnotationJoint data, GameObject templateIndicator)
    {
        indicators = new Dictionary<Annotation, GameObject>();
        this.data = data;
        this.templateIndicator = templateIndicator;
        this.data.OnJointAddAnnotationEvent += OnAddAnnotation;
        this.data.OnJointRemoveAnnotationEvent += OnRemoveAnnotation;
        UpdatePosition();
    }

    private void OnAddAnnotation(AnnotationJoint aj, Annotation annotation)
    {
        OnAddAnnotation(annotation);
    }

    private void OnRemoveAnnotation(AnnotationJoint aj, Annotation annotation)
    {
        OnRemoveAnnotation(annotation);
    }

    public void OnAddAnnotation(Annotation annotation)
    {
        UpdatePosition();
        GameObject g = Instantiate(templateIndicator);
        g.transform.parent = transform;
        indicators.Add(annotation, g);
        UpdateIndicatorTransform(annotation);
    }

    public void OnRemoveAnnotation(Annotation annotation)
    {
        UpdatePosition();
        Destroy(indicators[annotation]);
        indicators.Remove(annotation);
    }

    public void OnDestroy()
    {
        if(data!=null)
        {
            data.OnJointAddAnnotationEvent -= OnAddAnnotation;
            data.OnJointRemoveAnnotationEvent -= OnRemoveAnnotation;
        }
    }

    public void UpdatePosition()
    {
        Vector3 newPos = refTrans.MapPosition(data.position);
        if(newPos== transform.position)
        {
            return;
        }
        transform.position = newPos;
        foreach(Annotation k in indicators.Keys)
        {
            UpdateIndicatorTransform(k);
        }
    }

    public void UpdateIndicatorTransform(Annotation annotation)
    {
        GameObject g = indicators[annotation];
        Vector3 lookAt = (refTrans.MapPosition(annotation.renderInfo.Center) - transform.position).normalized;
        g.transform.position = transform.position + lookAt * offsetRadius;
        g.transform.rotation = Quaternion.FromToRotation(Vector3.up, lookAt);
    }
}
