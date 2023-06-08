using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VolumeCell<T> 
{
    public T[,,] data;

    Vector3 mul, add; // pos = int3((q+add)*mul)
    int xc, yc, zc;

    public int CountX =>xc;
    public int CountY =>yc;
    public int CountZ =>zc;

    public T this[int x,int y,int z]
    {
        get => data[x, y, z];
        set => data[x, y, z] = value;
    }

    public T this[Vector3 v]
    {
        get => this[v.x, v.y, v.z];
        set => this[v.x, v.y, v.z] = value;
    }

    public T this[float x, float y, float z]
    {
        get => data[
            Mathf.Min(Mathf.Max(0, (int)((x+add.x) * mul.x)), xc),
            Mathf.Min(Mathf.Max(0, (int)((y+add.y) * mul.y)), yc),
            Mathf.Min(Mathf.Max(0, (int)((z+add.z) * mul.z)), zc)
            ];
        set => data[
            Mathf.Min(Mathf.Max(0, (int)((x + add.x) * mul.x)), xc),
            Mathf.Min(Mathf.Max(0, (int)((y+add.y) * mul.y)), yc),
            Mathf.Min(Mathf.Max(0, (int)((z+add.z) * mul.z)), zc)
            ] = value;
    }

    public bool IsValidIndex(Vector3Int i)
    {
        return
            i.x >= 0 && i.x < xc &&
            i.y >= 0 && i.y < yc &&
            i.z >= 0 && i.z < zc;
    }

    public VolumeCell(Bounds bounds, float cellSize) :
        this(bounds,
            (int)(bounds.size.x / cellSize),
            (int)(bounds.size.y / cellSize),
            (int)(bounds.size.z / cellSize)
            )
    {
    }
    public VolumeCell(Bounds bounds, int xc, int yc, int zc)
    {
        Vector3 range = bounds.max - bounds.min;
        mul = new Vector3(xc / range.x, yc / range.y, zc / range.z);
        add = -bounds.min;
        data = new T[xc, yc, zc];
        this.xc = xc - 1;
        this.yc = yc - 1;
        this.zc = zc - 1;

    }


    public Vector3Int GetIndex(Vector3 pos)
    {
        Vector3 t = pos + add;
        return new Vector3Int(
            Mathf.Min(Mathf.Max(0, (int)(t.x * mul.x)), xc),
            Mathf.Min(Mathf.Max(0, (int)(t.y * mul.y)), yc),
            Mathf.Min(Mathf.Max(0, (int)(t.z * mul.z)), zc)
            );
    }

    public void ForEachAssign(T v)
    {
        for(int x=0;x<xc;x++)
        {
            for(int y=0;y<yc;y++)
            {
                for (int z = 0; z < zc; z++)
                {
                    data[x, y, z] = v;
                }
            }
        }
    }


    public T GetCell(Vector3Int index)
    {
        return data[index.x, index.y, index.z];
    }

    public T GetCell(Vector3 pos)
    {
        return GetCell(GetIndex(pos));
    }

    public void Set(Vector3 pos, T newData)
    {
        Vector3Int index = GetIndex(pos);
        data[index.x, index.y, index.z]=newData;
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
        for (int xi = minIndex.x; xi <= maxIndex.x; xi++)
        {
            for (int yi = minIndex.y; yi <= maxIndex.y; yi++)
            {
                for (int zi = minIndex.z; zi <= maxIndex.z; zi++)
                {
                    T cell = data[xi, yi, zi];
                    if (cell != null)
                    {
                        result.Add(cell);
                    }
                }
            }
        }
        return result;
    }

}
