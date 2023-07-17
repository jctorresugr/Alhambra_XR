using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportAnimation : MonoBehaviour
{
    public float movingSpeed = 1.0f;
    public Vector3 targetPos;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 delta = targetPos - transform.position;
        float dt = Time.deltaTime;
        float step = dt * movingSpeed;
        float stepSqr = step*step;
        if(delta.sqrMagnitude> stepSqr)
        {
            transform.position += step * delta;
        }else
        {
            transform.position = targetPos;
        }
    }

    public void TeleportTo(Vector3 pos)
    {
        targetPos = pos;
    }
}
