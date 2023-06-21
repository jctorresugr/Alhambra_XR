using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReferenceTransform : MonoBehaviour
{

    /// <summary>
    /// The transformation of the model, 
    /// </summary>
    public Transform referTransform;
    protected Bounds bounds;
    public Bounds ModelBound => bounds;
    //TODO: compute model bounds

    public Vector3 MapPosition(Vector3 pos)
    {
        if(referTransform==null)
        {
            return pos;
        }
        return referTransform.TransformPoint(pos);
    }

    public Vector3 InvMapPosition(Vector3 pos)
    {
        if (referTransform == null)
        {
            return pos;
        }
        return referTransform.InverseTransformPoint(pos);
    }

    public Quaternion MapRotation(Quaternion r)
    {
        if (referTransform == null)
        {
            return r;
        }
        return referTransform.rotation * r;
    }

    public float ScaleRefer => referTransform.lossyScale.magnitude;
}
