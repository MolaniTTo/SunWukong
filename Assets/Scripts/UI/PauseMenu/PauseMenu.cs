using UnityEngine;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject pausePanel; //panel de la UI del menu de pausa

    [Header("Ref")]
    [SerializeField] private PlayerStateMachine player; //ref del player per bloquejar controls

    private bool isPaused = false;

    private void Awake()
    {
        //Assegura que el joc comenci sense estar pausat
        Time.timeScale = 1f;
        pausePanel.SetActive(false);
    }


    void Update()
    {
        //Detectar tecla ESC o el boto de pausa del mando
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.JoystickButton7))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        pausePanel.SetActive(true);
        Time.timeScale = 0f; //pausa el joc

        //bloqueja els controls del player
        if (player != null)
        {
            player.dialogueLocked = true;
        }
    }

    public void ResumeGame()
    {
        isPaused = false;
        pausePanel.SetActive(false);
        Time.timeScale = 1f; //Renauda el joc

        //Desbloqueja els controls del player
        if (player != null)
        {
            player.dialogueLocked = false;
        }
    }

    //Boto de guardar i continuar
    public void SaveAndContinue()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SaveProgress(); //Guardar el progrés del joc en el ProgressManager
            Debug.Log("💾 Partida guardada!");
        }

        ResumeGame(); //Renauda el joc després de guardar
    }

    //boto de guardar i sortir al menu principal
    public void SaveAndExit()
    {
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.SaveProgress(); //goarda el progrés del joc en el ProgressManager
            Debug.Log("💾 Partida guardada!");
        }

        Time.timeScale = 1f; //Renauda el joc abans de canviar d'escena
        SceneManager.LoadScene("MainMenu"); //Cambia a l'escena del menu principal
    }

    //Boto de sortir sense guardar (opcional)
    public void ExitWithoutSaving()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}