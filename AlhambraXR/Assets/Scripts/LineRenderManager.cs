using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineRenderManager : MonoBehaviour
{
    public LineRenderer template;
    public LineRenderData data;

    public Dictionary<LineRenderEdge, LineRenderer> edgeObjects;


    //Try to re-generate all mesh (lines) 
    public void Redraw()
    {
        //generate line object for each edge
        //bruteforce way, not optimized! TODO: reduce object and reduce drawcall
        if(edgeObjects!=null)
        {
            foreach(LineRenderer lineRender in edgeObjects.Values)
            {
                Destroy(lineRender.gameObject);
            }
        }
        edgeObjects = new Dictionary<LineRenderEdge, LineRenderer>();
        foreach(LineRenderEdge edge in data.edges)
        {
            LineRenderer lineRenderer = Instantiate(template);
            lineRenderer.transform.parent = this.transform;
            lineRenderer.transform.position = Vector3.zero;
            lineRenderer.transform.rotation = Quaternion.identity;
            edgeObjects[edge] = lineRenderer;
        }
        foreach(LineRenderNode node in data.nodes)
        {
            NotifyUpdatePoint(node);
        }
    }

    public void NotifyUpdatePoint(LineRenderNode node)
    {
        foreach (LineRenderEdge edge in data.edges)
        {
            LineRenderer lineRenderer = edgeObjects[edge];
            lineRenderer.SetPositions(data.GenerateEdgePositionData(edge));
        }
    }

}
