using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitch : SocketDataBasic
{
    public Main main;
    // Start is called before the first frame update
    void Start()
    {
        FastReg<string>(OnReceiveSceneChange);
    }

    public void OnReceiveSceneChange(string sceneName)
    {
        main.AddTask(() =>
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single)
        );
    }
}
