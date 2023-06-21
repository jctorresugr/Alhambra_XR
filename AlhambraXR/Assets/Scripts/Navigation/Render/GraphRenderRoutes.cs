using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using N = VolumeNavigation.AnnotationNodeData;
using E = VolumeNavigation.EdgeDistanceData;

public class GraphRenderRoutes : BasicRouteGraphRender
{
    [Header("data")]
    public List<RouteInfo> data = null;
    public Dictionary<RouteInfo, LineRenderer> renders = new Dictionary<RouteInfo, LineRenderer>();
    protected Dictionary<GraphNode<N>, int> usedCount = new Dictionary<GraphNode<N>, int>();
    [Header("config")]
    public float offset = 0.0001f;
    [Header("config - curve")]
    public bool useSmoothCurve = true;
    public float alpha=2.0f;
    public float sampleDistanceRatio = 0.01f;

    public override void Redraw()
    {
        ClearDraw();
        if (data==null)
        {
            return;
        }
        graph.ForeachNode(n => usedCount.Add(n, 0));
        foreach(RouteInfo route in data)
        {
            LineRenderer lineRenderer = Instantiate(template);
            lineRenderer.transform.parent = this.transform;
            lineRenderer.transform.position = Vector3.zero;
            lineRenderer.transform.rotation = Quaternion.identity;
            lineRenderer.positionCount = route.nodes.Count;

            Gradient graident = new Gradient();
            GradientColorKey[] gradientColorKeys = route.GenerateGradientColor();
            GradientAlphaKey[] gradientAlphaKeys = route.GenerateGradientAlpha();
            graident.SetKeys(gradientColorKeys, gradientAlphaKeys);
            lineRenderer.colorGradient = graident;

            Vector3[] vertexData = GeneratePosData(route);
            lineRenderer.positionCount = vertexData.Length;
            lineRenderer.SetPositions(vertexData);
            renders.Add(route,lineRenderer);
        }
    }

    public override void ClearDraw()
    {
        foreach(LineRenderer renderer in renders.Values)
        {
            Destroy(renderer);
        }
        renders.Clear();
        usedCount.Clear();
    }

    protected Vector3[] GeneratePosData(RouteInfo ri)
    {
        
        var nodes = ri.nodes;
        Vector3[] data = new Vector3[nodes.Count];
        for (int i = 0; i < data.Length; i++)
        {
            var node = nodes[i];
            if (i==0)
            {
                data[i] = referenceTransform.MapPosition(node.data.centerPos);
                continue;
            }
            var nodePre = nodes[i - 1];
            Vector3 edgeDir = (node.data.centerPos - nodePre.data.centerPos).normalized;
            Vector3 pos = node.data.centerPos + usedCount[node] * edgeDir * offset;
            data[i] = referenceTransform.MapPosition(pos);
            usedCount[node]++;
        }
        if(useSmoothCurve)
        {
            float sampleDistance = sampleDistanceRatio * this.annotationData.ReferLength * referenceTransform.ScaleRefer;
            List <Vector3> dataRefined = CurveSample.GenerateCurve(data, sampleDistance, alpha);
            return dataRefined.ToArray();
        }
        return data;
    }

    public override void SetGraphData(Graph<N, E> graph, GraphNode<N> root = null)
    {
        this.graph = graph;
        data = RouteInfo.GenerateRoutes(graph,root);
    }
}
