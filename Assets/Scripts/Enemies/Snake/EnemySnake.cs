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

    [Header("Ground & Wall Detection")]
    public Transform groundCheck; // Punto para detectar suelo
    public Transform wallCheck; // Punto para detectar paredes
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer; // Capa del suelo/paredes

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public float patrolDistance = 5f; // Distancia máxima de patrulla desde posición inicial
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
    public AudioClip HissSound; // Sonido de siseo (patrulla y persecución)
    public AudioClip AttackSound; // Sonido de mordisco
    public AudioClip DeathSound; // Sonido de muerte

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
        startPosition = transform.position;

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

        StopHissSound();

        if (audioSource != null && DeathSound != null)
            audioSource.PlayOneShot(DeathSound);

        if (rb != null)
            rb.linearVelocity = Vector2.zero;
    }

    private void Damaged(float currentHealth, GameObject attacker)
    {
        animator.SetTrigger("Damaged");
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
        scale.y *= -1; // solo flipear Y
        bone_1.localScale = scale;
    }

    // Reproducir sonido de siseo en loop
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

    // Detener sonido de siseo
    public void StopHissSound()
    {
        if (audioSource != null && audioSource.clip == HissSound)
        {
            audioSource.Stop();
        }
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
        if (player == null) return false;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    public override void Move() { }

    public void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;

        Vector2 direction = (player.position - transform.position).normalized;

        // Asegurarse de que mira hacia el jugador
        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            Flip();
        }

        // Detectar si hay suelo delante antes de moverse
        Vector2 frontDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (frontDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);

        // Solo moverse si hay suelo delante
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
            // No hay suelo delante, detenerse
            StopMovement();
        }
    }

    public void Patrol()
    {
        // Calcular límites de patrulla basados en la posición inicial
        float leftLimit = startPosition.x - patrolDistance;
        float rightLimit = startPosition.x + patrolDistance;

        // Detectar pared delante
        Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, wallDirection, wallCheckDistance, groundLayer);

        // Detectar si hay suelo delante (para evitar caer)
        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (wallDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);

        // LÓGICA CORREGIDA: Primero determinar si necesita girar, luego hacer Flip UNA SOLA VEZ
        bool needsToFlip = false;

        // Verificar límites de patrulla
        if (transform.position.x <= leftLimit && !facingRight)
        {
            needsToFlip = true; // Necesita mirar a la derecha
        }
        else if (transform.position.x >= rightLimit && facingRight)
        {
            needsToFlip = true; // Necesita mirar a la izquierda
        }
        // Verificar pared
        else if (wallHit.collider != null)
        {
            needsToFlip = true; // Necesita girar
        }
        // Verificar borde (no hay suelo delante)
        else if (groundHit.collider == null)
        {
            needsToFlip = true; // Necesita girar
        }

        // Si necesita girar, hacer Flip una sola vez
        if (needsToFlip)
        {
            Flip();
        }

        // MOVIMIENTO: Usar facingRight como fuente de verdad
        // facingRight = false → mover a la izquierda (-1)
        // facingRight = true → mover a la derecha (+1)
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
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage, gameObject);
        }

        // Reproducir sonido de mordisco
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
        StopHissSound();
        if (biteCollider != null)
            biteCollider.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Mostrar límites de patrulla
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
        // Raycast de detección del jugador
        if (rayOrigin != null)
        {
            Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(rayOrigin.position, rayOrigin.position + (Vector3)(forwardDir * rayLength));
        }

        // Raycast de detección de pared
        if (wallCheck != null)
        {
            Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.magenta;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)(wallDirection * wallCheckDistance));
        }

        // Raycast de detección de suelo
        if (groundCheck != null)
        {
            Vector2 frontCheck = facingRight ? Vector2.right : Vector2.left;
            Vector3 checkPos = groundCheck.position + (Vector3)(frontCheck * 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(checkPos, checkPos + Vector3.down);
        }
    }
}