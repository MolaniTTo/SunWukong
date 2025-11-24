using UnityEngine;

public class SkyClimbCamera : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player; // El transform de Sun Wukong
    
    [Header("Configuración de Seguimiento")]
    [Tooltip("Velocidad con la que la cámara sigue al jugador")]
    public float smoothSpeed = 5f;
    
    [Tooltip("Offset vertical respecto al jugador (negativo = cámara más abajo)")]
    public float verticalOffset = -2f;
    
    [Tooltip("Si true, la cámara solo sigue en Y (arriba), X e Z fijos")]
    public bool followOnlyY = true;
    
    [Header("Límites")]
    [Tooltip("Altura mínima de la cámara")]
    public float minHeight = 0f;
    
    [Tooltip("Altura máxima de la cámara (0 = sin límite)")]
    public float maxHeight = 0f;
    
    private Vector3 velocity = Vector3.zero;

    void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPosition;

        if (followOnlyY)
        {
            // Solo seguimos la altura del jugador, X y Z se mantienen
            targetPosition = new Vector3(
                transform.position.x,
                player.position.y + verticalOffset,
                transform.position.z
            );
        }
        else
        {
            // Seguimos completamente al jugador con offset
            targetPosition = player.position + new Vector3(0, verticalOffset, 0);
            targetPosition.z = transform.position.z; // Mantenemos la Z para cámara 2D
        }

        // Aplicar límites de altura
        if (maxHeight > minHeight && maxHeight != 0)
        {
            targetPosition.y = Mathf.Clamp(targetPosition.y, minHeight, maxHeight);
        }
        else if (minHeight != 0)
        {
            targetPosition.y = Mathf.Max(targetPosition.y, minHeight);
        }

        // Movimiento suave de la cámara
        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref velocity,
            1f / smoothSpeed
        );
    }
}