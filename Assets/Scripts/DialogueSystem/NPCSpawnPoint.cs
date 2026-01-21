using UnityEngine;

public class NPCSpawnPoint : MonoBehaviour
{
    [Header("Spawn Point Settings")]
    public string locationID; 
    public string npcPrefabName = "MonjeBueno";


    private void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, 0.5f);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + Vector3.up * 1f);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(transform.position, 0.3f);
    }



}
