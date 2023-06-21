using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static VolumeNavigation;

public class NavigationCache: MonoBehaviour
{
    [Header("Component")]
    public VolumeNavigation navigation;
    public BasicRouteGraphRender render;

    [Header("Data")]
    public List<Annotation> data;
    public Transform rootPos;

    [SerializeField]
    private NavigationInfo cacheResult;
    public NavigationInfo CacheResult => cacheResult;


    public void GenerateNavigationInfo()
    {
        cacheResult = navigation.Navigate(data, rootPos.position);
    }

    public void Redraw()
    {
        Show();
        ClearDraw();
        render.SetGraphData(cacheResult.treeGraph, cacheResult.root);
        render.Redraw();
    }

    public void ClearDraw()
    {
        render.ClearDraw();
    }

    public void Hide()
    {
        render.enabled = false;
        render.gameObject.SetActive(false);
    }

    public void Show()
    {
        render.enabled = true;
        render.gameObject.SetActive(true);
    }

    public void ClearCache()
    {
        cacheResult = null;
        if(render.isActiveAndEnabled)
        {
            render.ClearDraw();
        }
    }

    public bool IsHided => render.enabled;
    public bool IsCached => cacheResult != null;
}
