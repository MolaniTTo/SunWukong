using UnityEngine;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class CheckpointTrigger : MonoBehaviour
{

    private bool isActivated = false;
    public Light2D checkpointLight;
    [SerializeField] private float activatedIntensity = 0.5f;
    [SerializeField] private float fadeInDuration = 0.5f;

    private void Start()
    {
        if (ProgressManager.Instance != null)
        {
            string checkpointID = gameObject.name;

            if (ProgressManager.Instance.GetCurrentProgress().unlockedCheckpoints.Contains(checkpointID))
            {
                isActivated = true;
                if (checkpointLight != null)
                {
                    checkpointLight.intensity = activatedIntensity;
                }
                Debug.Log($"Checkpoint {gameObject.name} restaurado con luz encendida");

            }

            else
            {
                if(checkpointLight != null)
                {
                    checkpointLight.intensity = 0f;
                }
            }

        }
    }

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

        if (checkpointLight != null)
        {
            StartCoroutine(FadeInLight());
        }

        // Actualizar el lastCheckPoint del jugador
        player.lastCheckPoint = transform;

        // Registrar en el ProgressManager
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.RegisterCheckpoint(transform);
        }

        Debug.Log($"Checkpoint {gameObject.name} activado!");
    }

    private IEnumerator FadeInLight()
    {
        float elapsed = 0f;
        float startIntensity = checkpointLight.intensity;

        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            checkpointLight.intensity = Mathf.Lerp(startIntensity, activatedIntensity, elapsed / fadeInDuration);
            yield return null;
        }

        checkpointLight.intensity = activatedIntensity;
    }
}