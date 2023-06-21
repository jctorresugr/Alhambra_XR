using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class VolumeAnalyze : MonoBehaviour
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct VolumeInfo
    {
        public int obstacle;
        public Vector3 normal0;
        public Vector3 normal1;
        public bool isVisited;

        public bool IsEmpty => obstacle == 0;

        public Vector3 this[int i]
        {
            get
            {
                switch(i)
                {
                    case 0: return normal0;
                    case 1: return normal1;
                }
                Debug.LogError("VolumeInfo Exceed limits "+i);
                return Vector3.zero;
            }
            set
            {
                switch (i)
                {
                    case 0: normal0=value;return;
                    case 1: normal1=value;return;
                }
                Debug.LogError("VolumeInfo Exceed limits " + i);
            }
        }


        public void AddNormal(Vector3 normal, float cosThreshold)
        {
            if (obstacle == 0)
            {
                this[0] = normal;
                obstacle++;
            }
            else if (obstacle < 2) 
            {
                if(ExistNormal(normal,cosThreshold))
                {
                    return;
                }
                this[obstacle] = normal;
                obstacle++;
            }
        }

        public bool ExistNormal(Vector3 refNormal, float cosThreshold)
        {
            for(int i=0;i<obstacle;i++)
            {
                if (Vector3.Dot(refNormal, this[i]) > cosThreshold)
                {
                    return true;
                }
            }
            return false;
        }

        public Vector3 GetNotSimilarNormal(Vector3 refNormal)
        {
            float minValue = 10.0f;
            int minIndex = -1;
            for (int i = 0; i < obstacle; i++)
            {
                float v = Vector3.Dot(refNormal, this[i]);
                if (v<minValue)
                {
                    minValue = v;
                    minIndex = i;
                }
            }
            if(minIndex>=0)
            {
                return this[minIndex];
            }else
            {
                return Vector3.zero;
            }
        }

        public static VolumeInfo GetDefault()
        {
            VolumeInfo tempVolumeInfo = new VolumeInfo();
            tempVolumeInfo.obstacle = 0;
            tempVolumeInfo.normal0 = tempVolumeInfo.normal1 = Vector3.zero;
            tempVolumeInfo.isVisited = false;
            return tempVolumeInfo;
        }
    }
    private static VolumeInfo EMPTY_INFO = VolumeInfo.GetDefault();
    public MeshFilter mesh;
    [Header("Parameters")]
    public float maxGridCount = 100 * 100 * 100;
    public float splitNormalDegreeThreshold = 30;
    public float CosSplitNormalDegreeThreshold => Mathf.Cos(splitNormalDegreeThreshold / 180.0f * Mathf.PI);

    public VolumeCell<VolumeInfo> volumeInfos;
    private bool isProcessed = false;

    public void Preprocess()
    {
        if(!isProcessed)
        {
            this.ComputeInfo(mesh.mesh);
            isProcessed = true;
        }
        
    }
    public void ComputeInfo(Mesh mesh)
    {
        Bounds meshBounds = mesh.bounds;
        Vector3 meshSize = meshBounds.size;
        float averageSize = Mathf.Pow(Utils.Volume(meshSize) / maxGridCount, 1.0f / 3.0f);
        volumeInfos = new VolumeCell<VolumeInfo>(meshBounds, averageSize);
        
        volumeInfos.ForEachAssign(VolumeInfo.GetDefault());

        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;
        float cosThreshold = CosSplitNormalDegreeThreshold;
        for (int i=0;i<vertices.Length;i++)
        {
            Vector3 normal = normals[i];
            Vector3 vertex = vertices[i];
            volumeInfos[vertex].AddNormal(normal, cosThreshold);
            //Debug.Log($"Volume Pos {volumeInfos.GetIndex(vertex)}");
        }

        //TODO: more analyze
    }

    public VolumeInfo SearchUntilHit(Vector3 pos, Vector3Int dir)
    {
        Vector3Int beginIndex = volumeInfos.GetIndex(pos);
        while(volumeInfos.IsValidIndex(beginIndex))
        {
            if (!volumeInfos[beginIndex].IsEmpty)
            {
                return volumeInfos[beginIndex];
            }
            beginIndex += dir;
        }
        return EMPTY_INFO;
    }

    public Vector3 SampleDir(Vector3 pos,Vector3Int dir)
    {
        VolumeInfo vi1 = SearchUntilHit(pos, dir);
        Vector3 n1 = vi1.GetNotSimilarNormal(dir);
        VolumeInfo vi2 = SearchUntilHit(pos, -dir);
        Vector3 n2 = vi2.GetNotSimilarNormal(-dir);
        Vector3 n = n1 - n2;
        if(n!=Vector3.zero)
        {
            return n.normalized;
        }else
        {
            return Vector3.zero;
        }
    }
}
