using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// When user watch at this object within `nearbyDistance` distance for `nearbyDisappearTime` time
/// It will fire OnAttentionFocused event
/// </summary>
public class FocusDetection : MonoBehaviour
{
    public Camera userCamera;
    [Header("Settings")]
    public float nearbyDistance = 2.0f;
    public float nearbyViewDegree = 30.0f;
    public float nearbyDisappearTime = 2.0f;

    public delegate void FocusAttentionEvent();

    public event FocusAttentionEvent OnAttentionFocused;
    public event FocusAttentionEvent OnLoseAttentionFocused;


    [SerializeField]
    private float timeCounter = 0.0f;

    public void Accerlate(float time)
    {
        timeCounter += time;
    }
    // Start is called before the first frame update
    void Start()
    {
        if(userCamera==null)
        {
            userCamera = Camera.main;
        }
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 d = transform.position - userCamera.transform.position;
        float length = d.magnitude;
        if(length<nearbyDistance)
        {
            d /= length;
            // direction judge
            if (Vector3.Dot(d,userCamera.transform.forward)>Mathf.Cos(nearbyDistance))
            {
                timeCounter += Time.deltaTime;
            }else
            {
                // if nearby, reduce 75% time threshold
                if(timeCounter<nearbyDisappearTime*0.75f)
                {
                    timeCounter += Time.deltaTime * 0.5f;
                }
            }

            if(timeCounter>nearbyDisappearTime)
            {
                OnAttentionFocused?.Invoke();
            }
        }
        else
        {
            if (timeCounter > nearbyDisappearTime)
            {
                OnLoseAttentionFocused?.Invoke();
            }
            timeCounter = 0.0f;
        }


    }
}
