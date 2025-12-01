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

    [Header("Stats")]
    public bool facingRight = false;
    public bool dialogueFinished = false;
    public bool playerIsOnConfiner = false; //si el player esta dins del confiner o no
    public bool lockFacing = true;

    [Header("Flee")]
    public float fleeSpeed = 5f;
    public float minFleeDistance = 3f; //distancia minima al jugador per fugir
    public float maxFleeDistance = 7f; //distancia maxima al jugador per fugir
    public float jumpForce = 10f;
    public float groundCheckRadius = 0.2f;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public bool isGrounded = false;


    [Header("Attack settings")]
    [HideInInspector] public int facingDirection = -1;
    public int[] attackPattern; //Patro d'atacs del monje
    public int attackIndex = 0; //Index actual del patro d'atacs

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
        RunState = new MonjeFlee(this);
        DeathState = new MonjeDeath(this);
        RetreatState = new MonjeRetreating(this);
        ThrowRayState = new MonjeThrowingRay(this);
        ThrowGasState = new MonjeThrowingGas(this);
        TeletransportState = new MonjeTeletransportAttack(this);

        var idleState = new MonjeIdle(this);
        StateMachine.Initialize(idleState); //Inicialitzem la maquina d'estats amb l'estat de idle

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
    }

    public void Flip()
    {
        if (!lockFacing)
        {
            if (player != null)
            {
                if (player.position.x > transform.position.x && !facingRight)
                {
                    facingRight = true;
                    facingDirection = 1;
                    Vector3 scale = transform.localScale;
                    scale.x = -Mathf.Abs(scale.x); // -1 = dreta ja que default mira a l'esquerra
                    transform.localScale = scale;
                }
                else if (player.position.x < transform.position.x && facingRight)
                {
                    facingRight = false;
                    facingDirection = -1; //esquerra
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x); // +1 = esquerra ja que default mira a l'esquerra
                    transform.localScale = scale;
                }

            }
        }
    }

    public void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }
    public void OnDialogueEnd() //es crida desde un event al DialogueManager quan acaba el dialogo
    {

    }








    //METODES COMUNS DELS ENEMICS (HERETATS DE ENEMYBASE)

    public override void Move() 
    {

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
