using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using N = VolumeNavigation.AnnotationNodeData;
using E = VolumeNavigation.EdgeDistanceData;

public class TreeRenderCurved : BasicRouteGraphRender
{
    [Header("data")]
    public GraphNode<N> rootNode;
    public List<LineRenderer> edgeObjects = new List<LineRenderer>();
    [Header("config")]
    public float alpha = 0.5f;
    public float sampleDistanceRatio = 0.005f;
    public float bendRatio = 0.1f;
    public Gradient color;

    public override void Redraw()
    {
        if (graph == null)
        {
            return;
        }
        ClearDraw();
        DrawNode(rootNode);
    }

    struct StateInfo
    {
        public GraphNode<N> preNode;
        public GraphNode<N> curNode;
        public Vector3 preDrawPos;
        public StateInfo(GraphNode<N> preNode, GraphNode<N> curNode,Vector3 preDrawPos)
        {
            this.preNode = preNode;
            this.curNode = curNode;
            this.preDrawPos = preDrawPos;
        }
    }
    protected void DrawNode(GraphNode<N> rootNode)
    {
        Queue<StateInfo> queue =new Queue<StateInfo>();
        queue.Enqueue(new StateInfo { preNode = null, curNode = rootNode });
        while(queue.Count>0)
        {
            StateInfo stateInfo = queue.Dequeue();
            GraphNode<N> preNode = stateInfo.preNode;
            GraphNode<N> curNode = stateInfo.curNode;
            Debug.Log($"Process: {preNode?.index} -> {curNode.index}");
            Vector3 preDrawPos = stateInfo.preDrawPos;
            if (preNode==null)//root node
            {
                Vector3 centerPos = curNode.data.centerPos;
                RemapPos(ref centerPos);
                graph.ForeachNodeNeighbor(curNode, (nextNode, e) =>
                {
                    Debug.Log($"Add line: root null -> {curNode.index} -> {nextNode.index}");
                    queue.Enqueue(new StateInfo(curNode, nextNode, centerPos));
                });
                continue;
            }

            Vector3 prePos = preNode.data.centerPos;
            Vector3 curPos = curNode.data.centerPos;
            RemapPos(ref prePos);
            RemapPos(ref curPos);
            
            if(curNode.Degree==1) //is leaf
            {
                GenerateSegment(preDrawPos, curPos);
                Debug.Log("Leaf! " + curNode.index);
            }
            else
            {
                Vector3 curDrawPos = Vector3.Lerp(curPos, prePos, bendRatio);
                GenerateSegment(preDrawPos, curDrawPos);
                graph.ForeachNodeNeighbor(curNode, preNode, (nextNode, e) =>
                {
                    Vector3 nextPos = nextNode.data.centerPos;
                    RemapPos(ref nextPos);
                    Vector3 nextDrawPos = Vector3.Lerp(curPos, nextPos, bendRatio);
                    GenerateCurveLine(preDrawPos, curDrawPos, nextDrawPos, nextPos);
                    Debug.Log($"Add line: root {preNode.index} -> {curNode.index} -> {nextNode.index}");
                    queue.Enqueue(new StateInfo(curNode, nextNode,nextDrawPos));
                });
            }
            
        }
        //Vector3 d = curNode.data.centerPos - preNode.data.centerPos;

    }

    protected LineRenderer DuplicateTemplate()
    {
        LineRenderer lineRenderer = Instantiate(template);
        lineRenderer.transform.parent = this.transform;
        lineRenderer.transform.position = Vector3.zero;
        lineRenderer.transform.rotation = Quaternion.identity;
        lineRenderer.colorGradient = color;
        return lineRenderer;
    }

    protected void GenerateSegment(Vector3 from, Vector3 to)
    {
        LineRenderer lineRenderer = DuplicateTemplate();
        lineRenderer.AssignPositionData(from, to);
        edgeObjects.Add(lineRenderer);
    }

    protected void RemapPos(ref Vector3 pos)
    {
        pos = referenceTransform.MapPosition(pos);
    }

    protected void GenerateCurveLine(Vector3 refPre, Vector3 from, Vector3 to,Vector3 refAfter)
    {
        List<Vector3> results = new List<Vector3>();
        results.Add(from);
        CurveSample.GenerateCurveBySegment(
            refPre, from, to, refAfter,
            alpha, annotationData.ReferLength*sampleDistanceRatio*referenceTransform.ScaleRefer, results);
        results.Add(to);
        LineRenderer lineRenderer = DuplicateTemplate();
        lineRenderer.AssignPositionData(results.ToArray());
        edgeObjects.Add(lineRenderer);

    }

    public override void ClearDraw()
    {
        if (edgeObjects != null)
        {
            foreach (LineRenderer lineRender in edgeObjects)
            {
                Destroy(lineRender.gameObject);
            }
        }
        edgeObjects.Clear();
    }

    public override void SetGraphData(Graph<N, E> graph, GraphNode<N> root = null)
    {
        this.graph = graph;
        this.rootNode = root;
    }
}
