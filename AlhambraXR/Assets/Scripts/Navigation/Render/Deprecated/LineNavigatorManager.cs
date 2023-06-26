using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LineNavigatorManager : MonoBehaviour
{
    public float normalOffset = 0.001f;
    public float splitThreshold = 0.05f;
    public List<Annotation> annotations;
    public Transform userTransform; // the position of user
    public ReferenceTransform refTrans; // original position is not real position, we need to transform
    public LineRenderManager lineRenderManager;
    public Vector3 viewOffset = new Vector3(0,-0.1f,0.2f);

    private bool isDirty = false;

    class AnnotationNodeInfo
    {
        public Annotation annot;
        public Vector3 position;
        public int clusterIndex; //-1: no cluster, >=0 cluster
        public LineRenderNode renderDataRef;
    }

    class VisualClusterInfo
    {
        public Vector3 position; //guide strokes will scatter here
        public int clusterIndex;
        public int annotationCount;
        public int preVisualCluster; //-1 means none
        public LineRenderNode renderDataRef;

    }
    // annotations to navigate
    private Dictionary<Annotation, AnnotationNodeInfo> annotationNodeInfos;
    private List<VisualClusterInfo> visualClusterInfos;
    //private List<LineRenderer> renderers;
    private int clusterCount = 0;
    private int rootClusterIndex = -1;

    // Start is called before the first frame update
    void Start()
    {
        Utils.EnsureComponent(this, ref lineRenderManager);
        Utils.EnsureComponent(this, ref refTrans);
    }

    // Update is called once per frame
    void Update()
    {
        if (isDirty) //update in the main thread!
        {
            isDirty = false;
            UpdateUI();
        }
        if (rootClusterIndex>=0)
        {
            VisualClusterInfo rootClusterInfo = visualClusterInfos[rootClusterIndex];
            LineRenderNode rootNode = rootClusterInfo.renderDataRef;
            Vector3 newPos = 
                userTransform.forward * viewOffset.z+
                userTransform.right*viewOffset.x+
                userTransform.up*viewOffset.y;
            rootClusterInfo.position = rootNode.position = newPos;
            lineRenderManager.NotifyUpdatePoint(rootNode);
        }
    }

    public void SetAnnotations(List<Annotation> annotations)
    {
        this.annotations = annotations;
        this.UpdateAnnotations();
    }

    public void SetAnnotations(Annotation annotation)
    {
        annotations.Clear();
        annotations.Add(annotation);
        this.UpdateAnnotations();
    }

    public void UpdateAnnotations()
    {
        UnionSet unionSet = new UnionSet();
        unionSet.SetSize(annotations.Count);
        annotationNodeInfos = new Dictionary<Annotation, AnnotationNodeInfo>();
        int tempIndex = 0;
        foreach(Annotation annot in annotations)
        {
            AnnotationNodeInfo ci = new AnnotationNodeInfo();
            ci.annot = annot;
            //TODO: check wall collision
            ci.position = annot.renderInfo.Center + annot.renderInfo.Normal * normalOffset;
            ci.clusterIndex = tempIndex;//none cluster
            tempIndex++;
            annotationNodeInfos.Add(annot, ci);
        }
        //density based clustering

        Dictionary<Annotation, AnnotationNodeInfo>.ValueCollection clusterInfoValues = annotationNodeInfos.Values;
        float sqrLimits = splitThreshold * splitThreshold;
        foreach (AnnotationNodeInfo ci1 in annotationNodeInfos.Values)
        {
            foreach (AnnotationNodeInfo ci2 in annotationNodeInfos.Values)
            {
                if(ci1.clusterIndex<=ci2.clusterIndex)
                {
                    continue;
                }
                Vector3 delta = ci1.position - ci2.position;
                float distanceSqr = Vector3.Dot(delta, delta);
                if(distanceSqr<=sqrLimits)
                {
                    unionSet.Union(ci1.clusterIndex, ci2.clusterIndex);
                }
            }
        }
        //remap cluster index
        HashSet<int> rootIndex = unionSet.GetAllRootIndex();
        Dictionary<int, int> indexMapping = new Dictionary<int, int>();
        tempIndex = 0;
        foreach(int index in rootIndex)
        {
            indexMapping.Add(index, tempIndex);
            tempIndex++;
        }
        tempIndex = 0;

        foreach(AnnotationNodeInfo ci in annotationNodeInfos.Values)
        {
            ci.clusterIndex = indexMapping[unionSet.FindRoot(tempIndex)];
            tempIndex++;
        }
        clusterCount = indexMapping.Count;

        //finish clustering, process clusters

        visualClusterInfos = new List<VisualClusterInfo>();
        rootClusterIndex = clusterCount;
        for (int i = 0; i <clusterCount;i++)
        {
            visualClusterInfos.Add(new VisualClusterInfo());
            visualClusterInfos[i].clusterIndex = i;
            visualClusterInfos[i].preVisualCluster = rootClusterIndex;
        }

        visualClusterInfos.Add(new VisualClusterInfo());
        visualClusterInfos[rootClusterIndex].position = Vector3.zero;
        visualClusterInfos[rootClusterIndex].annotationCount = 1; //avoid nan later


        // summary position information
        foreach (AnnotationNodeInfo ci in annotationNodeInfos.Values)
        {
            VisualClusterInfo visualClusterInfo = visualClusterInfos[ci.clusterIndex];
            visualClusterInfo.annotationCount++;
            visualClusterInfo.position += ci.position;
        }

        foreach(VisualClusterInfo vci in visualClusterInfos)
        {
            vci.position /= vci.annotationCount;
        }


        // Now we have the cluster
        // Bunch of cluster ... Root cluster 

        //TODO: a better cluster, we may need more cluster, or use graph to compute
        //Since this is a prototype, we just finish the basic idea.

        // Push data to the render manager

        //then update ui in the main thread;
        isDirty = true;
    }

    public void UpdateUI()
    {
        /*
        foreach (VisualClusterInfo vci in visualClusterInfos)
        {
            vci.position = refTrans.MapPosition(vci.position);
        }*/

        // Push data to the render manager
        LineRenderData renderData = lineRenderManager.data = new LineRenderData();
        //node
        foreach (AnnotationNodeInfo ci in annotationNodeInfos.Values)
        {
            LineRenderNode lrn = new LineRenderNode(0, refTrans.MapPosition(ci.position));
            ci.renderDataRef = lrn;
            renderData.AddNode(lrn);
        }
        foreach (VisualClusterInfo vci in visualClusterInfos)
        {
            LineRenderNode lrn = new LineRenderNode(0, refTrans.MapPosition(vci.position));
            vci.renderDataRef = lrn;
            renderData.AddNode(lrn);
        }
        //edge
        foreach (AnnotationNodeInfo ci in annotationNodeInfos.Values)
        {
            LineRenderNode anotherNode = visualClusterInfos[ci.clusterIndex].renderDataRef;
            renderData.AddEdge(ci.renderDataRef, anotherNode);
        }

        foreach (VisualClusterInfo vci in visualClusterInfos)
        {
            if (vci.preVisualCluster < 0)
            {
                continue;
            }
            renderData.AddEdge(vci.renderDataRef, visualClusterInfos[vci.preVisualCluster].renderDataRef);
        }

        lineRenderManager.Redraw();
    }
}
