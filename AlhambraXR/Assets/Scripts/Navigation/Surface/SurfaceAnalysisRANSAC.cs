using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class SurfaceAnalysisRANSAC : MonoBehaviour
{
    [Header("RANSAC Parameters")]
    public float thresholdDistance = 0.004f; //RANSAC outlier threshold
    public float minPointThresholdPercent = 0.05f;//discard surface if it only has inliner< this
    public int maxIterationCount = 20000;
    [Header("Topology Parameters")]
    public float clusterPointThreshold = 0.001f;//cluster points if they are enough near
    public float linkPointThreshold = 0.002f;//build an edge if two points are enough near

    //temporay save the result
    protected SurfaceInfoBundle surfaceInfos;
    /*public bool compute = true;
    public bool useCache = false;
    public bool isFinished = false;*/
    Thread computeThread = null;

    public delegate void OnFinishComputationListener(SurfaceInfoBundle result);
    public event OnFinishComputationListener finishEvent;


    [Header("Other settings")]
    public int sleepMS = 0; // avoid burned my laptop when it is 100% cpu+100% gpu.
    
    public void StartCompute(MeshFilter meshFilter)
    {
        surfaceInfos = null;
        Debug.Log("Start analyze mesh");
        List<VertexTopologyInfo> vtInfo = null;
        AnalyzeVertices(meshFilter.mesh, ref vtInfo);
        Debug.Log("Start analyze mesh thread");
        ThreadStart computeThreadStart = new ThreadStart(() => AnalyzeMesh(vtInfo));
        computeThread = new Thread(computeThreadStart);
        computeThread.Start();
        // wait, but do not block the main thread entirely, otherwise you cannot pause or stop or view logs in Unity :(
        //just for debugging
    }
    private void Update()
    {
        if (computeThread != null && computeThread.IsAlive)
        {
            Thread.Sleep(sleepMS); // I do not want to make my machine burned, so let the main thread sleep while computing, but not fully sutck at the same time
        }
        else
        {
            if (computeThread != null)
            {
                computeThread = null;
                finishEvent?.Invoke(surfaceInfos);
            }
        }
    }
    /*
    void Update()
    {
        if(useCache)
        {
            try
            {
                surfaceInfos = JsonUtility.FromJson<SurfaceInfoBundle>(Utils.ReadFile(filePath));
                isFinished = true;
            }
            catch(Exception e)
            {
                Debug.LogError(e);
                Debug.Log("Failed to load cache, compute!");
                useCache = false;
                compute = true;
            }
        }
        if (meshFilter != null && compute && computeThread==null && !useCache)
        {
            compute = false;
            Debug.Log("Start analyze mesh");

            List<VertexTopologyInfo> vtInfo = null;
            AnalyzeVertices(meshFilter.mesh, ref vtInfo);
            Debug.Log("Start analyze mesh thread");
            ThreadStart computeThreadStart = new ThreadStart(()=> AnalyzeMesh(vtInfo));
            computeThread = new Thread(computeThreadStart);
            computeThread.Start();
        }
        if(computeThread!=null && computeThread.IsAlive)
        {
            Thread.Sleep(sleepMS); // I do not want to make my machine burned, so let the main thread sleep while computing, but not fully sutck at the same time
        }else
        {
            if(computeThread!=null)
            {
                isFinished = true;
                computeThread = null;
            }
        }

    }

    public void DebugSurface()
    {
        if (displaySurface != null)
        {
            displaySurface.surfaceInfos = surfaceInfos;
            displaySurface.DebugSurface();
        }
    }*/
    [Serializable]
    public class VertexTopologyInfo
    {
        public int index;
        public int surfaceIndex = -1;
        public bool isVisited = false;
        public HashSet<int> neighborVertex = new HashSet<int>();
        public Vector3 averageNormal = Vector3.zero;
        public Vector3 pos = Vector3.zero;
    }

    [Serializable]
    public class SurfaceProcessInfo
    {
        public Vector3 normal;
        public float a, b, c, d;//aX+bY+cZ+D=0

        protected float inverseSquare;
        public int index;
        [NonSerialized]
        public List<int> verticeIndex = new List<int>();
        [NonSerialized]
        public HashSet<int> neighborSurface = new HashSet<int>();
        [NonSerialized]
        public HashSet<int> surroundingVertex = new HashSet<int>();
        public Bounds bounds;
        public void SetParameter(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            a = ((p2.y - p1.y) * (p3.z - p1.z) - (p2.z - p1.z) * (p3.y - p1.y));
            b = ((p2.z - p1.z) * (p3.x - p1.x) - (p2.x - p1.x) * (p3.z - p1.z));
            c = ((p2.x - p1.x) * (p3.y - p1.y) - (p2.y - p1.y) * (p3.x - p1.x));
            d = -(a * p1.x + b * p1.y + c * p1.z);
            inverseSquare = 1.0f / Mathf.Sqrt(a * a + b * b + c * c);
            normal = Vector3.Cross(p1 - p2, p3 - p2).normalized;
        }

        public float PointDistance(Vector3 p)
        {
            return Mathf.Abs(a * p.x + b * p.y + c * p.z + d) * inverseSquare;
        }
        
        public void CopySurfaceParameters(SurfaceProcessInfo s)
        {
            normal = s.normal;
            a = s.a;
            b = s.b;
            c = s.c;
            d = s.d;
            inverseSquare = s.inverseSquare;
        }
    }

    [Serializable]
    public class SurfaceInfo
    {
        [SerializeField]
        public int index;
        [SerializeField]
        public Vector3 normal = Vector3.zero;
        [SerializeField]
        public float a, b, c, d;//aX+bY+cZ+D=0
        [SerializeField]
        public Vector3 p0;
        [SerializeField]
        public Bounds bounds;
        //[SerializeField]
        public int VertexCount => vertices.Count;
        [SerializeField]
        public List<int> neighborSurface = new List<int>();
        [SerializeField]
        public List<Vector3> vertices = new List<Vector3>();
        public SurfaceInfo() { }
        public SurfaceInfo(SurfaceProcessInfo s)
        {
            index = s.index;
            normal = s.normal;
            bounds = s.bounds;
            a = s.a;
            b = s.b;
            c = s.c;
            d = s.d;
            neighborSurface.Union(s.neighborSurface);
        }

        public SurfaceInfo(SurfaceInfo s)
        {
            index = s.index;
            normal = s.normal;
            bounds = s.bounds;
            a = s.a;
            b = s.b;
            c = s.c;
            d = s.d;
            vertices.AddRange(s.vertices);
        }

        public void UnionSurface(SurfaceInfo s)
        {
            //normal = s.normal;
            normal = (normal * vertices.Count + s.normal * s.vertices.Count).normalized;
            
            if(VertexCount==0)
            {
                bounds = s.bounds;
            }else
            {
                bounds.Encapsulate(s.bounds);
            }
            vertices.AddRange(s.vertices);

            neighborSurface.Union(s.neighborSurface);
            /*a = normal.x;
            b = normal.y;
            c = normal.z;
            d = s.d;*/
            //bounds = s.bounds;
            //TODO: parallel
            
            Vector3 sum = Vector3.zero;
            foreach(Vector3 v in vertices)
            {
                sum += v;
            }
            sum /= vertices.Count;
            a = normal.x;
            b = normal.y;
            c = normal.z;
            d = -Vector3.Dot(normal, sum);
            p0 = sum;
        }

        public void ProcessDuplication()
        {
            neighborSurface = new List<int>(new HashSet<int>(neighborSurface));
        }

        public Bounds CalculateGlobalBounds()
        {
            Bounds bounds = new Bounds();
            bounds.SetMinMax(vertices[0], vertices[0]);
            foreach(Vector3 v in vertices)
            {
                bounds.Encapsulate(v);
            }
            return bounds;
        }

        public void RecalculateBounds()
        {
            Vector3 right = Right;//x
            Vector3 forward = Forward;//z
            bounds.SetMinMax(Vector3.zero, Vector3.zero);
            foreach(Vector3 p in vertices)
            {
                Vector3 dp = p - p0;
                float y = Vector3.Dot(normal, dp);
                float x = Vector3.Dot(right, dp);
                float z = Vector3.Dot(forward, dp);
                bounds.Encapsulate(new Vector3(x, y, z));
            }
            Vector3 c = bounds.center;
            p0 = c.x * right + c.y * normal + c.z * forward + p0;
            Vector3 sz = bounds.size/2;
            bounds.SetMinMax(-sz, sz);
            //bounds = CalculateGlobalBounds();
        }

        public Vector3 Right => Vector3.Cross(normal, Vector3.up).normalized;
        public Vector3 Forward => Vector3.Cross(normal, Right).normalized;
        public Vector3 Up => normal;
    };

    //Wrap the list, Unity JsonUtility cannot parse List<SurfaceInfo>, they just give you an empty list...
    //
    [Serializable]
    public class SurfaceInfoBundle
    {
        [SerializeField]
        public List<SurfaceInfo> surfaceInfos = new List<SurfaceInfo>();
    }

    public void AnalyzeMeshFilter(MeshFilter meshFilter)
    {
        if (meshFilter != null)
        {
            List<VertexTopologyInfo> vtInfo=null;
            AnalyzeVertices(meshFilter.mesh, ref vtInfo);
            AnalyzeMesh(vtInfo);
        }
    }

    protected void AnalyzeVertices(Mesh mesh, ref List<VertexTopologyInfo> vtInfo)
    {
        int verticesCount = mesh.vertexCount;
        int subMeshCount = mesh.subMeshCount;
        int[] vertexDataPos = new int[verticesCount];
        vtInfo = new List<VertexTopologyInfo>();
        Dictionary<Vector3, VertexTopologyInfo> posDic = new Dictionary<Vector3, VertexTopologyInfo>();

        Vector3[] vertices = mesh.vertices;
        Vector3 wholeSize = mesh.bounds.size;
        Vector3[] normals = mesh.normals;
        // hash cell for acclerating
        float volume = wholeSize.x * wholeSize.y * wholeSize.z;
        int maxElementSize = 1000000;//1M -> 16 MB
        float maxCellSize = Mathf.Pow(volume / maxElementSize, 1.0f / 3.0f);
        HashCell<VertexTopologyInfo> cells = new HashCell<VertexTopologyInfo>(mesh.bounds, Mathf.Max(maxCellSize, clusterPointThreshold));

        // cluster vertices, reduce computation, fast and simple mesh simplification
        float clusterPointThresholdSqr = clusterPointThreshold * clusterPointThreshold;
        for (int i = 0; i < verticesCount; i++)
        {
            Vector3 v = vertices[i];
            if (!posDic.ContainsKey(v)) // if not the same point
            {
                float minDistance = float.MaxValue;
                VertexTopologyInfo bestCell = null;
                List<VertexTopologyInfo> nearbyCells = cells.GetRangeCell(v, clusterPointThresholdSqr);
                foreach(VertexTopologyInfo cellElement in nearbyCells) // find nearby clustered points
                {
                    float disSqr = Vector3.SqrMagnitude(v - cellElement.pos);
                    if (disSqr < clusterPointThresholdSqr)
                    {
                        if(minDistance> disSqr)
                        {
                            minDistance = disSqr;
                            bestCell = cellElement;
                        }
                    }
                }
                if(bestCell!=null) // found, cluster this point
                {
                    bestCell.averageNormal += normals[i];
                    vertexDataPos[i] = bestCell.index;
                    continue;
                }
                VertexTopologyInfo vt = new VertexTopologyInfo();
                vt.index = vtInfo.Count;
                vtInfo.Add(vt);
                posDic.Add(v, vt);
                vertexDataPos[i] = vt.index;
                cells.Add(v,vt);
                vt.averageNormal = normals[i];
                vt.pos = v;
            }
            else // same vertex
            {
                vertexDataPos[i] = posDic[v].index;
                posDic[v].averageNormal += normals[i];
            }

        }

        Parallel.ForEach(vtInfo,
            vertexTopologyInfo => vertexTopologyInfo.averageNormal = vertexTopologyInfo.averageNormal.normalized);
        Debug.Log("Actual Vertex count " + vtInfo.Count);

        // build linkage if the two points are adequate near
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

        float linkPointThresholdSqr = linkPointThreshold * linkPointThreshold;
        int addedLinks = 0;
        for (int i=0;i<vtInfo.Count;i++)
        {
            VertexTopologyInfo vti = vtInfo[i];
            List<VertexTopologyInfo> candidates = cells.GetRangeCell(vti.pos, linkPointThreshold);
            foreach(VertexTopologyInfo vti2 in candidates)
            {
                if((vti2.pos-vti.pos).sqrMagnitude< linkPointThresholdSqr)
                {
                    vti.neighborVertex.Add(vti2.index);
                    addedLinks++;
                }
            }
        }
        Debug.Log("Add links: " + addedLinks);

    }
    protected void AnalyzeMesh(List<VertexTopologyInfo> vtInfo)
    {
        int minPointThreshold = (int)(vtInfo.Count * minPointThresholdPercent);
        List<SurfaceProcessInfo> surfaceProcessInfos;

        int skipCount = 0;
        int hugeLoopCount = 0;
        List<VertexTopologyInfo> vtInfoPart = vtInfo;
        surfaceProcessInfos = new List<SurfaceProcessInfo>();
        // now do the algorithm: RANSAC
        while (true)
        {
            hugeLoopCount++;
            int maxIndex = vtInfoPart.Count-1;
            int[] randomIndex = new int[3];
            int bestInliner = 0;
            SurfaceProcessInfo bestSurface = null;
            //RANSAC Iteraction
            for (int iterCount = 0; iterCount < maxIterationCount; iterCount++)
            {
                //get random 3 points
                GetRandomIndex(maxIndex, randomIndex);
                Vector3 p1 = vtInfo[randomIndex[0]].pos;
                Vector3 p2 = vtInfo[randomIndex[1]].pos;
                Vector3 p3 = vtInfo[randomIndex[2]].pos;
                SurfaceProcessInfo surfaceInfo = new SurfaceProcessInfo();
                // resolve surface equation
                surfaceInfo.SetParameter(p1, p2, p3);
                // compute inlier
                int totalInlier = 0;
                totalInlier = vtInfoPart.AsParallel().Count(x =>
                    surfaceInfo.PointDistance(x.pos) < thresholdDistance
                );
                //record if best
                if(totalInlier>bestInliner)
                {
                    bestInliner = totalInlier;
                    bestSurface = surfaceInfo;
                }
            }

            // post process: break surface based on topology connectivity, reorganize the left points.
            List<VertexTopologyInfo> newvtInfoPart = new List<VertexTopologyInfo>();
            {
                
                skipCount = 0;
                //filter out inliner points
                for(int i=0;i<vtInfoPart.Count;i++)
                {
                    VertexTopologyInfo vti = vtInfoPart[i];
                    if(bestSurface.PointDistance(vti.pos) < thresholdDistance)
                    {
                        if (bestSurface.verticeIndex.Count==0)
                        {
                            bestSurface.bounds.SetMinMax(vti.pos, vti.pos);
                        }else
                        {
                            bestSurface.bounds.Encapsulate(vti.pos);
                        }
                        bestSurface.verticeIndex.Add(vti.index);
                    }
                    else
                    {
                        newvtInfoPart.Add(vti);
                    }
                }
                //surfaceProcessInfos.Add(bestSurface);
                Debug.Log($" Find Surface, vc = {bestInliner}, left={newvtInfoPart.Count}, Loop {hugeLoopCount}, Detail:\n{JsonUtility.ToJson(bestSurface)}");
                
                // clip surface based on topology of the mesh
                Queue<int> bfs = new Queue<int>();

                // do not trust mesh, it is not topological continuously :(
                foreach (int vertexIndex in bestSurface.verticeIndex)
                {
                    VertexTopologyInfo vertexInfo = vtInfo[vertexIndex];
                    if(vertexInfo.isVisited)
                    {
                        continue;
                    }
                    SurfaceProcessInfo subSurface = new SurfaceProcessInfo();
                    subSurface.index = surfaceProcessInfos.Count;
                    subSurface.CopySurfaceParameters(bestSurface);
                    bfs.Enqueue(vertexIndex);
                    subSurface.bounds.SetMinMax(vertexInfo.pos, vertexInfo.pos);
                    subSurface.normal = Vector3.zero;
                    //BFS
                    while(bfs.Count>0)
                    {
                        int curIndex = bfs.Dequeue();
                        VertexTopologyInfo curVertexInfo = vtInfo[curIndex];
                        if(curVertexInfo.isVisited)
                        {
                            if (curVertexInfo.surfaceIndex >= 0)
                            {
                                subSurface.neighborSurface.Add(curVertexInfo.surfaceIndex);
                            }
                            continue;
                        }
                        if (subSurface.PointDistance(curVertexInfo.pos)<thresholdDistance)
                        { //satisify condition
                            curVertexInfo.surfaceIndex = subSurface.index;
                            curVertexInfo.isVisited = true;
                            subSurface.bounds.Encapsulate(curVertexInfo.pos);
                            subSurface.verticeIndex.Add(curIndex);
                            subSurface.normal += curVertexInfo.averageNormal;
                            // search nearby points
                            foreach (int neighborVertexIndex in vertexInfo.neighborVertex)
                            {
                                if(curVertexInfo.isVisited)
                                {
                                    if (vtInfo[neighborVertexIndex].surfaceIndex >= 0)
                                    {
                                        subSurface.neighborSurface.Add(curVertexInfo.surfaceIndex);
                                        continue;
                                    }
                                }
                                
                                bfs.Enqueue(neighborVertexIndex);
                            }
                        }else
                        {
                            // record connectivity 
                            subSurface.surroundingVertex.Add(curIndex);
                        }

                    }//end bfs
                    // add surface
                    subSurface.normal = subSurface.normal.normalized;
                    surfaceProcessInfos.Add(subSurface);
                    Debug.Log($" Sub Surface, vertices Count = {subSurface.verticeIndex.Count}, surround Index {subSurface.surroundingVertex.Count}, Detail:\n{JsonUtility.ToJson(subSurface)}");
                }

                bestSurface = null;
                bestInliner = 0;
                
            }
            vtInfoPart = newvtInfoPart;
            if (newvtInfoPart.Count<minPointThreshold || skipCount>5)
            {
                break;
            }
        }
        // resolve neighborhoods
        surfaceInfos = new SurfaceInfoBundle();
        List<SurfaceInfo> sinfos = surfaceInfos.surfaceInfos;
        foreach(SurfaceProcessInfo surface in surfaceProcessInfos)
        {
            foreach(int v in surface.surroundingVertex)
            {
                surface.neighborSurface.Add(vtInfo[v].surfaceIndex);
            }
            surface.neighborSurface.Remove(surface.index);
            SurfaceInfo si = new SurfaceInfo(surface);
            foreach(int vindex in surface.verticeIndex)
            {
                si.vertices.Add(vtInfo[vindex].pos);
            }
            sinfos.Add(si);
        }
        Debug.Log("Finish analyze mesh");
        Debug.Log($"Total surface count {sinfos.Count}");
        //-------------

    }

    private System.Random random = new System.Random();
    protected void GetRandomIndex(int maxIndex, int[] data)
    {
        int len = data.Length;
        for(int i=0;i<len;i++)
        {
            bool flag = true;
            while(flag)
            {
                data[i] = random.Next(0, maxIndex+1);
                flag = false;
                for (int j = 0; j < i - 1; j++)
                {
                    if (data[i] == data[j])
                    {
                        flag = true;
                        break;
                    }
                }
            }
        }
    }
}
