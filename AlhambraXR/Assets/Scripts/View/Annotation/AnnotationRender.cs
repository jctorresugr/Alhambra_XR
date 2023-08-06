using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AnnotationRender : AnnotationRenderBase
{
    [Header("Components")]
    public Annotation data;
    public Transform frame;
    public FloatPanelText floatPanelText;
    public ReferenceTransform referenceTransform;
    public ScaleAnimation frameScaleAnimation;
    //public Transform textStick;

    [Header("Idle Animation")]
    public float normalOffset = 0.05f;
    public float shrinkAnimationScale = 0.02f;
    public float shrinkAnimationTime = 4.0f;

    [Header("Text Plane Layout")]
    public float textMinOffset = 0.1f;
    public float textMaxOffset = 1.0f;
    public float defaultOffsetRelativeToOBB = 1.0f;
    public LayerMask modelLayer;

    private float animationTime = 0.0f;
    private Vector3 basicScale;
    private float textImpedeDownOffset = 0.0f;
    private float textImpedeUpOffset = 0.0f;

    public bool IsHidden => frameScaleAnimation.IsHidden;
    
    public void Show()
    {
        frameScaleAnimation.IsHidden = false;
        //floatPanelText.textScaleAnimation.IsHidden = true;
    }

    private void Update()
    {
        if(!frameScaleAnimation.IsHidden)
        {
            animationTime += Time.deltaTime;
            if (animationTime > shrinkAnimationTime)
            {
                animationTime = -shrinkAnimationTime;
            }
            /*
             * -t      => 0 =>       t
             * -1 => 0 => 1 => 0 => -1
             */
            float shrinkValue;
            shrinkValue = animationTime / shrinkAnimationTime;
            shrinkValue *= shrinkValue;//smoothe animation
            //shrinkValue = Mathf.Abs(shrinkValue);
            //1=>0=>1 mapping to -1=>1=>-1
            shrinkValue = (shrinkValue * -2) + 1;

            shrinkValue*= shrinkAnimationScale;
            frameScaleAnimation.targetScale = basicScale + new Vector3(shrinkValue, shrinkValue, shrinkValue);
        }

    }

    public void Hide()
    {
        frameScaleAnimation.IsHidden = true;
        //floatPanelText.textScaleAnimation.IsHidden = false;
    }
    public override void Init(Annotation data)
    {
        this.data = data;
        ResetPosition();
        basicScale = frame.localScale;
    }

    public void ResetPosition()
    {
        //compute layout
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
        frameScaleAnimation.targetScale = frame.localScale =
            Utils.MulVector3(
            referenceTransform.referTransform.lossyScale, data.renderInfo.OBBBounds.size);

        Vector3 ComputeOffset(float baseOffset=0.5f)
        {
            return frame.up *
            (
                data.renderInfo.OBBBounds.size.y * 
                baseOffset *
                referenceTransform.referTransform.lossyScale.y
            ) + 
            normalOffset* data.renderInfo.Normal
           ;
        }

        frame.position += ComputeOffset();
        floatPanelText.transform.position += ComputeOffset(defaultOffsetRelativeToOBB);
        RaycastHit hit;
        if(!Physics.Raycast(floatPanelText.transform.position,Vector3.down, out hit, float.MaxValue,modelLayer))
        {
            hit.distance = float.MaxValue;
        }
        textImpedeDownOffset = Mathf.Min(hit.distance, textMinOffset);
        if (!Physics.Raycast(floatPanelText.transform.position, Vector3.up, out hit, float.MaxValue, modelLayer))
        {
            hit.distance = float.MaxValue;
        }
        textImpedeUpOffset = Mathf.Min(hit.distance, textMaxOffset);
        Vector3 finalOffset = Vector3.up * (textImpedeUpOffset - textImpedeDownOffset);
        floatPanelText.transform.position += finalOffset;

        floatPanelText.SetText(data.info.Description);
        floatPanelText.SetAnimationPos(floatPanelText.transform.position, frame.transform.position);

        floatPanelText.textScaleAnimation.Hide();

    }

}
