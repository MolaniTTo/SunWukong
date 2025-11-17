using UnityEngine;
public class TriggerDialogueZone : MonoBehaviour
{
    public DialogueData dialogue; //referència al ScriptableObject de diàleg
    public bool startOnEnter = true; //inicia diàleg en entrar a la zona
    public bool closeOnExit = true; //tanca diàleg en sortir de la zona
    public bool blockPlayerDuringDialogue = true; // para tutorials a veces true
    public bool onlyOnce = true; //nomes s'executa el dialeg una vegada

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered && onlyOnce) { return; } //ja s'ha activat abans
        if (!startOnEnter) { return; } //no s'inicia en entrar
        if (!other.CompareTag("Player")) { return; } //no es el jugador

        if (DialogueManager.Instance != null && DialogueManager.Instance.DialogueActive) { return; } //si ja hi ha un diàleg actiu, no fem res

        DialogueManager.Instance.StartTriggerDialogue(dialogue, blockPlayerDuringDialogue, () => //callback quan acaba el diàleg
        {
            if (onlyOnce) { hasTriggered = true; } //si es nomes una vegada, marquem com a activat
        });
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!closeOnExit) { return; }
        if (!other.CompareTag("Player")) { return; }

        if (DialogueManager.Instance != null) //si hi ha un DialogueManager
        {
            DialogueManager.Instance.ForceClose(); //força el tancament del diàleg actual
        }
    }
}
