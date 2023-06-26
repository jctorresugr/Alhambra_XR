using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Threading;

public class CycleBin
{
    private long[] data;
    float scale;

    public CycleBin(int size)
    {
        data = new long[size];
        scale = 1.0f / (360.0f / size);
    }
    /*
    public long this[int index]
    {
        get => data[index];
        set => data[index]=value;
    }*/

    public void Tick()
    {
        Tick();
    }

    public int Index(float x, float y) => Index(new Vector2(x, y).normalized);

    public int Index(Vector2 v) => Index(Math.Atan2(v.x, v.y)*Mathf.Rad2Deg);
    public int Index(double deg) => (int)(Utils.DegRestrict(deg) * scale);

    public void AtomicTick(float x, float y,float ratio=1.0f)
    {
        AtomicTick(Index(x, y), Mathf.Sqrt(x*x+y*y)*ratio);
    }


    public void AtomicTick(int index,float distance)
    {
        Interlocked.Add(ref data[index], (long)(distance*distance * 100000000));
    }

    public float BestDeg()
    {
        int halfIndex = data.Length;// / 2;
        long[] temp = data;//new long[halfIndex];
        /*for (int i = 0; i < halfIndex; i++)
        {
            temp[i] = data[i] + data[i + halfIndex];
        }*/
        float invScale = 1.0f / scale;
        // find a lowest variance for all other values
        float bestScore = float.MaxValue;
        int bestIndex = 0;
        for(int i=0;i<halfIndex;i++)
        {
            //long vi = temp[i];
            float distance = 0;
            for (int j=0;j<halfIndex;j++)
            {
                long vj = temp[j];
                float degDis = (i - j) * invScale;
                float cosDis = Mathf.Abs(Mathf.Cos(degDis));
                distance += cosDis * vj;
            }
            if(distance<bestScore)
            {
                bestScore = distance;
                bestIndex = i;
            }
        }
        Debug.Log($"Best Index {bestIndex}");
        return bestIndex * invScale;
    }

}
