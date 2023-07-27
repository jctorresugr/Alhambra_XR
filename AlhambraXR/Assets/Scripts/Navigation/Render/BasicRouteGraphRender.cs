using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

using N = VolumeNavigation.AnnotationNodeData;
using E = VolumeNavigation.EdgeDistanceData;


public abstract class BasicRouteGraphRender: MonoBehaviour
{
    [Header("template and set up")]
    public LineRenderer template;
    public DataManager annotationData;
    public ReferenceTransform referenceTransform;
    public NavigationViewLineMapping mapping;
    [Header("data")]
    public Graph<N, E> graph = null;

    public abstract void Redraw();
    public abstract void ClearDraw();
    public abstract void SetGraphData(Graph<N, E> graph, GraphNode<N> root = null);
}
