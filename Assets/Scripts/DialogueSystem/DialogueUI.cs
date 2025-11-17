using System.Collections;
using TMPro;
using Unity.Cinemachine;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueUI : MonoBehaviour
{
    [Header("UI Refs")]
    public GameObject dialoguePanel; //ja sigui una vinyeta o un panell fix a la UI
    public TMP_Text dialogueText;
    public Button continueButton; //Botó per continuar el diàleg pero podem utilitzar la tecla E
    public float typingSpeed = 0.03f;


    [Header("Camera Zoom (optional)")]
    public bool useCameraZoom = true; // si quieres usar zoom via Camera.main
    public float zoomedOrthoSize = 3.5f; //Mida ortogràfica quan fem zoom
    public float zoomDuration = 0.35f;
    public CinemachineCamera vcam; //Referència a la càmera virtual de Cinemachine


    //ESTAT DEL DIÀLEG
    private DialogueData.DialogueLine[] lines; //Línies del diàleg actuals
    private int index; //Índex de la línia actual
    private bool isTyping;
    private bool canContinue;
    private System.Action onFinishCallback; //Callback quan el diàleg acaba
    private Animator currentTargetAnimator; //Referència a l'animator de l'NPC actual

    private float originalOrthoSize = -1f; 
    private Coroutine zoomCoroutine;

    private Coroutine typingCoroutine;

    private void Awake()
    {
        if (dialoguePanel != null) { dialoguePanel.SetActive(false); } //Assegura que el panell de diàleg està desactivat al principi
        if (continueButton != null) { continueButton.onClick.AddListener(OnContinuePressed); } //Assigna el botó de continuar
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E) && canContinue) //Permet continuar el diàleg amb la tecla E
        {
            OnContinuePressed();
        }
    }

    public void StartDialogue(DialogueData data, Animator targetAnimator = null, System.Action onFinish = null) //Inicia un diàleg amb les dades proporcionades
    {
        if (data == null) { return; }

        ForceClearInternalState();

        lines = data.lines;
        index = 0;
        onFinishCallback = onFinish;
        currentTargetAnimator = targetAnimator;

        if (dialoguePanel != null) { dialoguePanel.SetActive(true); }//Activa el panell de diàleg

        if (vcam != null && originalOrthoSize < 0) //Assegura que guardem la mida original de la càmera només una vegada
        {
            originalOrthoSize = vcam.Lens.OrthographicSize; 
        }

        ShowNextLine();
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

        if(useCameraZoom && vcam != null && originalOrthoSize >= 0) //Torna a la mida original de la càmera si cal
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            {
                StartCoroutine(ZoomTo(originalOrthoSize, zoomDuration));
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

        if (line.requestCameraZoom && useCameraZoom && vcam != null) //Fes zoom si la línia ho sol·licita
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(ZoomTo(zoomedOrthoSize, zoomDuration));
        }
        else
        {
            if (useCameraZoom && originalOrthoSize >= 0f && vcam != null) //Torna a la mida original de la càmera si cal
            {
                if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
                zoomCoroutine = StartCoroutine(ZoomTo(originalOrthoSize, zoomDuration));
            }
        }

        if (!string.IsNullOrEmpty(line.animatorTrigger) && currentTargetAnimator != null) //Fes l'animació si es proporciona un trigger
        {
            currentTargetAnimator.SetTrigger(line.animatorTrigger); //Activa el trigger de l'animator
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
    }

    IEnumerator ZoomTo(float targetSize, float duration) //Corrutina per fer zoom de la càmera
    {
        if (vcam == null) { yield break; }
        float start = vcam.Lens.OrthographicSize;
        float elapsed = 0f;
        while (elapsed < duration) //mentre no hagi acabat la durada del zoom 
        {
            elapsed += Time.deltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / duration);
            vcam.Lens.OrthographicSize = Mathf.Lerp(start, targetSize, t); //Interpolació suau entre la mida inicial i la mida objectiu
            yield return null;
        }
        vcam.Lens.OrthographicSize = targetSize;
    }
    private void EndDialogue()
    {
        if (dialoguePanel != null) { dialoguePanel.SetActive(false); } //Desactiva el panell de diàleg
        if (vcam != null && originalOrthoSize >= 0) //Torna a la mida original de la càmera si cal
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            {
                StartCoroutine(ZoomTo(originalOrthoSize, zoomDuration));
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
        if (!useCameraZoom) return;
        if (vcam == null) return;
    
        if (originalOrthoSize < 0f)
            originalOrthoSize = vcam.Lens.OrthographicSize;

        if (zoomCoroutine != null)
            StopCoroutine(zoomCoroutine);

        zoomCoroutine = StartCoroutine(ZoomTo(zoomedOrthoSize, zoomDuration));
    }
}
