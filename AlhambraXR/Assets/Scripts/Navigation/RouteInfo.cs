using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using N = VolumeNavigation.AnnotationNodeData;
using E = VolumeNavigation.EdgeDistanceData;
using static VolumeNavigation;

public class RouteInfo
{
    public List<GraphNode<N>> nodes = new List<GraphNode<N>>();
    public List<Vector3> offset = new List<Vector3>();
    public Color color = Color.blue;

    public RouteInfo()
    {

    }

    public RouteInfo(RouteInfo r)
    {
        nodes.AddRange(r.nodes);
        color = r.color;
    }

    public Vector3[] GeneratePosData(ReferenceTransform reference)
    {
        Vector3[] data = new Vector3[nodes.Count];
        for (int i = 0; i < data.Length; i++)
        {
            var node = nodes[i];
            data[i] = reference.MapPosition(node.data.centerPos);
        }
        return data;
    }

    //depreacted
    public Color[] GenerateColorData()
    {
        Color[] data = new Color[nodes.Count];
        for (int i = 0; i < data.Length; i++)
        {
            data[i] = color;
        }
        return data;
    }

    public GradientColorKey[] GenerateGradientColor()
    {
        return new GradientColorKey[] { new GradientColorKey(color, 0.0f), new GradientColorKey(color, 1.0f) };
    }

    public GradientAlphaKey[] GenerateGradientAlpha()
    {
        return new GradientAlphaKey[] { new GradientAlphaKey(1.0f, 0.0f), new GradientAlphaKey(1.0f, 1.0f) };
    }


    public static List<RouteInfo> GenerateRoutes(Graph<N,E> treeGraph, GraphNode<N> root)
    {

        List<RouteInfo> routes = new List<RouteInfo>();
        List<GraphNode<N>> nodes = new List<GraphNode<N>>();
        void RecusiveIter(Graph<N, E> data, GraphNode<N> pre, GraphNode<N> cur)
        {
            Debug.Log($"> Iter pre {pre.index}\t | cur {cur.index} \t| curRouteLen {GetIndexInfo(nodes)}");
            bool isEnd = true;
            data.ForeachNodeNeighbor(cur,
                (n, e) =>
                {
                    if (n != pre)
                    {
                        Debug.Log($"! Iter detect {n.index}\t | cur {cur.index} |\t {GetIndexInfo(nodes)}");
                        isEnd = false;
                        nodes.Add(n);
                        RecusiveIter(data, cur, n);
                        nodes.RemoveAt(nodes.Count - 1);
                    }
                }
            );
            if (isEnd)
            {
                Debug.Log($"## Iter Finish {GetIndexInfo(nodes)}");
                RouteInfo route = new RouteInfo();
                route.nodes.AddRange(nodes);
                routes.Add(route);
            }
            else
            {
                Debug.Log($"# Iter Leave {GetIndexInfo(nodes)}");
            }
        }

        //routes.Add(initRoute);
        nodes.Add(root);
        RecusiveIter(treeGraph, root, root);
        return routes;
    }

    public static List<RouteInfo> GenerateRoutes(NavigationInfo navigationInfo)
    {
        return GenerateRoutes(navigationInfo.treeGraph, navigationInfo.root);
    }

    public static string GetIndexInfo(List<GraphNode<N>> l)
    {
        string s = "";
        foreach(var n in l)
        {
            s += n.index + ">";
        }
        return s;
    }
}