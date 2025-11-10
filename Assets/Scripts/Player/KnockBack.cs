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
            Vector2 direction = transform.position - sender.transform.position; 
            isKnockedBack = true;
            rb.AddForce(direction.normalized * force, ForceMode2D.Impulse);
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
