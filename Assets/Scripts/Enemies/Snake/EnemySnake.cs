using UnityEngine;
using System.Collections;

public class EnemySnake : EnemyBase
{
    [Header("States")]
    public SnakePatrol PatrolState { get; private set; }
    public SnakeChase ChaseState { get; private set; }
    public SnakeAttack AttackState { get; private set; }
    public SnakeDeath DeathState { get; private set; }

    [Header("References")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;
    public CharacterHealth characterHealth;
    public Transform attackPoint;
    public GameObject projectilePrefab;

    [Header("Movement Stats")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public bool facingRight = true;

    [Header("Patrol Settings")]
    public Transform pointA;
    public Transform pointB;
    private Transform currentTarget;
    public float waypointReachDistance = 0.5f;
    public float waitTimeAtWaypoint = 1f;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    [Header("Detection Settings")]
    public float detectionRange = 5f;
    public float losePlayerRange = 8f;
    public LayerMask playerLayer;

    [Header("Attack Settings")]
    public float attackRange = 3f;
    public float minAttackDistance = 2f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;
    public float projectileSpeed = 6f;

    private int facingDirection = 1;

    protected override void Awake()
    {
        base.Awake();
        
        if (player == null)
        {
            var pGo = GameObject.FindGameObjectWithTag("Player");
            if (pGo != null) player = pGo.transform;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();
        
        if (characterHealth == null)
        {
            characterHealth = GetComponent<CharacterHealth>();
        }

        if (characterHealth != null)
        {
            characterHealth.OnDeath += HandleDeath;
            characterHealth.OnTakeDamage += (currentHealth, attacker) => HandleDamage();
        }

        if (pointA != null) currentTarget = pointA;
    }

    private void OnDestroy()
    {
        if (characterHealth != null)
        {
            characterHealth.OnDeath -= HandleDeath;
            characterHealth.OnTakeDamage -= (currentHealth, attacker) => HandleDamage();
        }
    }

    private void Start()
    {
        PatrolState = new SnakePatrol(this);
        ChaseState = new SnakeChase(this);
        AttackState = new SnakeAttack(this);
        DeathState = new SnakeDeath(this);

        StateMachine.Initialize(PatrolState);
    }

    protected override void Update()
    {
        StateMachine.Update();
    }

    public void PatrolMovement()
    {
        if (currentTarget == null || isWaiting)
        {
            StopMovement();
            return;
        }

        Vector2 direction = (currentTarget.position - transform.position).normalized;
        direction.y = 0;
        
        rb.linearVelocity = direction * patrolSpeed;

        float distance = Vector2.Distance(new Vector2(transform.position.x, 0), new Vector2(currentTarget.position.x, 0));
        
        if (distance <= waypointReachDistance)
        {
            StartCoroutine(WaitAtWaypoint());
        }

        if (direction.x > 0 && !facingRight)
        {
            Flip();
        }
        else if (direction.x < 0 && facingRight)
        {
            Flip();
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        StopMovement();
        
        yield return new WaitForSeconds(waitTimeAtWaypoint);
        
        if (currentTarget == pointA)
            currentTarget = pointB;
        else
            currentTarget = pointA;
        
        isWaiting = false;
    }

    public void ChaseMovement()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer < minAttackDistance)
        {
            Vector2 awayFromPlayer = (transform.position - player.position).normalized;
            awayFromPlayer.y = 0;
            rb.linearVelocity = awayFromPlayer * patrolSpeed;
        }
        else if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            rb.linearVelocity = direction * chaseSpeed;
        }
        else
        {
            StopMovement();
        }

        if (player.position.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (player.position.x < transform.position.x && facingRight)
        {
            Flip();
        }
    }

    public void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        facingDirection = facingRight ? 1 : -1;
        
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public override bool CanSeePlayer()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= detectionRange;
    }

    public bool HasLostPlayer()
    {
        if (player == null) return true;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance > losePlayerRange;
    }

    public bool IsInAttackRange()
    {
        if (player == null) return false;

        float distance = Vector2.Distance(transform.position, player.position);
        return distance <= attackRange && distance >= minAttackDistance;
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public override void Attack()
    {
        if (projectilePrefab != null && attackPoint != null)
        {
            GameObject projectile = Instantiate(projectilePrefab, attackPoint.position, Quaternion.identity);
            
            Vector2 direction = (player.position - attackPoint.position).normalized;
            
            Rigidbody2D projRb = projectile.GetComponent<Rigidbody2D>();
            if (projRb != null)
            {
                projRb.linearVelocity = direction * projectileSpeed;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            projectile.transform.rotation = Quaternion.Euler(0, 0, angle);

            lastAttackTime = Time.time;

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
        }
    }

    private void HandleDamage()
    {
        if (animator != null)
        {
            animator.SetTrigger("Damaged");
        }
    }

    private void HandleDeath()
    {
        if (DeathState != null && StateMachine != null)
        {
            StateMachine.ChangeState(DeathState);
        }
        StopMovement();
    }

    public override void Die()
    {
        StopMovement();
    }

    public override void Move()
    {
        // Implementado en PatrolMovement y ChaseMovement
    }
}