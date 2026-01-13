using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExitGame : MonoBehaviour
{
    public GameObject panel;
    public Button PlayButton;
    public Button OptionsButton;
    public Button ControllsButton;
    public Button ExitButton;

    public Button SelectedInPanel;

    private void Start()
    {
        panel.SetActive(false);
    }
    public void OpenExitPanel()
    {
        PlayButton.interactable = false;
        OptionsButton.interactable = false;
        ControllsButton.interactable = false;
        ExitButton.interactable = false;
        SelectedInPanel.Select();
        panel.SetActive(true);
    }

    public void CloseExitPanel()
    {
        PlayButton.interactable = true;
        OptionsButton.interactable = true;
        ControllsButton.interactable = true;
        ExitButton.interactable = true;
        panel.SetActive(false);
        ExitButton.Select();
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
