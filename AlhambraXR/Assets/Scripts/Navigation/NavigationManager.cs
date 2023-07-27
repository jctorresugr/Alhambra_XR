using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VolumeNavigation;
public class NavigationManager : MonoBehaviour
{
    [Header("Component")]
    public VolumeNavigation volumeNavigation;
    public SelectionModelData selectionModelData;
    public Transform user;
    public NavigationCache navigationCache;
    //[Header("Render Component")]
    //public BasicRouteGraphRender render;
    [Header("Config")]
    public bool refreshNow = false;
    public float forwardDistance = 0.1f;
    public float updateThreshold = 1.0f; //update if the user move far away

    public bool triggerDebug = false;

    protected Vector3 oldPos;
    // Start is called before the first frame update
    void Start()
    {
        Utils.EnsureComponent(this, ref volumeNavigation);
        oldPos = user.position;
    }

    // Update is called once per frame
    void Update()
    {
        float dis = (oldPos - user.position).sqrMagnitude;
        if(dis>updateThreshold || refreshNow)
        {
            refreshNow = false;
            Navigate();
        }
        //debugging code, TODO: remove
        if(triggerDebug)
        {
            triggerDebug = false;
            refreshNow = true;
            selectionModelData.ClearSelectedAnnotations();
            foreach (var a in navigationCache.render.annotationData.Annotations)
            {
                selectionModelData.AddSelectedAnnotations(a.ID);
            }
            
        }
    }

    public void Init(SelectionModelData selectionModelData)
    {
        this.selectionModelData = selectionModelData;
        volumeNavigation.Init();
    }

    public void Navigate()
    {
        oldPos = user.position;
        IReadOnlyList<AnnotationID> selectedAnnotations = selectionModelData.SelectedAnnotations;
        if (selectedAnnotations.Count==0)
        {
            navigationCache.ClearCache();
            return;
        }
        //relocate user root position (a bit forward)
        Vector3 userLocalPos = user.position + user.forward * forwardDistance;
        RaycastHit rayHit;
        if (Physics.Raycast(userLocalPos, Vector3.down, out rayHit))
        {
            userLocalPos = rayHit.point + Vector3.up * 0.3f;
        }

        navigationCache.GenerateNavigationInfo(selectedAnnotations,userLocalPos,user.forward);
        navigationCache.Redraw();
        navigationCache.Show();
        //NavigationInfo navigationInfo = volumeNavigation.Navigate(selectedAnnotations, userLocalPos, user.forward);
        //render.ClearDraw();
        //render.SetGraphData(navigationInfo.treeGraph, navigationInfo.root);
        //render.Redraw();
    }

}
