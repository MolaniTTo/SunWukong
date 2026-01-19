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
    
    [Header("Colores")]
    [SerializeField] private Image fillImage; // Referencia a la imagen de relleno del slider
    private Color colorOriginal;
    
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

        // Obtener la imagen de relleno del slider si no está asignada
        if (fillImage == null && sliderKi != null)
        {
            fillImage = sliderKi.fillRect?.GetComponent<Image>();
        }
        
        // Guardar el color original
        if (fillImage != null)
        {
            colorOriginal = fillImage.color;
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
    
    // Método público para actualizar el máximo de Ki cuando cambia (por ejemplo, al recoger plátanos)
    public void ActualizarMaxKi(float nuevoMaxKi)
    {
        if (sliderKi != null)
        {
            sliderKi.maxValue = nuevoMaxKi;
            Debug.Log($"Max Ki de la barra actualizado a: {nuevoMaxKi}");
        }
    }
    
    // Método para cambiar el color de la barra (usado por el plátano azul)
    public void SetBarColor(Color nuevoColor)
    {
        if (fillImage != null)
        {
            fillImage.color = nuevoColor;
            Debug.Log($"Color de la barra de Ki cambiado a: {nuevoColor}");
        }
        else
        {
            Debug.LogWarning("BarraDeKi: No se encontró la imagen de relleno para cambiar el color");
        }
    }
    
    // Método para restaurar el color original de la barra
    public void RestoreOriginalColor()
    {
        if (fillImage != null)
        {
            fillImage.color = colorOriginal;
            Debug.Log("Color de la barra de Ki restaurado al original");
        }
    }

    private void Update()
    {
        if (sliderKi == null) return;

        // Verificar si el máximo ha cambiado y actualizarlo
        if (playerStateMachine != null && sliderKi.maxValue != playerStateMachine.maxKi)
        {
            sliderKi.maxValue = playerStateMachine.maxKi;
        }

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