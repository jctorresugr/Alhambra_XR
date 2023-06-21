using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ApproachFadingEffect : MonoBehaviour
{
    public AnnotationJointLinesRender render;
    public Transform user;
    public float sqrThreshold = 1.0f;

    private bool stateShow = false;

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
            if(!stateShow)
            {
                render.navigateCacheData.Show();
            }
            stateShow = true;
        }
        else
        {
            if(stateShow)
            {
                render.navigateCacheData.Hide();

            }
            stateShow = false;
        }
    }
}
