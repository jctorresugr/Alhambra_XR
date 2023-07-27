using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using N = VolumeNavigation.AnnotationNodeData;
using E = VolumeNavigation.EdgeDistanceData;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Physics;

public class NavigationViewLineMapping : MonoBehaviour//, IMixedRealityInputActionHandler
{
    protected Dictionary<int, List<LineRenderer>> maps = new Dictionary<int, List<LineRenderer>>();
    protected Dictionary<LineRenderer, int> renderMaps = new Dictionary<LineRenderer, int>();
    public Graph<N, E> graph;
    public GraphNode<N> rootNode;

    public void Clear()
    {
        maps.Clear();
    }

    public void Record(int edgeIndex, LineRenderer render)
    {
        if(!maps.ContainsKey(edgeIndex))
        {
            maps.Add(edgeIndex, new List<LineRenderer>());
        }
        maps[edgeIndex].Add(render);
        renderMaps.Add(render, edgeIndex);
    }

    //interaction
    /*
    void Update()
    {
        foreach (var inputSource in CoreServices.InputSystem.DetectedInputSources)
        {
            if (inputSource.SourceType == InputSourceType.Hand)
            {
                SearchNearbyLines(inputSource.Pointers[0]);
            }
        }
    }

    protected void SearchNearbyLines(IMixedRealityPointer pointer)
    {
        RayStep rayStep = pointer.Rays[0];
        Ray ray = new Ray(rayStep.Origin, rayStep.Direction);
    }

    void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
    {
    }

    void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
    {
    }*/
}
