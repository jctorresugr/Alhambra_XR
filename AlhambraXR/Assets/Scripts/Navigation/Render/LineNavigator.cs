using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A simple version, navigate by lines
//TODO: implement the design
public class LineNavigator : MonoBehaviour
{
    public LineRenderer lineRender;

    public bool Visible
    {
        get => lineRender.enabled;
        set => lineRender.enabled = value;
    }

    public void SetPositions(Vector3 from, Vector3 to)
    {
        //test code
        lineRender.SetPositions(new Vector3[] { from, to });

    }

    // Start is called before the first frame update
    void Start()
    {
        Utils.EnsureComponent(this, ref lineRender);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
