using UnityEngine;
public class EarthquakeWave : MonoBehaviour
{
    public enum Owner
    {
        Enemy,
        Player
    }

    [Header("Wave Settings")]
    public Owner owner = Owner.Enemy;
    public float speed = 8f;
    public float lifetime = 3f;
    private float direction;
    private Transform ownerTransform;


    [Header("Collision Detection")]
    [SerializeField] private LayerMask GroundLayer;
    [SerializeField] private float groundCheckDistance = 0.5f; //Distancia del raycast per detectar el terra
    [SerializeField] private float wallCheckDistance = 0.5f; //Distancia del raycast per detectar parets




    private void Start()
    {
        switch(owner)
        {
            case Owner.Enemy:
                Gorila gorila = FindAnyObjectByType<Gorila>();
                if (gorila == null) { Debug.LogError("EarthquakeWave: No s'ha trobat el component Gorila al pare!");  return; }
                ownerTransform = gorila.transform; //Busquem el Gorila a l'escena
                direction = -Mathf.Sign(ownerTransform.localScale.x);
                break;

            case Owner.Player:
                PlayerStateMachine player = FindAnyObjectByType<PlayerStateMachine>();
                if(player == null) { Debug.LogError("EarthquakeWave: No s'ha trobat el component PlayerStateMachine al pare!"); return; }
                ownerTransform = player.transform; //Busquem el Player a l'escena
                direction = Mathf.Sign(ownerTransform.localScale.x);
                break;
        }
        
        Destroy(gameObject, lifetime); //Destrueix la ona després de 'lifetime' segonss

    }

    private void Update()
    {
        if (CheckForObstacles())
        {
            DestroyWave();
            return;
        }

        Vector2 movement = Vector2.right * direction * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    private bool CheckForObstacles()
    {
        Vector2 pos = transform.position;

        //RAYCAST FRONTAL (DETECTA PARETS)
        Vector2 frontRayOrigin = pos + new Vector2(0f, 0.2f); //Lleugerament elevat per evitar col·lisions amb el terra
        RaycastHit2D wallHit = Physics2D.Raycast(frontRayOrigin, Vector2.right * direction, wallCheckDistance, GroundLayer);

        //RAYCAST INFERIOR (DETECTA EL TERRA)
        Vector2 groundRayOrigin = pos + new Vector2(0f, -0.1f); //Lleugerament abaixat per evitar col·lisions amb parets
        RaycastHit2D groundHit = Physics2D.Raycast(groundRayOrigin, Vector2.down, groundCheckDistance, GroundLayer);

        Debug.DrawRay(frontRayOrigin, Vector2.right * direction * wallCheckDistance, Color.red);
        Debug.DrawRay(groundRayOrigin, Vector2.down * groundCheckDistance, Color.blue);

        return wallHit.collider != null || groundHit.collider == null;

    }

    private void DestroyWave()
    {
        Destroy(gameObject);
    }

    /*void OnTriggerEnter2D(Collider2D other)
    {
        if (owner == Owner.Enemy && other.CompareTag("Player"))
        {
            DamageTarget(other);
        }
        if (owner == Owner.Player && other.CompareTag("Enemy"))
        {
            DamageTarget(other);
        }
    }

    private void DamageTarget(Collider2D targetCollider)
    {
        CharacterHealth targetHealth = targetCollider.GetComponent<CharacterHealth>();
        KnockBack knockBack = targetCollider.GetComponent<KnockBack>();
        if (targetHealth != null)
        {
            targetHealth.TakeDamage(damage, ownerTransform.gameObject);
        }
        if (knockBack != null)
        {
            knockBack.ApplyKnockBack(ownerTransform.gameObject, 0.3f, 15f);
        }
        DestroyWave();

    }*/

}
