using UnityEngine;
using System.Collections;

public class KnockBack : MonoBehaviour
{
    [SerializeField] private float knockBackForce = 5f;
    [SerializeField] private float knockBachDuration = 0.2f;

    private Rigidbody2D rb;
    private bool isKnockedBack = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void ApplyKnockBack(Vector2 direction)
    {
        if (!isKnockedBack)
        {
            isKnockedBack = true;
            rb.AddForce(direction.normalized * knockBackForce, ForceMode2D.Impulse);
            StartCoroutine(EndKnockBack());
        }

    }

    private IEnumerator EndKnockBack()
    {
        yield return new WaitForSeconds(knockBachDuration); //Esperem el temps de knockback
        rb.linearVelocity = Vector2.zero; //Detenem l'empenta
        isKnockedBack = false;
    }



}
