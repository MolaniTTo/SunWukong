using UnityEngine;
using static EarthquakeWave;

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
    public float damage = 20f;

    private float direction;
    private Transform ownerTransform;

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
        Vector2 movement = Vector2.right * direction * speed * Time.deltaTime;
        transform.Translate(movement, Space.World);
    }

    void OnTriggerEnter2D(Collider2D other)
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
    }

}
