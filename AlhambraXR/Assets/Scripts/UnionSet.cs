using System.Collections;
using System.Collections.Generic;

class UnionSet
{
    private int[] info;

    public void SetSize(int size)
    {
        info = new int[size];
        for(int i=0;i<size;i++)
        {
            info[i] = i;
        }
    }

    public void Union(int indexA, int indexB)
    {
        info[indexA] = indexB;
    }

    public bool IsInSameGroup(int indexA, int indexB)
    {
        return FindRoot(indexA) == FindRoot(indexB);
    }

    public int FindRoot(int index)
    {
        int result = info[index];
        if(result != index)
        {
            int finalIndex = FindRoot(result);
            info[index] = finalIndex;
            return finalIndex;
        }else
        {
            return result;
        }
    }

    public HashSet<int> GetAllRootIndex()
    {
        HashSet<int> r = new HashSet<int>();
        foreach(int i in info)
        {
            r.Add(FindRoot(i));
        }
        return r;
    }
}
