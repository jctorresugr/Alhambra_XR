using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VolumeNavigation;
public class NavigationManager : MonoBehaviour
{
    [Header("Component")]
    public VolumeNavigation volumeNavigation;
    public GraphRenderSimple graphRenderSimple;
    public SelectionModelData selectionModelData;
    public GraphRenderRoutes graphRenderRoutes;
    public Transform user;
    public bool refreshNow = false;
    [Header("Config")]
    public float updateThreshold = 1.0f; //update if the user move far away
    public bool useDefaultRender = false;
    public bool useRouteRender = true;

    public bool triggerDebug = false;

    protected Vector3 oldPos;
    // Start is called before the first frame update
    void Start()
    {
        Utils.EnsureComponent(this, ref volumeNavigation);
        Utils.EnsureComponent(this, ref graphRenderSimple);
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
            foreach (var a in graphRenderRoutes.annotationData.Annotations)
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
        volumeNavigation.SetAnnotations(selectionModelData.SelectedAnnotations);
        if (volumeNavigation.annotations.Count==0)
        {
            graphRenderSimple.ClearDraw();
            return;
        }
        volumeNavigation.Preprocess();
        NavigationInfo navigationInfo = volumeNavigation.Navigate(user);
        if(useDefaultRender)
        {
            RenderGraph(navigationInfo);
        }
        if(useRouteRender)
        {
            RenderRoutes(navigationInfo);
        }
    }

    public void RenderRoutes(NavigationInfo navigationInfo)
    {
        if(graphRenderRoutes==null)
        {
            return;
        }
        graphRenderRoutes.data = RouteInfo.GenerateRoutes(navigationInfo);
        graphRenderRoutes.graph = navigationInfo.treeGraph;
        graphRenderRoutes.Redraw();
    }

    public void RenderGraph(NavigationInfo navigationInfo)
    {
        if(graphRenderSimple==null)
        {
            return;
        }
        graphRenderSimple.data = navigationInfo.treeGraph;
        graphRenderSimple.Redraw();
    }

    


}
