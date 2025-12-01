using UnityEngine;

public class AutoScrollContainer : MonoBehaviour
{
    [Header("Configuración de Scroll")]
    [Tooltip("Velocidad de desplazamiento hacia arriba")]
    public float velocidadScroll = 2f;
    
    [Tooltip("Límite superior donde se detiene el scroll (posición Y mundial)")]
    public float limiteScroll = 100f;
    
    [Header("Referencias")]
    [Tooltip("Dejar vacío para usar esta misma posición Y como inicio")]
    public Transform puntoInicio;
    
    private bool scrollActivo = true;
    private float posicionInicioY;
    
    void Start()
    {
        // Guardar la posición inicial
        posicionInicioY = puntoInicio != null ? puntoInicio.position.y : transform.position.y;
    }
    
   void Update()
{
    if (scrollActivo)
    {
        // En lugar de transform.position +=
        transform.Translate(Vector3.up * velocidadScroll * Time.deltaTime, Space.World);
        
        if (transform.position.y >= limiteScroll)
        {
            Vector3 posicion = transform.position;
            posicion.y = limiteScroll;
            transform.position = posicion;
            scrollActivo = false;
            OnLimiteAlcanzado();
        }
    }
}
    
    // Método que se llama cuando se alcanza el límite
    void OnLimiteAlcanzado()
    {
        Debug.Log("¡Límite de scroll alcanzado!");
        // Aquí puedes agregar lógica adicional, como mostrar un mensaje
        // o activar el siguiente nivel
    }
    
    // Método público para reactivar el scroll si es necesario
    public void ReactivarScroll()
    {
        scrollActivo = true;
    }
    
    // Método para detener manualmente el scroll
    public void DetenerScroll()
    {
        scrollActivo = false;
    }
    
    // Verificar si el scroll está activo
    public bool EstaScrollActivo()
    {
        return scrollActivo;
    }
    
    // Obtener el progreso del scroll (0 a 1)
    public float ObtenerProgreso()
    {
        float distanciaTotal = limiteScroll - posicionInicioY;
        float distanciaRecorrida = transform.position.y - posicionInicioY;
        return Mathf.Clamp01(distanciaRecorrida / distanciaTotal);
    }
}