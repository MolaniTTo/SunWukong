using UnityEngine;
using System.Collections;

public class PlayerStateMachine2 : MonoBehaviour
{
    public enum PlayerState
    {
        Idle = 0,
        Running = 1,
        Jumping = 2,
        AttackPunch = 3,
        AttackTail = 4,
        Block = 5,
        BeingHit = 6,
        Healing = 7,
        SpecialAttackPunch = 8,
        SpecialAttackStaff = 9,
        Swinging = 10,
        Climbing = 11,
        Death = 12
    }

    [Header("Movement")]
    public float speed = 5f;
    public float jumpForce = 15f;
    public float climbSpeed = 3f;
    public float groundCheckDistance = 1.1f;

    [Header("Combat")]
    public float attackDuration = 0.5f;
    public float specialAttackDuration = 1f;
    public float blockDuration = 0.3f; // Mínimo tiempo bloqueando
    public float hitStunDuration = 0.4f;
    public float healingDuration = 2f;

    [Header("Health")]
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float healAmount = 20f;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Detection")]
    [SerializeField] private LayerMask climbableLayer;
    [SerializeField] private float climbCheckDistance = 0.6f;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    private bool isGrounded = true;
    private bool canClimb = false;
    private float stateTimer = 0f; // Timer para estados con duración
    private bool isBlocking = false;

    public PlayerState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        if (rb == null)
        {
           
            enabled = false;
            return;
        }

