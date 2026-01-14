using UnityEngine;

public class BossSpawnController : MonoBehaviour
{
    public GameObject GorilaBossZonePrefab;
    public GameObject MonjeBossZonePrefab;

    public GameObject GorilaZoneSpawnPoint;
    public GameObject MonjeZoneSpawnPoint;

    public void SpawnGorilaBossZone()
    {
        if (GameObject.FindWithTag("GorilaBossZone") != null)
        {
            Destroy(GameObject.FindWithTag("GorilaBossZone"));
        }

        Instantiate(GorilaBossZonePrefab, GorilaZoneSpawnPoint.transform.position, Quaternion.identity);
    }
    public void SpawnMonjeBossZone()
    {
        if (GameObject.FindWithTag("MonjeBossZone") != null)
        {
            Destroy(GameObject.FindWithTag("MonjeBossZone"));
        }

        Instantiate(MonjeBossZonePrefab, MonjeZoneSpawnPoint.transform.position, Quaternion.identity);
    }
}
