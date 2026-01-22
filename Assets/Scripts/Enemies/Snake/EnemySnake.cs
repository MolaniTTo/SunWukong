using System.Collections;
using UnityEngine;

public class EnemySnake : EnemyBase
{
    [Header("States")]
    public SerpientePatrol PatrolState { get; private set; }
    public SerpienteChase ChaseState { get; private set; }
    public SerpienteAttack AttackState { get; private set; }
    public SerpienteDeath DeathState { get; private set; }

    [Header("Snake Settings")]
    public float detectionRange = 8f;
    public float attackRange = 0.8f;
    public LayerMask playerLayer;
    public bool facingRight = false;
    public Animator animator;
    public CharacterHealth characterHealth;

    [Header("Bone Reference")]
    public Transform bone_1;

    [Header("Raycast Settings")]
    public Transform rayOrigin;
    public float rayLength = 8f;
    public Vector2 rayOffset = new Vector2(0f, 0.4f);

    [Header("Ground & Wall Detection")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float wallCheckDistance = 1f;
    public float groundCheckDistance = 1f;
    public LayerMask groundLayer;
    public LayerMask wallLayer;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float patrolDistance = 5f;
    private Vector2 startPosition;

    [Header("Combat Settings")]
    public float attackDamage = 15f;
    public float attackCooldown = 1.5f;
    public float contactDamage = 10f;
    public float contactDamageCooldown = 1f;
    private float lastContactDamageTime = -999f;
    private float lastAttackTime = 0f;
    private float attackAnimationDuration = 1f;
    private float attackEndTime = -999f;
    public GameObject biteCollider;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip HissSound;
    public AudioClip AttackSound;
    public AudioClip DeathSound;
    public AudioClip HurtSound;

    [Header("Efecto de muerte Script")]
    public DeathEffectHandler deathEffectHandler;

    [Header("PlayerRef")]
    public PlayerStateMachine playerRef;

    [Header("Debug")]
    public bool enableDebug = true;

    private Rigidbody2D rb;
    private Transform player;
    private bool isAttacking = false;
    private bool isDead = false;

    public Transform Player => player;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        }

        if (animator == null)
            animator = GetComponent<Animator>();

        if (characterHealth == null)
            characterHealth = GetComponent<CharacterHealth>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (bone_1 == null)
        {
            bone_1 = transform.Find("SERPIENTE/bone_1");
            if (bone_1 == null)
            {
                Debug.LogWarning("EnemySnake: No se encontró bone_1");
            }
        }

        if (characterHealth != null)
        {
            characterHealth.OnDeath += Death;
            characterHealth.OnTakeDamage += Damaged;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;

        if (biteCollider != null)
            biteCollider.SetActive(false);
    }

    private void OnDestroy()
    {
        if (characterHealth != null)
        {
            characterHealth.OnDeath -= Death;
            characterHealth.OnTakeDamage -= Damaged;
        }
    }

    private void Start()
    {
        startPosition = transform.position;

        PatrolState = new SerpientePatrol(this);
        ChaseState = new SerpienteChase(this);
        AttackState = new SerpienteAttack(this);
        DeathState = new SerpienteDeath(this);

        StateMachine.Initialize(PatrolState);

        if (playerRef == null)
        {
            playerRef = FindAnyObjectByType<PlayerStateMachine>();
        }

        if (enableDebug) Debug.Log("[SNAKE] Initialized in PATROL state");
    }

    protected override void Update()
    {
        if (!isDead)
        {
            StateMachine.Update();
        }
    }

    private void Death()
    {
        if (isDead) return;
        
        isDead = true;
        if (enableDebug) Debug.Log("[SNAKE] DEATH triggered");
        
        animator.SetTrigger("Die");

        StopHissSound();

        if (audioSource != null && DeathSound != null)
            audioSource.PlayOneShot(DeathSound);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (biteCollider != null)
            biteCollider.SetActive(false);

        if (deathEffectHandler != null)
        {
            deathEffectHandler.TriggerDeathSequence();
        }
        else
        {
            Destroy(gameObject, 2f);
        }
    }

    private void Damaged(float currentHealth, GameObject attacker)
    {
        if (isDead) return;

        if (enableDebug) Debug.Log($"[SNAKE] DAMAGED - isAttacking before: {isAttacking}");

        ForceStopAttack();

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Damaged");

        if (enableDebug) Debug.Log($"[SNAKE] DAMAGED - isAttacking after: {isAttacking}");

        if (audioSource != null && HurtSound != null)
        {
            audioSource.PlayOneShot(HurtSound);
        }
    }

