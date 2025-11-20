using Unity.Cinemachine;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI Refs")]
    public GameObject dialoguePanel; //ja sigui una vinyeta o un panell fix a la UI
    public TMP_Text dialogueText;
    public Button continueButton; //Botó per continuar el diàleg pero podem utilitzar la tecla E
    public float typingSpeed = 0.03f;
    private bool autoAdvance = false;


    [Header("Camera Zoom (optional)")]
    public bool useCameraZoom = true; // si quieres usar zoom via Camera.main
    public float zoomDuration = 0.35f;
    public CinemachineCamera vcam;
    public float zoomedFOV = 30f;
    private float originalFOV = -1f;

    //ESTAT DEL DIÀLEG
    private DialogueData.DialogueLine[] lines; //Línies del diàleg actuals
    private int index; //Índex de la línia actual
    private bool isTyping;
    private bool canContinue;
    private System.Action onFinishCallback; //Callback quan el diàleg acaba
    private Animator currentTargetAnimator; //Referència a l'animator de l'NPC actual
    private NPCDialogue currentNPCDialogue; //Referència al NPCDialogue actual

    private Coroutine zoomCoroutine;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (dialoguePanel != null) { dialoguePanel.SetActive(false); } //Assegura que el panell de diàleg està desactivat al principi
        if (continueButton != null) { continueButton.onClick.AddListener(OnContinuePressed); } //Assigna el botó de continuar
    }

    private void Update()
    {
        if (!autoAdvance && Input.GetKeyDown(KeyCode.E) && canContinue)
        {
            OnContinuePressed();
        }
    }

    public void StartDialogue(DialogueData data, Animator targetAnimator = null, System.Action onFinish = null, bool autoAdvance = false) //Inicia un diàleg amb les dades proporcionades
    {
        if (data == null) { return; }

        ForceClearInternalState();

        lines = data.lines;
        index = 0;
        onFinishCallback = onFinish;
        currentTargetAnimator = targetAnimator;

        if (dialoguePanel != null) { dialoguePanel.SetActive(true); }//Activa el panell de diàleg

        if (vcam != null && originalFOV < 0) //Assegura que guardem la mida original de la càmera només una vegada
        {
            //originalFOV = vcam.Lens.VerticalFOV;
        }

        ShowNextLine();
        this.autoAdvance = autoAdvance;
    }

    public void ForceCloseUI()
    {
        ForceClearInternalState();

        if(dialoguePanel != null) { dialoguePanel.SetActive(false); } //Desactiva el panell de diàleg

        onFinishCallback?.Invoke(); //Invoca el callback quan el diàleg acaba

        lines = null;
        index = 0;
        currentTargetAnimator = null;
        onFinishCallback = null;

        if(useCameraZoom && vcam != null && originalFOV >= 0) //Torna a la mida original de la càmera si cal
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            {
                StartCoroutine(ZoomToFOV(originalFOV, zoomDuration));

            }
        }
    }

    private void ForceClearInternalState() //Neteja l'estat intern del diàleg
    {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine); //Atura l'escriptura si està en curs
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine); //Atura el zoom si està en curs

        isTyping = false;
        canContinue = false;
    }


    public void OnContinuePressed()
    {
        if (!canContinue) return;

        if (isTyping) //Si està escrivint, mostra la línia completa immediatament
        {
            if (typingCoroutine != null) { StopCoroutine(typingCoroutine); } //Atura l'escriptura en curs
            dialogueText.text = lines[index].text; //Mostra la línia completa
            isTyping = false;
            canContinue = true;
            return;
        }

        index++;
        if (index < lines.Length)
        {
            ShowNextLine();
        }
        else
        {
            EndDialogue();
        }
    }


    private void ShowNextLine()
    {
        if (lines == null || index < 0 || index >= lines.Length) return;

        DialogueData.DialogueLine line = lines[index]; //Línia actual del diàleg
        if (autoAdvance && continueButton != null)
        {
            continueButton.gameObject.SetActive(false);
        }

        if (line.requestCameraZoom && useCameraZoom && vcam != null) //Fes zoom si la línia ho sol·licita
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(ZoomToFOV(zoomedFOV, zoomDuration));
        }
        else
        {
            if (useCameraZoom && originalFOV >= 0f && vcam != null) //Torna a la mida original de la càmera si cal
            {
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(ZoomToFOV(originalFOV, zoomDuration));
            }
        }

        if (!string.IsNullOrEmpty(line.animatorTrigger) && currentTargetAnimator != null) //Fes l'animació si es proporciona un trigger
        {
            currentTargetAnimator.SetTrigger(line.animatorTrigger); //Activa el trigger de l'animator
        }
        if(line.deactivateObjects == true)
        {
            Debug.Log("DialogueUI: Desactivant objectes per la línia de diàleg " + index);
            if (currentNPCDialogue != null)
            {
                Debug.Log("DialogueUI: Desactivant l'objecte " + currentNPCDialogue.objectToHide.name);
                currentNPCDialogue.objectToHide.SetActive(false); 
                currentNPCDialogue.monjeBueno.ActivateStaffToPlayer();
            }
        }

        dialogueText.text = ""; //Neteja el text abans d'escriure la nova línia
        if (typingCoroutine != null) StopCoroutine(typingCoroutine); //Atura qualsevol escriptura en curs
        typingCoroutine = StartCoroutine(TypeLine(line.text)); //Inicia l'escriptura de la línia caràcter per caràcter
    }

    IEnumerator TypeLine(string line) //Corrutina per escriure la línia caràcter per caràcter
    {
        isTyping = true;
        canContinue = false;
        foreach (char c in line)
        {
            dialogueText.text += c;
            yield return new WaitForSeconds(typingSpeed); //Espera entre cada caràcter
        }
        isTyping = false;
        canContinue = true;
        if (autoAdvance)
        {
            yield return new WaitForSeconds(0.4f);
            OnContinuePressed();
        }
    }

    IEnumerator ZoomToFOV(float targetFOV, float duration)
    {
        //float startFOV = vcam.Lens.VerticalFOV;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            //vcam.Lens.VerticalFOV = Mathf.Lerp(startFOV, targetFOV, t);
            yield return null;
        }

        //cam.Lens.VerticalFOV = targetFOV;
    }

    private void EndDialogue()
    {
        if (dialoguePanel != null) { dialoguePanel.SetActive(false); } //Desactiva el panell de diàleg
        if (vcam != null && originalFOV >= 0) //Torna a la mida original de la càmera si cal
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            {
                StartCoroutine(ZoomToFOV(originalFOV, zoomDuration));
            }
            
        }
        onFinishCallback?.Invoke(); //Invoca el callback quan el diàleg acaba
        lines = null;
        index = 0;
        currentTargetAnimator = null;
        onFinishCallback = null;
        isTyping = false;
        canContinue = false;
    }

    public void ForceCameraZoom()
    {
        if (!useCameraZoom) return; //No fem res si no s'ha activat l'opció de zoom
        if (vcam == null) return;
    
        if (originalFOV < 0f)
            originalFOV = vcam.Lens.OrthographicSize;

        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomToFOV(zoomedFOV, zoomDuration));
    }

    public void AssignNPC(NPCDialogue npc)
    {
        currentNPCDialogue = npc;
    }
}
