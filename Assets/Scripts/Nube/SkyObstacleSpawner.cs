using UnityEngine;
using System.Collections.Generic;

public class SkyObstacleSpawner : MonoBehaviour
{
    [Header("Referencias")]
    public Transform player; // Sun Wukong
    public GameObject[] obstaclePrefabs; // Array de prefabs de obstáculos
    
    [Header("Spawn Configuration")]
    [Tooltip("Distancia vertical entre obstáculos")]
    public float spawnInterval = 5f;
    
    [Tooltip("Distancia por delante del jugador donde aparecen obstáculos")]
    public float spawnDistance = 20f;
    
    [Tooltip("Rango horizontal donde pueden aparecer (-x a +x)")]
    public float horizontalRange = 4f;
    
    [Header("Gestión de Obstáculos")]
    [Tooltip("Distancia detrás del jugador donde se destruyen")]
    public float destroyDistance = 10f;
    
    private float nextSpawnHeight;
    private List<GameObject> activeObstacles = new List<GameObject>();

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        nextSpawnHeight = player.position.y + spawnDistance;
    }

    void Update()
    {
        if (player == null || obstaclePrefabs.Length == 0) return;

        // Spawner nuevos obstáculos cuando el jugador se acerca
        while (player.position.y + spawnDistance > nextSpawnHeight)
        {
            SpawnObstacle();
            nextSpawnHeight += spawnInterval;
        }

        // Limpiar obstáculos que quedaron atrás
        CleanupObstacles();
    }

    void SpawnObstacle()
    {
        // Elegir prefab aleatorio
        GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Length)];
        
        // Posición aleatoria en X, fija en Y (altura de spawn)
        float randomX = Random.Range(-horizontalRange, horizontalRange);
        Vector3 spawnPosition = new Vector3(randomX, nextSpawnHeight, 0);

        // Instanciar obstáculo
        GameObject obstacle = Instantiate(prefab, spawnPosition, Quaternion.identity);
        obstacle.transform.parent = transform; // Organizar en jerarquía
        activeObstacles.Add(obstacle);
    }

    void CleanupObstacles()
    {
        // Eliminar obstáculos que quedaron muy atrás
        for (int i = activeObstacles.Count - 1; i >= 0; i--)
        {
            if (activeObstacles[i] == null)
            {
                activeObstacles.RemoveAt(i);
                continue;
            }

            if (activeObstacles[i].transform.position.y < player.position.y - destroyDistance)
            {
                Destroy(activeObstacles[i]);
                activeObstacles.RemoveAt(i);
            }
        }
    }

    void OnDrawGizmos()
    {
        if (player == null) return;

        // Visualizar zona de spawn
        Gizmos.color = Color.green;
        Vector3 spawnLine = new Vector3(0, player.position.y + spawnDistance, 0);
        Gizmos.DrawLine(spawnLine + Vector3.left * 10, spawnLine + Vector3.right * 10);

        // Visualizar zona de destrucción
        Gizmos.color = Color.red;
        Vector3 destroyLine = new Vector3(0, player.position.y - destroyDistance, 0);
        Gizmos.DrawLine(destroyLine + Vector3.left * 10, destroyLine + Vector3.right * 10);
    }
}