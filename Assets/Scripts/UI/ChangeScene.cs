using UnityEngine;

public class ChangeScene : MonoBehaviour
{
    public string sceneName;

    public void ChangeToScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

}
