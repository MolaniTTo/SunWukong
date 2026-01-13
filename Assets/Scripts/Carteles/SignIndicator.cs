using UnityEngine;

public class SignIndicator : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject indicator;
    
    [Header("Configuración de Detección")]
    [SerializeField] private float detectionRadius = 2f; // Ajusta este valor
    
    [Header("Configuración de Animación")]
    [SerializeField] private float bounceHeight = 0.3f;
    [SerializeField] private float bounceSpeed = 2f;
    
    private Vector3 initialPosition;
    private bool playerNearby = false;
    private CircleCollider2D triggerCollider;
    
    void Start()
    {
        if (indicator != null)
        {
            initialPosition = indicator.transform.localPosition;
            indicator.SetActive(false);
        }
        else
        {
            Debug.LogError("¡No hay indicador asignado en " + gameObject.name + "!");
        }
        
        // Configurar o crear el collider automáticamente
        SetupCollider();
    }
    
    void SetupCollider()
    {
        // Intentar obtener el collider existente
        triggerCollider = GetComponent<CircleCollider2D>();
        
        // Si no existe, crear uno nuevo
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<CircleCollider2D>();
        }
        
        // Configurar el collider
        triggerCollider.isTrigger = true;
        triggerCollider.radius = detectionRadius;
    }
    
    void Update()
    {
        if (playerNearby && indicator != null)
        {
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
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
            if (indicator != null)
            {
                indicator.SetActive(false);
            }
        }
    }
    
    // Para visualizar el área de detección en el editor
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}