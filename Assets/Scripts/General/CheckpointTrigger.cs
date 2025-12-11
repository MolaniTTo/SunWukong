using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{

    private bool isActivated = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !isActivated)
        {
            ActivateCheckpoint(other.GetComponent<PlayerStateMachine>());
        }
    }
    private void ActivateCheckpoint(PlayerStateMachine player)
    {
        if (player == null) return;

        isActivated = true;

        // Actualizar el lastCheckPoint del jugador
        player.lastCheckPoint = transform;

        // Registrar en el ProgressManager
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.RegisterCheckpoint(transform);
        }

        Debug.Log($"🚩 Checkpoint {gameObject.name} activado!");

        // Opcional: efecto de sonido/partículas
        // PlayActivationEffect();
    }
}