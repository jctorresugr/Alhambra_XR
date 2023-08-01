using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScreenText : MonoBehaviour
{
    /// <summary>
    /// Any text that has to be displayed
    /// </summary>
    public UnityEngine.UI.Text RandomText;

    /// <summary>
    /// The Random string value
    /// </summary>
    [SerializeField]
    private String m_randomStr = "";

    /// <summary>
    /// Should we enable the Random Text?
    /// </summary>
    private bool m_enableRandomText = false;

    /// <summary>
    /// Should we update the random text values?
    /// </summary>
    private bool m_updateRandomText = false;

    [SerializeField]
    private float displayTime = 0.0f;

    public void SetText(String text, float time = float.MaxValue)
    {
        lock (this)
        {
            m_randomStr = text;
            m_enableRandomText = true;
            m_updateRandomText = true;
            displayTime = time;
        }
    }

    public bool EnableText
    {
        get => m_enableRandomText;
        set
        {
            lock(this)
            {
                m_enableRandomText = value;
                m_updateRandomText = true;
            }
            
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        m_updateRandomText = true;
        m_enableRandomText = false;
    }

    // Update is called once per frame
    void Update()
    {
        lock (this)
        {
            //Update the displayed text requiring networking attention
            if (m_updateRandomText)
            {
                //Enable/Disable the random text
                RandomText.enabled = m_enableRandomText;

                //If we should enable the text, set the text value
                if (m_enableRandomText)
                {
                    RandomText.text = m_randomStr;
                }
                m_updateRandomText = false;
            }
            displayTime -= Time.deltaTime;
            if (displayTime < 0.0f)
            {
                this.EnableText = false;
            }
        }
            
    }
}
