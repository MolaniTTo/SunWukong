using UnityEngine;

public class WarningMover : MonoBehaviour
{
    [Header("Movement")]
    public float randomStrength = 1.5f; //intensitat del moviment aleatori
    public float followStrength = 0.65f; //intensitat de seguir al player

    [Header("Avoidance")]
    public float avoidMonjeDistance = 1.5f; //distancia minima per evitar al monje

    [Header("Limits")] //limits de moviment per cada warning
    public float minX; 
    public float maxX;

    [Header("Refs")]
    public Transform player;
    public Transform monje;

    private bool canMove = false;

    private void Start()
    {
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        if(monje == null)
        {
            monje = GameObject.FindGameObjectWithTag("Monje").transform;
        }
    }

    void Update()
    {
        if (!canMove) return;

        Vector3 pos = transform.position;

        //moviment aleatori suau
        pos.x += Mathf.Sin(Time.time * 2f + Mathf.PerlinNoise(Time.time, transform.position.x) * 3f) * randomStrength * Time.deltaTime; 

        //seguir al jugador si esta assota
        if (Mathf.Abs(player.position.x - pos.x) < 4f)
        {
            pos.x = Mathf.Lerp(pos.x, player.position.x, followStrength * Time.deltaTime);
        }

        //evitar al monje
        float monkDx = pos.x - monje.position.x;
        if (Mathf.Abs(monkDx) < avoidMonjeDistance)
        {
            pos.x += Mathf.Sign(monkDx) * (avoidMonjeDistance - Mathf.Abs(monkDx))
                     * 2f * Time.deltaTime;
        }

        //limit de moviment dins dels valors establerts
        pos.x = Mathf.Clamp(pos.x, minX, maxX);

        transform.position = pos;
    }

    public void StartMoving()
    {
        canMove = true;
    }

    public void StopMoving()
    {
        canMove = false;
    }
}
