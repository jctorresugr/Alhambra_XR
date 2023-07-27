using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static PickPano;

public class FadeAnnotationInteraction : MonoBehaviour, IPickPanoListener
{
    public AnnotationRender render;
    public PickPano pickPano;
    public Transform user;
    public float sqrDistanceThreshold = 3.0f;
    [SerializeField]
    private bool isSelected = false;


    //another interaction function is in FlaotPanelText.cs

    void IPickPanoListener.OnSelection(PickPano pano, Color c)
    {
        if(render.data.ID.IsIncludedInColor(c))
        {
            isSelected = true;
            if(!render.IsHidden)
            {
                render.Hide();
            }
            PositionScaleAnimation textScaleAnimation = render.floatPanelText.textScaleAnimation;
            if (textScaleAnimation.IsHidden)
            {
                textScaleAnimation.Show();
            }
        }
        else
        {
            isSelected = false;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        Utils.EnsureComponent(this, ref render);
        if(pickPano!=null)
        {
            pickPano.AddListener(this);
        }
    }

    
    // Update is called once per frame
    void Update()
    {
        if(isSelected)
        {
            render.Hide();
            return;
        }
        if(IsInsideRange())
        {
            if(!render.IsHidden)
            {
                render.Hide();
                render.floatPanelText.textScaleAnimation.Show();
            }
        }
        else
        {
            if (render.IsHidden)
            {
                render.Show();
                render.floatPanelText.textScaleAnimation.Hide();
            }
                
        }
    }

    bool IsInsideRange()
    {
        Vector3 delta = user.position - transform.position;
        float distance = delta.sqrMagnitude;
        return distance < sqrDistanceThreshold;
    }

    void OnDestroy()
    {
        pickPano.RemoveListener(this);
    }

    void IPickPanoListener.OnHover(PickPano pano, Color c)
    {
        if (render.data.ID.IsIncludedInColor(c))
        {
            isSelected = true;
            if (!render.IsHidden)
            {
                render.Hide();
                render.floatPanelText.textScaleAnimation.Show();
            }
        }
        else
        {
            isSelected = false;
        }
    }

    public void OnSetTexture(PickPano pano, Texture2D newIndexTexture)
    {
    }
}
