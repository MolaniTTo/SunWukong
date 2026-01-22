using System.Collections;
using UnityEngine;

public class EnemyTiger : EnemyBase
{
    [Header("Tiger Settings")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public LayerMask playerLayer;
    public bool facingRight = true;
    public Animator animator;
    public CharacterHealth characterHealth;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public float patrolDistance = 5f;
    private Vector2 startPosition;
    
    [Header("Raycast Settings")]
    public Transform rayOrigin;
    public float rayLength = 8f;

    [Header("Ground & Wall Detection")]
    public Transform groundCheck;
    public Transform wallCheck;
    public float wallCheckDistance = 0.5f;
    public LayerMask groundLayer;

    [Header("Combat Settings")]
    public float attackDamage = 30f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime = 0f;

    [Header("VFX Settings")]
    public GameObject attackVFXPrefab; // Prefab del efecto de impacto en el objetivo
    public float vfxDestroyDelay = 2f; // Tiempo antes de destruir el VFX
    public Vector3 vfxOffsetOnTarget = Vector3.zero; // Offset del VFX en el objetivo (ej: altura del torso)

    [Header("PlayerRef")]
    public PlayerStateMachine playerRef;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip InRangeSound;
    public AudioClip OutOfRangeSound;
    public AudioClip AttackSound;
    public AudioClip DeathSound;
    public AudioClip HurtSound;
    public AudioClip WalkSound;
    public AudioClip RunSound;

    [Header("Effecto de muerte Script")]
    public DeathEffectHandler deathEffectHandler;
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
        startPosition = transform.position;
        
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
            rb.linearVelocity = Vector2.zero;
            rb.bodyType = RigidbodyType2D.Static;
        }
        
        Collider2D col = GetComponent<Collider2D>();
        if (col != null) col.enabled = false;
        
        if (deathEffectHandler != null)
        {
            deathEffectHandler.TriggerDeathSequence();
        }
        else
        {
            Destroy(gameObject, 2f);
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

    public void PlayInRangeSound()
    {
        if (audioSource != null && InRangeSound != null)
        {
            audioSource.PlayOneShot(InRangeSound);
        }
    }

    public void PlayOutOfRangeSound()
    {
        if (audioSource != null && OutOfRangeSound != null)
        {
            audioSource.PlayOneShot(OutOfRangeSound);
        }
    }

    public void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void SyncMovementDirection()
    {
        // Ya no es necesario, eliminamos la variable movingRight del flujo
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
        if (player == null) return false;
        
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= attackRange;
    }

    public override void Move()
    {
    }

    public void MoveTowardsPlayer()
    {
        if (player == null || rb == null) return;

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
            rb.linearVelocity = new Vector2(direction.x * runSpeed, rb.linearVelocity.y);
            animator.SetBool("isRunning", true);
            animator.SetBool("isWalking", false);
        }
        else
        {
            StopMovement();
            animator.SetBool("isRunning", false);
            animator.SetBool("isWalking", false);
        }
    }

    public void Patrol()
    {
        float leftLimit = startPosition.x - patrolDistance;
        float rightLimit = startPosition.x + patrolDistance;
        float currentX = transform.position.x;

        bool needsFlip = false;
        
        // Si está fuera del límite izquierdo, DEBE mirar a la derecha
        if (currentX < leftLimit && !facingRight)
        {
            needsFlip = true;
        }
        // Si está fuera del límite derecho, DEBE mirar a la izquierda
        else if (currentX > rightLimit && facingRight)
        {
            needsFlip = true;
        }
        else
        {
            // Verificar obstáculos SOLO si está dentro de los límites
            Vector2 moveDirection = facingRight ? Vector2.right : Vector2.left;
            
            // Raycast para suelo delante (evitar caer)
            Vector2 frontGroundCheck = (Vector2)groundCheck.position + (moveDirection * 0.5f);
            RaycastHit2D groundHit = Physics2D.Raycast(frontGroundCheck, Vector2.down, 1f, groundLayer);
            
            // Raycast para pared delante
            RaycastHit2D wallHit = Physics2D.Raycast(wallCheck.position, moveDirection, wallCheckDistance, groundLayer);
            
            // Girar si no hay suelo o hay pared
            if (groundHit.collider == null)
            {
                needsFlip = true;
            }
            else if (wallHit.collider != null)
            {
                // Verificar que la pared esté realmente delante (altura positiva respecto al groundCheck)
                float wallHeight = wallHit.point.y - groundCheck.position.y;
                if (wallHeight > 0.2f)
                {
                    needsFlip = true;
                }
            }
        }
        
        // Ejecutar flip si es necesario
        if (needsFlip)
        {
            Flip();
        }
        
        // Moverse en la dirección actual
        float direction = facingRight ? 1f : -1f;
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

    // MÉTODO PARA SPAWNER VFX DE IMPACTO EN EL JUGADOR
    // Se llama automáticamente desde Attack() cuando golpea al jugador
    public void SpawnAttackVFX()
    {
        if (attackVFXPrefab == null || player == null) return;

        // Posición del jugador + offset
        Vector3 spawnPosition = player.position + vfxOffsetOnTarget;

        // Instanciar el VFX en la posición del jugador
        GameObject vfx = Instantiate(attackVFXPrefab, spawnPosition, Quaternion.identity);

        // Opcional: Orientar el VFX hacia el tigre (dirección del golpe)
        Vector3 directionToTiger = (transform.position - player.position).normalized;
        if (directionToTiger.x < 0)
        {
            // Si el tigre está a la izquierda, voltear el VFX
            Vector3 scale = vfx.transform.localScale;
            scale.x *= -1;
            vfx.transform.localScale = scale;
        }

        // Destruir el VFX después de un tiempo
        Destroy(vfx, vfxDestroyDelay);
    }

    public override void Attack()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distanceToPlayer <= attackRange)
        {
            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage, gameObject);
                
                // Spawner VFX de impacto en el jugador
                SpawnAttackVFX();
            }
        }

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
        throw new System.NotImplementedException();
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