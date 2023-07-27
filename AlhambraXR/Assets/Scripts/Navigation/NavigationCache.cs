using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static VolumeNavigation;

/// <summary>
/// Record Navigation result
/// To use this, 
/// first setup search: data= ....
/// Then invoke: GenerateNavigationInfo()
/// Then invoke: Redraw()
/// 
/// Invoke Show() Hide() to show/hide path
/// Invoke ClearDraw() will clean all drawed path forever
/// </summary>
public class NavigationCache: MonoBehaviour
{
    [Header("Component")]
    public VolumeNavigation navigation;
    public BasicRouteGraphRender render;

    [Header("Data")]
    public List<Annotation> data; //annotation to be searched for
    public Transform rootPos; // the basic position of start position

    [SerializeField]
    private NavigationInfo cacheResult;
    public NavigationInfo CacheResult => cacheResult;


    public void GenerateNavigationInfo()
    {
        cacheResult = navigation.Navigate(data, rootPos.position);
    }

    public void GenerateNavigationInfo(IReadOnlyList<AnnotationID> targetAnnotations, Vector3 beginPosition,Vector3 suggestForward)
    {
        cacheResult = navigation.Navigate(targetAnnotations, beginPosition, suggestForward);
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
        if(!render.isActiveAndEnabled)
        {
            Show();
        }
        render.ClearDraw();
    }

    public void RemoveNavigationToDestionation(Annotation annotation)
    {
        RemoveNavigationToDestionation(annotation.ID);
    }


    public void RemoveNavigationToDestionation(AnnotationID annotationID)
    {
        if(cacheResult==null)
        {
            return;
        }
        GraphNode<AnnotationNodeData> nodeToRemove = null;
        cacheResult.treeGraph.ForeachNode(n =>
        {
            if (n.data.id == annotationID)
            {
                nodeToRemove = n;
            }
        });
        if(nodeToRemove==null)
        {
            return;
        }
        RemoveNavigationToDestionation(nodeToRemove.Index);
    }

    /// <summary>
    /// Remove all routes to the leaf node  (nodeIndex)
    /// You cannot recover it unless recompute the navigation.
    /// root node cannot be removed
    /// non-leaf node cannot be removed
    /// non-connected node cannot be removed
    /// </summary>
    /// <param name="nodeIndex"></param>
    public void RemoveNavigationToDestionation(int nodeIndex)
    {
        if(cacheResult==null)
        {
            return;
        }
        Graph<AnnotationNodeData, EdgeDistanceData> treeGraph = cacheResult.treeGraph;
        GraphNode<AnnotationNodeData> node = treeGraph.GetNode(nodeIndex);
        if(node==null || node == cacheResult.root)
        {
            return;
        }
        //TODO: resolve UX when the node is the center node of route (visual impede)
        //degree==2: to leaf node & to root node path
        if(node.Degree!=1)
        {
            // exists other nodes
            GraphNode<AnnotationNodeData> leafNode = node;
            treeGraph.ForeachNodeNeighbor(node, (n, e) =>
            {
                if(n.Degree==1)
                {
                    leafNode = node;
                }
            });
            node = leafNode;
        }

        bool removed = false;
        //remove until no leaf node in this route
        while(node!=cacheResult.root)
        {
            int edgeIndex = node.EdgesIndex[0];
            GraphEdge<EdgeDistanceData> edge = treeGraph.GetEdge(edgeIndex);
            if(edge==null)
            {
                Debug.LogWarning("Cannot delete node " + nodeIndex + ". Encounter empty edge.");
                return;
            }
            int otherNodeIndex = edge.GetAnotherNodeIndex(node.Index);
            treeGraph.RemoveEdge(edge);
            treeGraph.RemoveNode(node);
            removed = true;
            node = treeGraph.GetNode(otherNodeIndex);

            if(node==null || node.Degree!=1) //is leaf?
            {
                break;
            }
        }
        if(removed)
        {
            Redraw();
        }
    }

    public bool IsHided => render.enabled;
    public bool IsCached => cacheResult != null;
}
