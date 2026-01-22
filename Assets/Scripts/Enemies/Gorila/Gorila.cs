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
    public GorilaEnrage EnrageState { get; private set; }

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
    public BossMusicController gorilaMusicController; //referencia al controlador de musica del boss

    [Header("Death Effect")]
    public DeathEffectHandler deathEffectHandler; // Sistema de efectos de muerte para el boss

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
    public bool hasEnraged = false;

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

    [Header("PlayerRef")]
    public PlayerStateMachine playerRef;

    [Header("Audio")]
    public AudioSource gorilaAudioSource;
    public AudioClip WakeUp;
    public AudioClip Walk;
    public AudioClip LowHealth;
    public AudioClip Death;
    public AudioClip AttackOnda;
    public AudioClip AttackCuffs;

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
            // Subscrivim al event OnDeath per reaccionar a la mort
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
        if (ProgressManager.Instance != null)
        {
            ProgressManager.Instance.RegisterBossDefeated("GorilaBoss");
        }

        // Canviem d'estat a DeathState
        if (DeathState != null && StateMachine != null)
        {
            StateMachine.ChangeState(DeathState);
        }

        // Alliberar confiner/zona boss
        if (bossTriggerZone != null)
        {
            bossTriggerZone.OnBossDefeated();
        }

        // Aturem moviment i desactivem collider d'atac
        StopMovement();
        if (punchCollider != null) 
        { 
            punchCollider.SetActive(false); 
        }
        
        // Hacer el rigidbody estático para que no se mueva durante la muerte
        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        // Deshabilitar colisiones principales
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        // Iniciar secuencia de muerte con efectos
        if (deathEffectHandler != null)
        {
            deathEffectHandler.TriggerDeathSequence();
        }
        else
        {
            // Fallback: destruir después de la animación de muerte
            Destroy(gameObject, 8f); // Más tiempo para un boss
        }
    }

    private void Start()
    {
        IdleState = new GorilaIdle(this);
        RunState = new GorilaRunning(this);
        PunchState = new GorilaPunchAttack(this);
        ChargedJumpState = new GorilaChargedJump(this);
        DeathState = new GorilaDeath(this);
        RetreatState = new GorilaRetreating(this);
        EnrageState = new GorilaEnrage(this);

        var sleepingState = new GorilaSleeping(this);
        StateMachine.Initialize(sleepingState);
        
        if(playerRef == null && player != null)
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
        float t = Mathf.InverseLerp(lowHealthThreshold, 0f, characterHealth != null ? characterHealth.currentHealth : 0f);
        float runSpeedMultiplier = Mathf.Lerp(baseSpeed, speedAtLowHealth, 1f - t);

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
                    scale.x = -Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }
                else if (player.position.x < transform.position.x && facingRight)
                {
                    facingRight = false;
                    facingDirection = -1;
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }
            }
        }
    }

    public bool Movement()
    {
        if (player == null) return false;

        timeSinceLastUpdate += Time.deltaTime;

        float verticalDiff = Mathf.Abs(player.position.y - transform.position.y);
        if (verticalDiff > verticalIgnoreThreshold)
        {
            rb.linearVelocity = Vector2.zero;
            return false;
        }

        if (timeSinceLastUpdate >= updateTargetInterval)
        {
            lastKnownPlayerPos = player.position;
            timeSinceLastUpdate = 0f;
        }

        Vector2 desiredDir = (lastKnownPlayerPos - (Vector2)transform.position);
        desiredDir.y = 0;

        float distanceToPlayer = Mathf.Abs(desiredDir.x);

        if (distanceToPlayer < 2.5f) 
        {
            rb.linearVelocity = Vector2.zero;
            return false;
        }

        desiredDir.Normalize();
        
        currentDir = Vector2.Lerp(currentDir, desiredDir, Time.deltaTime * 3f);

        float speed = (characterHealth != null && characterHealth.currentHealth <= lowHealthThreshold) ? speedAtLowHealth : baseSpeed;
        rb.linearVelocity = currentDir * speed;

        return true;
    }

    public override void Attack() { }

    public void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    public void StartWakeUpSequence()
    {
        var playerCtrl = player.GetComponent<PlayerStateMachine>();
        if (playerCtrl != null) 
        {
            playerCtrl.dialogueLocked = true; 
            playerCtrl.animator.SetFloat("speed", 0f);
            playerCtrl.rb.linearVelocity = Vector2.zero;
            playerCtrl.EnterDialogueMode();
        }
        
        if (animator != null) animator.SetTrigger("WakeUp");
        
        if(gorilaAudioSource != null && WakeUp != null)
        {
            gorilaAudioSource.PlayOneShot(WakeUp);
        }
    }

    public void OnWakeUpEnd()
    {
        var playerCtrl = player.GetComponent<PlayerStateMachine>();
        if (playerCtrl != null) playerCtrl.ExitDialogueMode();
        hasBeenAwaken = true;
        gorilaMusicController.StartBossMusic();
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

            cameraShake.Shake(3f, 3.5f, 1.2f);
        }
    }

    public void OnChargedJumpEnd()
    {
        animationFinished = true;
    }

    public void OnPunchImpact()
    {
        punchCollider.SetActive(true);
        cameraShake.Shake(1.5f, 2.0f, 0.4f);
    }

    public void OnPunchImpactEnd()
    {
        punchCollider.SetActive(false);
    }

    public void OnPunchEnd()
    {
        punchCounter++;
        animationFinished = true;
    }

    public void OnEnrageAnimationFinished()
    {
        EnrageState.OnEnrageAnimationFinished();
    }

    public bool IsPlayerTrapped()
    {
        if (player == null) return false;

        Vector2 origin = transform.position;
        Vector2 dir = Vector2.right * facingDirection;

        RaycastHit2D shortHit = Physics2D.Raycast(origin, dir, 4f, LayerMask.GetMask("Player"));
        RaycastHit2D longHit = Physics2D.Raycast(origin, dir, 8f, confinerWallMask);

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