using UnityEngine;
using UnityEngine.Rendering.Universal;

public class GasBall : MonoBehaviour
{
    public Animator animator;
    public Rigidbody2D rb;
    public int gasBounces = 0;
    public GameObject gasCollider;
    public Light2D gasLight;

    private void Start()
    {
        gasCollider.SetActive(false);
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(gasBounces == 3) 
        {
            rb.linearVelocity = Vector2.zero;
            if(gasLight != null)
            {
                gasLight.intensity = 0.5f;
                gasLight.pointLightOuterRadius = 15f;
                gasLight.falloffIntensity = 0.7f;
            }
            animator.SetTrigger("Gas");
            gasCollider.SetActive(true);
            return;
        }
        if (collision.CompareTag("Player"))
        {
            if (gasLight != null)
            {
                gasLight.intensity = 0.5f;
                gasLight.pointLightOuterRadius = 15f;
                gasLight.falloffIntensity = 0.7f;
            }
            animator.SetTrigger("Gas");
            gasCollider.SetActive(true);
            rb.linearVelocity = Vector2.zero;
            collision.gameObject.GetComponent<PlayerStateMachine>().InvertControlsForSeconds(10f);
            return;
        }
        //si el layer es ground o obstacle rebota
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground") || collision.gameObject.layer == LayerMask.NameToLayer("ConfinerWalls"))
        {
            gasBounces++;
        }
    }

    public void OnDestroy() //animator event
    {
        Destroy(gameObject);
    }

}
