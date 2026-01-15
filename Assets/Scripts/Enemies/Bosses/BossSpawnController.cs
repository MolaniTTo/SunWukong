using UnityEngine;

public class BossSpawnController : MonoBehaviour
{
    public GameObject GorilaBossZonePrefab;
    public GameObject MonjeBossZonePrefab;

    public GameObject GorilaZoneSpawnPoint;
    public GameObject MonjeZoneSpawnPoint;

    public void SpawnGorilaBossZone()
    {
        GameObject currentZone = GameObject.FindWithTag("GorilaBossZone");

        if (currentZone != null)
        {
            Destroy(currentZone);
        }

        GameObject newZone = Instantiate(GorilaBossZonePrefab, GorilaZoneSpawnPoint.transform.position, Quaternion.identity);

    }

    public void SpawnMonjeBossZone()
    {
        GameObject currentZone = GameObject.FindWithTag("MonjeBossZone");

        if (currentZone != null)
        {
            Destroy(currentZone);
        }

        GameObject newZone = Instantiate(MonjeBossZonePrefab, MonjeZoneSpawnPoint.transform.position, Quaternion.identity);
    }
}
