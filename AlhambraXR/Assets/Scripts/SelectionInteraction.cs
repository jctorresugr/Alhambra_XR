using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SelectionInteraction : SocketDataBasic
{
    public LineNavigatorManager lineNavigatorManager;
    public DataManager data;
    // Start is called before the first frame update
    void Start()
    {
        if(lineNavigatorManager==null)
        {
            lineNavigatorManager = main.lineNavigatorManager;
        }
        if(data==null)
        {
            data = main.data;
        }
        FastReg<int[]>(OnReceiveHighlightGroups);
    }

    public void OnReceiveHighlightGroups(int[] joints)
    {
        HashSet<Annotation> annotations = new HashSet<Annotation>();
        foreach(int jointId in joints)
        {
            AnnotationJoint annotationJoint = data.FindJointID(jointId);
            if(annotationJoint==null)
            {
                Debug.LogWarning("Cannot resolve joint id " + jointId);
                continue;
            }
            annotations.UnionWith(annotationJoint.Annotations);
        }
        lineNavigatorManager.SetAnnotations(annotations.ToList());
    }
}