    public bool CheckIfPlayerIsDead()
    {
        if (playerRef == null) return false;
        return playerRef.isDead;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (isDead) return;
        if (!collision.gameObject.CompareTag("Player")) return;

        if (Time.time >= lastContactDamageTime + contactDamageCooldown)
        {
            CharacterHealth playerHealth = collision.gameObject.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage, gameObject);
                lastContactDamageTime = Time.time;
                if (enableDebug) Debug.Log("[SNAKE] Contact damage dealt");
            }
        }
    }

    public void Flip()
    {
        if (bone_1 == null) return;

        facingRight = !facingRight;

        Vector3 scale = bone_1.localScale;
        scale.y *= -1;
        bone_1.localScale = scale;

        if (rayOrigin != null)
        {
            Vector3 pos = rayOrigin.localPosition;
            pos.x *= -1;
            rayOrigin.localPosition = pos;
        }

        if (enableDebug) Debug.Log($"[SNAKE] Flipped - facingRight: {facingRight}");
    }

    public void PlayHissSound()
    {
        if (audioSource != null && HissSound != null)
        {
            if (!audioSource.isPlaying || audioSource.clip != HissSound)
            {
                audioSource.clip = HissSound;
                audioSource.loop = true;
                audioSource.Play();
            }
        }
    }

    public void StopHissSound()
    {
        if (audioSource != null && audioSource.clip == HissSound)
        {
            audioSource.Stop();
        }
    }

    public override bool CanSeePlayer()
    {
        if (player == null || isDead) return false;

        if (CheckIfPlayerIsDead()) return false;

        Vector2 origenConOffset = (Vector2)transform.position + rayOffset;
        Vector2 direccion = facingRight ? Vector2.right : Vector2.left;

        float distanceToPlayer = Vector2.Distance(origenConOffset, player.position);
        if (distanceToPlayer > detectionRange) return false;

        Vector2 dirToPlayer = (player.position - (Vector3)origenConOffset).normalized;
        float dot = Vector2.Dot(direccion, dirToPlayer);

        RaycastHit2D hit = Physics2D.Raycast(origenConOffset, dirToPlayer, detectionRange, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            if (dot < 0)
            {
                Flip();
            }
            return true;
        }

        return false;
    }

    public bool IsPlayerInAttackRange()
    {
        if (player == null || isDead) return false;
        if (CheckIfPlayerIsDead()) return false;

        Vector2 centroCuerpo = (Vector2)transform.position + rayOffset;
        float distanceToPlayer = Vector2.Distance(centroCuerpo, player.position);
        return distanceToPlayer <= attackRange;
    }

    public override void Move() { }

    public void MoveTowardsPlayer()
    {
        if (player == null || rb == null || isDead) return;

        Vector2 frontDirection = facingRight ? Vector2.right : Vector2.left;
        
        RaycastHit2D wallHit = Physics2D.Raycast(
            wallCheck.position,
            frontDirection,
            wallCheckDistance,
            wallLayer
        );

        if (wallHit.collider != null)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isChasing", false);
            animator.SetBool("isMoving", false);
            return;
        }

        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (frontDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);

        if (groundHit.collider == null)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("isChasing", false);
            animator.SetBool("isMoving", false);
            return;
        }

        Vector2 direction = (player.position - transform.position).normalized;

        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            Flip();
        }

        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        if (!isAttacking)
        {
            animator.SetBool("isChasing", true);
            animator.SetBool("isMoving", false);
        }
    }

    public void Patrol()
    {
        if (isDead) return;

        float leftLimit = startPosition.x - patrolDistance;
        float rightLimit = startPosition.x + patrolDistance;

        Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(
            wallCheck.position,
            wallDirection,
            wallCheckDistance,
            wallLayer
        );

        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (wallDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(
            frontGroundCheck,
            Vector2.down,
            1f,
            groundLayer
        );

        bool needsToFlip = false;

        if (transform.position.x <= leftLimit && !facingRight) needsToFlip = true;
        else if (transform.position.x >= rightLimit && facingRight) needsToFlip = true;
        else if (wallHit.collider != null) needsToFlip = true;
        else if (groundHit.collider == null) needsToFlip = true;

        if (needsToFlip) Flip();

        float direction = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * patrolSpeed, rb.linearVelocity.y);

        animator.SetBool("isMoving", true);
        animator.SetBool("isChasing", false);
    }

    public void StopMovement()
    {
        if (rb != null)
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        animator.SetBool("isMoving", false);
        animator.SetBool("isChasing", false);
    }

    public override void Attack()
    {
        if (player == null || isDead) return;

        if (enableDebug) Debug.Log("[SNAKE] Attack() called");

        if (IsPlayerInAttackRange())
        {
            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, gameObject);
                if (enableDebug) Debug.Log("[SNAKE] Attack damage dealt to player");
            }
        }
        else
        {
            if (enableDebug) Debug.LogWarning("[SNAKE] Attack() called but player NOT in range!");
        }

        if (audioSource != null && AttackSound != null)
            audioSource.PlayOneShot(AttackSound);
    }

    public bool CanAttack()
    {
        float timeSinceLastAttack = Time.time - lastAttackTime;
        float timeSinceAttackEnd = Time.time - attackEndTime;
        
        bool cooldownReady = timeSinceLastAttack >= attackCooldown;
        bool notCurrentlyAttacking = !isAttacking;
        bool animationFinished = timeSinceAttackEnd >= 0.2f;
        bool notDead = !isDead;
        
        bool canAttack = cooldownReady && notCurrentlyAttacking && animationFinished && notDead;
        
        if (enableDebug && !canAttack)
        {
            Debug.Log($"[SNAKE] CanAttack = FALSE | " +
                      $"TimeSinceLastAttack: {timeSinceLastAttack:F2} (need {attackCooldown}) | " +
                      $"TimeSinceAttackEnd: {timeSinceAttackEnd:F2} | " +
                      $"isAttacking: {isAttacking} | " +
                      $"isDead: {isDead}");
        }
        
        return canAttack;
    }

    public void StartAttack()
    {
        if (isAttacking || isDead)
        {
            if (enableDebug) Debug.LogWarning($"[SNAKE] StartAttack BLOCKED - isAttacking: {isAttacking} | isDead: {isDead}");
            return;
        }

        if (Time.time - lastAttackTime < attackCooldown)
        {
            if (enableDebug) Debug.LogWarning($"[SNAKE] StartAttack BLOCKED - Cooldown not ready: {Time.time - lastAttackTime:F2}/{attackCooldown}");
            return;
        }

        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetBool("isChasing", false);
        animator.SetTrigger("Attack");

        if (enableDebug) Debug.Log($"[SNAKE] StartAttack SUCCESS - Time: {Time.time:F2}");
    }

    public void OnBiteImpact()
    {
        if (isDead) return;
        
        if (enableDebug) Debug.Log($"[SNAKE] OnBiteImpact - Time: {Time.time:F2}");
        
        if (biteCollider != null)
            biteCollider.SetActive(true);
        
        Attack();
    }

    public void OnBiteImpactEnd()
    {
        if (enableDebug) Debug.Log($"[SNAKE] OnBiteImpactEnd - Time: {Time.time:F2}");
        
        if (biteCollider != null)
            biteCollider.SetActive(false);
    }

    public void OnAttackEnd()
    {
        if (enableDebug) Debug.Log($"[SNAKE] OnAttackEnd - Animation Event - Time: {Time.time:F2} | isAttacking: {isAttacking}");
        
        attackEndTime = Time.time;
        
        if (isAttacking)
        {
            isAttacking = false;
            if (enableDebug) Debug.Log($"[SNAKE] OnAttackEnd - isAttacking set to FALSE");
        }
        
        if (biteCollider != null)
            biteCollider.SetActive(false);
    }

    public bool IsCurrentlyAttacking()
    {
        return isAttacking;
    }

    public void ForceStopAttack()
    {
        if (enableDebug) Debug.Log($"[SNAKE] ForceStopAttack called - Time: {Time.time:F2} | isAttacking before: {isAttacking}");
        
        isAttacking = false;
        attackEndTime = Time.time;
        
        if (biteCollider != null) 
            biteCollider.SetActive(false);
        
        animator.ResetTrigger("Attack");
            
        if (enableDebug) Debug.Log($"[SNAKE] ForceStopAttack - isAttacking after: {isAttacking}");
    }

    public bool IsAttackCooldownReady()
    {
        return Time.time - lastAttackTime >= attackCooldown && 
               Time.time - attackEndTime >= 0.2f;
    }

    public override void Die()
    {
        StopMovement();
        StopHissSound();
        if (biteCollider != null)
            biteCollider.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 centroCuerpo = transform.position + (Vector3)rayOffset;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(centroCuerpo, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(centroCuerpo, attackRange);

        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Vector3 leftLimit = new Vector3(startPosition.x - patrolDistance, transform.position.y, 0);
            Vector3 rightLimit = new Vector3(startPosition.x + patrolDistance, transform.position.y, 0);
            Gizmos.DrawLine(leftLimit + Vector3.up * 2, leftLimit - Vector3.up * 2);
            Gizmos.DrawLine(rightLimit + Vector3.up * 2, rightLimit - Vector3.up * 2);
        }
    }

    private void OnDrawGizmos()
    {
        Vector3 origenGizmo = transform.position + (Vector3)rayOffset;

        Vector3 forwardDir = facingRight ? Vector3.right : Vector3.left;

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(origenGizmo, origenGizmo + forwardDir * detectionRange);

        if (wallCheck != null)
        {
            Vector3 wallCheckPos = wallCheck.position + Vector3.down * 0.2f;

            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(
                wallCheckPos,
                wallCheckPos + forwardDir * wallCheckDistance
            );
        }

        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(
                groundCheck.position,
                groundCheck.position + Vector3.down * groundCheckDistance
            );
            
            Gizmos.DrawWireSphere(groundCheck.position, 0.1f);
        }
    }
}