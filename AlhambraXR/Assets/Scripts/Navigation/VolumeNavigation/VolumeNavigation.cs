using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class VolumeNavigation : MonoBehaviour
{
    [Header("Data source")]
    //public VolumeAnalyze volumeAnalyze;
    public DataManager data;
    public ReferenceTransform referenceTransform;
    public IHelpNodeProvider helpNodeProvider;
    [Header("Settings")]
    [Header("Settings - Nav Weight (Deprecated)")]
    public float depthRatio = 0.01f;
    public float branchRatio = 0.01f;
    public float splitEdgeThreshold = 0.001f;
    public float mainNodeWeight = 0.1f;
    public Vector3 tempX = Vector3.right;
    public bool useProjection = true;
    public float projectionExceedRatio = 0.05f;
    [Header("Settings - Nav Space Adjust")]
    public LayerMask layerMask = 1 >> 6;
    public float normalOffset = 0.001f;
    //public float clipEdgeThreshold=0.2f;
    [Header("Cache Data")]
    [SerializeField]
    Graph<AnnotationNodeData, EdgeDistanceData> graph;
    [Header("Debug")]
    public bool returnWholeGraph = false;
    [Serializable]
    public class NavigationInfo
    {
        public Graph<AnnotationNodeData, EdgeDistanceData> treeGraph;
        public GraphNode<AnnotationNodeData> root;
        public List<Annotation> annotations;
    }

    public struct AnnotationNodeData
    {
        public int index; //useless, only for computation temporary storage
        public bool resultSet; //useless, only for computation temporary storage
        public AnnotationID id;
        public Vector3 centerPos;
        //public XYZCoordinate coord;
        //Add more if required

        public AnnotationNodeData(AnnotationID id, Vector3 centerPos)
        {
            this.id = id;
            this.centerPos = centerPos;
            this.depth = -1;
            //this.coord = new XYZCoordinate();
            this.index = -1;
            this.resultSet = false;
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
        //volumeAnalyze.Preprocess();
    }

    protected Vector3 GetAnnotationPos(Annotation annot)
    {
        return annot.renderInfo.averagePosition + 
            annot.renderInfo.OBBBounds.extents.y*annot.renderInfo.CreateCoordinate().y + 
            annot.renderInfo.Normal * normalOffset;
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

   
    //add temp node
    public GraphNode<AnnotationNodeData> AddNode(AnnotationNodeData nodeData)
    {
        GraphNode<AnnotationNodeData> graphNode = graph.AddNode(nodeData);
        graph.ForeachNode(n =>
        {
            if (n != graphNode && !n.data.IsVisited)
            {
                if(!IsHit(nodeData.centerPos, n.data.centerPos))
                {
                    float distance = Distance(n, graphNode);
                    graph.AddEdge(n, graphNode, new EdgeDistanceData(distance));
                }
            }
        });
        return graphNode;
    }

    protected GraphEdge<EdgeDistanceData> AddEdge(GraphNode<AnnotationNodeData> n1, GraphNode<AnnotationNodeData> n2)
    {
        return graph.AddEdge(n1, n2, new EdgeDistanceData(Vector3.Distance(n1.data.centerPos, n2.data.centerPos)));
    }
    public struct EdgeExtraExtendInfo
    {
        /*
         * O
         * |
         * | < fromEdge
         * |
         * |--------O < nodeTo
         * |\
         * | position
         * O
         */
        public int nodeTo; 
        public int fromEdge;
        public Vector3 position;
    }
    public struct EdgePickInfo: IComparable<EdgePickInfo>
    {
        public int edgeIndex;
        public int nodeIndex;
        public float distance;

        //if true, then the edge and node not exists in the graph
        //we need add it when we want to use this edge
        public bool isFakeEdge;
        public EdgeExtraExtendInfo info;

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

    class DijkstraTempRecord : IComparable<DijkstraTempRecord>
    {
        public GraphNode<AnnotationNodeData> node;
        public GraphNode<AnnotationNodeData> preNode=null;
        public float minDistance = float.MaxValue;
        public bool optimized = false;

        public int CompareTo(DijkstraTempRecord other)
        {
            return this.minDistance.CompareTo(other.minDistance);
        }

    }

    public NavigationInfo Navigate(List<Annotation> annotations, Vector3 userLocalPos, Vector3 suggestForward)
    {
        annotations = annotations.FindAll(a => a.IsValid && a.renderInfo != null && a.renderInfo.IsValid);

        //=========================
        //preprocess: add nodes, compute edges
        //TODO: cache this graph
        graph.Clear();

        for (int i = 0; i < annotations.Count; i++)
        {
            graph.AddNode(new AnnotationNodeData(annotations[i].ID, GetAnnotationPos(annotations[i])));
            Debug.Log($"{annotations[i].ID} .bounds => {annotations[i].renderInfo.Bounds}");
        }


        List<GraphNode<AnnotationNodeData>> targetNodes = new List<GraphNode<AnnotationNodeData>>();
        graph.ForeachNode(n => targetNodes.Add(n));

        //Fetch helpNode
        if (helpNodeProvider != null)
        {
            List<Vector3> helpNodes = helpNodeProvider.GetHelpNodePositions();
            foreach (Vector3 v in helpNodes)
            {
                graph.AddNode(new AnnotationNodeData(AnnotationID.INVALID_ID, v));
            }
        }


        for (int i = 0; i < graph.NodeCount; i++)
        {
            GraphNode<AnnotationNodeData> ai = graph.GetNode(i);
            for (int j = i + 1; j < graph.NodeCount; j++)
            {
                GraphNode<AnnotationNodeData> aj = graph.GetNode(j);
                if (IsHit(ai.data.centerPos, aj.data.centerPos))
                {
                    continue;
                }
                graph.AddEdge(i, j, new EdgeDistanceData(Distance(ai, aj)));
            }
        }

        //==================================
        //add user position
        userLocalPos = referenceTransform.InvMapPosition(userLocalPos);

        GraphNode<AnnotationNodeData> userNode = AddNode(new AnnotationNodeData(AnnotationID.LIGHTALL_ID, userLocalPos));
        List<GraphNode<AnnotationNodeData>> originalNode = new List<GraphNode<AnnotationNodeData>>();
        graph.ForeachNode(n => originalNode.Add(n));
        userNode.data.depth = 0;
        userNode.data.index = -1;

        //===========================
        //Begin!

        //> graph node index, record
        List<DijkstraTempRecord> candidate = new List<DijkstraTempRecord>();
        List<DijkstraTempRecord> results = new List<DijkstraTempRecord>();


        void AddNodeToCandidate(GraphNode<AnnotationNodeData> n)
        {
            DijkstraTempRecord rec = new DijkstraTempRecord();
            rec.node = n;
            n.data.index = candidate.Count;
            GraphEdge<EdgeDistanceData> edge = graph.GetEdge(n, userNode);
            if(edge!=null)
            {
                rec.minDistance = edge.data.basicDistance;
                rec.preNode = userNode;
            }
            candidate.Add(rec);
        }

        DijkstraTempRecord GetRecord(GraphNode<AnnotationNodeData> n)
        {
            int ind = n.data.index;
            if(ind<0)
            {
                return null;
            }
            if(n.data.resultSet)
            {
                return results[ind];
            }
            else
            {
                return candidate[ind];
            }
        }

        graph.ForeachNodeExcept(userNode,AddNodeToCandidate);


        float minDistance = float.MaxValue;
        int curIndex=-1;
        for(int i=0;i<candidate.Count;i++)
        {
            DijkstraTempRecord rec = candidate[i];
            if (rec.minDistance<minDistance)
            {
                minDistance = rec.minDistance;
                curIndex = i;
            }
        }
        while(candidate.Count>0 && curIndex>=0)
        {
            DijkstraTempRecord curRec = candidate[curIndex];
            curRec.optimized = true;
            curRec.node.data.index = results.Count;
            curRec.node.data.resultSet = true;
            results.Add(curRec);
            candidate.RemoveBySwap(curIndex);
            if(candidate.Count==0)
            {
                break;
            }
            if(curIndex<candidate.Count)
            {
                candidate[curIndex].node.data.index = curIndex;
            }
            

            graph.ForeachNodeNeighbor(curRec.node,
                (nn, ne) =>
                {
                    DijkstraTempRecord nnRec = GetRecord(nn);
                    if(nnRec==null)
                    {
                        return;
                    }
                    float newDistance = curRec.minDistance + ne.data.basicDistance;
                    if(newDistance<nnRec.minDistance)
                    {
                        nnRec.minDistance = newDistance;
                        nnRec.preNode = curRec.node;
                       
                    }
                });
            minDistance = float.MaxValue;
            curIndex = -1;
            for (int i = 0; i < candidate.Count; i++)
            {
                DijkstraTempRecord rec = candidate[i];
                if (rec.minDistance < minDistance)
                {
                    minDistance = rec.minDistance;
                    curIndex = i;
                }
            }
        }

        Graph<AnnotationNodeData, EdgeDistanceData> treeGraph = new Graph<AnnotationNodeData, EdgeDistanceData>();
        treeGraph.CopyNodes(graph);
        foreach(var targetNode in targetNodes)
        {
            DijkstraTempRecord targetRec = results[targetNode.data.index];
            if(targetRec.node.Index!=targetNode.Index)
            {
                Debug.LogWarning("Error index: " + targetNode.Index);
            }

            while(true)
            {
                GraphEdge<EdgeDistanceData> edge = treeGraph.GetEdge(targetRec.node.Index, targetRec.preNode.Index);
                if(edge==null)
                {
                    treeGraph.AddEdge(targetRec.node.Index, targetRec.preNode.Index, new EdgeDistanceData());
                }
                int preIndex = targetRec.preNode.data.index;
                if(preIndex<0)
                {
                    break;
                }
                targetRec = results[preIndex];
            }
        }

        // add an extra node, point to the center of annotation
        foreach (var node in originalNode)
        {
            if (!node.data.id.IsValid)
            {
                continue;
            }
            Annotation annot = data.FindAnnotationID(node.data.id);
            Vector3 pos = annot.renderInfo.averagePosition;
            GraphNode<AnnotationNodeData> annotNode = treeGraph.AddNode(new AnnotationNodeData(AnnotationID.INVALID_ID, pos));
            treeGraph.AddEdge(node.Index, annotNode.Index, new EdgeDistanceData());
        }

        userNode = treeGraph.GetNode(userNode.Index);
        NavigationInfo result = new NavigationInfo();
        result.treeGraph = treeGraph;
        result.root = userNode;
        result.annotations = annotations;

        return result;
    }
    /**
     * pass the userTransform and begin to navigate! 
     */

    /*
    public NavigationInfo Navigate2(List<Annotation> annotations, Vector3 userLocalPos, Vector3 suggestForward)
    {
        annotations = annotations.FindAll(a => a.IsValid && a.renderInfo != null && a.renderInfo.IsValid);
        //=========================
        //preprocess
        graph.Clear();

        for (int i = 0; i < annotations.Count; i++)
        {
            graph.AddNode(new AnnotationNodeData(annotations[i].ID, GetAnnotationPos(annotations[i])));
            Debug.Log($"{annotations[i].ID} .bounds => {annotations[i].renderInfo.Bounds}");
        }

        //Fetch helpNode
        if(helpNodeProvider!=null)
        {
            List<Vector3> helpNodes = helpNodeProvider.GetHelpNodePositions();
            foreach (Vector3 v in helpNodes)
            {
                graph.AddNode(new AnnotationNodeData(AnnotationID.INVALID_ID, v));
            }
        }
        

        for (int i = 0; i < graph.NodeCount; i++)
        {
            GraphNode<AnnotationNodeData> ai = graph.GetNode(i);
            for (int j = i + 1; j < graph.NodeCount; j++)
            {
                GraphNode<AnnotationNodeData> aj = graph.GetNode(j);
                if(IsHit(ai.data.centerPos,aj.data.centerPos))
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
                    Debug.LogWarning("Cannot find a link for node " + n.Index + " \t| id=" + n.data.id);
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

        //debug code
        if(returnWholeGraph)
        {
            return new NavigationInfo { root = userNode, treeGraph = graph };
        }
        
        //functions
        // add a simulated edge
        void AddFakeEdge(GraphEdge<EdgeDistanceData> edge)
        {
            if(!useProjection)
            {
                return;
            }
            GraphNode<AnnotationNodeData> fromNode = graph.GetNode(edge.fromNode);
            GraphNode<AnnotationNodeData> toNode = graph.GetNode(edge.toNode);
            Vector3 fromPos = fromNode.data.centerPos;
            Vector3 toPos = toNode.data.centerPos;
            Vector3 dPos = toPos - fromPos;
            float maxDis = dPos.magnitude;
            if (dPos == Vector3.zero) // || maxDis<splitEdgeThreshold)
            {
                return;
            }
            // find visible nodes
            HashSet<GraphNode<AnnotationNodeData>> visibleNodes = new HashSet<GraphNode<AnnotationNodeData>>();
            graph.ForeachNodeNeighbor(fromNode,n => visibleNodes.Add(n));
            graph.ForeachNodeNeighbor(toNode,n => visibleNodes.Add(n));
            Vector3 dir = dPos.normalized;
            float depth = Mathf.Min(fromNode.data.depth, toNode.data.depth) + 1;
            foreach (var n in visibleNodes)
            {
                Vector3 nPos = n.data.centerPos;
                float dis = Vector3.Dot(dir, nPos-fromPos);
                if(dis<-maxDis* projectionExceedRatio || dis>maxDis*(1+ projectionExceedRatio))
                {
                    continue;
                }
                Vector3 intersectPos = fromPos + dis * dir;
                if(IsHit(nPos,intersectPos))
                {
                    continue;
                }
                //make fake edge
                EdgePickInfo epi = new EdgePickInfo();
                epi.isFakeEdge = true;
                epi.info.fromEdge = edge.index;
                epi.info.nodeTo = n.Index;
                epi.info.position = intersectPos;
                epi.distance = FakeEdgeDistance(n, depth, dis);
                edgeHeap.Enqueue(epi);
            }
        }

        float lastCost = 0.0f;

        void AddCandidateEdge(GraphNode<AnnotationNodeData> interNode)
        {
            graph.ForeachNodeNeighbor(
                 interNode,
                 (node, edge) =>
                 {
                     EdgePickInfo pi = new EdgePickInfo();
                     pi.edgeIndex = edge.Index;
                     pi.nodeIndex = edge.GetAnotherNodeIndex(interNode.Index);
                     pi.distance = lastCost + EdgeDistance(interNode, node, edge);
                     edgeHeap.Enqueue(pi);
                     //Debug.Log($"Push {edge.fromNode}->{edge.toNode} \t| dis = {pi.distance} \t| d={interNode.data.depth}");
                 }
                 );
        }

        //Begin!
        while (resultEdges.Count < (graph.NodeCount-1))
        {
            //add Edge
            AddCandidateEdge(lastNode);
            //pick up a minimum edge, add it to the tree
            
            while(edgeHeap.Count>0)
            {
                EdgePickInfo minEdge = edgeHeap.Dequeue();
                GraphNode<AnnotationNodeData> newNode = null;
                if (minEdge.isFakeEdge)
                {
                    newNode = graph.GetNode(minEdge.info.nodeTo);
                }else
                {
                    newNode = graph.GetNode(minEdge.nodeIndex);
                }
                
                
                if (newNode.data.IsVisited)
                {
                    continue;
                }
                //Add new edge!
                // add new node if possible
                GraphEdge<EdgeDistanceData> newEdge;
                GraphNode<AnnotationNodeData> anotherNode;
                if (minEdge.isFakeEdge)
                {
                    GraphEdge<EdgeDistanceData> oldEdge = graph.GetEdge(minEdge.info.fromEdge);
                    GraphNode<AnnotationNodeData> fromNode = graph.GetNode(oldEdge.fromNode);
                    GraphNode<AnnotationNodeData> toNode = graph.GetNode(oldEdge.toNode);

                    if(!resultEdges.Remove(oldEdge))
                    {
                        //already removed
                        continue;
                    }
                    
                    GraphNode<AnnotationNodeData> newIntersectNode =
                        AddNode(new AnnotationNodeData(AnnotationID.INVALID_ID, minEdge.info.position));
                    AddCandidateEdge(newIntersectNode);
                    newIntersectNode.data.depth = Mathf.Min(fromNode.data.depth, toNode.data.depth);

                    GraphEdge<EdgeDistanceData> edge1 = AddEdge(fromNode, newIntersectNode);
                    GraphEdge<EdgeDistanceData> edge2 = AddEdge(newIntersectNode, toNode);
                    newEdge = graph.GetEdge(newIntersectNode, newNode);
                    anotherNode = newIntersectNode;

                    if (newEdge == null)
                    {
                        continue;
                    }
                    resultEdges.Add(edge1);
                    resultEdges.Add(edge2);
                    AddFakeEdge(edge1);
                    AddFakeEdge(edge2);

                    Debug.Log($"--- Try to Add fake edge {newEdge.fromNode}->{newEdge.toNode} (New node:{newIntersectNode.Index}) Insert in {oldEdge.fromNode}=>{oldEdge.toNode}");
                    Debug.Log($"Add node (proj) {newIntersectNode.Index} d={newIntersectNode.data.depth}");

                    Debug.Log($"Remove edge {oldEdge.fromNode}->{oldEdge.toNode}");
                    Debug.Log($"Add edge {fromNode.Index}->{newIntersectNode.Index}");
                    Debug.Log($"Add edge {newIntersectNode.Index}->{toNode.Index}");
                    
                    newNode.data.depth = newIntersectNode.data.depth + 1;
                    resultEdges.Add(newEdge);
                    AddFakeEdge(newEdge);
                    break;
                }
                else
                {
                    newEdge = graph.GetEdge(minEdge.edgeIndex);
                    anotherNode = graph.GetNode(newEdge.GetAnotherNodeIndex(newNode.Index));
                    Debug.Log($"--- Try to Add edge {newEdge.fromNode}->{newEdge.toNode}");
                }

                lastNode = newNode;
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
                if(posSep==null)
                {
                    continue;
                }
                if (posSep.Count>1)
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
                        interNode.data.depth = lastNodeInner.data.depth;
                        interNode.data.coord = xyzCoordinate;
                        Debug.Log($"Add node (coord) {interNode.Index} d={interNode.data.depth}");
                        AddCandidateEdge(interNode);

                        GraphEdge<EdgeDistanceData> interNewEdge = 
                            graph.AddEdge(lastNodeInner, interNode,
                            new EdgeDistanceData(Vector3.Distance(lastNode.data.centerPos,interNode.data.centerPos)));//graph.GetEdge(lastNodeInner, interNode);
                        resultEdges.Add(interNewEdge);
                        Debug.Log($"Add edge {interNewEdge.fromNode}->{interNewEdge.toNode}");
                        AddFakeEdge(interNewEdge);
                        lastNodeInner = interNode;
                    }
                    // last
                    GraphEdge<EdgeDistanceData> lastEdge = graph.GetEdge(lastNodeInner, newNode);
                    if(lastEdge==null)
                    {
                        lastEdge = AddEdge(lastNodeInner, newNode); 
                        Debug.LogWarning($"Lack edge: Add edge {lastEdge.fromNode}->{lastEdge.toNode}");
                    }
                    resultEdges.Add(lastEdge);
                    Debug.Log($"Add edge {lastEdge.fromNode}->{lastEdge.toNode}");
                    newNode.data.depth = lastNodeInner.data.depth + 1;
                    AddFakeEdge(lastEdge);
                }
                else
                {
                    newNode.data.depth = anotherNode.data.depth + 1;
                    resultEdges.Add(newEdge);
                    Debug.Log($"Add edge {newEdge.fromNode}->{newEdge.toNode}");
                    AddFakeEdge(newEdge);
                }
                break;
            }
            if (edgeHeap.Count == 0)
            {
                Debug.LogWarning($"Cannot Navigate, not fully connected :( E:{resultEdges.Count} & N:{graph.NodeCount}");
                break;
            }
        }

        //result
        Graph<AnnotationNodeData, EdgeDistanceData> treeGraph = new Graph<AnnotationNodeData, EdgeDistanceData>();
        //build graph
        treeGraph.CopyNodes(graph);
        foreach(GraphEdge<EdgeDistanceData> edge in resultEdges)
        {
            treeGraph.AddEdge(edge.fromNode, edge.toNode, new EdgeDistanceData(edge.data.basicDistance));
        }

        userNode = treeGraph.GetNode(userNode.Index);
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
                    Debug.Log("Remove tree node " + n.Index);
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
            treeGraph.AddEdge(node.Index, annotNode.Index, new EdgeDistanceData());
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
                    // avoid y axis first
                    if(results[seq[seqi, 0]].y>splitEdgeThreshold)
                    {
                        score *= 0.9f;
                    }else if (results[seq[seqi, 1]].y > splitEdgeThreshold)
                    {
                        score *= 0.95f;
                    }

                    if (score>bestScore)
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
                return null;
            }
            return bestResult;
            
        }
        return results;
    }*/

    //return minimumY
    /*
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
    }*/

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

    protected bool IsHit(Vector3 modelPos1,Vector3 modelPos2)
    {
        modelPos1 = referenceTransform.MapPosition(modelPos1);
        modelPos2 = referenceTransform.MapPosition(modelPos2);
        Vector3 d = modelPos2 - modelPos1;
        if (d == Vector3.zero)
        {
            return false;
        }
        return Physics.Raycast(modelPos1, d.normalized, d.magnitude*0.9f, layerMask);
    }

    protected float EdgeDistance(
        GraphNode<AnnotationNodeData> nodeFrom,
        GraphNode<AnnotationNodeData> nodeTo,
        GraphEdge<EdgeDistanceData> edge
        )
    {
        float modelScaleFactor = 1.0f / data.modelBounds.size.magnitude;

        return edge.data.basicDistance +
            nodeFrom.data.depth * depthRatio * modelScaleFactor +
            nodeFrom.Degree * branchRatio * modelScaleFactor +
            (nodeFrom.data.id.IsValid ? 0 : mainNodeWeight) +
            (nodeTo.data.id.IsValid ? 0 : mainNodeWeight)
            ;
    }

    protected float FakeEdgeDistance(
        GraphNode<AnnotationNodeData> nodeTo,
        float depth,
        float basicDistance
        )
    {
        float modelScaleFactor = 1.0f / data.modelBounds.size.magnitude;
        return basicDistance +
            depth * depthRatio * modelScaleFactor +
            2 * branchRatio * modelScaleFactor +
            (nodeTo.data.id.IsValid ? 0 : mainNodeWeight)
            ;
    }

}
