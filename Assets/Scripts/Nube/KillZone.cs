using UnityEngine;

public class KillZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D col)
    {
        if (col.CompareTag("Player"))
        {
            CharacterHealth health = col.GetComponent<CharacterHealth>();

            if (health != null)
            {
                // Le quitamos toda la vida instantáneamente
                health.TakeDamage(health.currentHealth, gameObject);
            }
        }
    }
}
