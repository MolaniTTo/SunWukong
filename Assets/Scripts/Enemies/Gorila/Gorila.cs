using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class Gorila : EnemyBase
{
    public GorilaIdle IdleState { get; private set; }
    public GorilaRunning RunState { get; private set; }
    public GorilaPunchAttack PunchState { get; private set; }
    public GorilaChargedJump ChargedJumpState { get; private set; }
    public GorilaDeath DeathState { get; private set; }
    public GorilaRetreating RetreatState { get; private set; }

    [Header("Refs")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;
    public CinemachineCamera confinerCamera; //la cam
    public BossTriggerZone2D bossTriggerZone; //referencia a la zona de trigger del boss
    public GameObject punchCollider; //collider que s'activa durant l'atac de puny
    public CameraShake cameraShake; //referencia al component de camera shake
    public CharacterHealth characterHealth; //referencia al component de vida
    public MonjeBueno monjeBueno; //referencia al monje que canvia el seu diàleg un cop es derrota el gorila


    [Header("Stats")]
    public float baseSpeed = 3.5f;
    public float speedAtLowHealth = 5.0f;
    public float lowHealthThreshold = 50f;
    public bool facingRight = false;
    public bool animationFinished = false;
    public bool playerIsOnConfiner = false; //si el player esta dins del confiner o no
    public bool hasBeenAwaken = false; //si s'ha despertat o no
    public int punchCounter = 0; //contador de atacs normals
    public int punchsBeforeCharged = 2; //atacs a fer per carregar l'atac gran
    public bool lockFacing = true;

    [Header("Chase")]
    private Vector2 currentDir = Vector2.zero; //Direccio actual cap al jugador
    private Vector2 lastKnownPlayerPos; //Ultima posicio coneguda del jugador
    public float updateTargetInterval = 1f; //Interval per actualitzar la posicio del jugador
    public float timeSinceLastUpdate = 0f; //Temps des de l'ultima actualitzacio
    public float verticalIgnoreThreshold = 6f; //Separa la distancia vertical per ignorar-la en el seguiment


    [Header("Attack settings")]
    public Transform earthquakeSpawnPoint; //On instanciem la ona
    public GameObject earthquakePrefab; // Prefab de la ona

    [HideInInspector] public int facingDirection = -1;

    [Header("Confiner Awareness")]
    public LayerMask confinerWallMask; //Capa de les parets del confiner



    protected override void Awake()
    {
        base.Awake(); //Cridem a l'Awake de la classe base EnemyBase perque inicialitzi la maquina d'estats
        if (player == null)
        {
            var pGo = GameObject.FindGameObjectWithTag("Player");
            if (pGo != null) player = pGo.transform;
        }
        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if(punchCollider != null) punchCollider.SetActive(false); //Ens assegurem que el collider d'atac esta desactivat al iniciar

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
        monjeBueno.ChangeDialogue("Monje2");
    }

    private void Start()
    {
        IdleState = new GorilaIdle(this);
        RunState = new GorilaRunning(this);
        PunchState = new GorilaPunchAttack(this);
        ChargedJumpState = new GorilaChargedJump(this);
        DeathState = new GorilaDeath(this);
        RetreatState = new GorilaRetreating(this);

        var sleepingState = new GorilaSleeping(this); //Creem l'estat de sleeping i li passem una referencia a l'enemic (mirar)
        StateMachine.Initialize(sleepingState); //Inicialitzem la maquina d'estats amb l'estat de sleeping
    }

    protected override void Update()
    {
        float t = Mathf.InverseLerp(lowHealthThreshold, 0f, characterHealth != null ? characterHealth.currentHealth : 0f); //Calcula un valor entre 0 i 1 segons la vida actual
        float runSpeedMultiplier = Mathf.Lerp(baseSpeed, speedAtLowHealth, 1f - t); //Calcula la velocitat de moviment segons la vida actual

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

    //METODES COMUNS DELS ENEMICS

    public bool Movement()
    {
        if (player == null) return false;

        timeSinceLastUpdate += Time.deltaTime; //actualitzem el temps des de l'ultima actualitzacio

        float verticalDiff = Mathf.Abs(player.position.y - transform.position.y);
        if (verticalDiff > verticalIgnoreThreshold)
        {
            //si la diferencia vertical es massa gran, no actualitzem la direccio cap al jugador ja que a vida real no es notaria tant
            rb.linearVelocity = Vector2.zero;
            return false;
        }

        if (timeSinceLastUpdate >= updateTargetInterval)
        {
            lastKnownPlayerPos = player.position; //actualitzem la ultima posicio coneguda del jugador
            timeSinceLastUpdate = 0f; //resetejem el temps des de l'ultima actualitzacio
        }

        Vector2 desiredDir = (lastKnownPlayerPos - (Vector2)transform.position);
        desiredDir.y = 0; //ignorem la diferencia vertical

        float distanceToPlayer = Mathf.Abs(desiredDir.x); //distancia horitzontal al jugador

        if (distanceToPlayer < 2.5f) 
        {
            rb.linearVelocity = Vector2.zero;
            return false; //esta massa a prop del jugador, no es mou
        }

        desiredDir.Normalize(); //normalitzem la direccio desitjada (nomes tindra component X)
        
        currentDir = Vector2.Lerp(currentDir, desiredDir, Time.deltaTime * 3f); //interpolacio suau cap a la direccio desitjada

        float speed = (characterHealth != null && characterHealth.currentHealth <= lowHealthThreshold) ? speedAtLowHealth : baseSpeed; //ajustem la velocitat segons la vida actual
        rb.linearVelocity = currentDir * speed; //assignem la velocitat al rigidbody

        return true; //s'està movent cap al jugador
    }

    public override void Attack() { }


    public void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void StartWakeUpSequence()
    {
        var playerCtrl = player.GetComponent<PlayerStateMachine>(); //bloquejem el control del player mentre s'executa la animació de wakeUp
        if (playerCtrl != null) 
        {
            playerCtrl.rb.linearVelocity = Vector2.zero; //aturrem el moviment del player
            playerCtrl.ForceNewState(PlayerStateMachine.PlayerState.Idle); //forcem al player a l'estat d'idle perque no es mogui
            playerCtrl.enabled = false; //desactivem el control del player
        }
        if (animator != null) animator.SetTrigger("WakeUp"); //activem l'animacio de despertar
    }

    public void OnWakeUpEnd() //es crida mitjançant un event a la animacio de wakeUp quan acaba l'animacio
    {
        var playerCtrl = player.GetComponent<PlayerStateMachine>();
        if (playerCtrl != null) playerCtrl.enabled = true; //donem control al player una altra vegada
        hasBeenAwaken = true;
    }

    public void OnChargedImpact()
    {
        if (earthquakePrefab != null && earthquakeSpawnPoint != null)
        {
            GameObject wave = Object.Instantiate(
                earthquakePrefab,
                earthquakeSpawnPoint.position,
                Quaternion.identity
            );

            float sign = Mathf.Sign(transform.localScale.x);
            wave.transform.localScale = new Vector3(
                Mathf.Abs(wave.transform.localScale.x) * sign,
                wave.transform.localScale.y,
                wave.transform.localScale.z
            );

            cameraShake.Shake(3f, 3.5f, 1.2f); //sacsegem la camara en l'impacte del puny
        }
    }
    public void OnChargedJumpEnd()
    {
        animationFinished = true;
    }

    public void OnPunchImpact() //cridat mitjançant un event a la animacio de punch quan arriba al punt d'impacte
    {
        punchCollider.SetActive(true); //activem el collider d'atac durant el frame d'impacte
        cameraShake.Shake(1.5f, 2.0f, 0.4f); //sacsegem la camara en l'impacte del puny
    }
    public void OnPunchImpactEnd()
    {
        punchCollider.SetActive(false); //desactivem el collider d'atac un cop ha passat el frame d'impacte
    }
    

    public void OnPunchEnd() //cridat mitjançant un event a la animacio de punch quan acaba l'animacio
    {
        punchCounter++;
        animationFinished = true;
    }


    public bool IsPlayerTrapped()
    {
        if (player == null) return false;

        Vector2 origin = transform.position;
        Vector2 dir = Vector2.right * facingDirection; //direccio del raycast segons cap a on miri el gorila

        RaycastHit2D shortHit = Physics2D.Raycast(origin, dir, 4f, LayerMask.GetMask("Player")); //raycast curt per detectar al jugador
        RaycastHit2D longHit = Physics2D.Raycast(origin, dir, 8f, confinerWallMask); //raycast llarg per detectar la paret

        //si el shortcollider colisiona amb la layermask player i el longcollider colisiona amb la layermask de les parets del confiner, el jugador està acorralat
        if (shortHit.collider != null && longHit.collider != null)
        {
            return true;
        }
        return false;
    }

    public override bool CanSeePlayer()
    {
        throw new System.NotImplementedException();
    }

    public override void Die()
    {
        if (bossTriggerZone != null)
        {
            bossTriggerZone.OnBossDefeated();
        }
        StopMovement();
        if (punchCollider != null) punchCollider.SetActive(false);
    }

    public override void Move()
    {
        throw new System.NotImplementedException();
    }
}
