using UnityEngine;

public class PlayerCameraBounds : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float velocidadMovimiento = 5f;

    private Rigidbody2D rb;
    private CharacterHealth health; // referencia a la vida

    void Start()
    {
        // Obtener Rigidbody
        rb = GetComponent<Rigidbody2D>();

        // Obtener CharacterHealth
        health = GetComponent<CharacterHealth>();

        if (rb != null)
        {
            rb.freezeRotation = true;
            rb.gravityScale = 0f;
        }
    }

    void Update()
    {
        // Si no tiene vida, no se mueve
        if (health != null && health.currentHealth <= 0)
        {
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
            }
            return;
        }

        // Input
        float movimientoH = Input.GetAxisRaw("Horizontal");
        float movimientoV = Input.GetAxisRaw("Vertical");

        Vector3 movimiento = new Vector3(movimientoH, movimientoV, 0f);

        if (movimiento.magnitude > 1f)
        {
            movimiento.Normalize();
        }

        Vector3 velocidad = movimiento * velocidadMovimiento;

        if (rb != null)
        {
            rb.linearVelocity = new Vector2(velocidad.x, velocidad.y);
        }
        else
        {
            transform.position += velocidad * Time.deltaTime;
        }
    }
}
