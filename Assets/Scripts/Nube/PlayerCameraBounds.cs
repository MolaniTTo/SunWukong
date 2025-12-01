using UnityEngine;

public class PlayerCameraBounds : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento del jugador")]
    public float velocidadMovimiento = 5f;
    
    [Header("Referencias")]
    [Tooltip("Cámara para calcular los límites")]

    
    // Rigidbody2D si lo usas
    private Rigidbody2D rb;
    
    void Start()
    {
        // Si no se asigna cámara, usar la principal
      
        // Intentar obtener Rigidbody2D si existe
        rb = GetComponent<Rigidbody2D>();
        
        // Si tiene Rigidbody, configurarlo
        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f; // Gravedad en 0
        }
    }
    
    void Update()
    {
        // Obtener input del jugador
        float movimientoH = Input.GetAxisRaw("Horizontal");
        float movimientoV = Input.GetAxisRaw("Vertical");
        
        // Calcular movimiento
        Vector3 movimiento = new Vector3(movimientoH, movimientoV, 0f);
        
        // Si el movimiento no está normalizado y hay input diagonal
        if (movimiento.magnitude > 1f)
        {
            movimiento.Normalize();
        }
        
        // Calcular nueva posición
        Vector3 velocidad = movimiento * velocidadMovimiento;
        
        if (rb != null)
        {
            // Si usa Rigidbody2D, usar velocidad
            rb.linearVelocity = new Vector2(velocidad.x, velocidad.y);
        }
        else
        {
            // Sin Rigidbody, mover directamente
            transform.position += velocidad * Time.deltaTime;
        }
     
    }
      
}