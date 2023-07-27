using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlwaysFaceCamera : MonoBehaviour
{
    public Transform faceTo;
    // Start is called before the first frame update
    void Start()
    {
        if(faceTo==null)
        {
            faceTo = Camera.main.transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = faceTo.forward;
    }
}
