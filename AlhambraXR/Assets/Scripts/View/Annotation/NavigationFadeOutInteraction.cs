using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When comes nearby the annotation, fade out the navigation information
/// </summary>
public class NavigationFadeOutInteraction : MonoBehaviour, PickPano.IPickPanoListener
{
    public AnnotationRender annotationRender;
    public FocusDetection focusDetection;
    public NavigationCache navigationCache;
    public PickPano pickPano;

    void PickPano.IPickPanoListener.OnHover(PickPano pano, Color c)
    {
        focusDetection.Accerlate(Time.deltaTime);
    }

    void PickPano.IPickPanoListener.OnSelection(PickPano pano, Color c)
    {
        if(annotationRender.data.ID==(AnnotationID)c)
        {
            navigationCache.RemoveNavigationToDestionation(annotationRender.data);
        }
        
    }

    void PickPano.IPickPanoListener.OnSetTexture(PickPano pano, Texture2D newIndexTexture)
    {
    }

    void OnFocus()
    {
        navigationCache.RemoveNavigationToDestionation(annotationRender.data);
    }

    void OnLoseFocus()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        pickPano.AddListener(this);
        focusDetection.OnAttentionFocused += OnFocus;
        focusDetection.OnLoseAttentionFocused += OnLoseFocus;
    }

    void OnDestroy()
    {
        pickPano.RemoveListener(this);
        focusDetection.OnAttentionFocused -= OnFocus;
        focusDetection.OnLoseAttentionFocused -= OnLoseFocus;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
