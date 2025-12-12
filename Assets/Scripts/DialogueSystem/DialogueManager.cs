using UnityEngine;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;

    [Header("Refs")]
    public DialogueUI dialogueUI;
    public PlayerStateMachine player; //player per bloquejar moviment si cal
    public bool DialogueActive { get; private set; } = false;

    private string musicBeforeDialogue = "";

    private void Awake()
    {
        if (Instance == null) { Instance = this; } //Singleton
        else { Destroy(gameObject); }
    }

    public void StartNPCDialogue(DialogueData data, Animator targetAnimator = null, System.Action onFinish = null) //Inicia un diàleg amb les dades proporcionades i l'animator de l'NPC
    {
        if (data == null) { return; }
        if (DialogueActive) { return; } //si ja hi ha un diàleg actiu, no fem res

        DialogueActive = true;

        if (player != null)
        {
            player.EnterDialogueMode(); //a implementar a PlayerStateMachine per bloquejar moviment
        }

        dialogueUI.StartDialogue(data, targetAnimator, () => 
        {
            if (!data.exitDialogueModeByScripting && player != null) //Si el diàleg no gestiona l'script el mode diàleg, el desactiva automàticament
            {
                player.ExitDialogueMode();
            }
            DialogueActive = false; //marca que el diàleg ha acabat
            onFinish?.Invoke(); //Crida el callback quan el diàleg acaba
            Debug.Log("Diàleg acabat desde DialogueManager");
        });

        if (AudioManager.Instance != null && data.changeMusic)
        {
            musicBeforeDialogue = AudioManager.Instance.GetCurrentMusic(); //guarda la música actual abans de canviar-la

            AudioManager.Instance.PlayMusic(data.dialogueMusicKey, 1f); //canvia la música a la del diàleg

            Debug.Log($"Música de diálogo: {data.dialogueMusicKey}");
        }
    }

    public void EndDialogueMusic()
    {
        if (AudioManager.Instance != null && !string.IsNullOrEmpty(musicBeforeDialogue))
        {
            AudioManager.Instance.PlayMusic(musicBeforeDialogue, 1f);

            Debug.Log($"Restaurando música: {musicBeforeDialogue}");
            musicBeforeDialogue = "";
        }
    }

    public void StartTriggerDialogue(DialogueData data, bool blockPlayerDuringDialogue = false, System.Action onFinish = null, bool autoAdvance = false) //Inicia un diàleg des de trigger sense animator d'NPC
    {
        if (data == null) { return; }
        
        if (DialogueActive) { return; }

        DialogueActive = true;

        if (blockPlayerDuringDialogue && player != null)
        {
            player.EnterDialogueMode();
        }

        dialogueUI.StartDialogue(data, null, () =>
        {
            if (blockPlayerDuringDialogue && player != null)
                player.ExitDialogueMode();

            DialogueActive = false;
            onFinish?.Invoke();
        }, autoAdvance);
    }

    public void ForceClose() //Força el tancament del diàleg actual
    {
        if (!DialogueActive) return;

        if (dialogueUI != null)
        {
            dialogueUI.ForceCloseUI(); //tanca el UI de diàleg
        }

        if (player != null)
        {
            player.ExitDialogueMode(); //desbloqueja el jugador si estava bloquejat
        }

        DialogueActive = false;
    }


}
