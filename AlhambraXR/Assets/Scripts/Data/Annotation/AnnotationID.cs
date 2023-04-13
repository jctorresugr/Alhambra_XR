using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

[Serializable]
public struct AnnotationID : IEquatable<AnnotationID>
{

    public static readonly int LIGHT_ALL_INDEX = 255;
    public static readonly AnnotationID INVALID_ID = new AnnotationID(-1, -1);
    public static readonly AnnotationID LIGHTALL_ID = new AnnotationID(-1, LIGHT_ALL_INDEX);

    [SerializeField]
    private int m_layer;
    [SerializeField]
    private int m_index;

    public int Layer { get => m_layer; }
    public int ID { get => m_index; }

    public AnnotationID(int layer, int index)
    {
        this.m_layer = layer;
        this.m_index = index;
    }

    public AnnotationID(Color32 color)
    {
        if(color.r>0)
        {
            m_layer = 0;
            m_index = color.r;
        }
        else if (color.g > 0)
        {
            m_layer = 1;
            m_index = color.g;
        }
        else if (color.b > 0)
        {
            m_layer = 2;
            m_index = color.b;
        }
        else
        {
            m_layer = -1; //Invalid annotation ID
            m_index = 0;
            Debug.LogWarning("Create Invalid ID: " + this);
        }
    }

    public static explicit operator Color32(AnnotationID id)
    {
        if(0<=id.m_layer&& id.m_layer<=2)
        {
            Color32 color = new Color32();
            color[id.m_layer] = (byte)id.m_index;
            color.a = 1;
            return color;
        }
        return new Color32(0, 0, 0, 0);
    }

    public static explicit operator AnnotationID(Color32 c) => new AnnotationID(c);


    public bool Equals(AnnotationID other)
    {
        return 
            other.m_index == this.m_index && 
            other.m_layer == this.m_layer;
    }

    public override int GetHashCode()
    {
        return m_layer | (m_index<<4);
    }

    public override string ToString()
    {
        return String.Format("[ Layer= {0} , Index= {1}]", m_layer, m_index);
    }

    public static bool operator == (AnnotationID a, AnnotationID b)
    {
        return
                a.m_index == b.m_index &&
                a.m_layer == b.m_layer;
    }

    public static bool operator != (AnnotationID a, AnnotationID b)
    {
        return
                a.m_index != b.m_index ||
                a.m_layer != b.m_layer;
    }

    public override bool Equals(object obj)
    {
        if(obj is AnnotationID)
        {
            AnnotationID other = (AnnotationID)obj;
            return this == other;
        }
        return base.Equals(obj);
    }
}
