using UnityEngine;
using UnityEngine.UI;

public class BarraDeVida : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Slider sliderVida; // El componente Slider
    [SerializeField] private CharacterHealth characterHealth; // Referencia al CharacterHealth del player
    
    [Header("Configuración Opcional")]
    [SerializeField] private bool animarCambios = true;
    [SerializeField] private float velocidadAnimacion = 5f;
    
    private float vidaObjetivo;

    private void Start()
    {
        // Si no asignaste el slider en el inspector, buscarlo en este GameObject
        if (sliderVida == null)
        {
            sliderVida = GetComponent<Slider>();
        }

        // Si no asignaste el characterHealth en el inspector, buscarlo automáticamente
        if (characterHealth == null)
        {
            // Buscar el GameObject con el tag "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                characterHealth = player.GetComponent<CharacterHealth>();
            }
        }

        // Configurar el slider
        if (sliderVida != null && characterHealth != null)
        {
            sliderVida.maxValue = characterHealth.maxHealth;
            sliderVida.minValue = 0;
            sliderVida.value = characterHealth.currentHealth;
            vidaObjetivo = characterHealth.currentHealth;
        }

        // Suscribirse al evento de cambio de vida
        if (characterHealth != null)
        {
            characterHealth.OnHealthChanged += ActualizarBarraVida;
        }
        else
        {
            Debug.LogError("BarraDeVida: No se encontró el CharacterHealth del player!");
        }

        // Verificar que tenemos la referencia al slider
        if (sliderVida == null)
        {
            Debug.LogError("BarraDeVida: No se encontró el componente Slider!");
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento cuando se destruya el objeto
        if (characterHealth != null)
        {
            characterHealth.OnHealthChanged -= ActualizarBarraVida;
        }
    }

    private void ActualizarBarraVida(float vidaActual)
    {
        if (sliderVida == null) return;

        vidaObjetivo = vidaActual;
        
        Debug.Log($"Barra de vida actualizada: {vidaActual}/{characterHealth.maxHealth}");
    }
    
    // Método público para actualizar el máximo de vida cuando cambia (por ejemplo, al recoger plátanos)
    public void ActualizarMaxVida(float nuevaMaxVida)
    {
        if (sliderVida != null)
        {
            sliderVida.maxValue = nuevaMaxVida;
            Debug.Log($"Max Vida de la barra actualizado a: {nuevaMaxVida}");
        }
    }

    private void Update()
    {
        if (sliderVida == null) return;

        // Verificar si el máximo ha cambiado y actualizarlo
        if (characterHealth != null && sliderVida.maxValue != characterHealth.maxHealth)
        {
            sliderVida.maxValue = characterHealth.maxHealth;
        }

        // Animar el cambio de la barra suavemente
        if (animarCambios)
        {
            sliderVida.value = Mathf.Lerp(sliderVida.value, vidaObjetivo, Time.deltaTime * velocidadAnimacion);
        }
        else
        {
            sliderVida.value = vidaObjetivo;
        }
    }
}