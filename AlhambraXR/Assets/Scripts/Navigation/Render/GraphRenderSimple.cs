using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static VolumeNavigation;

//Unity does not support genertic type
//Also we need to tweak based on actual state
using N = VolumeNavigation.AnnotationNodeData;
using E = VolumeNavigation.EdgeDistanceData;

public class GraphRenderSimple : BasicRouteGraphRender
{
    [Header("data")]
    public Dictionary<GraphEdge<E>, LineRenderer> edgeObjects;



    //Try to re-generate all mesh (lines) 
    public override void Redraw()
    {
        if (graph == null)
        {
            return;
        }
        //generate line object for each edge
        //bruteforce way, not optimized! TODO: reduce object and reduce drawcall
        ClearDraw();
        edgeObjects = new Dictionary<GraphEdge<E>, LineRenderer>();
        graph.ForeachEdge(edge =>
        {
            LineRenderer lineRenderer = Instantiate(template);
            lineRenderer.transform.parent = this.transform;
            lineRenderer.transform.position = Vector3.zero;
            lineRenderer.transform.rotation = Quaternion.identity;
            edgeObjects[edge] = lineRenderer;
        });

        graph.ForeachNode(node =>
        {
            NotifyUpdatePoint(node);
        });
    }

    public override void ClearDraw()
    {
        if (edgeObjects != null)
        {
            foreach (LineRenderer lineRender in edgeObjects.Values)
            {
                Destroy(lineRender.gameObject);
            }
        }
    }

    public void NotifyUpdatePoint(GraphNode<N> node)
    {
        graph.ForeachNodeNeighbor(node,
            node => { },
            edge =>
            {
                LineRenderer lineRenderer = edgeObjects[edge];
                lineRenderer.SetPositions(GenerateEdgePositionData(edge));
            });

    }

    protected Vector3[] GenerateEdgePositionData(GraphEdge<E> edge)
    {
        Vector3[] posData = new Vector3[2];
        posData[0] = GetNodePos(edge.FromNode);
        posData[1] = GetNodePos(edge.ToNode);
        return posData;
    }

    protected Vector3 GetNodePos(int nodeIndex)
    {
        return referenceTransform.MapPosition(graph.GetNode(nodeIndex).data.centerPos);
    }

    public override void SetGraphData(Graph<N, E> graph, GraphNode<N> root = null)
    {
        this.graph = graph;
    }
}

