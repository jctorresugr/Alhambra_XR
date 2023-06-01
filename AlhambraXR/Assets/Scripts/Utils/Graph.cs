using System;
using System.Collections.Generic;
using UnityEngine;
[Serializable]
public class GraphNode<T>
{
    [SerializeField]
    internal int index;
    [SerializeField]
    public T data;
    [SerializeField]
    internal List<int> edgesIndex;

    public IReadOnlyList<int> EdgesIndex => edgesIndex;
    public int Index => index;
}
[Serializable]
public class GraphEdge<T>
{
    [SerializeField]
    internal int index;
    [SerializeField]
    internal int fromNode;
    [SerializeField]
    internal int toNode;
    [SerializeField]
    public T data;

    public int FromNode => fromNode;
    public int ToNode => toNode;
    public int Index => index;

    public int GetAnotherNodeIndex(int curIndex)
    {
        return curIndex == fromNode ? toNode : fromNode;
    }
}

[Serializable]
public class Graph<N,E>
{
    [SerializeField]
    protected List<GraphNode<N>> nodes;
    [SerializeField]
    protected List<GraphEdge<E>> edges;
    [SerializeField]
    private List<int> emptyNodes;
    [SerializeField]
    private List<int> emptyEdges;

    public Graph()
    {
        nodes = new List<GraphNode<N>>();
        edges = new List<GraphEdge<E>>();
        emptyNodes = new List<int>();
        emptyEdges = new List<int>();
    }

    public int NodeCount => nodes.Count - emptyNodes.Count;
    public int EdgeCount => edges.Count - emptyEdges.Count;

    public GraphNode<N> GetNode(int index)
    {
        return nodes[index];
    }

    public GraphEdge<E> GetEdge(int index)
    {
        return edges[index];
    }

    public GraphNode<N> AddNode(N data)
    {
        GraphNode<N> graphNode = new GraphNode<N>();
        if(emptyNodes.Count>0)
        {
            graphNode.index = emptyNodes[emptyNodes.Count - 1];
            emptyNodes.RemoveAt(emptyNodes.Count - 1);
        }else
        {
            graphNode.index = nodes.Count;
            nodes.Add(graphNode);
        }
        
        return graphNode;
    }
    public GraphNode<N> RemoveNode(GraphNode<N> node)
    {
        int index = node.index;
        foreach (int edgeIndex in node.edgesIndex)
        {
            RemoveEdge(edgeIndex);
        }
        if (index == nodes.Count - 1) // if last one, remove
        {
            nodes.RemoveAt(index);
        }
        else
        {
            nodes[index] = null;
            emptyNodes.Add(index);
        }
        return node;
    }
    public GraphNode<N> RemoveNode(int index)
    {
        GraphNode<N> node = nodes[index];
        return RemoveNode(node);
    }

    /*
    protected void ChangeNodeIndex(GraphNode<N> node, int newIndex)
    {
        int oldIndex = node.index;
        foreach(int edgeIndex in node.edgesIndex)
        {
            GraphEdge<E> edge = edges[edgeIndex];
            if(edge.fromNode==oldIndex)
            {
                edge.fromNode = newIndex;
            }else if(edge.toNode==oldIndex)
            {
                edge.toNode = newIndex;
            }else
            {
                Debug.LogError($"Detect unknown edge in node {node}, before change to id {newIndex}");
                continue;
            }
        }
        nodes[newIndex] = node;
        nodes[oldIndex] = null;
    }*/

    public GraphEdge<E> AddEdge(int n1, int n2, E data)
    {
        return AddEdge(nodes[n1], nodes[n2], data);
    }

    public GraphEdge<E> AddEdge(GraphNode<N> n1, GraphNode<N> n2, E data)
    {
        GraphEdge<E> graphEdge = new GraphEdge<E>();
        graphEdge.fromNode = n1.index;
        graphEdge.toNode = n2.index;
        graphEdge.data = data;
        n1.edgesIndex.Add(graphEdge.index);
        n2.edgesIndex.Add(graphEdge.index);
        if(emptyEdges.Count>0)
        {
            graphEdge.index = emptyEdges[emptyEdges.Count - 1];
            emptyEdges.RemoveAt(emptyEdges.Count - 1);
        }
        else
        {
            graphEdge.index = edges.Count;
            edges.Add(graphEdge);
        }
        return graphEdge;
    }

    public void RemoveEdge(int edgeIndex)
    {
        GraphEdge<E> edge = edges[edgeIndex];
        GraphNode<N> fromNode = nodes[edge.fromNode];
        GraphNode<N> toNode = nodes[edge.toNode];
        fromNode.edgesIndex.Remove(edgeIndex);
        toNode.edgesIndex.Remove(edgeIndex);
        edges[edgeIndex] = null;
        if(edgeIndex == edges.Count-1)
        {
            edges.RemoveAt(edgeIndex);
        }
        else
        {
            edges[edgeIndex] = null;
            emptyEdges.Add(edgeIndex);
        }
    }

    //this will retain index and data information
    //but remove all edge information
    public void CopyNodes(Graph<N,E> g)
    {
        foreach(GraphNode<N> node in g.nodes)
        {
            if(node!=null)
                this.AddNode(node.data);
            else
            {
                emptyNodes.Add(nodes.Count-1);
                nodes.Add(null);
            }
        }
    }

    public void ForeachNode(Action<GraphNode<N>> action)
    {
        foreach(GraphNode<N> node in nodes)
        {
            if (node != null)
                action(node);
        }
    }

    public void ForeachEdge(Action<GraphEdge<E>> action)
    {
        foreach (GraphEdge<E> edge in edges)
        {
            if (edge != null)
                action(edge);
        }
    }

    public void ForeachNodeNeighbor(GraphNode<N> baseNode, Action<GraphNode<N>> actionNode, Action<GraphEdge<E>> actionEdge = null)
    {;
        foreach (int edgeIndex in baseNode.edgesIndex)
        {
            GraphEdge<E> edge = edges[edgeIndex];
            actionEdge(edge);
            if (edge.fromNode == baseNode.index)
            {
                actionNode(nodes[edge.toNode]);
            }
            else
            {
                actionNode(nodes[edge.fromNode]);
            }
        }
    }

    public void ForeachNodeNeighbor(int nodeIndex, Action<GraphNode<N>> actionNode, Action<GraphEdge<E>> actionEdge=null)
    {
        GraphNode<N> baseNode = nodes[nodeIndex];
        ForeachNodeNeighbor(baseNode, actionNode, actionEdge);
    }


}
