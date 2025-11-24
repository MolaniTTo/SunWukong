using UnityEngine;

public class CloudRider : MonoBehaviour
{
    [Header("Movimiento Vertical")]
    [Tooltip("Velocidad de ascenso automático")]
    public float ascendSpeed = 3f;
    
    [Header("Movimiento Horizontal")]
    [Tooltip("Velocidad de movimiento izquierda/derecha")]
    public float horizontalSpeed = 5f;
    
    [Tooltip("Límite máximo a la izquierda")]
    public float leftBoundary = -4f;
    
    [Tooltip("Límite máximo a la derecha")]
    public float rightBoundary = 4f;
    
    [Header("Suavizado")]
    [Tooltip("Suavizado del movimiento horizontal")]
    public float horizontalSmoothing = 10f;
    
    private Rigidbody2D rb;
    private float horizontalInput;
    private float currentVelocityX;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // Configurar el Rigidbody2D para este tipo de juego
        if (rb != null)
        {
            rb.gravityScale = 0; // Sin gravedad, va volando
            rb.freezeRotation = true;
        }
    }

    void Update()
    {
        // Capturar input horizontal
        horizontalInput = Input.GetAxisRaw("Horizontal");
        
        // Girar sprite según dirección (opcional)
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
    }

    void FixedUpdate()
    {
        if (rb != null)
        {
            // Movimiento vertical automático constante
            float verticalVelocity = ascendSpeed;

            // Movimiento horizontal suavizado
            float targetVelocityX = horizontalInput * horizontalSpeed;
            currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, 
                                          horizontalSmoothing * Time.fixedDeltaTime);

            // Aplicar velocidad
            rb.linearVelocity = new Vector2(currentVelocityX, verticalVelocity);

            // Aplicar límites horizontales
            Vector3 clampedPosition = transform.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, leftBoundary, rightBoundary);
            transform.position = clampedPosition;
        }
    }

    // Método alternativo sin Rigidbody2D (usando Transform directamente)
    void AlternativeMovement()
    {
        // Movimiento vertical
        transform.position += Vector3.up * ascendSpeed * Time.deltaTime;

        // Movimiento horizontal
        float targetVelocityX = horizontalInput * horizontalSpeed;
        currentVelocityX = Mathf.Lerp(currentVelocityX, targetVelocityX, 
                                      horizontalSmoothing * Time.deltaTime);
        
        transform.position += Vector3.right * currentVelocityX * Time.deltaTime;

        // Aplicar límites
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, leftBoundary, rightBoundary);
        transform.position = pos;
    }

    // Visualizar límites en el editor
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(new Vector3(leftBoundary, -10, 0), 
                       new Vector3(leftBoundary, 100, 0));
        Gizmos.DrawLine(new Vector3(rightBoundary, -10, 0), 
                       new Vector3(rightBoundary, 100, 0));
    }
}