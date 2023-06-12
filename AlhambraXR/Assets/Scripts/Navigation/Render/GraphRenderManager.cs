using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using static VolumeNavigation;

//Unity does not support genertic type
//Also we need to tweak based on actual state
using N = VolumeNavigation.AnnotationNodeData;
using E = VolumeNavigation.EdgeDistanceData;

public class GraphRenderManager : MonoBehaviour
{
    [Header("template and set")]
    public LineRenderer template;
    public DataManager annotationData;
    public ReferenceTransform referenceTransform;
    //public LineRenderData data;
    [Header("data")]
    public Graph<N, E> data;
    public Dictionary<GraphEdge<E>, LineRenderer> edgeObjects;



    //Try to re-generate all mesh (lines) 
    public void Redraw()
    {
        if (data == null)
        {
            return;
        }
        //generate line object for each edge
        //bruteforce way, not optimized! TODO: reduce object and reduce drawcall
        ClearDraw();
        edgeObjects = new Dictionary<GraphEdge<E>, LineRenderer>();
        data.ForeachEdge(edge =>
        {
            LineRenderer lineRenderer = Instantiate(template);
            lineRenderer.transform.parent = this.transform;
            lineRenderer.transform.position = Vector3.zero;
            lineRenderer.transform.rotation = Quaternion.identity;
            edgeObjects[edge] = lineRenderer;
        });

        data.ForeachNode(node =>
        {
            NotifyUpdatePoint(node);
        });
    }

    public void ClearDraw()
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
        data.ForeachNodeNeighbor(node,
            node => { },
            edge =>
            {
                LineRenderer lineRenderer = edgeObjects[edge];
                lineRenderer.SetPositions(GenerateEdgePositionData(edge));
            });

    }

    public Vector3[] GenerateEdgePositionData(GraphEdge<E> edge)
    {
        Vector3[] posData = new Vector3[2];
        posData[0] = GetNodePos(edge.FromNode);
        posData[1] = GetNodePos(edge.ToNode);
        return posData;
    }

    protected Vector3 GetNodePos(int nodeIndex)
    {
        return referenceTransform.MapPosition(data.GetNode(nodeIndex).data.centerPos);
    }

}

