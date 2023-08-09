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
        {
            StartCoroutine(LoadAsyncScene(sceneName));
        });
    }

    IEnumerator LoadAsyncScene(string name)
    {
        // The Application loads the Scene in the background as the current Scene runs.
        // This is particularly good for creating loading screens.
        // You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
        // a sceneBuildIndex of 1 as shown in Build Settings.

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(name,LoadSceneMode.Single);

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
        yield break;
    }
}
