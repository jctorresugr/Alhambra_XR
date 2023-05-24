using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * This class should be removed, we need to consider another method :(
 */
public class SurfaceAnalyze : MonoBehaviour
{
    public MeshFilter meshFilter;
    public ReferenceTransform reference;
    public MeshRenderer debugSurfaceTemplate;
    public void Start()
    {
        ProcessMesh();
    }

    public class VertexTopologyInfo
    {
        public int index;
        public int surfaceIndex = -1;
        public bool isVisited = false;
        public HashSet<int> neighborVertex = new HashSet<int>();
        public Vector3 averageNormal = Vector3.zero;
        public Vector3 pos = Vector3.zero;
        //public List<int> triangleRef;
    }

    public class SurfaceGroupInfo
    {
        public int index;
        public HashSet<int> vertices = new HashSet<int>(); // belong to this surface
        public HashSet<int> failedVertices = new HashSet<int>(); // not belong to, but neighbor vertex
        public HashSet<int> neighborSurface = new HashSet<int>();
        public Vector3 averageNormal = Vector3.zero;
        public Vector3 minNormal = Vector3.zero;
        public Vector3 maxNormal = Vector3.zero;
        public bool isVisited = false;
        public int refPreIndex = -1;

        public void AddNormal(Vector3 normal)
        {
            averageNormal = (vertices.Count * averageNormal + normal).normalized;
        }

        public SurfaceGroupInfo() { }

        public SurfaceGroupInfo(SurfaceGroupInfo s) //simple copy
        {
            this.vertices.UnionWith(s.vertices);
            //this.failedVertices.UnionWith(s.failedVertices);
            this.averageNormal = s.averageNormal;
        }

        public void AddSurface(SurfaceGroupInfo s)
        {
            this.averageNormal = (this.averageNormal * this.vertices.Count + s.averageNormal * s.vertices.Count).normalized;
            this.vertices.UnionWith(s.vertices);
            //this.failedVertices.UnionWith(s.failedVertices);
            //this.failedVertices.ExceptWith(this.vertices);

        }
    }

    public class SurfaceInfo
    {
        public Vector3 normal;
        public Bounds bounds;
        public HashSet<int> verticesIndex;
        public HashSet<int> neighborSurface;
    }

    public List<SurfaceInfo> surfaceInfos;

    public void ProcessMesh()
    {
        if(meshFilter==null)
        {
            Debug.LogError("Null MeshFilter");
            return;
        }
        ExtractSurfaces(meshFilter.mesh);
        DebugSurface();
    }

    protected void ExtractSurfaces(Mesh mesh)
    {
        List<SurfaceGroupInfo> surfaces = new List<SurfaceGroupInfo>();
        // Basic vairables
        surfaces.Clear();
        int verticesCount = mesh.vertexCount;
        int subMeshCount = meshFilter.mesh.subMeshCount;
        int[] vertexDataPos = new int[verticesCount];
        List<VertexTopologyInfo> vtInfo = new List<VertexTopologyInfo>();
        Dictionary<Vector3, VertexTopologyInfo> posDic = new Dictionary<Vector3, VertexTopologyInfo>();
        //VertexTopologyInfo[] vtInfo = new VertexTopologyInfo[mesh.vertexCount];

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        //TODO: shrink duplicate vertex

        //========================================================
        // 1. construct a simple topology information

        for (int i = 0; i < verticesCount; i++)
        {
            Vector3 v = vertices[i];
            if(!posDic.ContainsKey(v))
            {
                VertexTopologyInfo vt = new VertexTopologyInfo();
                vt.index = vtInfo.Count;
                vtInfo.Add(vt);
                posDic.Add(v, vt);
                vertexDataPos[i] = vt.index;
                vt.averageNormal = normals[i];
                vt.pos = v;
            }else
            {
                vertexDataPos[i] = posDic[v].index;
                posDic[v].averageNormal += normals[i];
            }
            //vtInfo[i] = new VertexTopologyInfo();
            //vtInfo[i].index = i;
        }
        foreach(VertexTopologyInfo vertexTopologyInfo in vtInfo)
        {
            vertexTopologyInfo.averageNormal = vertexTopologyInfo.averageNormal.normalized;
        }
        for (int subMeshIndex = 0; subMeshIndex < subMeshCount; subMeshIndex++)
        {
            MeshTopology meshTopology = mesh.GetTopology(subMeshIndex);
            if (meshTopology == MeshTopology.Triangles)
            {
                int[] indices = mesh.GetTriangles(subMeshIndex);
                int triangleCount = indices.Length / 3;
                Debug.Log($"Submesh {subMeshIndex} > Indices size is {indices.Length}, Triangles > {triangleCount}");
                for (int i = 0; i < triangleCount; i++)
                {
                    int ind = i * 3;
                    int i0 = vertexDataPos[indices[ind + 0]];
                    int i1 = vertexDataPos[indices[ind + 1]];
                    int i2 = vertexDataPos[indices[ind + 2]];
                    vtInfo[i0].neighborVertex.Add(i1);
                    vtInfo[i0].neighborVertex.Add(i2);
                    vtInfo[i1].neighborVertex.Add(i0);
                    vtInfo[i1].neighborVertex.Add(i2);
                    vtInfo[i2].neighborVertex.Add(i0);
                    vtInfo[i2].neighborVertex.Add(i1);
                }
            }
            else
            {
                Debug.LogWarning("Not implemented for mesh topology type " + meshTopology + " , sub mesh index: " + subMeshIndex);
            }
        }

        //========================================================
        // 2. BFS (DB-Scan) cluster
        float thresholdStrict = Mathf.Cos(Mathf.PI / 180.0f * 15.0f);
        float thresholdLoose = Mathf.Cos(Mathf.PI / 180.0f * 30.0f);
        Queue<int> bfsQueue = new Queue<int>();

        for (int baseScanIndex = 0; baseScanIndex < vtInfo.Count; baseScanIndex++)
        {
            if (vtInfo[baseScanIndex].isVisited)
            {
                continue;
            }
            SurfaceGroupInfo surface = new SurfaceGroupInfo();
            surface.index = surfaces.Count;
            surfaces.Add(surface);
            surface.averageNormal = normals[baseScanIndex];
            bfsQueue.Enqueue(baseScanIndex);
            vtInfo[baseScanIndex].isVisited = true;
            vtInfo[baseScanIndex].surfaceIndex = surface.index;
            while (bfsQueue.Count != 0)
            {
                int curIndex = bfsQueue.Dequeue();
                VertexTopologyInfo curVertex = vtInfo[curIndex];
                foreach (int anotherIndex in curVertex.neighborVertex)
                {
                    VertexTopologyInfo anotherVertex = vtInfo[anotherIndex];
                    if (anotherVertex.isVisited)
                    {
                        surface.failedVertices.Add(anotherIndex);
                        continue;
                    }
                    Vector3 curNormal = curVertex.averageNormal;// normals[curIndex];
                    Vector3 anotherNormal = anotherVertex.averageNormal;//normals[anotherIndex];
                    // Condition: neighbor normal big tolerance
                    // for the whole group, low tolerance
                    // TODO: for a better estimation when there are few points (allow larger diff)
                    if (
                        Vector3.Dot(curNormal, anotherNormal) > thresholdLoose &&
                        Vector3.Dot(surface.averageNormal, anotherNormal) > thresholdStrict
                        )
                    {
                        surface.AddNormal(anotherNormal);
                        surface.vertices.Add(anotherIndex);
                        anotherVertex.isVisited = true;
                        anotherVertex.surfaceIndex = surface.index;
                        bfsQueue.Enqueue(anotherIndex);
                    }
                    else
                    {
                        surface.failedVertices.Add(anotherIndex);
                    }
                }
            }
        }

        //========================================================
        // 3. Resolve surface neighbors
        int surfaceCount = surfaces.Count;
        for (int curSurfaceIndex = 0; curSurfaceIndex < surfaceCount; curSurfaceIndex++)
        {
            SurfaceGroupInfo curSurface = surfaces[curSurfaceIndex];
            curSurface.maxNormal = curSurface.minNormal = curSurface.averageNormal;
            foreach (int failVertexIndex in curSurface.failedVertices)
            {
                curSurface.neighborSurface.Add(vtInfo[failVertexIndex].surfaceIndex);
            }
            curSurface.isVisited = false;
        }
        Debug.Log("Done First step (Surface Analysis) > Surface " + surfaceCount);

        List<SurfaceGroupInfo> oldSurface = new List<SurfaceGroupInfo>();
        List<SurfaceGroupInfo> newSurface = surfaces;

        while (oldSurface.Count != newSurface.Count)
        {
            oldSurface = newSurface;
            newSurface = new List<SurfaceGroupInfo>();
            
            

            //========================================================
            // 4. Cluster surfaces with the same algorithm (but strict condition, min max)
            //TODO: if this is required

            for (int baseSurfaceIndex = 0; baseSurfaceIndex < oldSurface.Count; baseSurfaceIndex++)
            {
                SurfaceGroupInfo baseSurface = oldSurface[baseSurfaceIndex];
                if (baseSurface.isVisited)
                {
                    continue;
                }
                SurfaceGroupInfo surface = new SurfaceGroupInfo(baseSurface);
                surface.index = newSurface.Count;
                surface.AddSurface(baseSurface);
                newSurface.Add(surface);
                bfsQueue.Enqueue(baseSurfaceIndex);
                baseSurface.refPreIndex = surface.index;
                baseSurface.isVisited = true;

                while (bfsQueue.Count != 0)
                {
                    int curIndex = bfsQueue.Dequeue();
                    SurfaceGroupInfo curSurface = oldSurface[curIndex];
                    foreach (int anotherIndex in curSurface.neighborSurface)
                    {
                        SurfaceGroupInfo anotherSurface = oldSurface[anotherIndex];
                        if (anotherSurface.isVisited)
                        {
                            surface.failedVertices.Add(anotherIndex);
                            continue;
                        }
                        Vector3 curNormal = curSurface.averageNormal;// normals[curIndex];
                        Vector3 anotherNormal = anotherSurface.averageNormal;//normals[anotherIndex];
                                                                             // Condition: neighbor normal big tolerance
                                                                             // for the whole group, low tolerance
                                                                             // TODO: for a better estimation when there are few points (allow larger diff)
                        if (
                            Vector3.Dot(curNormal, anotherNormal) > thresholdLoose &&
                            Vector3.Dot(surface.averageNormal, anotherNormal) > thresholdStrict
                            )
                        {
                            surface.AddSurface(anotherSurface);
                            anotherSurface.isVisited = true;
                            bfsQueue.Enqueue(anotherIndex);
                            anotherSurface.refPreIndex = surface.index;
                        }
                        else
                        {
                            surface.neighborSurface.Add(anotherIndex);
                        }
                    }
                }

            }//end for

            //sort out 
            for (int curSurfaceIndex = 0; curSurfaceIndex < newSurface.Count; curSurfaceIndex++)
            {
                SurfaceGroupInfo curSurface = newSurface[curSurfaceIndex];
                curSurface.maxNormal = curSurface.minNormal = curSurface.averageNormal;
                HashSet<int> neighborSurface = new HashSet<int>();
                foreach (int preNeighborIndex in curSurface.neighborSurface)
                {
                    neighborSurface.Add(oldSurface[preNeighborIndex].refPreIndex);
                }
                curSurface.neighborSurface = neighborSurface;
                curSurface.isVisited = false;
            }
            Debug.Log("Done cluster step (Surface Analysis) > Surface " + newSurface.Count);
        }//end while
        surfaces = newSurface;
        surfaceInfos = new List<SurfaceInfo>();
        foreach(SurfaceGroupInfo group in surfaces)
        {
            SurfaceInfo info = new SurfaceInfo();
            info.normal = group.averageNormal;
            info.verticesIndex = group.vertices;
            info.neighborSurface = group.neighborSurface;
            bool first = true;
            foreach(int vertexIndex in info.verticesIndex)
            {
                Vector3 p = vtInfo[vertexIndex].pos;
                if (first)
                {
                    info.bounds.SetMinMax(p,p);
                    first = false;
                    continue;
                }else
                {
                    info.bounds.Encapsulate(p);
                }
            }
            surfaceInfos.Add(info);
        }
    }//end method

    public void DebugSurface()
    {
        foreach(SurfaceInfo surface in surfaceInfos)
        {
            MeshRenderer r = Instantiate(debugSurfaceTemplate);
            r.transform.parent = reference.referTransform;
            r.transform.up = surface.normal;
            r.transform.localScale = surface.bounds.size;
            r.transform.localPosition = surface.bounds.center;
            r.gameObject.SetActive(true);
        }
    }
}
