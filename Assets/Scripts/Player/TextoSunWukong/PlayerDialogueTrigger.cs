/*using UnityEngine;

public class PlayerDialogueTrigger : MonoBehaviour
{
    [Header("Referencia al jugador")]
    public PlayerStateMachine player; // Para bloquear movimiento si quieres

    [Header("Sistema de diálogo")]
    public DialogueData dialogueToStart; // Diálogo que se mostrará
    public DialogueManager dialogueManager; // Referencia al sistema que muestra la viñeta

    private bool hasTriggered = false; // Para que solo ocurra una vez

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;                // Solo se ejecuta una vez
        if (!other.CompareTag("Player")) return; // Solo reacciona al jugador

        if (player != null)
        {
            // Bloqueamos movimiento del jugador si es necesario
            player.dialogueLocked = true;
            player.rb.linearVelocity = Vector2.zero;
            player.EnterDialogueMode(); // Método que bloquea al jugador (opcional)
        }

        if (dialogueManager != null && dialogueToStart != null)
        {
            // Iniciamos el diálogo en la viñeta
            dialogueManager.StartDialogue(dialogueToStart);
            hasTriggered = true;
            Debug.Log("PlayerDialogueTrigger: Diálogo iniciado.");
        }
        else
        {
            Debug.LogWarning("PlayerDialogueTrigger: DialogueManager o DialogueData no asignado.");
        }
    }
}
*/