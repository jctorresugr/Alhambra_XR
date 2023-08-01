using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO: avoid collision
public class ConstraintFaceCamera : MonoBehaviour
{
    
    public Transform faceTo;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.forward = faceTo.forward;
    }
}
