using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static VolumeNavigation;
public class NavigationManager : MonoBehaviour
{
    [Header("Component")]
    public VolumeAnalyze volumeAnalyze;
    public VolumeNavigation volumeNavigation;
    public GraphRenderManager graphRenderManager;
    public Transform user;
    public float updateThreshold = 1.0f;
    public bool refreshNow = false;//for debugging

    protected Vector3 oldPos;
    // Start is called before the first frame update
    void Start()
    {
        Utils.EnsureComponent(this, ref volumeAnalyze);
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

    public void Navigate()
    {
        oldPos = user.position;
        NavigationInfo navigationInfo = volumeNavigation.Navigate(user);
        //render Navigation
        GraphNode<AnnotationNodeData> root = navigationInfo.root;
        graphRenderManager.data = navigationInfo.treeGraph;
        graphRenderManager.Redraw();
    }
}
