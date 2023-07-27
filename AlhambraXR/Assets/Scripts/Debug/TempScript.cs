using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempScript : MonoBehaviour
{
    public GameObject target;
    // Start is called before the first frame update
    void Start()
    {
        target.AddComponent<PressableButton>();
        target.AddComponent<NearInteractionTouchable>();
        PressableButton pb = target.GetComponent<PressableButton>();
        pb.ButtonPressed.AddListener(PressedEvent);
    }

    public void PressedEvent()
    {
        Debug.LogWarning("Pressed :)");
    }
    // Update is called once per frame
    void Update()
    {
    }
}
