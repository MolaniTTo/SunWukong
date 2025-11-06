using UnityEngine;

public class PlayerStateMachine2 : MonoBehaviour
{
    public enum PlayerState
    {
        Idle = 0,       
        Running = 1,     
        Jumping = 2,     
        AttackPunch = 3,
        AttackTail  = 4,
        Block  = 5,
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
    [SerializeField] private LayerMask groundLayer;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    private Rigidbody2D rb;
    private Vector2 moveInput;
    [SerializeField] private bool isGrounded = true;

    public PlayerState currentState;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        
        // busca automáticamente el animator si no está asignado
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }
    }

    private void Start()
    {
        ChangeState(PlayerState.Idle);
    }

    private void Update()
    {
        // Obtenemos el input aquí en Update (mejor práctica para input)
        moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), 0);

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
                //HandleAttackPunch();
                break;

            case PlayerState.AttackTail:
                //HandleAttackTail();
                break;

            case PlayerState.Block:
                //HandleBlock();
                break;

            case PlayerState.BeingHit:
                //HandleBeingHit();
                break;

            case PlayerState.Healing:
                HandleHealing();
                break;

            case PlayerState.SpecialAttackPunch:
                //HandleSpecialAttackPunch();
                break;

            case PlayerState.SpecialAttackStaff:
                //HandleSpecialAttackStaff();
                break;

            case PlayerState.Swinging:
                //HandleSwinging();
                break;

            case PlayerState.Climbing:
                //HandleClimbing();
                break;

            case PlayerState.Death:
                //HandleDeath();
                break;

            default:
                ChangeState(PlayerState.Idle);
                break;
        }
    }

    private void HandleIdle() 
    {
        if (moveInput.x != 0)
        {
            ChangeState(PlayerState.Running);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            ChangeState(PlayerState.Jumping);
        }
    }

    private void HandleRunning()
    {
        if (Mathf.Abs(moveInput.x) < 0.1f)
        {
            ChangeState(PlayerState.Idle);
        }
        else if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            Jump();
            ChangeState(PlayerState.Jumping);
        }
    }

    private void HandleJumping()
    {
        CheckIfGrounded();

        if (isGrounded)
        {
            ChangeState(Mathf.Abs(moveInput.x) > 0.1f ? PlayerState.Running : PlayerState.Idle);
        }
    }
    private void HandleHealing()
    {
        // Aquí iría la lógica de curación
        // Por simplicidad, volvemos al estado Idle después de curar
        ChangeState(PlayerState.Idle);
    }
    

    private void ChangeState(PlayerState newState)
    {
        currentState = newState;
        
        // ESTO ES LO MÁS IMPORTANTE: actualizar el parámetro del Animator
        if (animator != null)
        {
            animator.SetInteger("state", (int)newState);
        }
    }

    private void Move()
    {
        // Movimiento horizontal
        float moveX = moveInput.x * speed;
        rb.linearVelocity = new Vector2(moveX, rb.linearVelocity.y);

        // Voltear el sprite según la dirección
        if (moveInput.x != 0)
        {
            transform.localScale = new Vector3(Mathf.Sign(moveInput.x), 1, 1);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        isGrounded = false;
    }

    private void CheckIfGrounded()
    {
        // Raycast hacia abajo para detectar el suelo
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.1f, LayerMask.GetMask("Ground"));
        isGrounded = hit.collider != null;
        
        // Debug opcional para ver el raycast en la Scene view
        //Debug.DrawRay(transform.position, Vector2.down * 1.1f, isGrounded ? Color.green : Color.red);
    }

    private void FixedUpdate()
    {
        // Aplicar movimiento físico
        if (currentState == PlayerState.Running || currentState == PlayerState.Jumping)
        {
            Move();
        }
        
        // Comprobar si está en el suelo
        CheckIfGrounded();
    }
}