        rb.gravityScale = 3f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        currentHealth = maxHealth;
    }

    private void Start()
    {
        ChangeState(PlayerState.Idle);
    }

    private void Update()
    {
        // Leer input
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(horizontal, vertical);

        // Actualizar timer si es necesario
        if (stateTimer > 0)
        {
            stateTimer -= Time.deltaTime;
        }

        // Máquina de estados
        switch (currentState)
        {
            case PlayerState.Idle:
                HandleIdle();
                break;

            case PlayerState.Running:
                HandleRunning();
                break;

            case PlayerState.Jumping:
                HandleJumping();
                break;

            case PlayerState.AttackPunch:
                HandleAttackPunch();
                break;

            case PlayerState.AttackTail:
                HandleAttackTail();
                break;

            case PlayerState.Block:
                HandleBlock();
                break;

            case PlayerState.BeingHit:
                HandleBeingHit();
                break;

            case PlayerState.Healing:
                HandleHealing();
                break;

            case PlayerState.SpecialAttackPunch:
                HandleSpecialAttackPunch();
                break;

            case PlayerState.SpecialAttackStaff:
                HandleSpecialAttackStaff();
                break;

            case PlayerState.Swinging:
                HandleSwinging();
                break;

            case PlayerState.Climbing:
                HandleClimbing();
                break;

            case PlayerState.Death:
                HandleDeath();
                break;
        }
    }

    // ==================== ESTADOS BÁSICOS ====================

    private void HandleIdle()
    {
        // Movimiento horizontal → Running
        if (moveInput.x != 0)
        {
            ChangeState(PlayerState.Running);
        }
        // Saltar
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            ChangeState(PlayerState.Jumping);
        }
        // Ataque Punch (tecla J)
        else if (Input.GetKeyDown(KeyCode.J))
        {
            StartAttack(PlayerState.AttackPunch, attackDuration);
        }
        // Ataque Tail (tecla K)
        else if (Input.GetKeyDown(KeyCode.K))
        {
            StartAttack(PlayerState.AttackTail, attackDuration);
        }
        // Bloquear (tecla L - mantener)
        else if (Input.GetKey(KeyCode.L))
        {
            StartBlock();
        }
        // Curarse (tecla H)
        else if (Input.GetKeyDown(KeyCode.H) && currentHealth < maxHealth)
        {
            StartHealing();
        }
        // Ataque especial Punch (tecla U)
        else if (Input.GetKeyDown(KeyCode.U))
        {
            StartAttack(PlayerState.SpecialAttackPunch, specialAttackDuration);
        }
        // Ataque especial Staff (tecla I)
        else if (Input.GetKeyDown(KeyCode.I))
        {
            StartAttack(PlayerState.SpecialAttackStaff, specialAttackDuration);
        }
        // Trepar (tecla arriba cerca de pared)
        else if (moveInput.y > 0 && canClimb)
        {
            ChangeState(PlayerState.Climbing);
        }
    }

    private void HandleRunning()
    {
        // Dejar de moverse → Idle
        if (Mathf.Abs(moveInput.x) < 0.1f)
        {
            ChangeState(PlayerState.Idle);
        }
        // Saltar mientras corre
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            ChangeState(PlayerState.Jumping);
        }
        // Ataque mientras corre (Punch)
        else if (Input.GetKeyDown(KeyCode.J))
        {
            StartAttack(PlayerState.AttackPunch, attackDuration);
        }
        // Ataque mientras corre (Tail)
        else if (Input.GetKeyDown(KeyCode.K))
        {
            StartAttack(PlayerState.AttackTail, attackDuration);
        }
    }

    private void HandleJumping()
    {
        // Cuando toca el suelo
        if (isGrounded)
        {
            ChangeState(Mathf.Abs(moveInput.x) > 0.1f ? PlayerState.Running : PlayerState.Idle);
        }
        // Ataque aéreo (solo ataques normales)
        else if (Input.GetKeyDown(KeyCode.J))
        {
            StartAttack(PlayerState.AttackPunch, attackDuration);
        }
        else if (Input.GetKeyDown(KeyCode.K))
        {
            StartAttack(PlayerState.AttackTail, attackDuration);
        }
    }

    // ==================== ATAQUES ====================

    private void HandleAttackPunch()
    {
        // Mantener velocidad horizontal si está en el aire
        if (!isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput.x * speed * 0.5f, rb.linearVelocity.y);
        }

        // Fin del ataque
        if (stateTimer <= 0)
        {
            if (isGrounded)
            {
                ChangeState(Mathf.Abs(moveInput.x) > 0.1f ? PlayerState.Running : PlayerState.Idle);
            }
            else
            {
                ChangeState(PlayerState.Jumping);
            }
        }
    }

    private void HandleAttackTail()
    {
        // Similar al punch
        if (!isGrounded)
        {
            rb.linearVelocity = new Vector2(moveInput.x * speed * 0.5f, rb.linearVelocity.y);
        }

        if (stateTimer <= 0)
        {
            if (isGrounded)
            {
                ChangeState(Mathf.Abs(moveInput.x) > 0.1f ? PlayerState.Running : PlayerState.Idle);
            }
            else
            {
                ChangeState(PlayerState.Jumping);
            }
        }
    }

    private void HandleSpecialAttackPunch()
    {
        // Los ataques especiales detienen el movimiento
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (stateTimer <= 0)
        {
            ChangeState(PlayerState.Idle);
        }
    }

    private void HandleSpecialAttackStaff()
    {
        // Los ataques especiales detienen el movimiento
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        if (stateTimer <= 0)
        {
            ChangeState(PlayerState.Idle);
        }
    }

    // ==================== DEFENSA Y DAÑO ====================

    private void HandleBlock()
    {
        // Bloquear detiene el movimiento
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Soltar la tecla de bloqueo
        if (!Input.GetKey(KeyCode.L) && stateTimer <= 0)
        {
            isBlocking = false;
            ChangeState(PlayerState.Idle);
        }
    }

    private void HandleBeingHit() 
    {
    //estado que he puesto para que se vea una animacion al recibir daño de momento lo dejo  asi
        if (stateTimer <= 0)
        {
            if (currentHealth <= 0)
            {
                ChangeState(PlayerState.Death);
            }
            else if (isGrounded)
            {
                ChangeState(PlayerState.Idle);
            }
            else
            {
                ChangeState(PlayerState.Jumping);
            }
        }
    }

    // ==================== CURACIÓN ====================

    private void HandleHealing()
    {
        // No se puede mover mientras se cura
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);

        // Cancelar curación si presiona otra tecla
        if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.K) || 
            Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.Space))
        {
           
            ChangeState(PlayerState.Idle);
            return;
        }

        // Curación completada
        if (stateTimer <= 0)
        {
            currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
           
            ChangeState(PlayerState.Idle);
        }
    }

    // ==================== ESTADOS ESPECIALES ====================

    private void HandleClimbing()
    {
        //falta implementar la logica
    }

    private void HandleSwinging()
    {
        // falta implementar lógica 
        // Por ahora, es un estado placeholder

        
    }

    private void HandleDeath()
    {
        // Detener todo movimiento
        rb.linearVelocity = Vector2.zero;
       
        
       
      
    }

    // ==================== FUNCIONES DE APOYO  ====================

    private void StartAttack(PlayerState attackState, float duration)
    {
        stateTimer = duration;
        ChangeState(attackState);
    }

    private void StartBlock()
    {
        isBlocking = true;
        stateTimer = blockDuration; // Tiempo mínimo bloqueando
        ChangeState(PlayerState.Block);
    }

    private void StartHealing()
    {
        stateTimer = healingDuration;
        ChangeState(PlayerState.Healing);
    }

    private void ChangeState(PlayerState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        
        if (animator != null)
        {
            animator.SetInteger("state", (int)newState);
        }
    }

    private void Move()
    {
        if (rb == null) return;

        float velocityX = moveInput.x * speed;
        rb.linearVelocity = new Vector2(velocityX, rb.linearVelocity.y);

        // Voltear sprite
        if (moveInput.x != 0)
        {
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * Mathf.Sign(moveInput.x);
            transform.localScale = scale;
        }
    }

    private void Jump()
    {
        if (rb == null) return;
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    private void CheckIfGrounded()
    {
        Vector2 rayOrigin = transform.position;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, groundCheckDistance, LayerMask.GetMask("Ground"));
        isGrounded = hit.collider != null;
        
        Debug.DrawRay(rayOrigin, Vector2.down * groundCheckDistance, isGrounded ? Color.green : Color.red);
    }

    

    private void FixedUpdate()
    {
        CheckIfGrounded();
       

        // Aplicar movimiento según el estado
        if (currentState == PlayerState.Running || currentState == PlayerState.Jumping)
        {
            Move();
        }
    }

    // ==================== MÉTODOS PÚBLICO (para recibir daño desde otros scripts) ====================

    public void TakeDamage(float damage, Vector2 knockbackDirection)
    {
        //falta implementar logica
    }



    // ==================== INFO ====================
    //esta info hace que muestre por pantalla el estado en el cual te encuentras, la vida actual y si estas en el suelo o no
    private void OnGUI()
    {
        // Mostrar info en pantalla
        GUI.Box(new Rect(10, 10, 200, 100), "");
        GUI.Label(new Rect(20, 20, 180, 20), $"Estado: {currentState}");
        GUI.Label(new Rect(20, 40, 180, 20), $"Vida: {currentHealth:F0}/{maxHealth}");
        GUI.Label(new Rect(20, 60, 180, 20), $"En suelo: {isGrounded}");
        
    }
}