using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyTiger : EnemyBase
{
    [Header("Tiger Settings")]
    public float detectionRange = 8f;
    public float attackRange = 2f;
    public LayerMask playerLayer;
    public bool facingRight = true;
    public Animator animator;

    [Header("Movement Settings")]
    public float walkSpeed = 2f;
    public float runSpeed = 5f;
    public Transform groundCheck;
    public Transform wallCheck;
    public LayerMask groundLayer;
    public float checkRadius = 0.2f;

    [Header("Patrol Settings")]
    public float patrolDistance = 5f;
    private Vector3 startPosition;
    private float leftBound;
    private float rightBound;

    [Header("Raycast Settings")]
    public Transform rayOrigin;
    public float rayLength = 8f;

    [Header("Attack Settings")]
    public float attackDamage = 10f;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;

    private Rigidbody2D rb;
    private Transform player;

    protected override void Awake()
    {
        base.Awake();
        rb = GetComponent<Rigidbody2D>();
        
        // Configurar límites de patrulla
        startPosition = transform.position;
        leftBound = startPosition.x - patrolDistance;
        rightBound = startPosition.x + patrolDistance;
    }

    private void Start()
    {
        // Buscar al jugador en la escena
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
            Debug.Log("Player encontrado: " + player.name);
        }
        else
        {
            Debug.LogWarning("No se encontró el jugador con tag 'Player'");
        }

        // Verificar componentes necesarios
        if (animator == null)
        {
            Debug.LogError("Animator no asignado en " + gameObject.name);
        }
        if (rb == null)
        {
            Debug.LogError("Rigidbody2D no encontrado en " + gameObject.name);
        }

        var idleState = new TigerIdle(this);
        StateMachine.Initialize(idleState);
        Debug.Log("Estado inicial: TigerIdle");
    }

    // MÉTODOS DE MOVIMIENTO

    public void MoveTowards(float speed)
    {
        float direction = facingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(direction * speed, rb.linearVelocity.y);
    }

    public void StopMovement()
    {
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y);
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    // MÉTODOS DE DETECCIÓN

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, checkRadius, groundLayer);
    }

    public bool IsWallAhead()
    {
        return Physics2D.OverlapCircle(wallCheck.position, checkRadius, groundLayer);
    }

    public bool IsAtPatrolBoundary()
    {
        if (facingRight && transform.position.x >= rightBound)
        {
            return true;
        }
        else if (!facingRight && transform.position.x <= leftBound)
        {
            return true;
        }
        return false;
    }

    public float GetDistanceToPlayer()
    {
        if (player != null)
        {
            return Vector2.Distance(transform.position, player.position);
        }
        return Mathf.Infinity;
    }

    public bool IsPlayerInAttackRange()
    {
        return GetDistanceToPlayer() <= attackRange;
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    // MÉTODOS ABSTRACTOS IMPLEMENTADOS

    public override bool CanSeePlayer()
    {
        if (player == null) return false;

        Vector2 forwardDir = facingRight ? Vector2.right : Vector2.left;
        Vector2 backwardDir = facingRight ? Vector2.left : Vector2.right;

        RaycastHit2D forwardHit = Physics2D.Raycast(rayOrigin.position, forwardDir, rayLength, playerLayer);
        RaycastHit2D backwardHit = Physics2D.Raycast(rayOrigin.position, backwardDir, rayLength, playerLayer);

        Debug.DrawRay(rayOrigin.position, forwardDir * rayLength, Color.red);
        Debug.DrawRay(rayOrigin.position, backwardDir * rayLength, Color.blue);

        if (forwardHit.collider != null && forwardHit.collider.CompareTag("Player"))
        {
            return true;
        }

        if (backwardHit.collider != null && backwardHit.collider.CompareTag("Player"))
        {
            Flip();
            return true;
        }

        return false;
    }

    public override void Move()
    {
        // Este método se usa desde los estados
        MoveTowards(walkSpeed);
    }

    public override void Attack()
    {
        // Este método se llama desde el AnimationEvent del ataque
        lastAttackTime = Time.time;
        
        // Detectar si el jugador está en rango y aplicar daño
        if (player != null && IsPlayerInAttackRange())
        {
            // Aquí aplicarías el daño al jugador
            // player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
            Debug.Log("Tiger attacked player!");
        }
    }

    public override void Die()
    {
        // Implementar muerte del tigre
        animator.SetTrigger("Death");
        StopMovement();
        enabled = false; // Desactivar el script
        
        // Opcional: destruir después de la animación
        Destroy(gameObject, 2f);
    }

    // VISUALIZACIÓN EN EL EDITOR

    private void OnDrawGizmosSelected()
    {
        // Rango de detección
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Rango de ataque
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Límites de patrulla
        if (Application.isPlaying)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(new Vector3(leftBound, transform.position.y, 0), 
                          new Vector3(leftBound, transform.position.y + 2, 0));
            Gizmos.DrawLine(new Vector3(rightBound, transform.position.y, 0), 
                          new Vector3(rightBound, transform.position.y + 2, 0));
        }

        // Ground check
        if (groundCheck != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(groundCheck.position, checkRadius);
        }

        // Wall check
        if (wallCheck != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(wallCheck.position, checkRadius);
        }
    }
}