using UnityEngine;

public class LightningZone : MonoBehaviour
{
    [Header("Configuración de Daño")]
    [Tooltip("Daño que hace al jugador (1 = un intento perdido)")]
    public float danio = 1f; // Cambiado a float para coincidir con CharacterHealth

    [Tooltip("Tiempo entre daños si el jugador permanece en la zona")]
    public float intervaloDanio = 1.5f;

    [Header("Opcional")]
    [Tooltip("Reproducir sonido al hacer daño")]
    public AudioClip sonidoRayo;

    private float tiempoUltimoDanio = -999f;

    private void OnTriggerStay2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            // Verificar si ha pasado suficiente tiempo desde el último daño
            if (Time.time - tiempoUltimoDanio >= intervaloDanio)
            {
                CharacterHealth health = col.GetComponent<CharacterHealth>();

                if (health != null)
                {
                    health.TakeDamage(danio, gameObject);
                    tiempoUltimoDanio = Time.time;
                }

                // Reproducir sonido si existe
                if (sonidoRayo != null)
                {
                    AudioSource.PlayClipAtPoint(sonidoRayo, transform.position);
                }

                Debug.Log($"¡Rayo impactó! Vida restante: {health.currentHealth}");
            }
        }
    }
}
