using UnityEngine;

public class CloudRider : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad de movimiento vertical")]
    public float verticalSpeed = 5f;
    
    [Tooltip("Velocidad de movimiento horizontal")]
    public float horizontalSpeed = 5f;
    
    [Header("Referencia al Contenedor")]
    [Tooltip("Referencia al contenedor (LevelContainer) - los límites se detectan automáticamente")]
    public Transform containerReference;
    
    [Tooltip("Margen de seguridad desde los bordes del contenedor")]
    public float edgeMargin = 0.5f;
    
    [Header("Suavizado")]
    [Tooltip("Suavizado del movimiento")]
    public float movementSmoothing = 10f;
    
    private float horizontalInput;
    private float verticalInput;
    private Vector2 currentVelocity;
    private Vector3 lastParentPosition;
    private BoxCollider2D containerCollider;
    private Bounds containerBounds;

    void Start()
    {
        // Si no se asignó manualmente, buscar el padre
        if (containerReference == null && transform.parent != null)
        {
            containerReference = transform.parent;
        }
        
        // Buscar el BoxCollider2D del contenedor
        if (containerReference != null)
        {
            containerCollider = containerReference.GetComponent<BoxCollider2D>();
            if (containerCollider != null)
            {
                UpdateContainerBounds();
            }
            else
            {
                Debug.LogWarning("CloudRider: No se encontró BoxCollider2D en el contenedor. Añade uno para definir los límites.");
            }
            
            lastParentPosition = containerReference.position;
        }
    }

    void Update()
    {
        // ANULAR el movimiento del padre
        if (containerReference != null)
        {
            Vector3 parentMovement = containerReference.position - lastParentPosition;
            transform.position -= parentMovement; // Contrarresta el arrastre
            lastParentPosition = containerReference.position;
            
            // Actualizar límites del contenedor
            if (containerCollider != null)
            {
                UpdateContainerBounds();
            }
        }
        
        // Capturar input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        // Girar sprite según dirección
        if (horizontalInput < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), 
                                               transform.localScale.y, 
                                               transform.localScale.z);
        }
        else if (horizontalInput > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), 
                                               transform.localScale.y, 
                                               transform.localScale.z);
        }

        // Movimiento
        MovePlayer();
    }

    void UpdateContainerBounds()
    {
        // Obtener los límites actuales del BoxCollider2D
        containerBounds = containerCollider.bounds;
    }

    void MovePlayer()
    {
        // Calcular velocidad objetivo
        Vector2 targetVelocity = new Vector2(
            horizontalInput * horizontalSpeed,
            verticalInput * verticalSpeed
        );

        // Suavizado
        currentVelocity = Vector2.Lerp(currentVelocity, targetVelocity, 
                                       movementSmoothing * Time.deltaTime);

        // Mover personaje en espacio GLOBAL
        transform.position += (Vector3)currentVelocity * Time.deltaTime;

        // Aplicar límites basados en el BoxCollider2D del contenedor
        if (containerCollider != null)
        {
            Vector3 worldPos = transform.position;
            
            // Aplicar límites con margen
            worldPos.x = Mathf.Clamp(worldPos.x, 
                                    containerBounds.min.x + edgeMargin, 
                                    containerBounds.max.x - edgeMargin);
            worldPos.y = Mathf.Clamp(worldPos.y, 
                                    containerBounds.min.y + edgeMargin, 
                                    containerBounds.max.y - edgeMargin);
            
            transform.position = worldPos;
        }
    }

    // Visualizar límites del contenedor en el editor
    void OnDrawGizmos()
    {
        if (containerReference == null) return;
        
        BoxCollider2D collider = containerReference.GetComponent<BoxCollider2D>();
        if (collider == null) return;
        
        Bounds bounds = collider.bounds;
        
        // Dibujar el área de movimiento
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(bounds.center, bounds.size);
        
        // Dibujar el área con margen
        Gizmos.color = Color.yellow;
        Vector3 marginSize = bounds.size - new Vector3(edgeMargin * 2, edgeMargin * 2, 0);
        Gizmos.DrawWireCube(bounds.center, marginSize);
    }
}