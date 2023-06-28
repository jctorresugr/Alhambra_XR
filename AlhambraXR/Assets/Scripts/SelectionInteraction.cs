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
