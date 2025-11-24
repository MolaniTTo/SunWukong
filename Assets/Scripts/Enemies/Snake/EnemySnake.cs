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
    public float attackRange = 2.5f;
    public LayerMask playerLayer;
    public bool facingRight = false;
    public Animator animator;
    public CharacterHealth characterHealth;

    [Header("Movement Settings")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public Transform pointA;
    public Transform pointB;
    private bool movingRight = true;

    public bool MovingRight => movingRight;

    [Header("Combat Settings")]
    public float attackDamage = 15f;
    public float attackCooldown = 0f;
    public float contactDamage = 10f;
    public float contactDamageCooldown = 1f;
    private float lastContactDamageTime = -999f;
    private float lastAttackTime = 0f;
    public GameObject biteCollider;

    private Rigidbody2D rb;
    private Transform player;
    private bool isAttacking = false;

    public Transform Player => player;

    protected override void Awake()
    {
        base.Awake();

        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        if (animator == null) animator = GetComponent<Animator>();
        if (characterHealth == null) characterHealth = GetComponent<CharacterHealth>();

        if (characterHealth != null)
        {
            characterHealth.OnDeath += Death;
            characterHealth.OnTakeDamage += Damaged;
        }

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null) player = playerObj.transform;

        if (biteCollider != null) biteCollider.SetActive(false);
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
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1f;
        transform.localScale = scale;
    }

    public override bool CanSeePlayer()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        return distanceToPlayer <= detectionRange;
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

        if ((direction.x > 0 && !facingRight) || (direction.x < 0 && facingRight))
            Flip();

        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        animator.SetBool("isChasing", true);
        animator.SetBool("isMoving", false);
    }

    public void Patrol()
    {
        if (pointA == null || pointB == null)
        {
            StopMovement();
            return;
        }

        float leftLimit = Mathf.Min(pointA.position.x, pointB.position.x);
        float rightLimit = Mathf.Max(pointA.position.x, pointB.position.x);

        if (transform.position.x <= leftLimit) movingRight = true;
        else if (transform.position.x >= rightLimit) movingRight = false;

        float dir = movingRight ? 1f : -1f;

        if ((dir > 0 && !facingRight) || (dir < 0 && facingRight)) Flip();

        rb.linearVelocity = new Vector2(dir * patrolSpeed, rb.linearVelocity.y);

        animator.SetBool("isMoving", true);
        animator.SetBool("isChasing", false);
    }

    public void StopMovement()
    {
        if (rb != null) rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("isMoving", false);
        animator.SetBool("isChasing", false);
    }

    public override void Attack()
    {
        if (player == null) return;

        if (Vector2.Distance(transform.position, player.position) <= attackRange)
        {
            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null) playerHealth.TakeDamage(attackDamage, gameObject);
        }
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
        if (biteCollider != null) biteCollider.SetActive(true);
        Attack();
    }

    public void OnBiteImpactEnd()
    {
        if (biteCollider != null) biteCollider.SetActive(false);
    }

    public void OnAttackEnd()
    {
        isAttacking = false;
    }

    public override void Die()
    {
        StopMovement();
        if (biteCollider != null) biteCollider.SetActive(false);
    }
}
