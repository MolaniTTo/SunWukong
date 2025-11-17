using UnityEngine;

public class Bullet : MonoBehaviour
{
    private EnemyPlant plant; //Referència a la planta que ha disparat la bala
    private Rigidbody2D rb;
    public float speed = 5f; //Velocitat de la bala
    private Vector2 direction; //Direcció de la bala segons el facingRight de la planta 
 
    
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetEnemyPlant(EnemyPlant enemyPlant)
    {
        this.plant = enemyPlant; //Assignem la planta que ha disparat la bala
    }

    public void Launch(bool facingRight) 
    {
        direction = facingRight ? Vector2.right : Vector2.left; //Assignem la direcció segons el facingRight de la planta

        Vector3 scale = transform.localScale; //agafem l'escala actual de la bala
        scale.x = Mathf.Abs(scale.x) * (facingRight ? -1 : 1); //valor absolut de 1 o -1 segons la direcció
        transform.localScale = scale; //assignem la nova escala a la bala

        rb.linearVelocity = direction * speed; //Assignem la velocitat a la bala segons la direcció i la velocitat

    }

    private void Update()
    {
        if(Mathf.Abs(transform.position.x - plant.spawnPoint.position.x) > 15f) //calculem la posicio de la bala respecte el spawnpoint de la planta
        {
            plant.RechargeBullet(gameObject); //si la distancia es major a 15 la recarreguem a la pool igualment
        }
       
    }


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player")) //Si colisiona amb el jugador
        {
            rb.linearVelocity = Vector2.zero;
            plant.RechargeBullet(gameObject); //Recarreguem la bala a la pool
        }

    }






}
