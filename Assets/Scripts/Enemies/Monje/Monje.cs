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
    public BossTriggerZone2D bossTriggerZone; //referencia a la zona de trigger del boss
    public GameObject punchCollider; //collider que s'activa amb el atac de teletransport
    public CameraShake cameraShake; //referencia al component de camera shake
    public CharacterHealth characterHealth; //referencia al component de vida
    public Transform throwingGasSpawnPoint; //punt des d'on es llença el gas
    public GameObject gasPrefab; //prefab de la bola de gas
    public RayManager rayManager; //referencia al gestor de raigs


    [Header("Stats")]
    public bool facingRight = false;
    public bool dialogueFinished = false;
    public bool playerIsOnConfiner = false; //si el player esta dins del confiner o no
    public bool lockFacing = true;
    public bool lookAtPlayer = false;
    public bool firstRayThrowed = false; //per controlar que el monje llanci el primer raig nomes un cop
    public bool animationFinished = false;

    [Header("Flee")]
    public float fleeSpeed = 5f;
    public float minFleeDistance = 3f; //distancia minima al jugador per fugir
    public float maxFleeDistance = 7f; //distancia maxima al jugador per fugir
    public float jumpForce = 10f;
    public float groundCheckRadius = 0.2f;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public bool isGrounded = false;


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
        if (lockFacing || player == null) { return; }

        bool shouldFaceRight; //direccio que hauria de mirar

        if (lookAtPlayer) //si ha de mirar al player
        {
            shouldFaceRight = player.position.x > transform.position.x; //Mirar sempre cap al jugador
        }
        else
        {
            shouldFaceRight = player.position.x < transform.position.x; //Sempre a la direccio oposada del jugador
        }

        if (shouldFaceRight == facingRight) { return; } //si ja esta mirant a la direccio correcta, no fem res


        facingRight = shouldFaceRight; //actualitzem la variable de direccio
        facingDirection = facingRight ? 1 : -1; //actualitzem la variable numerica de direccio on 1 = dreta i -1 = esquerra (perque es un boss)

        Vector3 scale = transform.localScale;

        scale.x = facingRight ? 1f : -1f; //actualitzem l'escala en X segons la direccio

        transform.localScale = scale;
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
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);

        //si esta molt a prop -> fugir
        if (distance < minFleeDistance)
            return true;

        //si esta molt lluny -> no fugir
        if (distance > maxFleeDistance)
            return false;

        //si no esta ni molt a prop ni molt lluny -> no fugir
        return false;
    }

    //METODES PER EL TELETRANSPORT

    public void Teletransport() //es crida desde un animation event al final de la animacio de iniciTeletranport
    {
        HideMonje(true);

        Vector3 newPosition = player.position;
        newPosition.y += teleportYOffset; //afegim l'offset en Y perque spawnei a dalt del player
        transform.position = newPosition;

        isFallingFromTeleport = true; //indiquem que esta caient desde el teletransport

        rb.linearVelocity = new Vector2(0, -20); //donem una velocitat cap avall perque caigui rapidament

        HideMonje(false);
    }

    public void HideMonje(bool hide)
    {
        isInvisible = hide;
        SpriteRenderer[] spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
        foreach (var sr in spriteRenderers)
        {
            sr.enabled = !hide;
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
            cameraShake.Shake(2f, 5f, 0.3f); //amplitud, frequencia, duracio
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
    }

    //METODES PER EL THROW GAS
    public void ThrowGas() //es crida desde un animation event a la animacio de throw gas
    {
        GameObject gasBall = Instantiate(gasPrefab, throwingGasSpawnPoint.position, Quaternion.identity);
        Vector2 directionToPlayer = (player.position - throwingGasSpawnPoint.position).normalized;
        //lo demes es controla desde el script de la bola de gas
    }

    public void OnThrowGasEnd() //es crida desde un animation event al final de la animacio de throw gas
    {
        animationFinished = true; //indiquem que ha acabat l'animacio de llan?ament de gas
    }


    //METODES PER EL THROW RAY

    public void OnThrowRay() //es crida desde un animation event de la animacio de throwRay
    {
        rayManager.ThrowRaysRoutine();
    }

    public void OnThrowRayEnd() //es crida desde un animation event al final de la animacio de throwRay
    {
        animationFinished = true; //indiquem que ha acabat l'animacio de llan?ament de raig
    }





    //METODES COMUNS DELS ENEMICS (HERETATS DE ENEMYBASE)

    public override void Move()
    {
        if (player == null || rb == null) { return; }


        float distance = Vector2.Distance(transform.position, player.position);

        if (distance < minFleeDistance) //si esta massa a prop
        {
            Vector2 directionAway = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(directionAway.x * fleeSpeed, rb.linearVelocity.y);
            return;
        }

        //si esta a una distancia mitjana
        if (distance < maxFleeDistance)
        {
            //s'allunya pero mes lentament
            Vector2 directionAway = (transform.position - player.position).normalized;
            rb.linearVelocity = new Vector2(directionAway.x * (fleeSpeed * 0.5f), rb.linearVelocity.y);
            return;
        }

        //si esta massa lluny -> no es mou
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
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
