using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugAveragePos : MonoBehaviour
{

    public ReferenceTransform referenceTransform;
    public AnnotationRender render;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        transform.position = referenceTransform.MapPosition(render.data.renderInfo.averagePosition)+transform.up*0.05f;
    }
}
