using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class Monje : EnemyBase
{
    public MonjeIdle IdleState { get; private set; } //estat de idle
    public MonjeFlee RunState { get; private set; } //estat de fugir (run i saltar)
    public MonjeDeath DeathState { get; private set; } //estat de mort
    public MonjeRetreating RetreatState { get; private set; } //estat de retirada enrere (per si acorralem al sunwukong)
    public MonjeThrowingRay ThrowRayState { get; private set; } //estat d'atac de llan?ament de raigs
    public MonjeThrowingGas ThrowGasState { get; private set; } //estat d'atac de llan?ament de gas
    public MonjeTeletransportAttack TeletransportState { get; private set; } //estat d'atac de teletransport


    [Header("Refs")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;
    public CinemachineCamera confinerCamera; //la cam
    public Collider2D bossZone; //collider del confiner de la zona del boss
    public BossTriggerZone2D bossTriggerZone; //referencia a la zona de trigger del boss
    public GameObject punchCollider; //collider que s'activa amb el atac de teletransport
    public CameraShake cameraShake; //referencia al component de camera shake
    public CharacterHealth characterHealth; //referencia al component de vida
    public Transform throwingGasSpawnPoint; //punt des d'on es llença el gas
    public GameObject gasPrefab; //prefab de la bola de gas
    public RayManager rayManager; //referencia al gestor de raigs
    public NPCDialogue npcDialogue;
    public GameObject teletransportParticle;
    public GameObject teletransportSpawnPoint;
    public BossMusicController monjeMusicController; //referencia al controlador de musica del boss


    [Header("Stats")]
    public bool facingRight = false;
    public bool dialogueFinished = false;
    public bool playerIsOnConfiner = false; //si el player esta dins del confiner o no
    public bool lockFacing = true;
    public bool lookAtPlayer = false;
    public bool firstRayThrowed = false; //per controlar que el monje llanci el primer raig nomes un cop
    public bool animationFinished = false;
    public bool raysFinished = false; //si ha acabat de llen?ar tots els raigs

    [Header("Flee")]

    public float groundCheckRadius = 0.2f;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public bool isGrounded = false;

    public float fleeSpeed = 5f;
    public float minDistanceToFlee = 2f; //distancia que si la traspasa, el monje fuig
    public float maxDistanceToStopFlee = 5f; //distancia que si la traspasa, el monje para de fugir
    public float criticalDistanceToPlayer = 1.5f; //distancia critica al player per activar l'estat de fuga critica
    public float optimalDistanceToPlayer = 4f; //distancia optima al player per sortir de l'estat de fuga critica
    public bool isFleeing = false;
    public bool criticalFleeState = false;
    public bool isTrapped = false; //si el monje esta acorralat i no pot fugir
    public bool isInOptimalDistance = false; //si el monje esta a la distancia optima al player per atacar
    public bool isTeletransportingToFlee = false; //si el monje esta teletransportant-se per fugir



    [Header("Attack settings")]
    [HideInInspector] public int facingDirection = -1;
    public int attackIndex = 5; //Index actual del patro d'atacs

    [Header("Teletransport settings")]
    public float teleportYOffset = 3f; //altura sobre el player
    public float fallImpactThreshold = -2f; //velocitat a la que cau per activar l'impacte
    public bool isFallingFromTeleport = false;
    public bool isInvisible = false;

    [Header("Confiner Awareness")]
    public LayerMask confinerWallMask; //Capa de les parets del confiner

    [Header("PlayerRef")]
    public PlayerStateMachine playerRef;

    [Header("Audio")]
    public AudioSource monjeAudioSource;
    public AudioClip DeathSound;
    public AudioClip RunSound;
    public AudioClip TeletransportImpactSound;
    public AudioClip TeletransportSound;
    public AudioClip TeletransportToFleeSound;
    public AudioClip ThrowLightningSound;
    public AudioClip ThrowToxicGasSound;



    protected override void Awake()
    {
        base.Awake(); //Cridem a l'Awake de la classe base EnemyBase perque inicialitzi la maquina d'estats
        if (player == null)
        {
            var pGo = GameObject.FindGameObjectWithTag("Player");
            if (pGo != null) player = pGo.transform;
        }
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (punchCollider != null) punchCollider.SetActive(false); //Ens assegurem que el collider d'atac esta desactivat al iniciar

        if (characterHealth == null)
        {
            characterHealth = GetComponent<CharacterHealth>();
        }

        if (characterHealth != null)
        {
            // Subscrivim al event OnDeath per reaccionar a la mort (CharacterHealth no crida Gorila.Die per nosaltres)
            characterHealth.OnDeath += HandleCharacterDeath;
        }
        if (!dialogueFinished)
        {
            rb.bodyType = RigidbodyType2D.Static; //si no ha acabat el diàleg, el monje no es mou
        }
    }

    private void OnDestroy()
    {
        if (characterHealth != null)
        {
            characterHealth.OnDeath -= HandleCharacterDeath;
        }
    }
    private void HandleCharacterDeath()
    {
        //Quan CharacterHealth dispara OnDeath -> fem el canvi d'estat a DeathState i alliberem el confiner (boss defeated)
        //Canviem d'estat si la màquina està inicialitzada
        if (DeathState != null && StateMachine != null)
        {
            StateMachine.ChangeState(DeathState);
        }

        //Alliberar confiner/zonaboss (si està assignat). Ho fem aquí per assegurar que el nivell es desbloqueja.
        if (bossTriggerZone != null)
        {
            bossTriggerZone.OnBossDefeated();
        }

        // Aturem moviment i desactivem collider d'atac
        StopMovement();
        if (punchCollider != null) { punchCollider.SetActive(false); }
    }

    private void Start()
    {
        IdleState = new MonjeIdle(this);
        RunState = new MonjeFlee(this);
        DeathState = new MonjeDeath(this);
        RetreatState = new MonjeRetreating(this);
        ThrowRayState = new MonjeThrowingRay(this);
        ThrowGasState = new MonjeThrowingGas(this);
        TeletransportState = new MonjeTeletransportAttack(this);

        StateMachine.Initialize(IdleState); //Inicialitzem la maquina d'estats amb l'estat de idle

        if (playerRef == null && player != null)
        {
            playerRef = player.GetComponent<PlayerStateMachine>();
        }
    }

    public bool CheckIfPlayerIsDead()
    {
        return playerRef.isDead;
    }

    protected override void Update()
    {
        StateMachine.Update();

        if (isFallingFromTeleport)
        {
            if (IsGrounded())
            {
                animator.SetTrigger("TeletransportImpact");
                isFallingFromTeleport = false;
            }
        }
    }

    public void Flip()
    {
        if (lockFacing || player == null) return;

        bool shouldFaceRight;

        if (lookAtPlayer)
            shouldFaceRight = player.position.x > transform.position.x;
        else
            shouldFaceRight = player.position.x < transform.position.x;

        if (shouldFaceRight == facingRight) return;

        SetFacing(shouldFaceRight);
    }

    public void SetFacing(bool faceRight)
    {
        facingRight = faceRight;
        facingDirection = facingRight ? 1 : -1;

        Vector3 scale = transform.localScale;
        scale.x = facingRight ? 1f : -1f;
        transform.localScale = scale;
    }

    public void FacePlayer()
    {
        if (player == null) return;
        bool shouldFaceRight = player.position.x > transform.position.x;
        SetFacing(shouldFaceRight);
    }




    public void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
    public void OnDialogueEnd() //es crida desde un event al DialogueManager quan acaba el dialogo
    {

    }

    private bool IsGrounded()
    {
        RaycastHit2D hit1 = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckRadius, groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckRadius, playerLayer);

        //dibuuixar els rays per debug
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckRadius, Color.red);
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckRadius, Color.blue);

        return hit1.collider != null || hit2.collider != null;
    }

    public bool HasToFlee()
    {
        //tirar un raycast para que sea visual
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, maxDistanceToStopFlee);
        Debug.DrawRay(transform.position, (player.position - transform.position).normalized * maxDistanceToStopFlee, Color.green);

        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, minDistanceToFlee);
        Debug.DrawRay(transform.position, (player.position - transform.position).normalized * minDistanceToFlee, Color.yellow);

        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);
        if (horizontalDistance < minDistanceToFlee) //si la distancia es menor que la minima distancia per fugir
        {
            isFleeing = true;
            return isFleeing;
        }
        else if (horizontalDistance < maxDistanceToStopFlee && isFleeing) //si la distancia es menor que la maxima distancia per deixar de fugir i ja estava fugint
        {
            isFleeing = true;
            return isFleeing;
        }
        else //si la distancia es major que la maxima distancia per deixar de fugir
        {
            isFleeing = false;
            return isFleeing;
        }
    }

    public bool CriticalFleeState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < criticalDistanceToPlayer) //si la distancia es menor que la distancia critica
        {
            criticalFleeState = true;
            return true;
        }

        criticalFleeState = false;
        return false;
    }

    //METODES PER EL TELETRANSPORT --> FINALITZAT
    public void Teletransport() //es crida desde un animation event al final de la animacio de iniciTeletranport
    {
        Instantiate(teletransportParticle, teletransportSpawnPoint.transform.position, Quaternion.identity); //particules de teletransport a la posicio del monje
        HideMonje(true);

        Vector3 newPosition = player.position;
        newPosition.y += teleportYOffset; //afegim l'offset en Y perque spawnei a dalt del player
        transform.position = newPosition;

        isFallingFromTeleport = true; //indiquem que esta caient desde el teletransport

        rb.linearVelocity = new Vector2(0, -20); //donem una velocitat cap avall perque caigui rapidament

        Instantiate(teletransportParticle, teletransportSpawnPoint.transform.position, Quaternion.identity);
        HideMonje(false);
    }

    public void TeletransportToFlee()
    {
        HideMonje(true);

        float fleeDirection;

        //si te una pared davant
        if (IsNearWall())
        {
            fleeDirection = transform.localScale.x > 0 ? -1f : 1f; //fugeux en la direccio oposada a la que mira
        }
        else
        {
            //si no hi ha paret, fugeux en la direccio oposada al player
            fleeDirection = player.position.x > transform.position.x ? -1f : 1f;
        }

        //nova posicio per teletransportar-se
        Vector3 newPosition = new Vector3(transform.position.x + fleeDirection * maxDistanceToStopFlee, transform.position.y, transform.position.z);

        // Limitar dentro de los bordes de la boss zone si quieres
        if (bossZone != null)
        {
            newPosition.x = Mathf.Clamp(newPosition.x, bossZone.bounds.min.x, bossZone.bounds.max.x);
        }


        rb.linearVelocity = Vector2.zero;
        rb.bodyType = RigidbodyType2D.Kinematic;
        transform.position = newPosition;
        isTeletransportingToFlee = true;
        rb.bodyType = RigidbodyType2D.Dynamic;
        HideMonje(false);
    }


    public void HideMonje(bool hide)
    {
        isInvisible = hide;
        //busca un gameObject hijo de manera recursiva con un nmbre especifico
        foreach (Transform child in transform)
        {
            if (child.name == "Mesh")
            {
                //agafem tots els sprite renderers dels fills del mesh i els desactivem/activem
                SpriteRenderer[] spriteRenderers = child.GetComponentsInChildren<SpriteRenderer>();
                foreach (var sr in spriteRenderers)
                {
                    sr.enabled = !hide;
                }
            }
        }
    }
    public void OnTeletransportAttackImpact()
    {
        //activar el collider de l'atac de teletransport
        if (punchCollider != null)
        {
            punchCollider.SetActive(true);
            Debug.Log("Punch collider activated");
        }
        //fer shake a la camara
        if (cameraShake != null)
        {
            cameraShake.Shake(8f, 5f, 1f); //amplitud, frequencia, duracio
        }
        if (monjeAudioSource != null && TeletransportImpactSound != null)
        {
            monjeAudioSource.PlayOneShot(TeletransportImpactSound);
        }
    }

    public void OnTeletransportAttackImpactEnd()
    {
        //desactivar el collider de l'atac de teletransport
        if (punchCollider != null)
        {
            punchCollider.SetActive(false);
        }
        animationFinished = true; //indiquem que ha acabat l'animacio de l'atac de teletransport
        isTeletransportingToFlee = false;
    }

    //METODES PER EL THROW GAS --> FINALITZAT
    public void ThrowGas() //es crida desde un animation event a la animacio de throw gas
    {
        GameObject gasBall = Instantiate(gasPrefab, throwingGasSpawnPoint.position, Quaternion.identity);
        //aqui quiero hacer que la bola (que tiene rigidbody2D) se mueva hacia alante ya que el monje la lanza hacia adelante.
        Rigidbody2D gasRb = gasBall.GetComponent<Rigidbody2D>();
        if (gasRb != null)
        {
            float throwForce = 25f; //fuerza con la que se lanza la bola de gas
            Vector2 throwDirection = facingRight ? Vector2.right : Vector2.left; //direccion del lanzamiento segun la direccion del monje
            gasRb.linearVelocity = throwDirection * throwForce; //asignamos la velocidad al rigidbody2D de la bola de gas
        }

    }

    public void OnThrowGasEnd() //es crida desde un animation event al final de la animacio de throw gas
    {
        animationFinished = true; //indiquem que ha acabat l'animacio de llan?ament de gas
    }


    //METODES PER EL THROW RAY --> FINALITZAT
    public void OnThrowRayShakeCam() //es crida desde event de animator durant la animacio de throwRay
    {
        cameraShake.Shake(8f, 5f, 0.5f); //amplitud, frequencia, duracio
    }
    public void OnRayImpactShakeCam() //es crida desde event de animator durant la animacio de throwRay
    {
        cameraShake.Shake(8f, 8f, 2f); //amplitud, frequencia, duracio
    }
    public void OnThrowRay() //es crida desde un animation event de la animacio de throwRay
    {
        rayManager.ThrowRaysRoutine();
    }
    public void OnThrowRayEnd() //es crida desde un animation event al final de la animacio de throwRay
    {
        animationFinished = true; //indiquem que ha acabat l'animacio de llan?ament de raig
    }

    public bool IsNearWall()
    {
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, forward, 1f, confinerWallMask);
        Debug.DrawRay(transform.position, forward * 2f, Color.cyan);
        return hit.collider != null;
    }

    //METODES COMUNS DELS ENEMICS (HERETATS DE ENEMYBASE)

    public override void Move()
    {
        if (player == null || rb == null) { return; }

        float directionX = player.position.x > transform.position.x ? -1f : 1f;
        rb.linearVelocity = new Vector2(directionX * fleeSpeed, rb.linearVelocityY);

    }



    public override void Attack() { }
    public override bool CanSeePlayer()
    {
        throw new System.NotImplementedException();
    }

    public override void Die()
    {
        //ho gestiona el CharacterHealth
    }

}
