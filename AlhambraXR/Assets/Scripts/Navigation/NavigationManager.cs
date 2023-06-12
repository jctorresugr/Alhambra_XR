using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VolumeNavigation;
public class NavigationManager : MonoBehaviour
{
    [Header("Component")]
    public VolumeNavigation volumeNavigation;
    public GraphRenderManager graphRenderManager;
    public SelectionModelData selectionModelData;
    public Transform user;
    public float updateThreshold = 1.0f;
    public bool refreshNow = false;//for debugging

    protected Vector3 oldPos;
    // Start is called before the first frame update
    void Start()
    {
        Utils.EnsureComponent(this, ref volumeNavigation);
        Utils.EnsureComponent(this, ref graphRenderManager);
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
            graphRenderManager.ClearDraw();
            return;
        }
        volumeNavigation.Preprocess();
        NavigationInfo navigationInfo = volumeNavigation.Navigate(user);
        //render Navigation
        GraphNode<AnnotationNodeData> root = navigationInfo.root;
        graphRenderManager.data = navigationInfo.treeGraph;
        graphRenderManager.Redraw();
    }


}
