using UnityEngine;

public class GasBall : MonoBehaviour
{
    public Animator animator;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") || collision.CompareTag("Ground"))
        {
            animator.SetTrigger("Gas");
        }
    }

    public void OnDestroy()
    {
        Destroy(gameObject);
    }

}
