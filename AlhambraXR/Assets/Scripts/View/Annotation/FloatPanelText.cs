using Microsoft.MixedReality.Toolkit;
using Microsoft.MixedReality.Toolkit.Input;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class FloatPanelText : MonoBehaviour, IMixedRealityInputActionHandler
{
    [Header("Components")]
    public TextMeshPro textMeshPro;
    public Transform textPanel;
    public Transform textPanelEdge;
    public PositionScaleAnimation textScaleAnimation;
    public BoxCollider boxCollider;

    [Header("UI")]
    public float textPanelSize = 0.15f;
    public int maxTextLength = 100;
    [Header("Interaction")]
    public float hideDistanceSqr = 1.0f;
    public Transform user;
    private bool causedByDistance = false;
    private bool tempHide = false;
    private Vector3 lastPosition; //position for distance judgement

    //invoke when teleport
    // will hide the annotation even if outside hideDistance range
    // this effect will be removed if user is outside 2*hideDistance range
    public void MarkAsTemporaryHide()
    {
        tempHide = true;
    }

    public void SetText(string text)
    {
        textMeshPro.text = ProcessText(text);
        textMeshPro.ForceMeshUpdate();
        Vector3 textBounds = textMeshPro.textBounds.size;
        textPanel.localScale = new Vector3(Mathf.Abs(textBounds.x) * textPanelSize, 0.01f, Mathf.Abs(textBounds.y) * textPanelSize);

        textScaleAnimation.targetScale = textPanel.localScale;
        textPanel.position = textMeshPro.transform.position + textMeshPro.transform.forward * 0.01f;
        if(boxCollider!=null)
        {
            boxCollider.size = textBounds;
        }
    }

    public void SetAnimationPos(Vector3 target, Vector3 hide)
    {
        textScaleAnimation.targetPos = target;
        textScaleAnimation.hidePos = hide;
        lastPosition = target;
    }

    public string ProcessText(string text)
    {
        if(text.Length>maxTextLength)
        {
            return text.Substring(0, text.Length)+ " ... [truncated]";
        }
        return text;
    }

    //interaction codes

    void Update()
    {
        float distanceSqr = (lastPosition - user.transform.position).sqrMagnitude;
        if (distanceSqr < hideDistanceSqr)
        {
            textScaleAnimation.Hide();
            causedByDistance = true;
        }
        else if(causedByDistance)
        {
            if(!tempHide)
            {
                textScaleAnimation.Show();
            }
            causedByDistance = false;
        }
        else if(tempHide && distanceSqr > hideDistanceSqr*4)
        {
            tempHide = false;
        }
    }

    void IMixedRealityInputActionHandler.OnActionStarted(BaseInputEventData eventData)
    {
        if (eventData.InputSource.SourceType == InputSourceType.Hand && eventData.MixedRealityInputAction.Description == "Select")
        {
            textScaleAnimation.Hide();
            causedByDistance = false;
        }
    }

    void IMixedRealityInputActionHandler.OnActionEnded(BaseInputEventData eventData)
    {
    }
}
