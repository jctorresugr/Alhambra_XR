using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SurfaceAnalysisRANSAC;

// step 2, analyze scattered surfaces
//TODO: resolve non-convex situation
public class SurfaceAnalyzeSemantic : MonoBehaviour
{
    public float degreeThreshold = 30;
    public float distanceThreshold = 0.01f;
    public float cosThreshold => Mathf.Cos(degreeThreshold / 180.0f * 3.14159265358979f);
    public bool ignoreThisProcess = false;

    public SurfaceInfoBundle StartCompute(SurfaceInfoBundle surfaceInfos)
    {
        if (ignoreThisProcess)
        {
            return surfaceInfos;
        }
        SurfaceInfoBundle result = null;
        int count = 0;
        while(true)
        {
            count++;
            result = StartComputeInner(surfaceInfos);
            Debug.Log($" Post Analyze {count} times | new:{result.surfaceInfos.Count} | old: {surfaceInfos.surfaceInfos.Count}");
            if (result.surfaceInfos.Count==surfaceInfos.surfaceInfos.Count)
            {
                foreach(var surface in result.surfaceInfos)
                {
                    //surface.RecalculateBounds();
                }
                return result;
            }
            surfaceInfos = result;
        }
    }

    protected SurfaceInfoBundle StartComputeInner(SurfaceInfoBundle surfaceInfos)
    {
        //TODO: Cluster surface based on position and normal degree
        List<SurfaceInfo> oldSurfaces = surfaceInfos.surfaceInfos;
        List<SurfaceInfo> newSurfaces = new List<SurfaceInfo>();

        bool[] used = new bool[oldSurfaces.Count];
        int[] oldSurfaceRef = new int[oldSurfaces.Count];
        for(int i=0;i<used.Length;i++)
        {
            used[i] = false;
            oldSurfaceRef[i] = -1;
        }
        Queue<int> bfs = new Queue<int>();
        /*
        for(int baseSurfaceIndex=0;baseSurfaceIndex<oldSurfaces.Count;baseSurfaceIndex++)
        {
            SurfaceInfo baseSurface = oldSurfaces[baseSurfaceIndex];
            if (used[baseSurfaceIndex])
            {
                continue;
            }
            bfs.Enqueue(baseSurfaceIndex);
            SurfaceInfo newSurface = new SurfaceInfo();
            newSurface.index = newSurfaces.Count;
            newSurfaces.Add(newSurface);
            while(bfs.Count>0)
            {
                int curIndex = bfs.Dequeue();
                if(used[curIndex])
                {
                    continue;
                }
                SurfaceInfo curSurface = oldSurfaces[curIndex];
                // condition: judge curSurface and baseSurface
                if(
                    Vector3.Dot(curSurface.normal,baseSurface.normal)> cosThreshold &&
                    curSurface.VertexCount>0 &&
                    Mathf.Abs(curSurface.d-baseSurface.d)<distanceThreshold || 
                    newSurface.VertexCount==0
                    )
                {
                    //newSurfaces.Add(curSurface);
                    newSurface.UnionSurface(curSurface);
                    Debug.Log($"Union Surface {curIndex} and {baseSurface.index}> c:{curSurface.normal} b:{baseSurface.normal} u:{newSurface.normal}");
                    oldSurfaceRef[curIndex] = newSurface.index;
                    used[curIndex] = true;
                    foreach (int neighborIndex in curSurface.neighborSurface)
                    {
                        if(!used[neighborIndex])
                        {
                            bfs.Enqueue(neighborIndex);
                        }
                    }
                }
            }
        }
        */
        for (int baseSurfaceIndex = 0; baseSurfaceIndex < oldSurfaces.Count; baseSurfaceIndex++)
        {
            SurfaceInfo baseSurface = oldSurfaces[baseSurfaceIndex];
            if (used[baseSurfaceIndex])
            {
                continue;
            }
            bfs.Enqueue(baseSurfaceIndex);
            SurfaceInfo newSurface = new SurfaceInfo();
            newSurface.index = newSurfaces.Count;
            newSurfaces.Add(newSurface);
            for (int curIndex = baseSurfaceIndex; curIndex < oldSurfaces.Count; curIndex++)
            {
                if (used[curIndex])
                {
                    continue;
                }
                SurfaceInfo curSurface = oldSurfaces[curIndex];
                // condition: judge curSurface and baseSurface
                if (
                    Vector3.Dot(curSurface.normal, baseSurface.normal) > cosThreshold &&
                    curSurface.VertexCount > 0 &&
                    Mathf.Abs(curSurface.d - baseSurface.d) < distanceThreshold &&
                    Utils.BoundsDistance(curSurface.bounds,baseSurface.bounds)< distanceThreshold
                    ||
                    newSurface.VertexCount == 0
                    )
                {
                    //newSurfaces.Add(curSurface);
                    newSurface.UnionSurface(curSurface);
                    Debug.Log($"Union Surface {curIndex} and {baseSurface.index}> c:{curSurface.normal} b:{baseSurface.normal} u:{newSurface.normal}");
                    oldSurfaceRef[curIndex] = newSurface.index;
                    used[curIndex] = true;
                }
            }
        }
        // resolve neighborhoods
        for (int baseSurfaceIndex = 0; baseSurfaceIndex < newSurfaces.Count; baseSurfaceIndex++)
        {
            HashSet<int> remapNeighbors = new HashSet<int>();
            SurfaceInfo curSurface = newSurfaces[baseSurfaceIndex];
            foreach (int oldNeighborIndex in curSurface.neighborSurface)//TODO: write here!
            {
                remapNeighbors.Add(oldSurfaceRef[oldNeighborIndex]);
            }
            curSurface.neighborSurface = new List<int>(remapNeighbors);
        }

        SurfaceInfoBundle newSurfaceInfoBundle = new SurfaceInfoBundle();
        newSurfaceInfoBundle.surfaceInfos = newSurfaces;
        return newSurfaceInfoBundle;
        //TODO: Step 1 , process each OBB box, to figure out edge and surface range
        // Step 2: seperate non-convex into convex one
        // Step 3: 
    }

}
