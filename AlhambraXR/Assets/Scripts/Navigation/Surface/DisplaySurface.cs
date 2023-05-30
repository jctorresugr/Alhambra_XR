using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static SurfaceAnalysisRANSAC;

public class DisplaySurface : MonoBehaviour
{
    public ReferenceTransform reference;
    public MeshRenderer debugSurfaceTemplate;
    public int filterThreshold = 0;
    private int oldFilterThreshold = 0;

    public SurfaceInfoBundle surfaceInfos;
    public List<MeshRenderer> objs = new List<MeshRenderer>();

    public void Update()
    {
        if(oldFilterThreshold!=filterThreshold)
        {
            DebugSurface();
            oldFilterThreshold = filterThreshold;
        }
    }

    public void DebugSurface()
    {
        RemoveSurface();
        foreach (SurfaceInfo surface in surfaceInfos.surfaceInfos)
        {
            if(surface.VertexCount<filterThreshold)
            {
                continue;
            }
            //surface.RecalculateBounds();
            MeshRenderer r = Instantiate(debugSurfaceTemplate);
            r.transform.parent = reference.referTransform;
            r.transform.up = surface.normal;
            //r.transform.right = surface.Right;
            r.transform.localScale = new Vector3(surface.bounds.size.x, 0.0001f, surface.bounds.size.z);
            r.transform.localPosition = surface.bounds.center;// surface.p0;
            r.gameObject.SetActive(true);
            objs.Add(r);
        }
    }

    public void RemoveSurface()
    {
        foreach(MeshRenderer mr in objs)
        {
            Destroy(mr.gameObject);
        }
        objs.Clear();
    }
}
