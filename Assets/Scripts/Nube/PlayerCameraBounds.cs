using UnityEngine;

public class PlayerCameraBounds : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    [Tooltip("Velocidad de movimiento del jugador")]
    public float velocidadMovimiento = 5f;
    
    [Header("Referencias")]
    [Tooltip("Cámara para calcular los límites")]
    public Camera camaraJuego;
    
    [Header("Límites de Pantalla")]
    [Tooltip("Margen desde los bordes izquierdo y derecho (en unidades)")]
    public float margenHorizontal = 0.5f;
    [Tooltip("Margen inferior (en unidades)")]
    public float margenInferior = 0.5f;
    [Tooltip("Margen superior (en unidades)")]
    public float margenSuperior = 0.5f;
    
    // Rigidbody2D si lo usas
    private Rigidbody2D rb;
    
    void Start()
    {
        // Si no se asigna cámara, usar la principal
        if (camaraJuego == null)
        {
            camaraJuego = Camera.main;
        }
        
        // Intentar obtener Rigidbody2D si existe
        rb = GetComponent<Rigidbody2D>();
        
        // Si tiene Rigidbody, configurarlo para que no rote
        if (rb != null)
        {
            rb.freezeRotation = true;
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
        
        // Aplicar límites
        AplicarLimites();
    }
    
    void AplicarLimites()
    {
        if (camaraJuego == null) return;
        
        Vector3 posicionActual = transform.position;
        
        // Calcular límites de la cámara en el mundo
        float alturaCamera, anchoCamera;
        
        if (camaraJuego.orthographic)
        {
            // Cámara ortográfica
            alturaCamera = camaraJuego.orthographicSize * 2f;
            anchoCamera = alturaCamera * camaraJuego.aspect;
        }
        else
        {
            // Cámara en perspectiva
            float distancia = Mathf.Abs(posicionActual.z - camaraJuego.transform.position.z);
            alturaCamera = 2f * distancia * Mathf.Tan(camaraJuego.fieldOfView * 0.5f * Mathf.Deg2Rad);
            anchoCamera = alturaCamera * camaraJuego.aspect;
        }
        
        // Posición de la cámara
        Vector3 posCamera = camaraJuego.transform.position;
        
        // Calcular límites
        float limiteIzquierdo = posCamera.x - (anchoCamera / 2f) + margenHorizontal;
        float limiteDerecho = posCamera.x + (anchoCamera / 2f) - margenHorizontal;
        float limiteInferior = posCamera.y - (alturaCamera / 2f) + margenInferior;
        float limiteSuperior = posCamera.y + (alturaCamera / 2f) - margenSuperior;
        
        // Aplicar límites en X (izquierda y derecha)
        posicionActual.x = Mathf.Clamp(posicionActual.x, limiteIzquierdo, limiteDerecho);
        
        // Aplicar límites en Y (arriba y abajo)
        posicionActual.y = Mathf.Clamp(posicionActual.y, limiteInferior, limiteSuperior);
        
        // Aplicar posición corregida
        transform.position = posicionActual;
        
        // Si tiene Rigidbody y se limitó, ajustar velocidad
        if (rb != null)
        {
            Vector2 vel = rb.linearVelocity;
            
            // Si tocó el límite izquierdo o derecho, cancelar velocidad horizontal
            if (transform.position.x <= limiteIzquierdo || transform.position.x >= limiteDerecho)
            {
                vel.x = 0;
            }
            
            // Si tocó el límite inferior, cancelar velocidad hacia abajo
            if (transform.position.y <= limiteInferior && vel.y < 0)
            {
                vel.y = 0;
            }
            
            rb.linearVelocity = vel;
        }
    }
    
    void OnDrawGizmos()
    {
        if (camaraJuego == null || !Application.isPlaying) return;
        
        Gizmos.color = Color.yellow;
        
        // Calcular el área visible
        float alturaCamera, anchoCamera;
        Vector3 posCamera = camaraJuego.transform.position;
        
        if (camaraJuego.orthographic)
        {
            alturaCamera = camaraJuego.orthographicSize * 2f;
            anchoCamera = alturaCamera * camaraJuego.aspect;
        }
        else
        {
            float distancia = Mathf.Abs(transform.position.z - camaraJuego.transform.position.z);
            alturaCamera = 2f * distancia * Mathf.Tan(camaraJuego.fieldOfView * 0.5f * Mathf.Deg2Rad);
            anchoCamera = alturaCamera * camaraJuego.aspect;
        }
        
        // Dibujar el área de juego
        Vector3 centro = new Vector3(posCamera.x, posCamera.y, transform.position.z);
        Vector3 tamano = new Vector3(anchoCamera - (margenHorizontal * 2), alturaCamera, 0.1f);
        
        Gizmos.DrawWireCube(centro, tamano);
        
        // Dibujar línea del límite inferior
        Gizmos.color = Color.red;
        float limiteInf = posCamera.y - (alturaCamera / 2f) + margenInferior;
        Gizmos.DrawLine(
            new Vector3(posCamera.x - anchoCamera/2, limiteInf, transform.position.z),
            new Vector3(posCamera.x + anchoCamera/2, limiteInf, transform.position.z)
        );
    }
}