using UnityEngine;
using System.Collections;

public class BossSpawnController : MonoBehaviour
{
    public GameObject GorilaBossZonePrefab;
    public GameObject MonjeBossZonePrefab;

    public GameObject GorilaZoneSpawnPoint;
    public GameObject MonjeZoneSpawnPoint;

    public IEnumerator SpawnGorilaBossZone()
    {
        GameObject currentZone = GameObject.FindWithTag("GorilaBossZone");

        if (currentZone != null)
        {
            yield return new WaitForSeconds(1f); //Petita espera per assegurar que no hi hagi conflictes
            BossMusicController bossMusicController = currentZone.GetComponentInChildren<BossMusicController>();
            if (bossMusicController != null)
            {
                bossMusicController.ReturnToNormalMusic();
            }
            yield return new WaitForSeconds(0.2f);
            Destroy(currentZone);
        }

        yield return new WaitForSeconds(0.5f); // Espera abans de spawnar la nova zona
        GameObject newZone = Instantiate(GorilaBossZonePrefab, GorilaZoneSpawnPoint.transform.position, Quaternion.identity);

    }

    public IEnumerator SpawnMonjeBossZone()
    {
        GameObject currentZone = GameObject.FindWithTag("MonjeBossZone");

        if (currentZone != null)
        {
            yield return new WaitForSeconds(1f); //Petita espera per assegurar que no hi hagi conflictes
            BossMusicController bossMusicController = currentZone.GetComponentInChildren<BossMusicController>();
            if (bossMusicController != null)
            {
                bossMusicController.ReturnToNormalMusic();
            }
            yield return new WaitForSeconds(0.3f);
            Destroy(currentZone);
        }

        yield return new WaitForSeconds(1.5f); // Espera abans de spawnar la nova zona
        GameObject newZone = Instantiate(MonjeBossZonePrefab, MonjeZoneSpawnPoint.transform.position, Quaternion.identity);
    }
}
