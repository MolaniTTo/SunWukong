using System.Collections;
using UnityEngine;

public class EnemyTiger : EnemyBase
{
    [Header("Tiger Settings")]
    public float detectionRange = 8f; // Rango de detección del jugador
    public float attackRange = 2f; // Rango de ataque cuerpo a cuerpo
    public LayerMask playerLayer; // Capa del jugador
    public bool facingRight = true; // Dirección hacia donde mira el tigre
    public Animator animator; // Referencia al animator
    public CharacterHealth characterHealth; // Referencia al componente de vida

    [Header("Movement Settings")]
    public float walkSpeed = 2f; // Velocidad de patrulla
    public float runSpeed = 5f; // Velocidad de persecución
    public float patrolDistance = 5f; // Distancia que patrulla a cada lado
    private Vector2 startPosition; // Posición inicial del tigre
    private bool movingRight = true; // Dirección de patrulla
    
    [Header("Raycast Settings")]
    public Transform rayOrigin; // Origen del raycast (para detección)
    public float rayLength = 8f; // Longitud del raycast

    [Header("Ground & Wall Detection")]
    public Transform groundCheck; // Punto para detectar suelo
    public Transform wallCheck; // Punto para detectar paredes
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer; // Capa del suelo/paredes

    [Header("Combat Settings")]
    public float attackDamage = 15f; // Daño del ataque
    public float attackCooldown = 1.5f; // Tiempo entre ataques
    private float lastAttackTime = 0f;

    [Header("PlayerRef")]
    public PlayerStateMachine playerRef;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip InRangeSound; // Sonido cuando detecta al jugador
    public AudioClip OutOfRangeSound; // Sonido cuando pierde al jugador
    public AudioClip AttackSound; // Sonido de ataque
    public AudioClip DeathSound; // Sonido de muerte
    public AudioClip HurtSound; // Sonido al recibir daño
    public AudioClip WalkSound; // Sonido de caminar (opcional)
    public AudioClip RunSound; // Sonido de correr (opcional)

    private Rigidbody2D rb;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (characterHealth == null)
            characterHealth = GetComponent<CharacterHealth>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (characterHealth != null)
        {
            characterHealth.OnDeath += Death;
            characterHealth.OnTakeDamage += (currentHealth, attacker) => Damaged();
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
            player = playerObj.transform;
    }

    private void OnDestroy()
    {
        if (characterHealth != null)
        {
            characterHealth.OnDeath -= Death;
            characterHealth.OnTakeDamage -= (currentHealth, attacker) => Damaged();
        }
    }

    private void Start()
    {
        startPosition = transform.position; // Guardar posición inicial
        if (facingRight == false) {
            Flip();  // Asegura que el tigre empiece mirando hacia la derecha
        }
        var idleState = new TigerIdle(this);
        StateMachine.Initialize(idleState);
        if(playerRef == null)
        {
            playerRef = FindAnyObjectByType<PlayerStateMachine>();
        }
    }

