using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

//heap implementation from
//https://www.cnblogs.com/jn-shao/p/14369451.html
//C# .Net does not have a implementation of Heap Data Strucutre
//So I copy the code :(

public class Heap<T>
{
    private T[] _array;//���飬��Ŷ�����
    private int _count;//����������
    private HeapType _typeName;//������
    private const int _DefaultCapacity = 4;//Ĭ����������/��С����
    private const int _ShrinkThreshold = 50;//������ֵ���ٷֱȣ�
    private const int _MinimumGrow = 4;//��С������
    private const int _GrowFactor = 200;  // �������ݰٷֱ�,Ĭ��2��
    private IComparer<T> _comparer;//�Ƚ���
    private Func<T, T, bool> _comparerFunc;//�ȽϺ���


    //����������
    public int Count => _count;
    //������
    public HeapType TypeName => _typeName;


    public Heap() : this(_DefaultCapacity, HeapType.MinHeap, null) { }
    public Heap(int capacity) : this(capacity, HeapType.MinHeap, null) { }
    public Heap(HeapType heapType) : this(_DefaultCapacity, heapType, null) { }
    public Heap(int capacity, HeapType heapType, IComparer<T> comparer)
    {
        Init(capacity, heapType, comparer);
    }
    public Heap(IEnumerable<T> collection, HeapType heapType, IComparer<T> comparer)
    {
        if (collection == null)
            throw new IndexOutOfRangeException();
        Init(collection.Count(), heapType, comparer);
        using (IEnumerator<T> en = collection.GetEnumerator())//����T��GC�����з��й���Դ��GC�����ͷţ����ֶ�
        {
            while (en.MoveNext())
                Enqueue(en.Current);
        }
    }
    private void Init(int capacity, HeapType heapType, IComparer<T> comparer)
    {
        if (capacity < 0)
            throw new IndexOutOfRangeException();
        _count = 0;
        _array = new T[capacity];
        _comparer = comparer ?? Comparer<T>.Default;
        _typeName = heapType;
        switch (heapType)
        {
            default:
            case HeapType.MinHeap:
                _comparerFunc = (T t1, T t2) => _comparer.Compare(t1, t2) > 0;//Ŀ�����t2С
                break;
            case HeapType.MaxHeap:
                _comparerFunc = (T t1, T t2) => _comparer.Compare(t1, t2) < 0;//Ŀ�����t2��
                break;
        }
    }


    public T Dequeue()
    {
        if (_count == 0)
            throw new InvalidOperationException();
        T result = _array[0];
        _array[0] = _array[--_count];
        _array[_count] = default(T);

        if (_array.Length > _DefaultCapacity && _count * 100 <= _array.Length * _ShrinkThreshold)//����
        {
            int newCapacity = Math.Max(_DefaultCapacity, (int)((long)_array.Length * (long)_ShrinkThreshold / 100));
            SetCapacity(newCapacity);
        }
        AdjustHeap(_array, 0, _count);
        return result;
    }
    public void Enqueue(T item)
    {
        if (_count >= _array.Length)//����
        {
            int newCapacity = Math.Max(_array.Length + _MinimumGrow, (int)((long)_array.Length * (long)_GrowFactor / 100));
            SetCapacity(newCapacity);
        }

        _array[_count++] = item;
        int parentIndex;
        int targetIndex;
        int targetCount = _count;
        while (targetCount > 1)
        {
            parentIndex = targetCount / 2 - 1;
            targetIndex = targetCount - 1;
            if (!_comparerFunc.Invoke(_array[parentIndex], _array[targetIndex]))
                break;
            Swap(_array, parentIndex, targetIndex);
            targetCount = parentIndex + 1;
        }
    }
    private void AdjustHeap(T[] array, int parentIndex, int count)
    {
        if (_count < 2)
            return;
        int childLeftIndex = parentIndex * 2 + 1;
        int childRightIndex = (parentIndex + 1) * 2;

        int targetIndex = parentIndex;
        if (childLeftIndex < count && _comparerFunc.Invoke(array[parentIndex], array[childLeftIndex]))
            targetIndex = childLeftIndex;
        if (childRightIndex < count && _comparerFunc.Invoke(array[targetIndex], array[childRightIndex]))
            targetIndex = childRightIndex;
        if (targetIndex != parentIndex)
        {
            Swap(_array, parentIndex, targetIndex);
            AdjustHeap(_array, targetIndex, _count);
        }
    }

    private void SetCapacity(int capacity)
    {
        T[] newArray = new T[capacity];
        Array.Copy(_array, newArray, _count);
        _array = newArray;
    }

    private void Swap(T[] array, int index1, int index2)
    {
        T temp = array[index1];
        array[index1] = array[index2];
        array[index2] = temp;
    }

    public void Clear()
    {
        Array.Clear(_array, 0, _count);
        Init(_DefaultCapacity, HeapType.MinHeap, null);
    }
}

public enum HeapType { MinHeap, MaxHeap }