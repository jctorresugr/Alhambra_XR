using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static SelectionModelData;

public class SelectionInteraction : SocketDataBasic, ISelectionModelDataListener
{
    //public LineNavigatorManager lineNavigatorManager;
    public NavigationManager navigationManager;
    public DataManager data;
    // Start is called before the first frame update
    void Start()
    {
        if(data==null)
        {
            data = main.data;
        }
        main.SelectionData.AddListener(this);
        FastReg<int[]>(OnReceiveHighlightGroups);
    }

    public void OnReceiveHighlightGroups(int[] joints)
    {
        /*
        HashSet<AnnotationID> annotations = new HashSet<AnnotationID>();
        foreach(int jointId in joints)
        {
            AnnotationJoint annotationJoint = data.FindJointID(jointId);
            if(annotationJoint==null)
            {
                Debug.LogWarning("Cannot resolve joint id " + jointId);
                continue;
            }
            foreach(Annotation annot in annotationJoint.Annotations)
            {
                annotations.Add(annot.ID);
            }
        }
        main.SelectionData.SelectedAnnotations = annotations.ToList();
        navigationManager.refreshNow = true;*/
    }

    public void OnSetCurrentAction(SelectionModelData model, CurrentAction action)
    {
    }

    public void OnSetCurrentHighlight(SelectionModelData model, AnnotationID mainID, AnnotationID secondID)
    {
    }

    public void OnSetSelectedAnnotations(SelectionModelData model, List<AnnotationID> ids)
    {
        navigationManager.refreshNow = true;
    }
}
