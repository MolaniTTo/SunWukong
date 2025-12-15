using UnityEngine;

public class SignIndicator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject indicator;
    
    [Header("Configuración de Animación")]
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceSpeed = 2f;
    
    private Vector3 initialPosition;
    private bool playerNearby = false;
    
    void Start()
    {
        if (indicator != null)
        {
            // Guardamos la posición inicial
            initialPosition = indicator.transform.localPosition;
            // Ocultamos el indicador al inicio
            indicator.SetActive(false);
        }
        else
        {
            Debug.LogError("¡No hay indicador asignado en " + gameObject.name + "!");
        }
    }
    
    void Update()
    {
        if (playerNearby && indicator != null)
        {
            // Animación de rebote (sube y baja)
            float newY = initialPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            indicator.transform.localPosition = new Vector3(
                initialPosition.x, 
                newY, 
                initialPosition.z
            );
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Cuando el jugador entra al área del cartel
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
            if (indicator != null)
            {
                indicator.SetActive(true);
            }
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        // Cuando el jugador sale del área del cartel
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }
    }
}