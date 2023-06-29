using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachFadingEffect : MonoBehaviour
{
    public AnnotationJointLinesRender render;
    public Transform user;
    public float sqrThreshold = 1.0f;

    void Start()
    {
        Utils.EnsureComponent(this, ref render);
    }
    // Update is called once per frame
    void Update()
    {
        float distance = (user.position - transform.position).sqrMagnitude;
        if(distance<sqrThreshold)
        {
            if(render.navigateCacheData.IsHided)
            {
                render.navigateCacheData.Show();
            }
        }
        else
        {
            if(!render.navigateCacheData.IsHided)
            {
                render.navigateCacheData.Hide();

            }
        }
    }
}
