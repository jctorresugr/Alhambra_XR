using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HashCell<T>
{
    public List<T>[,,] data;

    public Vector3 mul, add; // pos = int3((q+add)*mul)
    int xc, yc, zc;

    public HashCell(Bounds bounds, float cellSize):
        this(bounds,
            (int)(bounds.size.x/cellSize),
            (int)(bounds.size.y/cellSize),
            (int)(bounds.size.z/cellSize)
            )
    {
    }
    public HashCell(Bounds bounds, int xc, int yc, int zc)
    {
        Vector3 range = bounds.max - bounds.min;
        mul = new Vector3(xc/range.x, yc/range.y, zc/range.z);
        add = -bounds.min;
        data = new List<T>[xc, yc, zc];
        this.xc = xc-1;
        this.yc = yc-1;
        this.zc = zc-1;
        
    }

    public Vector3Int GetIndex(Vector3 pos)
    {
        Vector3 t = pos + add;
        return new Vector3Int(
            Mathf.Min(Mathf.Max(0,(int)(t.x * mul.x)),xc),
            Mathf.Min(Mathf.Max(0,(int)(t.y * mul.y)),yc),
            Mathf.Min(Mathf.Max(0,(int)(t.z * mul.z)),zc)
            );
    }


    public List<T> GetCell(Vector3Int index)
    {
        return data[index.x, index.y, index.z];
    }

    public List<T> GetCell(Vector3 pos)
    {
        return GetCell(GetIndex(pos));
    }

    public void Add(Vector3 pos, T newData)
    {
        Vector3Int index = GetIndex(pos);
        CreateCell(index);
        data[index.x, index.y, index.z].Add(newData);
    }

    public List<T> GetRangeCell(Vector3 pos, float range)
    {
        Vector3 minPos = new Vector3(pos.x - range, pos.y - range, pos.z - range);
        Vector3 maxPos = new Vector3(pos.x + range, pos.y + range, pos.z + range);
        return GetRangeCell(minPos, maxPos);
    }

    public List<T> GetRangeCell(Vector3 minPos, Vector3 maxPos)
    {
        List<T> result = new List<T>();
        Vector3Int minIndex = GetIndex(minPos);
        Vector3Int maxIndex = GetIndex(maxPos);
        for(int xi=minIndex.x;xi<=maxIndex.x;xi++)
        {
            for (int yi = minIndex.y; yi <= maxIndex.y; yi++)
            {
                for (int zi = minIndex.z; zi <= maxIndex.z; zi++)
                {
                    List<T> cell = data[xi, yi, zi];
                    if(cell!=null)
                    {
                        result.AddRange(cell);
                    }
                }
            }
        }
        return result;
    }

    public void CreateCell(Vector3Int index)
    {
        if(data[index.x, index.y, index.z]==null)
        {
            data[index.x, index.y, index.z] = new List<T>();
        }
    }
}
