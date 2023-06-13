using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct XYZCoordinate
{
    public Vector3 x,y,z;


    public XYZCoordinate(Vector3 x, Vector3 y, Vector3 z)
    {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public void Orthogonalization()
    {
        RemoveZeroVector();
        y = Vector3.Cross(z, x).normalized;
        z = Vector3.Cross(x, y).normalized;
        x = Vector3.Cross(y, z).normalized;
    }

    public void RemoveZeroVector()
    {
        if (x == Vector3.zero)
        {
            x = Vector3.right;
        }
        if (y == Vector3.zero)
        {
            y = Vector3.up;
        }
        if (z == Vector3.zero)
        {
            z = Vector3.forward;
        }
    }

    //to local
    public Vector3 TransformToLocalPos(Vector3 pos)
    {
        return
            new Vector3(
                pos.x*x.x+pos.y*y.x+pos.z*z.x,
                pos.x*x.y+pos.y*y.y+pos.z*z.y,
                pos.x*x.z+pos.y*y.z+pos.z*z.z
                );
    }

    //to global
    public Vector3 TransformToGlobalPos(Vector3 pos)
    {//TODO: revise
        return new Vector3(
            pos.x * x.x + pos.y * x.y + pos.z * x.z,
            pos.x * y.x + pos.y * y.y + pos.z * y.z,
            pos.x * z.x + pos.y * z.y + pos.z * z.z
            );
    }

}
