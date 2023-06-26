using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugOBB: MonoBehaviour
{
    public ReferenceTransform referenceTransform;
    public AnnotationRender render;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 center = render.data.renderInfo.averagePosition;
        Vector3 normal = render.data.renderInfo.Normal;
        Vector3 localCenter = render.data.renderInfo.OBBBounds.center;
        Vector3 tangent = render.data.renderInfo.Tangent;
        XYZCoordinate xYZCoordinate = new XYZCoordinate(normal,tangent);
        xYZCoordinate.translatePos = center;
        xYZCoordinate.Orthogonalization();
        transform.rotation =  Quaternion.LookRotation(xYZCoordinate.z, xYZCoordinate.y);
        transform.position = referenceTransform.MapPosition(center);
            referenceTransform.MapPosition(
          xYZCoordinate.TransformToGlobalPos(localCenter)
          );
        transform.localScale = 
            Utils.MulVector3(
            referenceTransform.referTransform.transform.lossyScale,
            render.data.renderInfo.OBBBounds.size);
    }
}
