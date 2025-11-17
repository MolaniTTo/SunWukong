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
            
            // Si aún no lo encontramos, buscar cualquier CharacterHealth que sea player
            if (characterHealth == null)
            {
                CharacterHealth[] allHealthComponents = FindObjectsOfType<CharacterHealth>();
                foreach (var health in allHealthComponents)
                {
                    if (health.isPlayer)
                    {
                        characterHealth = health;
                        break;
                    }
                }
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

    private void Update()
    {
        if (sliderVida == null) return;

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