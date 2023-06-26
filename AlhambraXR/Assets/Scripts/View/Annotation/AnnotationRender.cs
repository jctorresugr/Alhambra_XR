using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationRender : AnnotationRenderBase
{
    public Annotation data;
    public Transform frame;
    public ReferenceTransform referenceTransform;
    protected Vector3 targetScale;
    [SerializeField]
    private bool isHidden = true;

    public bool IsHidden => isHidden;
    
    public void Show()
    {
        //frame.localScale = targetScale;
        isHidden = false;
    }

    private void Update()
    {
        if(isHidden)
        {
            frame.localScale *= Mathf.Pow(0.01f, Time.deltaTime);
        }
        else
        {
            frame.localScale += (targetScale - frame.localScale) * Mathf.Min(1.0f,Time.deltaTime*4.0f);
        }
    }

    public void Hide()
    {
        //frame.localScale = Vector3.zero;
        isHidden = true;
    }
    public override void Init(Annotation data)
    {
        this.data = data;
        ResetPosition();
    }

    public void ResetPosition()
    {/*
        frame.up = data.renderInfo.Normal;
        frame.position = 
            referenceTransform.MapPosition(GetProperAnnotationPos());
        targetScale = frame.localScale = 
            Utils.MulVector3(
                data.renderInfo.OBBBounds.size,
            referenceTransform.referTransform.lossyScale)*50.0f;
        */
        Vector3 center = data.renderInfo.averagePosition;
        Vector3 normal = data.renderInfo.Normal;
        Vector3 localCenter = data.renderInfo.OBBBounds.center;
        Vector3 tangent = data.renderInfo.Tangent;
        XYZCoordinate xYZCoordinate = new XYZCoordinate(normal, tangent);
        xYZCoordinate.translatePos = center;
        xYZCoordinate.Orthogonalization();
        frame.rotation = Quaternion.LookRotation(xYZCoordinate.x, xYZCoordinate.y);
        frame.position = referenceTransform.MapPosition(
          xYZCoordinate.TransformToGlobalPos(
              localCenter//Vector3.zero
              //new Vector3(0,data.renderInfo.Bounds.size.y*0.51f,0)
              )
          );
        targetScale = frame.localScale =
            Utils.MulVector3(
            referenceTransform.referTransform.lossyScale, data.renderInfo.OBBBounds.size);
        frame.position += frame.up * data.renderInfo.OBBBounds.size.y * 0.5f * referenceTransform.referTransform.lossyScale.y;
    }
    //TODO: optimize OBB calculation
    /*protected Vector3 GetProperAnnotationPos()
    {
        /*
        Vector3 center = data.renderInfo.Center;
        Vector3 normal = data.renderInfo.Normal;
        Vector3 tangent = data.renderInfo.Tangent;
        
        Vector3 size = data.renderInfo.Bounds.size * 0.5f;
        Vector3 sizeLocal = data.renderInfo.OBBBounds.size * 0.5f;
        XYZCoordinate xYZCoordinate = new XYZCoordinate(normal, tangent);
        xYZCoordinate.translatePos = center;
        Vector3 localPos = new Vector3(0.0f, -sizeLocal.y, 0.0f)+data.renderInfo.OBBBounds.center;
        //Vector3 localPos = new Vector3(0.0f, 0.05f, 0.0f);

        return xYZCoordinate.TransformToGlobalPos(localPos);
        
        float outBoxDis = float.MaxValue;
        for (int i = 0; i < 3; i++)
        {
            if (normal[i] != 0)
            {
                outBoxDis = Mathf.Min(outBoxDis, Mathf.Abs(size[i] / normal[i]));
            }
        }
        if (outBoxDis == float.MaxValue)
        {
            outBoxDis = 0.0f;
        }
        // with a tiny offset;
        return center + data.renderInfo.Normal * outBoxDis*0.5f;*//*
        Vector3 center = data.renderInfo.averagePosition;
        Vector3 normal = data.renderInfo.Normal;
        Vector3 localCenter = data.renderInfo.OBBBounds.center;
        Vector3 tangent = data.renderInfo.Tangent;
        XYZCoordinate xYZCoordinate = new XYZCoordinate(normal, tangent);
        xYZCoordinate.translatePos = center;
        return
          xYZCoordinate.TransformToGlobalPos(localCenter);
    }*/
}
