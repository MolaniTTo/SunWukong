using Unity.VisualScripting;
using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class Monje : EnemyBase
{
    public MonjeIdle IdleState { get; private set; }
    public MonjeFlee RunState { get; private set; }
    public MonjeDeath DeathState { get; private set; }
    public MonjeRetreating RetreatState { get; private set; }
    public MonjeThrowingRay ThrowRayState { get; private set; }
    public MonjeThrowingGas ThrowGasState { get; private set; }
    public MonjeTeletransportAttack TeletransportState { get; private set; }

    [Header("Refs")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;
    public CinemachineCamera confinerCamera;
    public Collider2D bossZone;
    public BossTriggerZone2D bossTriggerZone;
    public GameObject punchCollider;
    public CameraShake cameraShake;
    public CharacterHealth characterHealth;
    public Transform throwingGasSpawnPoint;
    public GameObject gasPrefab;
    public RayManager rayManager;
    public NPCDialogue npcDialogue;
    public GameObject teletransportParticle;
    public GameObject teletransportSpawnPoint;
    public BossMusicController monjeMusicController;
    public GameObject LowHealthPrefab;
    public GameObject LowHealthPrefabSpawnPoint;

    [Header("Death Effect")]
    public DeathEffectHandler deathEffectHandler; // Sistema de efectos de muerte para el boss

    [Header("Stats")]
    public bool facingRight = false;
    public bool dialogueFinished = false;
    public bool playerIsOnConfiner = false;
    public bool lockFacing = true;
    public bool lookAtPlayer = false;
    public bool firstRayThrowed = false;
    public bool animationFinished = false;
    public bool raysFinished = false;
    public bool LowHealthPrefabInstantiated = false;

    [Header("Flee")]
    public float groundCheckRadius = 0.2f;
    public Transform groundCheckPoint;
    public LayerMask groundLayer;
    public LayerMask playerLayer;
    public bool isGrounded = false;
    public float fleeSpeed = 5f;
    public float minDistanceToFlee = 2f;
    public float maxDistanceToStopFlee = 5f;
    public float criticalDistanceToPlayer = 1.5f;
    public float optimalDistanceToPlayer = 4f;
    public bool isFleeing = false;
    public bool criticalFleeState = false;
    public bool isTrapped = false;
    public bool isInOptimalDistance = false;
    public bool isTeletransportingToFlee = false;

    [Header("Attack settings")]
    [HideInInspector] public int facingDirection = -1;
    public int attackIndex = 5;

    [Header("Teletransport settings")]
    public float teleportYOffset = 3f;
    public float fallImpactThreshold = -2f;
    public bool isFallingFromTeleport = false;
    public bool isInvisible = false;

    [Header("Confiner Awareness")]
    public LayerMask confinerWallMask;

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

    [Header("Spawn Settings")]
    [SerializeField] private Vector3 defaultScale = new Vector3(-1, 1, 1);
    private bool justSpawned = true;

    private void OnEnable()
    {
        transform.localScale = defaultScale;
        justSpawned = true;

        lockFacing = true;
        lookAtPlayer = false;
    }

    protected override void Awake()
    {
        base.Awake();
        
        if (player == null)
        {
            var pGo = GameObject.FindGameObjectWithTag("Player");
            if (pGo != null) player = pGo.transform;
        }

        if (justSpawned)
        {
            transform.localScale = defaultScale;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (punchCollider != null) punchCollider.SetActive(false);

        if (characterHealth == null)
        {
            characterHealth = GetComponent<CharacterHealth>();
        }

        if (characterHealth != null)
        {
            characterHealth.OnDeath += HandleCharacterDeath;
            characterHealth.isInvincible = true;
        }
        
        if (!dialogueFinished)
        {
            rb.bodyType = RigidbodyType2D.Static;
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
        // Cambiar a estado de muerte
        if (DeathState != null && StateMachine != null)
        {
            StateMachine.ChangeState(DeathState);
        }

        // Liberar confiner/zona boss
        if (bossTriggerZone != null)
        {
            bossTriggerZone.OnBossDefeated();
        }

        // Detener movimiento y desactivar collider de ataque
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
        
        // Si está invisible, hacerlo visible para la animación de muerte
        if (isInvisible)
        {
            HideMonje(false);
        }
        
        // Iniciar secuencia de muerte con efectos
        if (deathEffectHandler != null)
        {
            deathEffectHandler.TriggerDeathSequence();
        }
        else
        {
            // Fallback: destruir después de la animación de muerte
            Destroy(gameObject, 3f); // Más tiempo para un boss
        }
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

        StateMachine.Initialize(IdleState);

        if (playerRef == null && player != null)
        {
            playerRef = player.GetComponent<PlayerStateMachine>();
        }
        if(player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player").transform;
        }
        StartCoroutine(AllowFlipAfterSpawn());
        transform.localScale = defaultScale;
    }

    private IEnumerator AllowFlipAfterSpawn()
    {
        yield return new WaitForSeconds(0.5f);
        justSpawned = false;
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

        if (characterHealth != null && characterHealth.currentHealth <= characterHealth.maxHealth * 0.5f) //si tiene menos del 50% de vida
        {
            if (LowHealthPrefab != null && LowHealthPrefabSpawnPoint != null && !LowHealthPrefabInstantiated)
            {
                //instanciar el prefab como hijo del spawnPoint
                Instantiate(LowHealthPrefab, LowHealthPrefabSpawnPoint.transform.position, Quaternion.identity, LowHealthPrefabSpawnPoint.transform);
                LowHealthPrefabInstantiated = true;
                Debug.LogWarning("Monje: LowHealthPrefab instantiated.");

            }
        }
    }

    public void Flip()
    {
        if (lockFacing || player == null || justSpawned) return;

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

    public void OnDialogueEnd()
    {
        // Implementar si es necesario
    }

    private bool IsGrounded()
    {
        RaycastHit2D hit1 = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckRadius, groundLayer);
        RaycastHit2D hit2 = Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckRadius, playerLayer);

        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckRadius, Color.red);
        Debug.DrawRay(groundCheckPoint.position, Vector2.down * groundCheckRadius, Color.blue);

        return hit1.collider != null || hit2.collider != null;
    }

    public bool HasToFlee()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, maxDistanceToStopFlee);
        Debug.DrawRay(transform.position, (player.position - transform.position).normalized * maxDistanceToStopFlee, Color.green);

        RaycastHit2D hit2 = Physics2D.Raycast(transform.position, (player.position - transform.position).normalized, minDistanceToFlee);
        Debug.DrawRay(transform.position, (player.position - transform.position).normalized * minDistanceToFlee, Color.yellow);

        float horizontalDistance = Mathf.Abs(transform.position.x - player.position.x);
        if (horizontalDistance < minDistanceToFlee)
        {
            isFleeing = true;
            return isFleeing;
        }
        else if (horizontalDistance < maxDistanceToStopFlee && isFleeing)
        {
            isFleeing = true;
            return isFleeing;
        }
        else
        {
            isFleeing = false;
            return isFleeing;
        }
    }

    public bool CriticalFleeState()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < criticalDistanceToPlayer)
        {
            criticalFleeState = true;
            return true;
        }

        criticalFleeState = false;
        return false;
    }

    public void Teletransport()
    {
        Instantiate(teletransportParticle, teletransportSpawnPoint.transform.position, Quaternion.identity);
        HideMonje(true);

        Vector3 newPosition = player.position;
        newPosition.y += teleportYOffset;
        transform.position = newPosition;

        isFallingFromTeleport = true;
        rb.linearVelocity = new Vector2(0, -20);

        Instantiate(teletransportParticle, teletransportSpawnPoint.transform.position, Quaternion.identity);
        HideMonje(false);
    }

    public void TeletransportToFlee()
    {
        HideMonje(true);

        float fleeDirection;

        if (IsNearWall())
        {
            fleeDirection = transform.localScale.x > 0 ? -1f : 1f;
        }
        else
        {
            fleeDirection = player.position.x > transform.position.x ? -1f : 1f;
        }

        Vector3 newPosition = new Vector3(transform.position.x + fleeDirection * maxDistanceToStopFlee, transform.position.y, transform.position.z);

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
        foreach (Transform child in transform)
        {
            if (child.name == "Mesh")
            {
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
        if (punchCollider != null)
        {
            punchCollider.SetActive(true);
            Debug.Log("Punch collider activated");
        }
        
        if (cameraShake != null)
        {
            cameraShake.Shake(8f, 5f, 1f);
        }
        
        if (monjeAudioSource != null && TeletransportImpactSound != null)
        {
            monjeAudioSource.PlayOneShot(TeletransportImpactSound);
        }
    }

    public void OnTeletransportAttackImpactEnd()
    {
        if (punchCollider != null)
        {
            punchCollider.SetActive(false);
        }
        animationFinished = true;
        isTeletransportingToFlee = false;
    }

    public void ThrowGas()
    {
        GameObject gasBall = Instantiate(gasPrefab, throwingGasSpawnPoint.position, Quaternion.identity);
        Rigidbody2D gasRb = gasBall.GetComponent<Rigidbody2D>();
        if (gasRb != null)
        {
            float throwForce = 25f;
            Vector2 throwDirection = facingRight ? Vector2.right : Vector2.left;
            gasRb.linearVelocity = throwDirection * throwForce;
        }
    }

    public void OnThrowGasEnd()
    {
        animationFinished = true;
    }

    public void OnThrowRayShakeCam()
    {
        cameraShake.Shake(8f, 5f, 0.5f);
    }

    public void OnRayImpactShakeCam()
    {
        cameraShake.Shake(8f, 8f, 2f);
    }

    public void OnThrowRay()
    {
        rayManager.ThrowRaysRoutine();
    }

    public void OnThrowRayEnd()
    {
        animationFinished = true;
    }

    public bool IsNearWall()
    {
        Vector2 forward = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, forward, 1f, confinerWallMask);
        Debug.DrawRay(transform.position, forward * 2f, Color.cyan);
        return hit.collider != null;
    }

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
        // Gestionado por CharacterHealth
    }
}