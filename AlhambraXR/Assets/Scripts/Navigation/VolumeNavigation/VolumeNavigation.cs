using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VolumeNavigation : MonoBehaviour
{
    [Header("Data source")]
    public VolumeAnalyze volumeAnalyze;
    public DataManager data;
    public ReferenceTransform referenceTransform;
    public IHelpNodeProvider helpNodeProvider;
    [Header("Settings")]
    [Header("Settings - Nav Weight")]
    public float depthRatio = 0.01f;
    public float branchRatio = 0.01f;
    public float splitEdgeThreshold = 0.001f;
    public float mainNodeWeight = 0.1f;
    [Header("Settings - Nav Space Adjust")]
    public LayerMask layerMask = 1 >> 6;
    public float normalOffset = 0.001f;
    public Vector3 tempX = Vector3.right;
    //public float clipEdgeThreshold=0.2f;
    [Header("Cache Data")]
    [SerializeField]
    Graph<AnnotationNodeData, EdgeDistanceData> graph;
    [Serializable]
    public class NavigationInfo
    {
        public Graph<AnnotationNodeData, EdgeDistanceData> treeGraph;
        public GraphNode<AnnotationNodeData> root;
    }

    public struct AnnotationNodeData
    {
        public AnnotationID id;
        public Vector3 centerPos;
        public XYZCoordinate coord;
        //Add more if required

        public AnnotationNodeData(AnnotationID id, Vector3 centerPos)
        {
            this.id = id;
            this.centerPos = centerPos;
            this.depth = -1;
            this.coord = new XYZCoordinate();
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

    public void Init()
    {
        graph = new Graph<AnnotationNodeData, EdgeDistanceData>();
        volumeAnalyze.Preprocess();
    }

    protected Vector3 GetAnnotationPos(Annotation annot)
    {
        return annot.renderInfo.averagePosition + annot.renderInfo.Normal * normalOffset;
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

    public NavigationInfo Navigate(IReadOnlyList<AnnotationID> annotationIDs, Vector3 userLocalPos, Vector3 suggestForward)
    {
        List<Annotation> filteredAnnots = new List<Annotation>();
        foreach(AnnotationID id in annotationIDs)
        {
            Annotation annotation = data.FindAnnotationID(id);
            if(annotation.IsValid)
            {
                filteredAnnots.Add(annotation);
            }
        }
        return Navigate(filteredAnnots, userLocalPos, suggestForward);
    }

    public NavigationInfo Navigate(List<Annotation> annotations, Vector3 userLocalPos)
    {
        return Navigate(annotations, userLocalPos, Vector3.zero);
    }
    /**
     * pass the userTransform and begin to navigate! 
     */
    public NavigationInfo Navigate(List<Annotation> annotations, Vector3 userLocalPos, Vector3 suggestForward)
    {
        annotations = annotations.FindAll(a => a.IsValid && a.renderInfo != null && a.renderInfo.IsValid);
        //=========================
        //preprocess
        graph.Clear();
        int annotCount = annotations.Count;

        for (int i = 0; i < annotCount; i++)
        {
            graph.AddNode(new AnnotationNodeData(annotations[i].ID, GetAnnotationPos(annotations[i])));
            Debug.Log($"{annotations[i].ID} .bounds => {annotations[i].renderInfo.Bounds}");
        }

        //Fetch helpNode
        List<Vector3> helpNodes = helpNodeProvider.GetHelpNodePositions();
        foreach(Vector3 v in helpNodes)
        {
            graph.AddNode(new AnnotationNodeData(AnnotationID.INVALID_ID, v));
        }

        for (int i = 0; i < annotCount; i++)
        {
            GraphNode<AnnotationNodeData> ai = graph.GetNode(i);
            for (int j = i + 1; j < annotCount; j++)
            {
                GraphNode<AnnotationNodeData> aj = graph.GetNode(j);
                if(isHit(ai.data.centerPos,aj.data.centerPos))
                {
                    continue;
                }
                graph.AddEdge(i, j, new EdgeDistanceData(Distance(ai, aj)));
            }
        }

        //debug code
        graph.ForeachNode(
            n =>
            {
                if(n.Degree==0)
                {
                    Debug.LogWarning("Cannot find a link for node " + n.index + " \t| id=" + n.data.id);
                }
            });

        float modelScaleFactor = 1.0f / data.modelBounds.size.magnitude;

        //==================================
        //add user position
        userLocalPos = referenceTransform.InvMapPosition(userLocalPos);
        
        List<GraphNode<AnnotationNodeData>> originalNode = new List<GraphNode<AnnotationNodeData>>();
        graph.ForeachNode(n => originalNode.Add(n));
        GraphNode<AnnotationNodeData> userNode = AddNode(new AnnotationNodeData(AnnotationID.LIGHTALL_ID, userLocalPos));
        userNode.data.depth = 0;

        //==================================
        // begin!
        // Minimum span tree algorithm (Prim), but we have a fixed root, with extra evaluation calculations
        Heap<EdgePickInfo> edgeHeap = new Heap<EdgePickInfo>();

        List<GraphEdge<EdgeDistanceData>> resultEdges = new List<GraphEdge<EdgeDistanceData>>();
        List<GraphNode<AnnotationNodeData>> additionalNode = new List<GraphNode<AnnotationNodeData>>();
        additionalNode.Add(userNode);
        GraphNode<AnnotationNodeData> lastNode = userNode;
        bool flag = true;
        while (resultEdges.Count < (graph.NodeCount-1) && flag)
        {
            //add Edge
            graph.ForeachNodeNeighbor(
                lastNode,
                (node,edge)=>
                {
                    EdgePickInfo pi = new EdgePickInfo();
                    pi.edgeIndex = edge.index;
                    pi.nodeIndex = edge.GetAnotherNodeIndex(lastNode.index);
                    pi.distance = 
                        edge.data.basicDistance + 
                        lastNode.data.depth * depthRatio * modelScaleFactor + 
                        lastNode.Degree * branchRatio * modelScaleFactor +
                        (lastNode.data.id.IsValid ? 0:mainNodeWeight) +
                        (node.data.id.IsValid ? 0: mainNodeWeight)
                        ;
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
                lastNode = newNode;
                // add new node if possible
                GraphEdge<EdgeDistanceData> newEdge = graph.GetEdge(minEdge.edgeIndex);
                GraphNode<AnnotationNodeData> anotherNode = graph.GetNode(newEdge.GetAnotherNodeIndex(newNode.index));
                Debug.Log($"Add edge {newEdge.fromNode}->{newEdge.toNode}");
                //Vector3 dirX = volumeAnalyze.SampleDir(newNode.data.centerPos, Vector3Int.right);
                //Vector3 dirY = volumeAnalyze.SampleDir(newNode.data.centerPos, Vector3Int.up);
                //Vector3 dirZ = volumeAnalyze.SampleDir(newNode.data.centerPos, Vector3Int.forward);

                Vector3 dirX = tempX.normalized;//new Vector3(2.0f, 0, 0.1f).normalized;////SampleNormal(newNode.data.centerPos, Vector3.right);
                Vector3 dirY = Vector3.up; //SampleNormal(newNode.data.centerPos, Vector3.up);
                Vector3 dirZ = Vector3.forward; //SampleNormal(newNode.data.centerPos, Vector3.forward);
                XYZCoordinate xyzCoordinate = new XYZCoordinate(dirX, dirY, dirZ);
                newNode.data.coord = xyzCoordinate;
                xyzCoordinate.Orthogonalization();
                // ensure the line is align with the space
                Vector3 pos1 = xyzCoordinate.TransformToLocalPos(newNode.data.centerPos);
                Vector3 pos2 = xyzCoordinate.TransformToLocalPos(anotherNode.data.centerPos);
                // edge: pos2 (another) ---> pos1 (new)
                Vector3 posDelta = pos1 - pos2;
                //judge x,y,z, if too large, split them
                List<Vector3> posSep = SeperateVector(posDelta,pos1,pos2,suggestForward);
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
                        interNode.data.coord = xyzCoordinate;
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
        NavigationInfo result = new NavigationInfo();
        result.treeGraph = treeGraph;
        result.root = userNode;

        //remove useless leaf node
        List<GraphNode<AnnotationNodeData>> removeNodes = new List<GraphNode<AnnotationNodeData>>();
        while(true)
        {
            treeGraph.ForeachNode(
            n =>
            {
                if (n.Degree==1 && n.data.id == AnnotationID.INVALID_ID)
                {
                    removeNodes.Add(n);
                    Debug.Log("Remove tree node " + n.index);
                }
            });
            if(removeNodes.Count==0)
            {
                break;
            }
            foreach(var n in removeNodes)
            {
                treeGraph.RemoveNode(n);
            }
            removeNodes.Clear();
        }
        

        //further process result
        foreach(var node in originalNode)
        {
            if(!node.data.id.IsValid)
            {
                continue;
            }
            Annotation annot = data.FindAnnotationID(node.data.id);
            Vector3 pos = annot.renderInfo.averagePosition;
            GraphNode<AnnotationNodeData> annotNode = treeGraph.AddNode(new AnnotationNodeData(AnnotationID.INVALID_ID, pos));
            treeGraph.AddEdge(node.index, annotNode.index, new EdgeDistanceData());
        }
        return result;
    }

    private static readonly int[,] iterSeq2 = new int[,]
            {
                { 0,1},
                { 1,0}
            };
    private static readonly int[,] iterSeq3 = new int[,]
            {
                { 0,1,2},
                { 0,2,1},
                { 1,0,2},
                { 1,2,0},
                { 2,1,0},
                { 2,0,1},
            };
    protected List<Vector3> SeperateVector(Vector3 dv,Vector3 posTo, Vector3 posFrom,Vector3 suggestForward)
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
        if(results.Count>1)
        {
            List<Vector3> bestResult = results;
            float bestScore = -100.0f;
            int[,] seq = results.Count == 2 ? iterSeq2 : iterSeq3;
            for(int seqi=0;seqi<seq.GetLength(0);seqi++)
            {
                Vector3 curPos = posFrom;
                bool isImpeded = false;
                for (int i0 = 0; i0 < results.Count; i0++)
                {
                    int i = seq[seqi, i0];
                    Vector3 interPos = curPos + results[i];
                    Vector3 worldCurPos = referenceTransform.MapPosition(curPos);
                    Vector3 worldInterPos = referenceTransform.MapPosition(interPos);
                    if (Physics.Raycast(worldCurPos, results[i].normalized, (worldInterPos - worldCurPos).magnitude, layerMask))
                    {
                        isImpeded = true;
                        break;
                    }
                    curPos = interPos;
                }
                if(!isImpeded)
                {
                    float score = Vector3.Dot(suggestForward, results[seq[seqi, 0]]);
                    if(score>bestScore)
                    {
                        List<Vector3> sortedResult = new List<Vector3>();
                        for (int i0 = 0; i0 < results.Count; i0++)
                        {
                            int i = seq[seqi, i0];
                            sortedResult.Add(results[i]);
                        }
                        bestScore = score;
                        bestResult = sortedResult;
                    }
                    
                }
            }
            if(bestScore<=0)
            {
                Debug.LogWarning($"Navigate line may hit wall {posFrom} -> {posTo}");
            }
            return bestResult;
            
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

    protected bool isHit(Vector3 modelPos1,Vector3 modelPos2)
    {
        modelPos1 = referenceTransform.MapPosition(modelPos1);
        modelPos2 = referenceTransform.MapPosition(modelPos2);
        Vector3 d = modelPos2 - modelPos1;
        if (d == Vector3.zero)
        {
            return false;
        }
        return Physics.Raycast(modelPos1, d.normalized, d.magnitude, layerMask);
    }

}
