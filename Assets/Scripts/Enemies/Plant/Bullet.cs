using UnityEngine;

public class Bullet : MonoBehaviour
{
    private EnemyPlant plant; //Refer�ncia a la planta que ha disparat la bala
    private Rigidbody2D rb;
    public float speed = 5f; //Velocitat de la bala
    private Vector2 direction; //Direcci� de la bala segons el facingRight de la planta 
 
    
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
        direction = facingRight ? Vector2.right : Vector2.left; //Assignem la direcci� segons el facingRight de la planta

        Vector3 scale = transform.localScale; //agafem l'escala actual de la bala
        scale.x = Mathf.Abs(scale.x) * (facingRight ? -1 : 1); //valor absolut de 1 o -1 segons la direcci�
        transform.localScale = scale; //assignem la nova escala a la bala

        rb.linearVelocity = direction * speed; //Assignem la velocitat a la bala segons la direcci� i la velocitat

    }

    private void Update()
    {
        if(Mathf.Abs(transform.position.x - plant.spawnPoint.position.x) > 15f) //calculem la posicio de la bala respecte el spawnpoint de la planta
        {
            plant.RechargeBullet(gameObject); //si la distancia es major a 15 la recarreguem a la pool igualment
        }
       
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Obstacle"))
        {
            rb.linearVelocity = Vector2.zero; //aturar la bala en colisio
            plant.RechargeBullet(gameObject);
            //Aqui ja restarem la vida al player accedint a la classe PlayerHealth o similar
        }
    }





}
