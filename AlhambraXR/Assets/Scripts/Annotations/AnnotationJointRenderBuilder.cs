using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationJointRenderBuilder : MonoBehaviour
{
    public DataManager data;
    /// <summary>
    /// Assign an gameobject to represent it as a Annotation Joint
    /// </summary>
    public GameObject templateJoint;
    public GameObject templateIndicator;

    private Dictionary<int, GameObject> jointUIs;

    private bool isInited = false;
    public void Init()
    {
        if(isInited)
        {
            return;
        }
        if (templateJoint == null || templateIndicator == null) 
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
            jointUIs = new Dictionary<int, GameObject>();
        }
    }

    public void AddAnnotationJointUI(AnnotationJoint aj)
    {
        GameObject g = Instantiate(templateJoint);
        g.SetActive(true);
        AnnotationJointRender ajr=null;
        Utils.ForceToGetComponent(g, ref ajr);
        ajr.Init(aj,templateIndicator);
        jointUIs.Add(aj.ID, g);
    }

    public void RemoveAnnotationJointUI(AnnotationJoint aj)
    {
        GameObject g = jointUIs[aj.ID];
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
