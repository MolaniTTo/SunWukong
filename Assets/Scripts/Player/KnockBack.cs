using UnityEngine;
using System.Collections;

public class KnockBack : MonoBehaviour
{
    
    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockBack(GameObject sender, float duration, float force)
    {
        if (!isKnockedBack)
        {
            // Diferencia de posiciones
            Vector2 rawDir = (transform.position - sender.transform.position);

            // Solo queremos knockback horizontal ? eliminamos el eje Y
            Vector2 horizontalDir = new Vector2(rawDir.x, 0f).normalized;

            // Por si sender está EXACTAMENTE en la misma X (evitar NaN)
            if (horizontalDir == Vector2.zero)
                horizontalDir = new Vector2(transform.localScale.x, 0f);   // fallback

            isKnockedBack = true;

            rb.AddForce(horizontalDir * force, ForceMode2D.Impulse);

            StartCoroutine(EndKnockBack(duration));
        }
    }


    private IEnumerator EndKnockBack(float duration)
    {
        yield return new WaitForSeconds(duration); //Esperem el temps de knockback
        rb.linearVelocity = Vector2.zero; //Detenem l'empenta
        isKnockedBack = false;
    }



}
