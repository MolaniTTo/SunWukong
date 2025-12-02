using System.Collections;
using UnityEngine;

public class Ray : MonoBehaviour
{
    [Header("Bones")]
    public Transform topRay; //os de adal
    public Transform tipRay; //os de abaix

    [Header("Ray Growing Settings")]
    public float growSpeed = 8f; //velocitat de creixement cap a baix
    public float retractSpeed = 10f; //velocitat de retracció cap a dalt
    public float maxLength = 8f; //limit de longitud máxima

    [Header("Ground Check")]
    public float tipRadius = 0.1f;
    public LayerMask groundLayer;

    [Header("References")]
    public Animator circleAnimator; //animació del cercle a terra

    [Header("Collider")]
    public BoxCollider2D rayCollider;


    public bool hitGround = false;
    private Vector3 groundPoint;

    private void Start()
    {
        StartCoroutine(RayRoutine());
    }

    private IEnumerator RayRoutine()
    {
        //creix fins a tocar terra
        while (!hitGround)
        {
            //mou el os tip cap avall (estira la mesh)
            tipRay.position += Vector3.down * growSpeed * Time.deltaTime;
            UpdateCollider(); //actualitza el collider
            //per si de cas s'ha passat del maxim
            if (Vector3.Distance(topRay.position, tipRay.position) > maxLength) { break; }

            //detecta el el terra
            Collider2D hit = Physics2D.OverlapCircle(tipRay.position, tipRadius, groundLayer);
            if (hit != null)
            {
                hitGround = true;
                groundPoint = tipRay.position;

                //crida a la animació del cercle
                if (circleAnimator != null) { circleAnimator.SetTrigger("Hit"); }
            }

            yield return null;
        }

        
        yield return new WaitForSeconds(2f);

        while (Vector3.Distance(topRay.position, tipRay.position) > 0.05f)
        {
            //mou el os top cap avall
            topRay.position = Vector3.MoveTowards(topRay.position, tipRay.position, retractSpeed * Time.deltaTime);

            yield return null;
        }

        //Destruir el raig
        Destroy(gameObject);
    }

    private void UpdateCollider()
    {
        if (rayCollider == null) return;

        float length = Vector2.Distance(topRay.position, tipRay.position); //distancia entre top i tip

        rayCollider.size = new Vector2(rayCollider.size.x, length); //ajusta el tamany del collider

        Vector2 midPoint = (topRay.position + tipRay.position) * 0.5f;
        Vector2 localMid = rayCollider.transform.InverseTransformPoint(midPoint);

        rayCollider.offset = new Vector2(0, localMid.y);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(tipRay.position, tipRadius);
    }
}