    private void Death()
    {
        animator.SetTrigger("Death");
        if (audioSource != null && DeathSound != null)
        {
            audioSource.PlayOneShot(DeathSound);
        }
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero; // Detener movimiento
        }
    }

    private void Damaged()
    {
        animator.SetTrigger("BeingHit");
        if (audioSource != null && HurtSound != null)
        {
            audioSource.PlayOneShot(HurtSound);
        }
    }

    public bool CheckIfPlayerIsDead()
    {
        return playerRef.isDead;
    }

    // Método para reproducir sonido cuando detecta al jugador
    public void PlayInRangeSound()
    {
        if (audioSource != null && InRangeSound != null)
        {
            audioSource.PlayOneShot(InRangeSound);
        }
    }

    // Método para reproducir sonido cuando pierde al jugador
    public void PlayOutOfRangeSound()
    {
        if (audioSource != null && OutOfRangeSound != null)
        {
            audioSource.PlayOneShot(OutOfRangeSound);
        }
    }

    // MÉTODO PARA GIRAR EL TIGRE
    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // Sincronizar dirección de movimiento con la dirección visual
    public void SyncMovementDirection()
    {
        movingRight = facingRight;
    }

    // MÉTODOS COMUNES DE LOS ENEMIGOS
    public override bool CanSeePlayer()
    {
        if (player == null) return false;

        // Distancia al jugador
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer > detectionRange) return false;

        // Dirección del raycast según hacia dónde mire el tigre
        Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left;
        
        // Detectar si el jugador está en la dirección correcta
        Vector2 dirToPlayer = (player.position - transform.position).normalized;
        float dot = Vector2.Dot(forwardDir, dirToPlayer);

        // Raycast para detectar al jugador
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

    public override void Move()
    {
        // Este método se usa desde los estados para mover al tigre
    }

    public void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;

        Vector2 direction = (player.position - transform.position).normalized;
        
        // Asegurarse de que mira hacia el jugador
        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
        {
            Flip();
        }

        // IMPORTANTE: Detectar si hay suelo delante antes de moverse
        Vector2 frontDirection = facingRight ? Vector2.right : Vector2.left;
        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (frontDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);

        // Solo moverse si hay suelo delante, sino detenerse
        if (groundHit.collider != null)
        {
            rb.linearVelocity = new Vector2(direction.x * runSpeed, rb.linearVelocity.y);
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
        }
        else
        {
            // No hay suelo delante, detenerse en el borde
            StopMovement();
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", false);
        }
    }

    public void Patrol()
    {
        // Calcular los límites de patrulla basados en la posición inicial
        float leftLimit = startPosition.x - patrolDistance;
        float rightLimit = startPosition.x + patrolDistance;

        // Detectar pared delante del tigre
        Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
        RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, wallDirection, wallCheckDistance, groundLayer);
        
        // Detectar si hay suelo delante (para evitar caer)
        Vector2 frontGroundCheck = (Vector2)groundCheck.position + (wallDirection * 0.5f);
        RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);

        // Cambiar dirección si:
        // 1. Llega a los límites de patrulla
        // 2. Detecta una pared
        // 3. No hay suelo delante (borde de plataforma)
        if (transform.position.x <= leftLimit || (wallHit.collider != null && !movingRight))
        {
            movingRight = true;
            if (!facingRight) Flip();
        }
        else if (transform.position.x >= rightLimit || (wallHit.collider != null && movingRight))
        {
            movingRight = false;
            if (facingRight) Flip();
        }

        // Cambiar dirección si no hay suelo delante
        if (movingRight && groundHit.collider == null)
        {
            movingRight = false;
            if (facingRight) Flip();
        }
        else if (!movingRight && groundHit.collider == null)
        {
            movingRight = true;
            if (!facingRight) Flip();
        }

        // Moverse en la dirección actual
        float direction = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * walkSpeed, rb.linearVelocity.y);
        
        animator.SetBool("isWalking", true);
        animator.SetBool("isRunning", false);
    }

    public void StopMovement()
    {
        if (rb != null)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
        animator.SetBool("isWalking", false);
        animator.SetBool("isRunning", false);
    }

    public override void Attack()
    {
        // Este método se llama desde el evento de animación
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, gameObject);
            }
        }

        // Reproducir sonido de ataque
        if (audioSource != null && AttackSound != null)
        {
            audioSource.PlayOneShot(AttackSound);
        }
    }

    public bool CanAttack()
    {
        return Time.time - lastAttackTime >= attackCooldown;
    }

    public void StartAttack()
    {
        lastAttackTime = Time.time;
        StopMovement();
        animator.SetTrigger("Attack");
    }

    public override void Die()
    {
        // No implementado ya que la muerte se gestiona desde el componente CharacterHealth
        throw new System.NotImplementedException();
    }

    // Métodos para visualizar rangos en el editor
    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    private void OnDrawGizmos()
    {
        if (wallCheck != null)
        {
            Vector2 wallDirection = facingRight ? Vector2.right : Vector2.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + (Vector3)(wallDirection * wallCheckDistance));
        }
        
        if (groundCheck != null)
        {
            Vector2 frontCheck = facingRight ? Vector2.right : Vector2.left;
            Vector3 checkPos = groundCheck.position + (Vector3)(frontCheck * 0.5f);
            Gizmos.color = Color.green;
            Gizmos.DrawLine(checkPos, checkPos + Vector3.down);
        }
    }
}