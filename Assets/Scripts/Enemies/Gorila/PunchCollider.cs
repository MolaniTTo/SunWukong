using UnityEngine;

public class PunchCollider : MonoBehaviour
{
    [SerializeField] private float damage = 20f;         // Dany del cop de puny

    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("PunchCollider: OnTriggerEnter2D detectat amb " + other.name);
        if (other.CompareTag("Player"))
        {
            CharacterHealth characterHealth = other.GetComponent<CharacterHealth>();
            KnockBack knockBack = other.GetComponent<KnockBack>();
            Debug.Log("PunchCollider: Colision amb el jugador detectada.");
            if (knockBack != null) 
            {
                knockBack.ApplyKnockBack(this.gameObject, 0.5f, 10f);
            }
            if (characterHealth != null) 
            {
                characterHealth.TakeDamage(damage); 
            }
        }

    }
}
