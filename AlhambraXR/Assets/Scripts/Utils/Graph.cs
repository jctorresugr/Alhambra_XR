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

    public GraphNode<T> Clone()
    {
        GraphNode<T> n = new GraphNode<T>();
        n.index = index;
        n.data = data;
        if(edgesIndex!=null)
        {
            n.edgesIndex = new List<int>();
            n.edgesIndex.AddRange(edgesIndex);
        }
        return n;
    }

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

    public GraphEdge<T> Clone()
    {
        GraphEdge<T> e = new GraphEdge<T>();
        e.index = index;
        e.fromNode = fromNode;
        e.toNode = toNode;
        e.data = data;
        return e;
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
        graphNode.data = data;
        if(emptyNodes.Count>0)
        {
            graphNode.index = emptyNodes[emptyNodes.Count - 1];
            nodes[emptyNodes[emptyNodes.Count - 1]] = graphNode;
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
        while(node.edgesIndex.Count>0)
        {
            RemoveEdge(node.edgesIndex[node.edgesIndex.Count-1]);
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

    public Graph<N,E> Clone()
    {
        Graph<N, E> g = new Graph<N, E>();
        foreach(var n in nodes)
        {
            if(n==null)
            {
                g.nodes.Add(null);
            }else
            {
                g.nodes.Add(n.Clone());
            }
        }
        foreach (var e in edges)
        {
            if (e == null)
            {
                g.edges.Add(null);
            }
            else
            {
                g.edges.Add(e.Clone());
            }
        }
        g.emptyEdges.AddRange(emptyEdges);
        g.emptyNodes.AddRange(emptyNodes);
        return g;
    }

    public void Clear()
    {
        nodes.Clear();
        edges.Clear();
        emptyEdges.Clear();
        emptyNodes.Clear();
    }

    public GraphEdge<E> AddEdge(GraphNode<N> n1, GraphNode<N> n2, E data)
    {
        GraphEdge<E> graphEdge = new GraphEdge<E>();
        graphEdge.fromNode = n1.index;
        graphEdge.toNode = n2.index;
        graphEdge.data = data;
        if(n1.edgesIndex==null)
        {
            n1.edgesIndex = new List<int>();
        }
        if (n2.edgesIndex == null)
        {
            n2.edgesIndex = new List<int>();
        }
        if(emptyEdges.Count>0)
        {
            graphEdge.index = emptyEdges[emptyEdges.Count - 1];
            edges[emptyEdges[emptyEdges.Count - 1]] = graphEdge;
            emptyEdges.RemoveAt(emptyEdges.Count - 1);
        }
        else
        {
            graphEdge.index = edges.Count;
            edges.Add(graphEdge);
        }
        n1.edgesIndex.Add(graphEdge.index);
        n2.edgesIndex.Add(graphEdge.index);
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
            if(g.IsValidNode(node))
                this.AddNode(node.data);
            else
            {
                emptyNodes.Add(nodes.Count);
                nodes.Add(null);
            }
        }
    }

    public void ForeachNode(Action<GraphNode<N>> action)
    {
        foreach(GraphNode<N> node in nodes)
        {
            if (IsValidNode(node))
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
    {
        if(baseNode.edgesIndex==null)
        {
            return;
        }
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

    public void ForeachNodeNeighbor(GraphNode<N> baseNode, Action<GraphNode<N>, GraphEdge<E>> action)
    {
        if (baseNode.edgesIndex == null)
        {
            return;
        }
        foreach (int edgeIndex in baseNode.edgesIndex)
        {
            GraphEdge<E> edge = edges[edgeIndex];
            GraphNode<N> node;
            if (edge.fromNode == baseNode.index)
            {
                node = nodes[edge.toNode];
            }
            else
            {
                node = nodes[edge.fromNode];
            }
            action(node, edge);
        }
    }

    protected bool IsValidNode(GraphNode<N> n)
    {
        // the C# has some wired behavior, they will create an empty object for the null value in the list
        if(n != null && n.index>=0 && n.index<nodes.Count)
        {
            return nodes[n.index] == n;
        }
        return false;
    }

    public void ForeachNodeNeighbor(int nodeIndex, Action<GraphNode<N>> actionNode, Action<GraphEdge<E>> actionEdge=null)
    {
        GraphNode<N> baseNode = nodes[nodeIndex];
        ForeachNodeNeighbor(baseNode, actionNode, actionEdge);
    }

    public GraphEdge<E> GetEdge(GraphNode<N> node1, GraphNode<N> node2)
    {
        if(node1==null||node2==null)
        {
            return null;
        }
        foreach(int edgeIndex in node1.edgesIndex)
        {
            GraphEdge<E> edge = edges[edgeIndex];
            if(edge.GetAnotherNodeIndex(node1.index)==node2.index)
            {
                return edge;
            }
        }
        return null;
    }


}
