using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CurveSample
{

    //https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline
    public static List<Vector3> GenerateCurve(Vector3[] points,float sampleDistance,float alpha)
    {
        if(points.Length<=2)
        {
            return new List<Vector3>(points);
        }

        List<Vector3> results = new List<Vector3>();

        results.Add(points[0]);
        GenerateCurveBySegment(
           HeadMoveAlong(points[0],points[1]), points[0], points[1], points[2],
            alpha, sampleDistance, results);

        int lastIndex = points.Length - 1;
        for (int i=1;i<lastIndex-1;i++)
        {
            results.Add(points[i]);
            GenerateCurveBySegment(
            points[i-1], points[i], points[i+1], points[i+2],
            alpha, sampleDistance, results);
        }
        results.Add(points[lastIndex-1]);
        GenerateCurveBySegment(
            points[lastIndex - 2], points[lastIndex - 1], points[lastIndex], TailMoveAlong(points[lastIndex - 1], points[lastIndex]),
            alpha, sampleDistance, results);
        results.Add(points[lastIndex]);

        return results;

    }

    //result-p0-p1-p2
    protected static Vector3 HeadMoveAlong(Vector3 p0, Vector3 p1)
    {
        Vector3 d = p0 - p1;
        float offset = d.magnitude * 0.1f;
        return p0 + offset * d;

    }

    //result-p0-p1-p2
    protected static Vector3 TailMoveAlong(Vector3 p0, Vector3 p1)
    {
        Vector3 d = p1 - p0;
        float offset = d.magnitude * 0.1f;
        return p1 + offset * d;

    }

    public static void GenerateCurveBySegment(
        Vector3 p0, Vector3 p1,Vector3 p2, Vector3 p3,
        float alpha, float sampleDistance, // sampleCount: 1 means sample 1 point on the middle of segment
        List<Vector3> results)
    {
        CatmullRomCurve catmullRomCurve = new CatmullRomCurve(p0, p1, p2, p3,alpha);
        int sampleCount = (int)Mathf.Max(1, (p2 - p1).magnitude / sampleDistance);
        float sampleStep = 1.0f / (sampleCount + 1);
        float curSampleT = 0.0f;
        for(int i=0;i<sampleCount;i++)
        {
            curSampleT += sampleStep;
            results.Add(catmullRomCurve.GetPoint(curSampleT));
        }

    }
}
