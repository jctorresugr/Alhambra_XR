using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeNavigation : MonoBehaviour
{
    [Header("Data source")]
    public VolumeAnalyze volumeAnalyze;
    public DataManager data;
    public ReferenceTransform referenceTransform;
    [Header("Settings")]
    public float depthRatio = 0.01f;
    //public float clipEdgeThreshold=0.2f;
    [Header("Cache Data")]
    [SerializeField]
    Graph<AnnotationNodeData, EdgeDistanceData> graph;
    public struct NavigationInfo
    {
        public Graph<AnnotationNodeData, EdgeDistanceData> treeGraph;
        public GraphNode<AnnotationNodeData> root;
    }

    public struct AnnotationNodeData
    {
        public AnnotationID id;
        public Vector3 centerPos;
        //Add more if required

        public AnnotationNodeData(AnnotationID id, Vector3 centerPos)
        {
            this.id = id;
            this.centerPos = centerPos;
            this.depth = -1;
        }

        public int depth;

        public bool IsVisited => depth >= 0;
    }

    public struct EdgeDistanceData
    {
        public float basicDistance;

        public EdgeDistanceData(float distance=0.0f)
        {
            basicDistance = distance;
        }
    }


    /**
     * First step: generate a n*n graph for annotations
     * TODO: consider spacial important position
     */
    public void Preprocess()
    {
        graph = new Graph<AnnotationNodeData, EdgeDistanceData>();
        // gather all annotations and 
        IReadOnlyList<Annotation> annotations = data.Annotations;
        int annotCount = annotations.Count;

        for (int i = 0; i < annotCount; i++)
        {
            graph.AddNode(new AnnotationNodeData(annotations[i].ID,annotations[i].renderInfo.Bounds.center));
        }
        for (int i=0;i<annotCount;i++)
        {
            GraphNode<AnnotationNodeData> ai = graph.GetNode(i);
            for (int j=i+1;j<annotCount;j++)
            {
                GraphNode<AnnotationNodeData> aj = graph.GetNode(i);
                //TODO: optimize the graph generation
                graph.AddEdge(i, j, new EdgeDistanceData(Distance(ai,aj)));
            }
        }
    }

    public float Distance(GraphNode<AnnotationNodeData> a, GraphNode<AnnotationNodeData> b)
    {
        bool asp = a.data.id.IsSpeical;
        bool bsp = b.data.id.IsSpeical;
        object boundA,boundB;
        //C# 9 only, I am unhappy, but compiler happy :(
        //boundA = a.data.id.IsSpeical ? a.data.centerPos : data.FindAnnotationID(a.data.id).renderInfo.Bounds;
        boundA = null;
        boundB = null;
        if(a.data.id.IsSpeical)
        {
            boundA = a.data.centerPos;
        }
        else
        {
            boundA = data.FindAnnotationID(a.data.id).renderInfo.Bounds;
        }
        if (b.data.id.IsSpeical)
        {
            boundB = b.data.centerPos;
        }
        else
        {
            boundB = data.FindAnnotationID(b.data.id).renderInfo.Bounds;
        }
        //cannot dynamic :(
        //return Utils.BoundsDistance(boundA ?? a.data.centerPos, boundB ?? b.data.centerPos);
        return BoundDistance(boundA, boundB);
    }

    //dynamically bound function :(
    public float BoundDistance(object a, object b)
    {
        if(a is Bounds && b is Bounds)
        {
            return Utils.BoundsDistance((Bounds)a, (Bounds)b);
        }
        else if(a is Bounds && b is Vector3)
        {
            return Utils.BoundsDistance((Bounds)a, (Vector3)b);
        }
        else if (a is Vector3 && b is Bounds)
        {
            return Utils.BoundsDistance((Vector3)a, (Bounds)b);
        }
        else if (a is Vector3 && b is Vector3)
        {
            return Utils.BoundsDistance((Vector3)a, (Vector3)b);
        }
        Debug.LogError($"Cannot dynamically match method BoundDistance({a.GetType()},{b.GetType()})");
        return 0.0f;
    }

   

    public GraphNode<AnnotationNodeData> AddNode(AnnotationNodeData nodeData)
    {
        GraphNode<AnnotationNodeData> graphNode = graph.AddNode(nodeData);
        graph.ForeachNode(n =>
        {
            if (n != graphNode)
            {
                float distance = Distance(n, graphNode);
                graph.AddEdge(n, graphNode, new EdgeDistanceData(distance));
            }
        });
        return graphNode;
    }

    struct EdgePickInfo: IComparable<EdgePickInfo>
    {
        public int edgeIndex;
        public int nodeIndex;
        public float distance;

        public int CompareTo(EdgePickInfo other)
        {
            return this.distance.CompareTo(other.distance);
        }
    }

    protected void ClearVisited()
    {
        graph.ForeachNode(n => n.data.depth = -1);
    }

    /**
     * Step 2: pass the userTransform and begin to navigate! 
     */
    public NavigationInfo Navigate(Transform userTransform)
    {
        // user node, should always as a root node
        Vector3 userLocalPos = referenceTransform.InvMapPosition(userTransform.position);
        GraphNode<AnnotationNodeData> userNode = AddNode(new AnnotationNodeData(AnnotationID.LIGHTALL_ID, userLocalPos));
        userNode.data.depth = 0;

        // normalize distance
        //TODO:.... get bounds and process;

        // Minimum span tree algorithm (Prim), but we have a fixed root, with extra evaluation calculations
        Heap<EdgePickInfo> edgeHeap = new Heap<EdgePickInfo>();

        List<GraphEdge<EdgeDistanceData>> resultEdges = new List<GraphEdge<EdgeDistanceData>>();
        int allNodeCount = graph.NodeCount;
        int addNodeCount = 1;
        GraphNode<AnnotationNodeData> lastNode = userNode;
        while (addNodeCount < allNodeCount)
        {
            //add Edge
            graph.ForeachNodeNeighbor(
                lastNode,
                node=> { },
                edge=>
                {
                    EdgePickInfo pi = new EdgePickInfo();
                    pi.edgeIndex = edge.index;
                    pi.nodeIndex = edge.GetAnotherNodeIndex(lastNode.index);
                    pi.distance = pi.distance + lastNode.data.depth * 0.01f;
                }
                );
            //pick up a minimum edge, add it to the tree
            while(true)
            {
                EdgePickInfo minEdge = edgeHeap.Dequeue();
                GraphNode<AnnotationNodeData> newNode = graph.GetNode(minEdge.nodeIndex);
                if (newNode.data.IsVisited)
                {
                    if(edgeHeap.Count==0)
                    {
                        Debug.LogError("No more edge found, the graph is not fully connected!");
                        break;
                    }
                    continue;
                }
                GraphEdge<EdgeDistanceData> newEdge = graph.GetEdge(minEdge.edgeIndex);
                newNode.data.depth =
                    graph.GetNode(newEdge.GetAnotherNodeIndex(newNode.index)).data.depth + 1;
                resultEdges.Add(newEdge);
                lastNode = newNode;
                addNodeCount++;
                break;
            }
        }

        //build graph
        Graph<AnnotationNodeData, EdgeDistanceData> treeGraph = new Graph<AnnotationNodeData, EdgeDistanceData>();
        treeGraph.CopyNodes(graph);
        foreach(GraphEdge<EdgeDistanceData> edge in resultEdges)
        {
            treeGraph.AddEdge(edge.fromNode, edge.toNode, new EdgeDistanceData(edge.data.basicDistance));
        }
        userNode = treeGraph.GetNode(userNode.index);
        graph.RemoveNode(userNode);
        NavigationInfo result = new NavigationInfo();
        result.treeGraph = treeGraph;
        result.root = userNode;
        return result;
    }


    //return minimumY
    protected int SearchFloor(int x, int y, int z)
    {
        VolumeCell<VolumeAnalyze.VolumeInfo> volumeInfos = volumeAnalyze.volumeInfos;
        while(true)
        {
            if(volumeInfos[x,y,z].IsEmpty)
            {
                y--;
                if(y<0)
                {
                    return 0;
                }
            }else
            {
                return y;
            }
        }
    }

}
