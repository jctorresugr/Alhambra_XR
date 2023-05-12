using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class LineRenderEdge
{
    public int index;
    public List<LineRenderNode> points = new List<LineRenderNode>();
    public LineRenderEdge(int index)
    {
        this.index = index;
    }
    public LineRenderEdge(int index, LineRenderNode n1, LineRenderNode n2)
    {
        this.index = index;
        points.Add(n1);
        points.Add(n2);
    }
}

public class LineRenderNode
{
    public Vector3 position;
    public int index;

    public List<LineRenderEdge> edges = new List<LineRenderEdge>();

    public LineRenderNode(int index, Vector3 position)
    {
        this.position = position;
        this.index = index;
    }
}

public class LineRenderData
{
    public List<LineRenderNode> nodes = new List<LineRenderNode>();
    public List<LineRenderEdge> edges = new List<LineRenderEdge>();

    public void AddEdge(LineRenderNode n1, LineRenderNode n2)
    {
        LineRenderEdge edge = new LineRenderEdge(edges.Count,n1, n2);
        n2.edges.Add(edge);
        n1.edges.Add(edge);
        edges.Add(edge);
    }

    public void RemoveEdge(LineRenderEdge edge)
    {
        foreach(LineRenderNode node in edge.points)
        {
            node.edges.Remove(edge);
        }
        edges.RemoveAt(edge.index);
    }

    public void AddNode(LineRenderNode node)
    {
        node.index = nodes.Count;
        nodes.Add(node);
    }

    public void RemoveNode(LineRenderNode node)
    {
        foreach(LineRenderEdge edge in node.edges)
        {
            RemoveEdge(edge);
        }
        nodes.RemoveAt(node.index);
        node.edges.Clear();
    }

    public Vector3[] GenerateEdgePositionData(LineRenderEdge edge)
    {
        int count = edge.points.Count;
        Vector3[] posData = new Vector3[count];
        for (int i = 0; i < edge.points.Count; i++)
        {
            LineRenderNode node = edge.points[i];
            posData[i] = node.position;
        }
        return posData;
    }
}
