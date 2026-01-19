using Unity.VisualScripting;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    public float health = 100f;

    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            Debug.Log("Player Died");
        }
        LowHealthOverlay lowHealthOverlay = FindObjectOfType<LowHealthOverlay>();
        if (lowHealthOverlay != null)
        {
            lowHealthOverlay.Refresh();
        }
    }
}
