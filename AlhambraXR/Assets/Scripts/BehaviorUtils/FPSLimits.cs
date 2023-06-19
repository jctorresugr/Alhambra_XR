using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class FPSLimits : MonoBehaviour
{
    public float limitFPS = 60;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(limitFPS<=0.1f)
        {
            limitFPS = 0.1f;
        }
        Thread.Sleep((int)(1000 / limitFPS));
    }
}
