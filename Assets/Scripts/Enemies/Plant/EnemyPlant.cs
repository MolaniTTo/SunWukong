using System.Collections;
using System.Collections.Generic;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyPlant : EnemyBase
{
    [Header("Plant Settings")]
    public float detectionRange = 5f;
    public LayerMask playerLayer; //Capa del jugador per detectar col�lisions amb el jugador
    public bool facingRight = true; //Indica si la planta mira cap a la dreta o cap a l'esquerra
    public Animator animator; //Refer�ncia a l'animator de la planta

    [Header("Raycast Settings")]
    public Transform rayOrigin; //Origen del raycast (el cap)
    public float rayLength = 5f; //Longitud del raycast

    [Header("Pool Settings")]
    public GameObject bulletPrefab; //Prefab de la bala que dispara la planta
    public int poolSize = 2; //Mida de la pool de bales
    private Stack<GameObject> bulletStack;
    public Transform spawnPoint;


    protected override void Awake()
    {
        base.Awake(); //Cridem a l'Awake de la classe base EnemyBase perque inicialitzi la maquina d'estats
        InitializeBulletPool(); //inicialitzem la pool de bales
    }

    private void Start()
    {
        var idleState = new PlantIdle(this); //Creem l'estat d'idle i li passem una referencia a l'enemic (mirar)
        StateMachine.Initialize(idleState); //Inicialitzem la maquina d'estats amb l'estat d'idle
    }

    //METODE PER GIRAR LA PLANTA

    private void Flip()
    {
        facingRight = !facingRight; //canvia la direccio a la que mira la planta
        Vector3 scale = transform.localScale; //agafem l'escala actual de la planta
        scale.x *= -1; //invertim l'escala en l'eix X per girar la planta
        transform.localScale = scale; //assignem la nova escala a la planta
    }

    
    //METODES DE LA POOL DE BALES

    public void InitializeBulletPool()
    {
        bulletStack = new Stack<GameObject>(); //inicialitzem la pila

        for(int i=0; i<poolSize; i++) //omplim la pila amb les bales
        {
            GameObject bullet = Instantiate(bulletPrefab);
            bullet.SetActive(false);
            bulletStack.Push(bullet); //afegim la bala a la pila
        }
    }

    public void RechargeBullet(GameObject bullet)
    {
        bullet.SetActive(false); //desactivem la bala
        bulletStack.Push(bullet); //la tornem a afegir a la pila
    }


    //METODES COMUNS DELS ENEMICS

    public override bool CanSeePlayer()
    {
        //Direccions del raycast segons cap a on miri la planta
        Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left; //Si mira a la dreta, el raycast va cap a la dreta, sino cap a l'esquerra
        Vector2 backwardDir = facingRight ? Vector2.left : Vector2.right; //Direccio contraria al raycast (esquena)

        //Tira els raycasts per detectar el jugador
        RaycastHit2D forwardHit = Physics2D.Raycast(rayOrigin.position, forwardDir, rayLength, playerLayer);
        RaycastHit2D backwardHit = Physics2D.Raycast(rayOrigin.position, backwardDir, rayLength, playerLayer);

        //Dibuixa els raycasts a l'escena per visualitzar-los
        Debug.DrawRay(rayOrigin.position, forwardDir * rayLength, Color.red);
        Debug.DrawRay(rayOrigin.position, backwardDir * rayLength, Color.blue);

        if(forwardHit.collider != null) { return true; } //el jugador esta davant

        if (backwardHit.collider != null)
        {
            Flip(); //gira la planta per mirar al jugador
            return true; //el jugador esta darrera
        }

        return false; //no ha detectat el jugador

    }

    public override void Attack() //Aquest metode es crida des de l'animacio d'atac mitjancant un event per disparar la bala a l'hora que toca
    {
        if (bulletStack.Count > 0)
        {
            GameObject bullet = bulletStack.Pop(); //treiem una bala de la pila
            bullet.transform.position = spawnPoint.position; //posicionem la bala al punt d'spawn
            bullet.transform.rotation = Quaternion.identity; //resetejem la rotacio de la bala
            bullet.SetActive(true); //activem la bala

            Bullet bulletScript = bullet.GetComponent<Bullet>();
            bulletScript.SetEnemyPlant(this); //assignem la planta que ha disparat la bala
            bulletScript.Launch(facingRight); //disparem la bala en la direccio que toca
        }
    }

    public override void Die()
    {
        throw new System.NotImplementedException();
    }

    public override void Move()
    {
        //Aquest enemic no es mou, per tant no implementem res aqui
    }
}
