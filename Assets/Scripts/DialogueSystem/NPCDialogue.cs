using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;     //el text del diàleg           
    public Animator npcAnimator;                 
    public DialogueUI npcDialogueUI;       //la UI específica per al NPC

    [Header("Bubble Icon")]
    public GameObject bubbleSprite;            
    public float showDistance = 3f;

    [Header("Settings")]
    public bool requireButton = true;       //si cal prémer un botó per parlar    
    public KeyCode talkKey = KeyCode.E;         //la tecla per parlar amb l'NPC
    public bool forceCameraZoomOnStart = false; //si volem forçar el zoom de càmera en començar el diàleg
    private bool recentlyFinished = false;

    [Header("Input Actions")]
    [SerializeField] private InputActionReference interactAction;

    [Header("Monje Bueno Ref")]
    public GameObject objectToHide;
    public MonjeBueno monjeBueno;

    [Header("Monje Boss Ref")]
    public Monje monjeBoss;

    private bool playerInRange = false; //si el jugador està a l'abast
    private Transform player;

    private void Awake()
    {
        if (bubbleSprite != null) { bubbleSprite.SetActive(false); }
        if (npcDialogueUI == null) { npcDialogueUI = GetComponentInChildren<DialogueUI>(); }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform; //referència al jugador
                                                                        // Comprobar si este diálogo ya está completado según el ProgressManager
        if (ProgressManager.Instance != null && dialogue != null)
        {
            if (ProgressManager.Instance.IsDialogueCompleted(dialogue))
            {
                dialogue.hasBeenUsed = true;
                Debug.Log($"Diálogo '{dialogue.name}' ya completado anteriormente");
            }
        }
    }

    private void OnEnable()
    {
        if (interactAction != null)
        {
            interactAction.action.Enable();
        }
    }

    private void OnDisable()
    {
        if (interactAction != null)
        { 
             interactAction.action.Disable();
        }
    }

    private void Update()
    {
        if (recentlyFinished) { return; } //si acabem de finalitzar un diàleg, no fem res aquest frame
        if (player == null) { return; }

        /*if (!playerInRange && showDistance > 0f) //si el jugador esta fora de l'abast i tenim showDistance activat
        {
            float d = Vector2.Distance(transform.position, player.position);
            if (d <= showDistance)
            {
                playerInRange = true;
                if (bubbleSprite != null) { bubbleSprite.SetActive(true); }
            }
        }*/

        if (!playerInRange) { return; }//si el jugador no està a l'abast, no fem res
        if (DialogueManager.Instance != null && DialogueManager.Instance.DialogueActive) { return; }//si ja hi ha un diàleg actiu, no fem res

        if (requireButton) //si es requereix prémer un botó per parlar
        {
            if (interactAction != null && interactAction.action.WasPerformedThisFrame())
            {
                bubbleSprite.SetActive(false);
                OpenDialogue();
            }   
        }
        else //si no es requereix prémer un botó, obrim el diàleg automàticament
        {
            bubbleSprite.SetActive(false);
            OpenDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (!dialogue.onlyOnce || !dialogue.hasBeenUsed) //si el diàleg no és només una vegada o no s'ha utilitzat encara
        {
            if (bubbleSprite != null)
                bubbleSprite.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (bubbleSprite != null)
            bubbleSprite.SetActive(false);
    }

    private void OpenDialogue()
    {
        if (dialogue.onlyOnce && dialogue.hasBeenUsed) return;
        if (dialogue == null) return;
        if (DialogueManager.Instance.DialogueActive) return; //si ja hi ha un diàleg actiu, no fem res

        if (bubbleSprite != null)
            bubbleSprite.SetActive(false);

        // Le decimos al Manager qué UI debe usar para este NPC
        DialogueManager.Instance.dialogueUI = npcDialogueUI;

        npcDialogueUI.AssignNPC(this);

        DialogueManager.Instance.StartNPCDialogue(
            dialogue,
            npcAnimator,
            OnDialogueFinished
        );

        // Si quieres forzar zoom nada más entrar ignorando líneas:
        if (forceCameraZoomOnStart && npcDialogueUI != null)
        {
            npcDialogueUI.ForceCameraZoom();
        }
    }

    private void OnDialogueFinished()
    {
        if (ProgressManager.Instance != null && dialogue != null)
        {
            ProgressManager.Instance.RegisterDialogueCompleted(dialogue);
        }

        Debug.Log("NPCDialogue: Diálogo finalizado para " + gameObject.name);

        if (monjeBoss != null) { monjeBoss.dialogueFinished = true; }

        if (dialogue.onlyOnce)
        {
            dialogue.hasBeenUsed = true;

            if (bubbleSprite != null)
                bubbleSprite.SetActive(false);

            playerInRange = false;
            return;
        }

        if (bubbleSprite != null)
        {
            bubbleSprite.SetActive(true);
        }

        StartCoroutine(PreventImmediateRestart());

        DialogueManager.Instance.EndDialogueMusic();


    }

    private IEnumerator PreventImmediateRestart()
    {
        recentlyFinished = true;
        yield return null; // espera UN frame
        recentlyFinished = false;
    }
}