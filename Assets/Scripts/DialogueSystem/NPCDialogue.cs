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

    [Header("NPC Identity")]
    public string npcID = "";

    [Header("Teleport VFX")]
    public ParticleSystem particleEffect; // Sistema de partículas
    public AudioClip teleportSound; // Sonido opcional
    public AudioSource audioSource; // Fuente de audio para reproducir el sonido

    private bool playerInRange = false; //si el jugador està a l'abast
    private Transform player;
    private bool isTeleporting = false;

    private void Awake()
    {
        if (bubbleSprite != null) { bubbleSprite.SetActive(false); }
        if (npcDialogueUI == null) { npcDialogueUI = GetComponentInChildren<DialogueUI>(); }

        if(string.IsNullOrEmpty(npcID))
        {
            npcID = gameObject.name;
        }
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
         
        if(ProgressManager.Instance != null && !string.IsNullOrEmpty(npcID)) //si tenim un ID d'NPC vàlid
        {
            string savedDialogueKey = ProgressManager.Instance.GetNPCCurrentDialogue(npcID); //obtenim el diàleg guardat per a aquest NPC

            if (!string.IsNullOrEmpty(savedDialogueKey)) //si hi ha un diàleg guardat
            {
                //Carreguem el diàleg guardat des del Resources
                DialogueData loadedDialogue = Resources.Load<DialogueData>($"Dialogues/{savedDialogueKey}");
                if (loadedDialogue != null)
                {
                    dialogue = loadedDialogue;
                    Debug.Log($"NPCDialogue: Diálogo cargado desde progreso: {savedDialogueKey}");
                }
            }

            if (dialogue != null && ProgressManager.Instance.IsDialogueCompleted(dialogue))
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
        if (recentlyFinished || isTeleporting) { return; } //si acabem de finalitzar un diàleg, no fem res aquest frame
        if (player == null) { return; }
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

        if(!recentlyFinished && !isTeleporting && (!dialogue.onlyOnce || !dialogue.hasBeenUsed))
        {
            if (bubbleSprite != null)
            {
                bubbleSprite.SetActive(true);
            }
        } 
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = false;

        if (bubbleSprite != null)
            bubbleSprite.SetActive(false);

        recentlyFinished = false; //resetejem la variable per permetre reiniciar el diàleg si el jugador surt i torna a entrar
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

        if (dialogue != null && dialogue.teleportNPCAfterDialogue)
        {
            StartCoroutine(TeleportSequence());
            return;
        }

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
            bubbleSprite.SetActive(false);
        }

        playerInRange = false;
        StartCoroutine(PreventImmediateRestart());
        DialogueManager.Instance.EndDialogueMusic();
    }

    private IEnumerator TeleportSequence()
    {
        isTeleporting = true;

        if (bubbleSprite != null)
            bubbleSprite.SetActive(false);

        yield return new WaitForSeconds(0.3f);


        if (particleEffect != null)
        {
            particleEffect.Play();
            audioSource.PlayOneShot(teleportSound);
            Debug.Log("NPCDialogue: Partículas de teleport activadas");
            yield return new WaitForSeconds(0.3f);
        }

        HandleNPCTeleport();

        DialogueManager.Instance.EndDialogueMusic();
    }

    private void HandleNPCTeleport()
    {
        if(ProgressManager.Instance == null || string.IsNullOrEmpty(npcID)) { return; }

        Debug.Log($"NPCDialogue: Teleportando {npcID} a {dialogue.nextLocationID}");

        bool needsConditions = false;

        if (!string.IsNullOrEmpty(dialogue.nextDialogueKey))
        {
            DialogueData nextDialogue = Resources.Load<DialogueData>($"Dialogues/{dialogue.nextDialogueKey}");
            if (nextDialogue != null && nextDialogue.requiresBossDefeated)
            {
                needsConditions = true;
                Debug.Log($"NPCDialogue: Nueva ubicación requiere que el boss '{nextDialogue.requiredBossID}' esté derrotado");
            }
        }

        ProgressManager.Instance.SetNPCLocation(npcID, dialogue.nextLocationID, needsConditions);

        if (!string.IsNullOrEmpty(dialogue.nextDialogueKey))
        {
            ProgressManager.Instance.SetNPCDialogue(npcID, dialogue.nextDialogueKey);
        }

        ProgressManager.Instance.SpawnNPCAtLocation(npcID);

        gameObject.SetActive(false); //desactivem l'NPC actual
    }

    private IEnumerator PreventImmediateRestart()
    {
        recentlyFinished = true;
        yield return null; // espera UN frame
        recentlyFinished = false;
    }
}