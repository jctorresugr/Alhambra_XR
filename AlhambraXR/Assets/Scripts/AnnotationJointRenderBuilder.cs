using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnnotationJointRenderBuilder : MonoBehaviour
{
    public GameObject template;
    // Start is called before the first frame update
    void Start()
    {
        if(template==null)
        {
            Debug.LogWarning("AnnotationJointRenderBuilder requires a template to generate visual AnnotationJoint");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
