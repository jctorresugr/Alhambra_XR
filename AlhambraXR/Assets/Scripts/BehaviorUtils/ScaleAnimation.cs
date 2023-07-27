using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleAnimation : MonoBehaviour
{
    //if isHidden, scale to 0
    [SerializeField]
    public bool IsHidden { get; set; }
    //original scale
    public Vector3 targetScale;
    // Start is called before the first frame update
    protected void Start()
    {
        targetScale = this.transform.localScale;
    }

    public void Show()
    {
        IsHidden = false;
    }

    public void Hide()
    {
        IsHidden = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (IsHidden)
        {
            transform.localScale *= Mathf.Pow(0.01f, Time.deltaTime);
        }else
        {

            transform.localScale += (targetScale - transform.localScale) * Mathf.Min(1.0f, Time.deltaTime * 4.0f);
        }
    }
}
