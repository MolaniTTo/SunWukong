using UnityEngine;

public class SkyClimbCamera : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player;

    [Header("Ascenso Automático")]
    public float autoClimbSpeed = 2f;

    [Header("Seguimiento Horizontal")]
    public float horizontalSmoothSpeed = 4f;
    public float horizontalOffset = 0f;

    [Header("Offset Vertical")]
    public float verticalOffset = 3f;

    private float currentHeight;

    void Start()
    {
        currentHeight = transform.position.y;
    }

    void LateUpdate()
    {
        if (player == null) return;

        // La cámara sube sola
        currentHeight += autoClimbSpeed * Time.deltaTime;

        // Seguir horizontal con suavizado
        float targetX = player.position.x + horizontalOffset;
        float smoothX = Mathf.Lerp(transform.position.x, targetX, horizontalSmoothSpeed * Time.deltaTime);

        // Nueva posición
        transform.position = new Vector3(
            smoothX,
            currentHeight + verticalOffset,
            transform.position.z
        );
    }
}
