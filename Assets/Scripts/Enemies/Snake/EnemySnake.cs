using UnityEngine;
using System.Collections;

public class EnemySnake : EnemyBase
{
    [Header("States")]
    public SerpientePatrol PatrolState { get; private set; }
    public SerpienteChase ChaseState { get; private set; }
    public SerpienteAttack AttackState { get; private set; }
    public SerpienteDeath DeathState { get; private set; }

    [Header("References")]
    public Transform player;
    public Rigidbody2D rb;
    public Animator animator;
    public CharacterHealth characterHealth;
    public GameObject biteCollider;

    [Header("Movement Stats")]
    public float patrolSpeed = 2f;
    public float chaseSpeed = 3.5f;
    public bool facingRight = true;
    public bool animationFinished = false;
    public bool lockFacing = false;

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
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    private float lastAttackTime = 0f;

    [HideInInspector] public int facingDirection = 1;

    protected override void Awake()
    {
        base.Awake();

        if (player == null)
        {
            var pGo = GameObject.FindGameObjectWithTag("Player");
            if (pGo != null) player = pGo.transform;
        }

        if (rb == null) rb = GetComponent<Rigidbody2D>();

        if (biteCollider != null) biteCollider.SetActive(false);

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

    public void Flip()
    {
        if (!lockFacing)
        {
            if (player != null)
            {
                if (player.position.x > transform.position.x && facingRight)
                {
                    facingRight = false;
                    facingDirection = -1;
                    Vector3 scale = transform.localScale;
                    scale.x = -Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }
                else if (player.position.x < transform.position.x && !facingRight)
                {
                    facingRight = true;
                    facingDirection = 1;
                    Vector3 scale = transform.localScale;
                    scale.x = Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }
            }
        }
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

        if (direction.x > 0 && facingRight)
        {
            facingRight = false;
            facingDirection = -1;
            Vector3 scale = transform.localScale;
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
        }
        else if (direction.x < 0 && !facingRight)
        {
            facingRight = true;
            facingDirection = 1;
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;
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

        if (distanceToPlayer > attackRange)
        {
            Vector2 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            rb.linearVelocity = direction * chaseSpeed;
        }
        else
        {
            StopMovement();
        }
    }

    public void StopMovement()
    {
        if (rb != null) rb.linearVelocity = Vector2.zero;
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
        return distance <= attackRange;
    }

    public bool CanAttack()
    {
        return Time.time >= lastAttackTime + attackCooldown;
    }

    public override void Attack()
    {
        lastAttackTime = Time.time;
    }

    public void OnBiteImpact()
    {
        biteCollider.SetActive(true);
    }

    public void OnBiteImpactEnd()
    {
        biteCollider.SetActive(false);
    }

    public void OnAttackEnd()
    {
        animationFinished = true;
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
        if (biteCollider != null) biteCollider.SetActive(false);
    }

    public override void Move()
    {
    }
}
