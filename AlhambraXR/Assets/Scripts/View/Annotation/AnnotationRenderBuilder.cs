using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationRenderBuilder : MonoBehaviour
{
    public DataManager data;
    /// <summary>
    /// Assign an gameobject to represent it as a Annotation Joint
    /// </summary>
    public AnnotationRender template;
    public ReferenceTransform referenceTransform;

    private Dictionary<AnnotationID, AnnotationRender> annotationUIs;

    private bool isInited = false;
    public void Init()
    {
        if (isInited)
        {
            return;
        }
        if (template == null)
        {
            Debug.LogWarning("AnnotationJointRenderBuilder requires a template to generate visual AnnotationJoint");
        }
        else if (data == null)
        {
            Debug.LogWarning("Require data manager to receive data change notification");
        }
        else
        {
            isInited = true;
            data.OnAnnotationAddEvent += AddAnnotationUI;
            data.OnAnnotationRemoveEvent += RemoveAnnotationUI;
            annotationUIs = new Dictionary<AnnotationID, AnnotationRender>();
        }
    }

    public void DrawAllAnnotation()
    {
        foreach (var a in data.Annotations)
        {
            AddAnnotationUI(a);
        }
    }

    public void AddAnnotationUI(Annotation a)
    {
        AnnotationRender g = Instantiate(template);
        g.gameObject.SetActive(true);
        g.transform.position = referenceTransform.MapPosition(a.renderInfo.Center);
        g.Init(a);
        annotationUIs.Add(a.ID, g);
    }

    public void RemoveAnnotationUI(Annotation a)
    {
        GameObject g = annotationUIs[a.ID].gameObject;
        Destroy(g);
        annotationUIs.Remove(a.ID);
    }

    public void OnDestroy()
    {
        if (data == null)
        {
            return;
        }
        data.OnAnnotationAddEvent -= AddAnnotationUI;
        data.OnAnnotationRemoveEvent -= RemoveAnnotationUI;
    }

    public AnnotationRender GetAnnotationUI(AnnotationID id)
    {
        return annotationUIs[id];
    }
}
