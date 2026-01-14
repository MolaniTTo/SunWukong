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
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;

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
        animator.SetTrigger("Die");

        StopHissSound();

        if (audioSource != null && DeathSound != null)
            audioSource.PlayOneShot(DeathSound);

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }

        // Deshabilitar colisiones
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;

        if (biteCollider != null)
            biteCollider.SetActive(false);

        // Iniciar secuencia de muerte con efectos
        if (deathEffectHandler != null)
        {
            deathEffectHandler.TriggerDeathSequence();
        }
        else
        {
            // Fallback: destruir después de un tiempo
            Destroy(gameObject, 2f);
        }
    }

    private void Damaged(float currentHealth, GameObject attacker)
    {
        if (isDead) return;

        isAttacking = false;
        if (biteCollider != null) 
            biteCollider.SetActive(false);

        animator.ResetTrigger("Attack");
        animator.SetTrigger("Damaged");

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

        // Verificar si el jugador está muerto
        if (CheckIfPlayerIsDead()) return false;

        Vector2 origenConOffset = (Vector2)transform.position + rayOffset;
        Vector2 direccion = facingRight ? Vector2.right : Vector2.left;

        // Primero verificar distancia
        float distanceToPlayer = Vector2.Distance(origenConOffset, player.position);
        if (distanceToPlayer > detectionRange) return false;

        // Verificar dirección del jugador
        Vector2 dirToPlayer = (player.position - (Vector3)origenConOffset).normalized;
        float dot = Vector2.Dot(direccion, dirToPlayer);

        // Raycast para detectar al jugador
        RaycastHit2D hit = Physics2D.Raycast(origenConOffset, dirToPlayer, detectionRange, playerLayer);

        if (hit.collider != null && hit.collider.CompareTag("Player"))
        {
            // Si el jugador está detrás, girar
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

        Vector2 direction = (player.position - transform.position).normalized;

        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            Flip();
        }

        Vector2 frontDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (frontDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);

        if (groundHit.collider != null)
        {
            rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

            if (!isAttacking)
            {
                animator.SetBool("isChasing", true);
                animator.SetBool("isMoving", false);
            }
        }
        else
        {
            StopMovement();
        }
    }

    public void Patrol()
    {
        if (isDead) return;

        float leftLimit = startPosition.x - patrolDistance;
        float rightLimit = startPosition.x + patrolDistance;

        Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, wallDirection, wallCheckDistance, groundLayer);

        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (wallDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);

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

        if (IsPlayerInAttackRange())
        {
            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage, gameObject);
        }

        if (audioSource != null && AttackSound != null)
            audioSource.PlayOneShot(AttackSound);
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown && !isAttacking && !isDead;
    }

    public void StartAttack()
    {
        if (isAttacking || isDead) return;

        isAttacking = true;
        lastAttackTime = Time.time;

        animator.SetBool("isMoving", false);
        animator.SetBool("isChasing", false);

        animator.SetTrigger("Attack");
    }

    public void OnBiteImpact()
    {
        if (isDead) return;
        
        if (biteCollider != null)
            biteCollider.SetActive(true);
        Attack();
    }

    public void OnBiteImpactEnd()
    {
        if (biteCollider != null)
            biteCollider.SetActive(false);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
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
            Vector3 wallDir = facingRight ? Vector3.right : Vector3.left;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + wallDir * wallCheckDistance);
        }
    }
}