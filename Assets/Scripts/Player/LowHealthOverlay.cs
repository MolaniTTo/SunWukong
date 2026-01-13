using UnityEngine;
using UnityEngine.UI;

public class LowHealthOverlay : MonoBehaviour
{
    public PlayerHealth playerHealth;   // Referencia al script de salud
    public Image overlayImage;          // Referencia a la imagen del borde rojo
    public float lowHealthThreshold = 20f;  // Umbral de vida
    public float blinkSpeed = 2f;           // Velocidad del parpadeo

    private bool isLowHealth => playerHealth.health <= lowHealthThreshold;

    private void Update()
    {
        if (isLowHealth)
        {
            // Parpadeo usando sin
            float alpha = Mathf.Abs(Mathf.Sin(Time.time * blinkSpeed));
            Color c = overlayImage.color;
            c.a = alpha; // cambia solo la transparencia
            overlayImage.color = c;
        }
        else
        {
            // Transparente si no está en low health
            Color c = overlayImage.color;
            c.a = 0f;
            overlayImage.color = c;
        }
    }
}
