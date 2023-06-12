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
    public List<Annotation> annotations = new List<Annotation>();
    [Header("Settings")]
    public float depthRatio = 0.01f;
    public float splitEdgeThreshold = 0.001f;
    public LayerMask layerMask = 1 >> 6;
    public float normalOffset = 0.001f;
    public Vector3 tempX = Vector3.right;
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
        graph.Clear();
        // gather all annotations and 
        int annotCount = annotations.Count;

        for (int i = 0; i < annotCount; i++)
        {
            graph.AddNode(new AnnotationNodeData(annotations[i].ID, GetAnnotationPos(annotations[i])));
            Debug.Log($"{annotations[i].ID} .bounds => {annotations[i].renderInfo.Bounds}");
        }
        for (int i=0;i<annotCount;i++)
        {
            GraphNode<AnnotationNodeData> ai = graph.GetNode(i);
            for (int j=i+1;j<annotCount;j++)
            {
                GraphNode<AnnotationNodeData> aj = graph.GetNode(j);
                //TODO: optimize the graph generation
                graph.AddEdge(i, j, new EdgeDistanceData(Distance(ai,aj)));
            }
        }
    }

    public void Init()
    {
        graph = new Graph<AnnotationNodeData, EdgeDistanceData>();
        volumeAnalyze.Preprocess();
    }

    public void SetAnnotations(IReadOnlyList<Annotation> aids)
    {
        if(aids==null)
        {
            annotations.Clear();
            return;
        }
        annotations.AddRange(aids);
    }

    public void SetAnnotations(IReadOnlyList<AnnotationID> aids)
    {
        annotations.Clear();
        foreach(AnnotationID aid in aids)
        {
            annotations.Add(data.FindAnnotationID(aid));
        }
    }

    protected Vector3 GetAnnotationPos(Annotation annot)
    {
        Vector3 center = annot.renderInfo.Center;
        Vector3 normal = annot.renderInfo.Normal;
        Vector3 size = annot.renderInfo.Bounds.size*0.5f;
        float outBoxDis = float.MaxValue;
        for(int i=0;i<3;i++)
        {
            if(normal[i]!=0)
            {
                outBoxDis = Mathf.Min(outBoxDis, Mathf.Abs(size[i] / normal[i]));
            }
        }
        if(outBoxDis==float.MaxValue)
        {
            outBoxDis = 0.0f;
        }
        return center + annot.renderInfo.Normal * (normalOffset + outBoxDis);
    }

    protected float Distance(GraphNode<AnnotationNodeData> a, GraphNode<AnnotationNodeData> b)
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
    protected float BoundDistance(object a, object b)
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
        RaycastHit rayHit;
        if (Physics.Raycast(userTransform.position,Vector3.down, out rayHit))
        {
            userLocalPos = referenceTransform.InvMapPosition(rayHit.point);
        }
        
        List<GraphNode<AnnotationNodeData>> originalNode = new List<GraphNode<AnnotationNodeData>>();
        graph.ForeachNode(n => originalNode.Add(n));
        GraphNode<AnnotationNodeData> userNode = AddNode(new AnnotationNodeData(AnnotationID.LIGHTALL_ID, userLocalPos));
        userNode.data.depth = 0;

        // normalize distance
        //TODO:.... get bounds and process;

        // Minimum span tree algorithm (Prim), but we have a fixed root, with extra evaluation calculations
        Heap<EdgePickInfo> edgeHeap = new Heap<EdgePickInfo>();

        List<GraphEdge<EdgeDistanceData>> resultEdges = new List<GraphEdge<EdgeDistanceData>>();
        List<GraphNode<AnnotationNodeData>> additionalNode = new List<GraphNode<AnnotationNodeData>>();
        additionalNode.Add(userNode);
        GraphNode<AnnotationNodeData> lastNode = userNode;
        bool flag = true;
        while (resultEdges.Count < graph.NodeCount && flag)
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
                    pi.distance = edge.data.basicDistance + lastNode.data.depth * depthRatio;
                    edgeHeap.Enqueue(pi);
                    Debug.Log($"Push {edge.fromNode}->{edge.toNode} \t| dis = {pi.distance} \t| d={lastNode.data.depth}");
                }
                );
            //pick up a minimum edge, add it to the tree
            while(edgeHeap.Count>0)
            {
                EdgePickInfo minEdge = edgeHeap.Dequeue();
                GraphNode<AnnotationNodeData> newNode = graph.GetNode(minEdge.nodeIndex);
                if (newNode.data.IsVisited)
                {
                    if(edgeHeap.Count==0)
                    {
                        Debug.LogWarning($"No more edge found, the graph isn't fully connected! | edges:{resultEdges.Count} | node:{graph.NodeCount}");
                        flag = false;//break the outer loop
                        break;
                    }
                    continue;
                }
                //Add new edge!
                /*
                GraphEdge<EdgeDistanceData> newEdge = graph.GetEdge(minEdge.edgeIndex);
                newNode.data.depth =
                    graph.GetNode(newEdge.GetAnotherNodeIndex(newNode.index)).data.depth + 1;
                resultEdges.Add(newEdge);*/
                lastNode = newNode;
                // add new node if possible
                GraphEdge<EdgeDistanceData> newEdge = graph.GetEdge(minEdge.edgeIndex);
                GraphNode<AnnotationNodeData> anotherNode = graph.GetNode(newEdge.GetAnotherNodeIndex(newNode.index));
                Debug.Log($"Add edge {newEdge.fromNode}->{newEdge.toNode}");
                //Vector3 dirX = volumeAnalyze.SampleDir(newNode.data.centerPos, Vector3Int.right);
                //Vector3 dirY = volumeAnalyze.SampleDir(newNode.data.centerPos, Vector3Int.up);
                //Vector3 dirZ = volumeAnalyze.SampleDir(newNode.data.centerPos, Vector3Int.forward);

                //Vector3 dirX = Vector3.right;//new Vector3(0.027f, 0, 0.0465f).normalized;////SampleNormal(newNode.data.centerPos, Vector3.right);
                //Vector3 dirY = Vector3.up;//SampleNormal(newNode.data.centerPos, Vector3.up);
                //Vector3 dirZ = Vector3.forward;//SampleNormal(newNode.data.centerPos, Vector3.forward);

                Vector3 dirX = tempX.normalized;//new Vector3(2.0f, 0, 0.1f).normalized;////SampleNormal(newNode.data.centerPos, Vector3.right);
                Vector3 dirY = Vector3.up; //SampleNormal(newNode.data.centerPos, Vector3.up);
                Vector3 dirZ = Vector3.forward; //SampleNormal(newNode.data.centerPos, Vector3.forward);

                XYZCoordinate xyzCoordinate = new XYZCoordinate(dirX, dirY, dirZ);
                xyzCoordinate.Orthogonalization();
                // ensure the line is align with the space
                Vector3 pos1 = xyzCoordinate.TransformToLocalPos(newNode.data.centerPos);
                Vector3 pos2 = xyzCoordinate.TransformToLocalPos(anotherNode.data.centerPos);
                // edge: pos2 (another) ---> pos1 (new)
                Vector3 posDelta = pos1 - pos2;
                //judge x,y,z, if too large, split them
                List<Vector3> posSep = SeperateVector(posDelta);
                if(posSep.Count>1)
                {
                    Vector3 posCur = pos2;
                    GraphNode<AnnotationNodeData> lastNodeInner = anotherNode;
                    for (int i=0;i<posSep.Count-1;i++)
                    {
                        posCur += posSep[i];
                        GraphNode<AnnotationNodeData> interNode = AddNode(
                            new AnnotationNodeData(
                                AnnotationID.INVALID_ID,
                                xyzCoordinate.TransformToGlobalPos(posCur)));
                        additionalNode.Add(interNode);
                        interNode.data.depth = lastNodeInner.data.depth + 1;
                        Debug.Log($"Add node {interNode.index} d={interNode.data.depth}");
                        graph.ForeachNodeNeighbor(
                            interNode,
                            node => { },
                            edge =>
                            {
                                EdgePickInfo pi = new EdgePickInfo();
                                pi.edgeIndex = edge.index;
                                pi.nodeIndex = edge.GetAnotherNodeIndex(interNode.index);
                                pi.distance = edge.data.basicDistance + interNode.data.depth * depthRatio;
                                edgeHeap.Enqueue(pi);
                                Debug.Log($"Push {edge.fromNode}->{edge.toNode} \t| dis = {pi.distance} \t| d={interNode.data.depth}");
                            }
                            );
                        GraphEdge<EdgeDistanceData> interNewEdge = graph.GetEdge(lastNodeInner, interNode);
                        resultEdges.Add(interNewEdge);
                        lastNodeInner = interNode;
                    }
                    // last
                    resultEdges.Add(graph.GetEdge(lastNodeInner, newNode));
                    newNode.data.depth = lastNodeInner.data.depth + 1;
                }
                else
                {
                    newNode.data.depth = anotherNode.data.depth + 1;
                    resultEdges.Add(newEdge);
                    Debug.Log($"Set node depth {newNode.index} d={newNode.data.depth}");
                }
                //addNodeCount++;
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
        foreach(var node in additionalNode)
        {
            graph.RemoveNode(node);
        }
        userNode = treeGraph.GetNode(userNode.index);
        NavigationInfo result = new NavigationInfo();
        result.treeGraph = treeGraph;
        result.root = userNode;
        ClearVisited();

        //further process result
        foreach(var node in originalNode)
        {
            Annotation annot = data.FindAnnotationID(node.data.id);
            Vector3 pos = node.data.centerPos-annot.renderInfo.Normal*normalOffset;
            GraphNode<AnnotationNodeData> annotNode = treeGraph.AddNode(new AnnotationNodeData(AnnotationID.INVALID_ID, pos));
            treeGraph.AddEdge(node, annotNode, new EdgeDistanceData());
        }
        return result;
    }

    protected List<Vector3> SeperateVector(Vector3 dv)
    {
        List<Vector3> results = new List<Vector3>();
        Vector3 cur = Vector3.zero;
        for(int i=0;i<3;i++)
        {
            cur[i] = dv[i];
            if (Mathf.Abs(dv[i])>splitEdgeThreshold)
            {
                results.Add(cur);
                cur = Vector3.zero;
            }
        }
        if(cur!=Vector3.zero)
        {
            results.Add(cur);
        }
        return results;
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

    protected Vector3 SearchHit(Vector3 pos, Vector3 dir)
    {
        RaycastHit hit;
        if (Physics.Raycast(referenceTransform.MapPosition(pos),dir, out hit, Mathf.Infinity,layerMask))
        {
            Debug.DrawRay(referenceTransform.MapPosition(pos), dir * 10.0f, Color.blue);
            return hit.normal;
        }
        Debug.DrawRay(referenceTransform.MapPosition(pos), dir * 10.0f, Color.yellow);
        return Vector3.zero;
    }

    protected Vector3 SampleNormal(Vector3 pos, Vector3 dir)
    {
        Vector3 n1 = SearchHit(pos, dir);
        Vector3 n2 = SearchHit(pos, -dir);
        Vector3 nd = n2 - n1;
        return nd == Vector3.zero ? nd : nd.normalized;
    }

}
