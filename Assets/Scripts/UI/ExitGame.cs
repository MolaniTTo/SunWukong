using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExitGame : MonoBehaviour
{
    public GameObject panel;
    public Button PlayButton;
    public Button OptionsButton;
    public Button ExitButton;

    private void Start()
    {
        panel.SetActive(false);
    }
    public void OpenExitPanel()
    {
        PlayButton.interactable = false;
        OptionsButton.interactable = false;
        ExitButton.interactable = false;
        panel.SetActive(true);
    }

    public void CloseExitPanel()
    {
        PlayButton.interactable = true;
        OptionsButton.interactable = true;
        ExitButton.interactable = true;
        panel.SetActive(false);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
