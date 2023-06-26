using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BoundingBox
{
    /*
    public static void ComputeInclusiveOBBFromAABB(Bounds aabb,Vector3 normal)
    {
        Vector3 min = aabb.min;
        Vector3 max = aabb.max;
        Vector3[] points = new Vector3[8]
        {
            new Vector3(min.x,min.y,min.z),
            new Vector3(min.x,min.y,max.z),
            new Vector3(min.x,max.y,min.z),
            new Vector3(min.x,max.y,max.z),
            new Vector3(max.x,min.y,min.z),
            new Vector3(max.x,min.y,max.z),
            new Vector3(max.x,max.y,min.z),
            new Vector3(max.x,max.y,max.z)
        };

        XYZCoordinate coord = new XYZCoordinate(normal);
        Bounds coordBound = new Bounds();
        Vector3 initPos = coord.TransformToLocalPos(points[0]);
        coordBound.SetMinMax(initPos, initPos);
        for(int i=0;i<8;i++)
        {
            coordBound.Encapsulate(coord.TransformToLocalPos(points[i]));
        }
    }*/
}
