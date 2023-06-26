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
        isHidden = true;
    }
    public override void Init(Annotation data)
    {
        this.data = data;
        ResetPosition();
    }

    public void ResetPosition()
    {
        Vector3 center = data.renderInfo.averagePosition;
        Vector3 normal = data.renderInfo.Normal;
        Vector3 localCenter = data.renderInfo.OBBBounds.center;
        Vector3 tangent = data.renderInfo.Tangent;
        XYZCoordinate xYZCoordinate = new XYZCoordinate(normal, tangent);
        xYZCoordinate.translatePos = center;
        xYZCoordinate.Orthogonalization();
        frame.rotation = Quaternion.LookRotation(xYZCoordinate.z, xYZCoordinate.y);
        frame.position = referenceTransform.MapPosition(
          xYZCoordinate.TransformToGlobalPos(
              localCenter
              )
          );
        targetScale = frame.localScale =
            Utils.MulVector3(
            referenceTransform.referTransform.lossyScale, data.renderInfo.OBBBounds.size);
        frame.position += frame.up * data.renderInfo.OBBBounds.size.y * 0.4f * referenceTransform.referTransform.lossyScale.y;
    }

}
