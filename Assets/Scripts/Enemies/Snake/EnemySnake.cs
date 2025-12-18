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
    public bool facingRight = false; // sprite mira a la izquierda por defecto
    public Animator animator;
    public CharacterHealth characterHealth;

    [Header("Bone Reference")]
    public Transform bone_1;

    [Header("Raycast Settings")]
    public Transform rayOrigin;
    public float rayLength = 8f;

    [Header("Ground Detection")]
    public Transform groundCheck;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float patrolDistance = 5f;
    public Transform pointA;
    public Transform pointB;
    private Vector2 startPosition;
    private bool movingRight = false; // comienza hacia la izquierda

    public bool MovingRight => movingRight;

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
    public AudioClip InRangeSound;
    public AudioClip OutOfRangeSound;
    public AudioClip AttackSound;
    public AudioClip DeathSound;
    public AudioClip HurtSound;

    private Rigidbody2D rb;
    private Transform player;
    private bool isAttacking = false;

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
        startPosition = transform.position; // posición inicial

        // comenzar moviéndose a la izquierda (sin Flip)
        movingRight = false;

        PatrolState = new SerpientePatrol(this);
        ChaseState = new SerpienteChase(this);
        AttackState = new SerpienteAttack(this);
        DeathState = new SerpienteDeath(this);

        StateMachine.Initialize(PatrolState);
    }

    protected override void Update()
    {
        StateMachine.Update();
    }

    private void Death()
    {
        animator.SetTrigger("Die");

        if (audioSource != null && DeathSound != null)
            audioSource.PlayOneShot(DeathSound);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Damaged(float currentHealth, GameObject attacker)
    {
        animator.SetTrigger("Damaged");

        if (audioSource != null && HurtSound != null)
            audioSource.PlayOneShot(HurtSound);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
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
        scale.y *= -1; // solo flipear Y, nunca X
        bone_1.localScale = scale;
    }

    public void SyncMovementDirection()
    {
        movingRight = facingRight;
    }

    public void PlayInRangeSound()
    {
        if (audioSource != null && InRangeSound != null)
            audioSource.PlayOneShot(InRangeSound);
    }

    public void PlayOutOfRangeSound()
    {
        if (audioSource != null && OutOfRangeSound != null)
            audioSource.PlayOneShot(OutOfRangeSound);
    }

public override bool CanSeePlayer()
{
    if (player == null) return false;

    float distanceToPlayer = Vector2.Distance(transform.position, player.position);

    if (distanceToPlayer > detectionRange) return false;

    Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left;

    Vector2 dirToPlayer = (player.position - transform.position).normalized;
    float dot = Vector2.Dot(forwardDir, dirToPlayer);

    RaycastHit2D hit = Physics2D.Raycast(rayOrigin.position, dirToPlayer, detectionRange, playerLayer);

    Debug.DrawRay(rayOrigin.position, dirToPlayer * detectionRange, Color.red);

    if (hit.collider != null && hit.collider.CompareTag("Player"))
    {
        // !ELIMINAR Flip() aquí
        // if (dot < 0) Flip();
        return true;
    }

    return false;
}


    public bool IsPlayerInAttackRange()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    public override void Move() { }

    // --- Movimiento hacia jugador sin moonwalk ---
    public void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        // No activar la animación de moverse si estamos atacando
        if (!isAttacking)
        {
            animator.SetBool("isChasing", true);
            animator.SetBool("isMoving", false);
        }
    }



    public void Patrol()
    {
        float leftLimit = pointA != null && pointB != null ? Mathf.Min(pointA.position.x, pointB.position.x) : startPosition.x - patrolDistance;
        float rightLimit = pointA != null && pointB != null ? Mathf.Max(pointA.position.x, pointB.position.x) : startPosition.x + patrolDistance;

        if (transform.position.x <= leftLimit)
        {
            movingRight = true;
            if (!facingRight) Flip();
        }
        else if (transform.position.x >= rightLimit)
        {
            movingRight = false;
            if (facingRight) Flip();
        }

        float direction = movingRight ? 1f : -1f;
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
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
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
        return Time.time - lastAttackTime >= attackCooldown && !isAttacking;
    }

    public void StartAttack()
    {
        if (isAttacking) return;

        isAttacking = true;
        lastAttackTime = Time.time;
        StopMovement();

        animator.SetBool("isMoving", false);
        animator.SetBool("isChasing", false);
        animator.SetTrigger("Attack");
    }

    public void OnBiteImpact()
    {
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
        if (biteCollider != null)
            biteCollider.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void OnDrawGizmos()
    {
        if (rayOrigin != null)
        {
            Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + (Vector3)(forwardDir * rayLength));
        }
    }
}
