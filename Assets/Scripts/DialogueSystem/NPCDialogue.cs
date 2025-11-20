using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    [Header("Dialogue")]
    public DialogueData dialogue;               
    public Animator npcAnimator;                 
    public DialogueUI npcDialogueUI;            

    [Header("Bubble Icon")]
    public GameObject bubbleSprite;            
    public float showDistance = 3f;

    [Header("Settings")]
    public bool requireButton = true;           
    public KeyCode talkKey = KeyCode.E;         
    public bool forceCameraZoomOnStart = false;

    [Header("Monje References")]
    public GameObject objectToHide;
    public MonjeBueno monjeBueno;


    private bool playerInRange = false;
    private Transform player;

    private void Awake()
    {
        if (bubbleSprite != null)
            bubbleSprite.SetActive(false);

        if (npcDialogueUI == null)
            npcDialogueUI = GetComponentInChildren<DialogueUI>();
    }

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
    }

    private void Update()
    {
        if (player == null) return;

        if (!playerInRange && showDistance > 0f)
        {
            float d = Vector2.Distance(transform.position, player.position);
            if (d <= showDistance)
            {
                playerInRange = true;
                if (bubbleSprite != null)
                    bubbleSprite.SetActive(true);
            }
        }

        if (!playerInRange)
            return;

       
        if (DialogueManager.Instance != null && DialogueManager.Instance.DialogueActive)
            return;

        if (requireButton)
        {
            if (Input.GetKeyDown(talkKey))
                OpenDialogue();
        }
        else
        {
            OpenDialogue();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        playerInRange = true;

        if (bubbleSprite != null)
            bubbleSprite.SetActive(true);
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
        if (dialogue == null) return;
        if (DialogueManager.Instance.DialogueActive) return;

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
        // Cuando acaba, vuelve a mostrar la burbuja
        if (bubbleSprite != null)
            bubbleSprite.SetActive(true);
    }
}