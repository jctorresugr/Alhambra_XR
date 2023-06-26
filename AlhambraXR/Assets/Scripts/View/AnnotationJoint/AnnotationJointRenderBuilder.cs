using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationJointRenderBuilder : MonoBehaviour
{
    public DataManager data;
    /// <summary>
    /// Assign an gameobject to represent it as a Annotation Joint
    /// </summary>
    public AnnotationJointRenderBase templateJoint;
    public ReferenceTransform referenceTransform;

    private Dictionary<int, AnnotationJointRenderBase> jointUIs;

    private bool isInited = false;
    public void Init()
    {
        if(isInited)
        {
            return;
        }
        if (templateJoint == null) 
        {
            Debug.LogWarning("AnnotationJointRenderBuilder requires a template to generate visual AnnotationJoint");
        }
        else if(data==null)
        {
            Debug.LogWarning("Require data manager to receive data change notification");
        }
        else
        {
            isInited = true;
            data.OnAnnotationJointAddEvent += AddAnnotationJointUI;
            data.OnAnnotationJointRemoveEvent += RemoveAnnotationJointUI;
            jointUIs = new Dictionary<int, AnnotationJointRenderBase>();
        }
    }

    public void DrawAllAnnotationJoints()
    {
        foreach(var aj in data.AnnotationJoints)
        {
            AddAnnotationJointUI(aj);
        }
    }

    public void AddAnnotationJointUI(AnnotationJoint aj)
    {
        AnnotationJointRenderBase g = Instantiate(templateJoint);
        g.gameObject.SetActive(true);
        g.transform.position = referenceTransform.MapPosition(aj.position);
        g.Init(aj);
        jointUIs.Add(aj.ID, g);
    }

    public void RemoveAnnotationJointUI(AnnotationJoint aj)
    {
        GameObject g = jointUIs[aj.ID].gameObject;
        Destroy(g);
        jointUIs.Remove(aj.ID);
    }

    public void OnDestroy()
    {
        if(data==null)
        {
            return;
        }
        data.OnAnnotationJointAddEvent -= AddAnnotationJointUI;
        data.OnAnnotationJointRemoveEvent -= RemoveAnnotationJointUI;
    }
}
