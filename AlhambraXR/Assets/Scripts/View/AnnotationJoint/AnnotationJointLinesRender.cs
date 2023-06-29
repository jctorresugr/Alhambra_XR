using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationJointLinesRender : AnnotationJointRenderBase
{
    public NavigationCache navigateCacheData;
    public AnnotationJoint data;

    public override void Init(AnnotationJoint data)
    {
        Utils.EnsureComponent(this, ref navigateCacheData);
        Utils.EnsureComponentIncludeChild(
            this.gameObject,ref navigateCacheData.render);
        this.data = data;
        navigateCacheData.rootPos = this.transform;
        data.OnJointAddAnnotationEvent+=OnDataChange;
        data.OnJointRemoveAnnotationEvent+=OnDataChange;
        ComputeData();
        navigateCacheData.Hide();
    }

    public void OnDataChange(AnnotationJoint annotationJoint, Annotation annotation)
    {
        ComputeData();
        navigateCacheData.Hide();
    }

    public void ComputeData()
    {
        navigateCacheData.ClearCache();
        navigateCacheData.data = new List<Annotation>(data.Annotations);
        navigateCacheData.GenerateNavigationInfo();
        navigateCacheData.Redraw();
    }

    public void OnDestroy()
    {
        if (data != null)
        {
            data.OnJointAddAnnotationEvent -= OnDataChange;
            data.OnJointRemoveAnnotationEvent -= OnDataChange;
        }
    }

}
