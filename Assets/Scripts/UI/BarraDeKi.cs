using UnityEngine;
using UnityEngine.UI;

public class BarraDeKi : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private Slider sliderKi; // El componente Slider
    [SerializeField] private PlayerStateMachine playerStateMachine; // Referencia al PlayerStateMachine
    
    [Header("Configuración Opcional")]
    [SerializeField] private bool animarCambios = true;
    [SerializeField] private float velocidadAnimacion = 5f;
    
    private float kiObjetivo;

    private void Start()
    {
        // Si no asignaste el slider en el inspector, buscarlo en este GameObject
        if (sliderKi == null)
        {
            sliderKi = GetComponent<Slider>();
        }

        // Si no asignaste el playerStateMachine en el inspector, buscarlo automáticamente
        if (playerStateMachine == null)
        {
            // Buscar el GameObject con el tag "Player"
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerStateMachine = player.GetComponent<PlayerStateMachine>();
            }
        }

        // Configurar el slider
        if (sliderKi != null && playerStateMachine != null)
        {
            sliderKi.maxValue = playerStateMachine.maxKi;
            sliderKi.minValue = 0;
            sliderKi.value = playerStateMachine.currentKi;
            kiObjetivo = playerStateMachine.currentKi;
        }

        // Suscribirse al evento de cambio de Ki
        if (playerStateMachine != null)
        {
            playerStateMachine.OnKiChanged += ActualizarBarraKi;
        }
        else
        {
            Debug.LogError("BarraDeKi: No se encontró el PlayerStateMachine!");
        }

        // Verificar que tenemos la referencia al slider
        if (sliderKi == null)
        {
            Debug.LogError("BarraDeKi: No se encontró el componente Slider!");
        }
    }

    private void OnDestroy()
    {
        // Desuscribirse del evento cuando se destruya el objeto
        if (playerStateMachine != null)
        {
            playerStateMachine.OnKiChanged -= ActualizarBarraKi;
        }
    }

    private void ActualizarBarraKi(float kiActual)
    {
        if (sliderKi == null) return;

        kiObjetivo = kiActual;
        
        Debug.Log($"Barra de Ki actualizada: {kiActual}/{playerStateMachine.maxKi}");
    }

    private void Update()
    {
        if (sliderKi == null) return;

        // Animar el cambio de la barra suavemente
        if (animarCambios)
        {
            sliderKi.value = Mathf.Lerp(sliderKi.value, kiObjetivo, Time.deltaTime * velocidadAnimacion);
        }
        else
        {
            sliderKi.value = kiObjetivo;
        }
    }
}