using UnityEngine;
using UnityEngine.UI;

public class LowHealthOverlay : MonoBehaviour
{
    [Header("UI References")]
    public Image lowHealthImage;   // Arrastrar aquí LowHealthImage

    [Header("Settings")]
    public float lowHealthThreshold = 20f; // Vida mínima para activar efecto
    public float flashSpeed = 1.7f;          // Velocidad del parpadeo

    public CharacterHealth playerHealth;   // Arrastrar aquí el CharacterHealth del jugador

    private void Start()
    {
        if (playerHealth == null)
        {
            Debug.LogError("No se ha asignado CharacterHealth al LowHealthOverlay");
            return;
        }

        // Suscribirse al evento de cambio de vida
        playerHealth.OnHealthChanged += HandleHealthChanged;

        // Inicialmente invisible
        SetAlpha(0f);
    }
    public void Refresh()
{
    if (playerHealth == null) return;

    float currentHealth = playerHealth.currentHealth;

    if (currentHealth <= lowHealthThreshold)
    {
        if (!isInvoking)
        {
            isInvoking = true;
            InvokeRepeating(nameof(Flash), 0f, 0.01f);
        }
    }
    else
    {
        if (isInvoking)
        {
            isInvoking = false;
            CancelInvoke(nameof(Flash));
            SetAlpha(0f);
        }
    }
}

    private void HandleHealthChanged(float currentHealth)
    {
        if (currentHealth <= lowHealthThreshold)
        {
            // Inicia el parpadeo
            if (!isInvoking)
            {
                isInvoking = true;
                InvokeRepeating(nameof(Flash), 0f, 0.01f);
            }
        }
        else
        {
            // Detener parpadeo
            if (isInvoking)
            {
                isInvoking = false;
                CancelInvoke(nameof(Flash));
                SetAlpha(0f);
            }
        }
    }

    private bool isInvoking = false;

    private void Flash()
{
    if (lowHealthImage == null) return;

    float minAlpha = 0.2f; // mínimo visible
    float maxAlpha = 0.7f; // máximo rojo intenso

    // Oscila entre minAlpha y maxAlpha
    float alpha = minAlpha + (Mathf.Abs(Mathf.Sin(Time.time * flashSpeed)) * (maxAlpha - minAlpha));
    
    SetAlpha(alpha);
}


    private void SetAlpha(float alpha)
    {
        if (lowHealthImage != null)
        {
            Color c = lowHealthImage.color;
            c.a = alpha;
            lowHealthImage.color = c;
        }
    }

    private void OnDestroy()
    {
        // Evitar errores al destruir el objeto
        if (playerHealth != null)
            playerHealth.OnHealthChanged -= HandleHealthChanged;
    }
}
