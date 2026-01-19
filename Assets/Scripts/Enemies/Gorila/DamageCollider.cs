using UnityEngine;

public class DamageCollider : MonoBehaviour
{
    [Header("Damage Settings")]
    [SerializeField] private float damage = 20f;         // Dany del cop de puny
    [SerializeField] private float knockBackForce = 10f; // Força de retrocés
    [SerializeField] private float knockBackDuration = 0.5f; // Durada del retrocés

    [Header("Damage Filter")]
    [SerializeField] private string tagToDamage = "Player"; // Tag dels objectes que poden rebre danys

    [Header("Owner")]
    [SerializeField] private GameObject owner; // Referència al propietari del collider (ex: Gorila)

    [Header("Particle")]
    [SerializeField] private GameObject particleHitPrefab;
    [SerializeField] private Transform particleSpawnPoint;


    private void Awake()
    {
        if (owner == null) { owner = transform.root.gameObject; } //Assignem el propietari si no està assignat
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(tagToDamage)) { return; } // Si l'objecte no té la tag correcta, sortim

        CharacterHealth characterHealth = other.GetComponent<CharacterHealth>();
        KnockBack knockBack = other.GetComponent<KnockBack>();

        if (characterHealth == null) { return; } // Si no té CharacterHealth, sortim

        if (characterHealth.isInvincible) { return; } // Si és invencible, sortim

        if (knockBack != null) 
        {
            knockBack.ApplyKnockBack(this.gameObject, knockBackDuration, knockBackForce);
        }

        if (particleHitPrefab != null && particleSpawnPoint != null)
        {
            Instantiate(particleHitPrefab, particleSpawnPoint.position, Quaternion.identity); // Creem les partícules d'impacte
        }

        characterHealth.TakeDamage(damage, owner); // Apliquem el dany

    }
}
