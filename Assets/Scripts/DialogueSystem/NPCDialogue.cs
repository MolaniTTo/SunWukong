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
    private InputSystem_Actions inputActions;
    private InputAction interactInputAction;
    private bool interactPressed = false; // Flag para detectar input

    [Header("Monje Bueno Ref")]
    public GameObject objectToHide;
    public MonjeBueno monjeBueno;

    [Header("Monje Boss Ref")]
    public Monje monjeBoss;

    [Header("NPC Identity")]
    public string npcID = "";
    private bool isSpawnedNPC = false;

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

        inputActions = new InputSystem_Actions();
        interactInputAction = inputActions.Player.Interact;
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        Debug.Log($"START ejecutado para {npcID}, isSpawnedNPC: {isSpawnedNPC}");

        if (ProgressManager.Instance != null && !string.IsNullOrEmpty(npcID)) //si tenim un ID d'NPC vàlid
        {
            string savedLocation = ProgressManager.Instance.GetNPCLocation(npcID);

            Debug.Log($"   - Ubicación guardada: {savedLocation}"); // NUEVO LOG

            if (!isSpawnedNPC && !string.IsNullOrEmpty(savedLocation))
            {
                Debug.Log($"NPC {npcID} ya se teletransportó a {savedLocation}. Desactivando NPC original de la escena.");
                gameObject.SetActive(false);
                return;
            }

            string savedDialogueKey = ProgressManager.Instance.GetNPCCurrentDialogue(npcID); //obtenim el diàleg guardat per a aquest NPC
            
            Debug.Log($"   - Diálogo guardado: {savedDialogueKey}"); // NUEVO LOG

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
        Debug.Log($"Estado final de {npcID}: dialogue={dialogue?.name}, hasBeenUsed={dialogue?.hasBeenUsed}"); // NUEVO LOG
    }

    public void MarkAsSpawned()
    {
        isSpawnedNPC = true;
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"NPCDialogue ({npcID}): Player encontrado");
            }
            else
            {
                Debug.LogWarning($"NPCDialogue ({npcID}): Player no encontrado con tag 'Player'");
            }
        }
    }


    private void OnEnable()
    {
        Debug.Log($"🟩 OnEnable ejecutado para {npcID}");

        // ⭐ NUEVO: Habilitar y suscribirse al callback
        inputActions.Player.Enable();
        interactInputAction.performed += OnInteractPerformed;

        Debug.Log($"   - InputActions habilitado y callback suscrito para {npcID}");
    }

    private void OnDisable()
    {
        // ⭐ NUEVO: Deshabilitar y desuscribirse del callback
        inputActions.Player.Disable();
        interactInputAction.performed -= OnInteractPerformed;
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        Debug.Log($"🟢 OnInteractPerformed llamado para {npcID}");
        interactPressed = true;
    }

    private void Update()
    {
        if (recentlyFinished || isTeleporting) { return; }
        if (player == null) { return; }
        if (!playerInRange) { return; }
        if (DialogueManager.Instance != null && DialogueManager.Instance.DialogueActive) { return; }

        if (requireButton)
        {
            // ⭐ NUEVO: Verificar el flag del callback
            if (interactPressed)
            {
                Debug.Log($"🔵 Input detectado para {npcID}");
                interactPressed = false; // ⭐ Resetear flag
                bubbleSprite?.SetActive(false);
                OpenDialogue();
            }
        }
        else
        {
            bubbleSprite?.SetActive(false);
            OpenDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        Debug.Log($"Player entró en rango de {npcID}");
        playerInRange = true;

        if (!recentlyFinished && (!dialogue.onlyOnce || !dialogue.hasBeenUsed)) //si no s'ha acabat recentment i el diàleg no és "onlyOnce" o no s'ha usat encara
        {
            if (bubbleSprite != null)
            {
                bubbleSprite.SetActive(true);
                Debug.Log($"Burbuja activada para {npcID}");
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
        Debug.Log($"Intentando abrir diálogo para {npcID}"); // NUEVO LOG
        Debug.Log($"   - dialogue es null: {dialogue == null}"); // NUEVO LOG
        Debug.Log($"   - dialogue.onlyOnce: {dialogue?.onlyOnce}"); // NUEVO LOG
        Debug.Log($"   - dialogue.hasBeenUsed: {dialogue?.hasBeenUsed}"); // NUEVO LOG

        if (dialogue.onlyOnce && dialogue.hasBeenUsed) { Debug.LogWarning("NPCDialogue: Diálogo ya ha sido usado y es 'onlyOnce', no se inicia."); return; }
        if (dialogue == null) { Debug.LogWarning("NPCDialogue: No hay diálogo asignado, no se inicia."); return; }
        if (DialogueManager.Instance.DialogueActive) { Debug.LogWarning("NPCDialogue: Ya hay un diálogo activo, no se inicia otro."); return; }

        if (bubbleSprite != null)
            bubbleSprite.SetActive(false);

        DialogueManager.Instance.dialogueUI = npcDialogueUI;
        npcDialogueUI.AssignNPC(this);

        DialogueManager.Instance.StartNPCDialogue(
            dialogue,
            npcAnimator,
            OnDialogueFinished
        );

        if (forceCameraZoomOnStart && npcDialogueUI != null)
        {
            npcDialogueUI.ForceCameraZoom();
        }

        Debug.Log($"Diálogo iniciado para {npcID}"); // NUEVO LOG
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
            StartCoroutine(HandleNPCTeleport());
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
        if (dialogue != null && !dialogue.teleportNPCAfterDialogue)
        {
            StartCoroutine(PreventImmediateRestart());
        }
        DialogueManager.Instance.EndDialogueMusic();
    }


    private IEnumerator HandleNPCTeleport()
    {
        if(ProgressManager.Instance == null || string.IsNullOrEmpty(npcID)) { yield break; }

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

        particleEffect.Play();
        audioSource.PlayOneShot(teleportSound);
        yield return new WaitForSeconds(0.3f);

        gameObject.SetActive(false); //desactivem l'NPC actual

        ProgressManager.Instance.SpawnNPCAtLocation(npcID);
    }

    private IEnumerator PreventImmediateRestart()
    {
        recentlyFinished = true;
        yield return null; // espera UN frame
        recentlyFinished = false;
    }
}